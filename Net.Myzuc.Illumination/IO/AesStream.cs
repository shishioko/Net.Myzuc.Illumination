//https://github.com/MCCTeam/Minecraft-Console-Client/blob/master/MinecraftClient/Crypto/
using Microsoft.VisualStudio.Threading;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Me.Shishioko.Illumination
{
    internal class AesStream : Stream
    {
        private readonly Aes Aes;
        private bool InEnd = false;
        private readonly byte[] ReadStreamIV = new byte[16];
        private readonly byte[] WriteStreamIV = new byte[16];
        public Stream BaseStream { get; set; }
        public AesStream(Stream stream, byte[] key)
        {
            BaseStream = stream;
            Aes = Aes.Create();
            Aes.BlockSize = 128;
            Aes.KeySize = 128;
            Aes.Key = key;
            Aes.Mode = CipherMode.ECB;
            Aes.Padding = PaddingMode.None;
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
        public override int ReadByte()
        {
            if (InEnd)
                return -1;

            int inputBuf = BaseStream.ReadByte();
            if (inputBuf == -1)
            {
                InEnd = true;
                return -1;
            }

            Span<byte> blockOutput = stackalloc byte[16];
            Aes.EncryptEcb(ReadStreamIV, blockOutput, PaddingMode.None);
            Array.Copy(ReadStreamIV, 1, ReadStreamIV, 0, 15);
            ReadStreamIV[15] = (byte)inputBuf;

            return (byte)(blockOutput[0] ^ inputBuf);
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public override int Read(byte[] buffer, int outOffset, int required)
        {
            if (InEnd) return 0;
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
                OrderablePartitioner<Tuple<int, int>> rangePartitioner = curRead <= 256 ? Partitioner.Create(readed, processEnd, 32) : Partitioner.Create(readed, processEnd);
                Parallel.ForEach(rangePartitioner, (range, loopState) =>
                {
                    Span<byte> blockOutput = stackalloc byte[16];
                    for (int idx = range.Item1; idx < range.Item2; idx++)
                    {
                        ReadOnlySpan<byte> blockInput = new(inputBuf, idx, 16);
                        Aes.EncryptEcb(blockInput, blockOutput, PaddingMode.None);
                        buffer[outOffset + idx] = (byte)(blockOutput[0] ^ inputBuf[idx + 16]);
                    }
                });
            }
            Array.Copy(inputBuf, required, ReadStreamIV, 0, 16);
            return required;
        }
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void WriteByte(byte b)
        {
            Span<byte> blockOutput = stackalloc byte[16];
            Aes!.EncryptEcb(WriteStreamIV, blockOutput, PaddingMode.None);
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
                Aes!.EncryptEcb(blockInput, blockOutput, PaddingMode.None);
                outputBuf[16 + wirtten] = (byte)(blockOutput[0] ^ input[offset + wirtten]);
            }
            BaseStream.WriteAsync(outputBuf, 16, required).Forget();
            Array.Copy(outputBuf, required, WriteStreamIV, 0, 16);
        }
    }
}
