// https://github.com/MCCTeam/Minecraft-Console-Client/blob/master/MinecraftClient/Crypto/
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Net.Myzuc.Illumination
{
    internal class FastAesStream : Stream
    {
        public static bool Supported => Sse2.IsSupported && Aes.IsSupported;
        private static Vector128<byte>[] KeyExpansion(Span<byte> key)
        {
            var keys = new Vector128<byte>[20];
            keys[0] = Unsafe.ReadUnaligned<Vector128<byte>>(ref key[0]);
            MakeRoundKey(keys, 1, 0x01);
            MakeRoundKey(keys, 2, 0x02);
            MakeRoundKey(keys, 3, 0x04);
            MakeRoundKey(keys, 4, 0x08);
            MakeRoundKey(keys, 5, 0x10);
            MakeRoundKey(keys, 6, 0x20);
            MakeRoundKey(keys, 7, 0x40);
            MakeRoundKey(keys, 8, 0x80);
            MakeRoundKey(keys, 9, 0x1b);
            MakeRoundKey(keys, 10, 0x36);
            for (int i = 1; i < 10; i++)
            {
                keys[10 + i] = Aes.InverseMixColumns(keys[i]);
            }
            return keys;
        }
        private static void MakeRoundKey(Vector128<byte>[] keys, int i, byte rcon)
        {
            Vector128<byte> s = keys[i - 1];
            Vector128<byte> t = keys[i - 1];
            t = Aes.KeygenAssist(t, rcon);
            t = Sse2.Shuffle(t.AsUInt32(), 0xFF).AsByte();
            s = Sse2.Xor(s, Sse2.ShiftLeftLogical128BitLane(s, 4));
            s = Sse2.Xor(s, Sse2.ShiftLeftLogical128BitLane(s, 8));
            keys[i] = Sse2.Xor(s, t);
        }
        private Vector128<byte>[] RoundKeys { get; }
        private bool InEnd = false;
        private readonly byte[] ReadStreamIV = new byte[16];
        private readonly byte[] WriteStreamIV = new byte[16];
        public Stream BaseStream { get; set; }
        public FastAesStream(Stream stream, byte[] key)
        {
            BaseStream = stream;
            RoundKeys = KeyExpansion(key);
            Array.Copy(key, ReadStreamIV, 16);
            Array.Copy(key, WriteStreamIV, 16);
        }
        public override bool CanRead => BaseStream.CanRead;
        public override bool CanSeek => false;
        public override bool CanWrite => BaseStream.CanWrite;
        public override void Flush() => BaseStream.Flush();
        public override long Length => throw new NotSupportedException();
        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing) BaseStream.Dispose();
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void EncryptEcb(ReadOnlySpan<byte> plaintext, Span<byte> destination)
        {
            Vector128<byte>[] keys = RoundKeys;
            ReadOnlySpan<Vector128<byte>> blocks = MemoryMarshal.Cast<byte, Vector128<byte>>(plaintext);
            Span<Vector128<byte>> dest = MemoryMarshal.Cast<byte, Vector128<byte>>(destination);
            _ = keys[10];
            for (int i = 0; i < blocks.Length; i++)
            {
                Vector128<byte> b = blocks[i];
                b = Sse2.Xor(b, keys[0]);
                b = Aes.Encrypt(b, keys[1]);
                b = Aes.Encrypt(b, keys[2]);
                b = Aes.Encrypt(b, keys[3]);
                b = Aes.Encrypt(b, keys[4]);
                b = Aes.Encrypt(b, keys[5]);
                b = Aes.Encrypt(b, keys[6]);
                b = Aes.Encrypt(b, keys[7]);
                b = Aes.Encrypt(b, keys[8]);
                b = Aes.Encrypt(b, keys[9]);
                b = Aes.EncryptLast(b, keys[10]);
                dest[i] = b;
            }
        }
        public override int ReadByte()
        {
            if (InEnd) return -1;
            int inputBuf = BaseStream.ReadByte();
            if (inputBuf == -1)
            {
                InEnd = true;
                return -1;
            }
            Span<byte> blockOutput = stackalloc byte[16];
            EncryptEcb(ReadStreamIV, blockOutput);
            Array.Copy(ReadStreamIV, 1, ReadStreamIV, 0, 15);
            ReadStreamIV[15] = (byte)inputBuf;
            return (byte)(blockOutput[0] ^ inputBuf);
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public override int Read(byte[] buffer, int outOffset, int required)
        {
            if (InEnd) return 0;
            Span<byte> blockOutput = stackalloc byte[16];
            byte[] inputBuf = new byte[16 + required];
            Array.Copy(ReadStreamIV, inputBuf, 16);
            for (int readed = 0, curRead; readed < required; readed += curRead)
            {
                curRead = BaseStream.Read(inputBuf, 16 + readed, required - readed);
                if (curRead == 0)
                {
                    InEnd = true;
                    return readed;
                }
                int processEnd = readed + curRead;
                for (int idx = readed; idx < processEnd; idx++)
                {
                    ReadOnlySpan<byte> blockInput = new(inputBuf, idx, 16);
                    EncryptEcb(blockInput, blockOutput);
                    buffer[outOffset + idx] = (byte)(blockOutput[0] ^ inputBuf[idx + 16]);
                }
            }
            Array.Copy(inputBuf, required, ReadStreamIV, 0, 16);
            return required;
        }
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void WriteByte(byte b)
        {
            Span<byte> blockOutput = stackalloc byte[16];
            EncryptEcb(WriteStreamIV, blockOutput);
            byte outputBuf = (byte)(blockOutput[0] ^ b);
            BaseStream.WriteByte(outputBuf);
            Array.Copy(WriteStreamIV, 1, WriteStreamIV, 0, 15);
            WriteStreamIV[15] = outputBuf;
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public override void Write(byte[] input, int offset, int required)
        {
            byte[] outputBuf = new byte[16 + required];
            Array.Copy(WriteStreamIV, outputBuf, 16);

            Span<byte> blockOutput = stackalloc byte[16];
            for (int wirtten = 0; wirtten < required; ++wirtten)
            {
                ReadOnlySpan<byte> blockInput = new(outputBuf, wirtten, 16);
                EncryptEcb(blockInput, blockOutput);
                outputBuf[16 + wirtten] = (byte)(blockOutput[0] ^ input[offset + wirtten]);
            }
            BaseStream.Write(outputBuf, 16, required);
            Array.Copy(outputBuf, required, WriteStreamIV, 0, 16);
        }
    }
}
