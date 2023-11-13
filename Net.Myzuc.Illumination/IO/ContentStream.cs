using Newtonsoft.Json.Linq;
using System;
using System.Buffers.Binary;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

namespace Net.Myzuc.Illumination.Net
{
    public sealed class ContentStream : IDisposable
    {
        private readonly bool CanWrite;
        private readonly bool CanRead;
        internal Stream Stream;
        public ContentStream()
        {
            Stream = new MemoryStream();
            CanRead = false;
            CanWrite = true;
        }
        public ContentStream(ReadOnlySpan<byte> data)
        {
            Stream = new MemoryStream(data.ToArray());
            CanRead = true;
            CanWrite = false;
        }
        public ContentStream(Stream stream)
        {
            CanRead = true;
            CanWrite = true;
            Stream = stream;
        }
        public void Dispose()
        {
            Stream.Dispose();
        }
        public Span<byte> Get()
        {
            Contract.Requires(CanRead != CanWrite);
            if (Stream is not MemoryStream ms) throw new InvalidOperationException("Not a write-in-only stream!");
            return ms.ToArray().AsSpan();
        }
        public void WriteU8A(ReadOnlySpan<byte> data)
        {
            Contract.Requires(CanWrite);
            Stream.Write(data);
        }
        public void WriteS8A(ReadOnlySpan<sbyte> data)
        {
            Contract.Requires(CanWrite);
            Stream.Write(MemoryMarshal.AsBytes(data));
        }
        public void WriteU16A(ReadOnlySpan<ushort> data)
        {
            Contract.Requires(CanWrite);
            Contract.Requires(data.Length * sizeof(ushort) <= int.MaxValue);
            Span<byte> span = stackalloc byte[data.Length * sizeof(ushort)];
            for (int i = 0; i < data.Length; i++)
            {
                BinaryPrimitives.WriteUInt16BigEndian(span[(i * sizeof(ushort))..], data[i]);
            }
            Stream.Write(span);
        }
        public void WriteS16A(ReadOnlySpan<short> data)
        {
            Contract.Requires(CanWrite);
            Contract.Requires(data.Length * sizeof(short) <= int.MaxValue);
            Span<byte> span = stackalloc byte[data.Length * sizeof(short)];
            for (int i = 0; i < data.Length; i++)
            {
                BinaryPrimitives.WriteInt16BigEndian(span[(i * sizeof(short))..], data[i]);
            }
            Stream.Write(span);
        }
        public void WriteU32A(ReadOnlySpan<uint> data)
        {
            Contract.Requires(CanWrite);
            Contract.Requires(data.Length * sizeof(uint) <= int.MaxValue);
            Span<byte> span = stackalloc byte[data.Length * sizeof(uint)];
            for (int i = 0; i < data.Length; i++)
            {
                BinaryPrimitives.WriteUInt32BigEndian(span[(i * sizeof(uint))..], data[i]);
            }
            Stream.Write(span);
        }
        public void WriteS32A(ReadOnlySpan<int> data)
        {
            Contract.Requires(CanWrite);
            Contract.Requires(data.Length * sizeof(int) <= int.MaxValue);
            Span<byte> span = stackalloc byte[data.Length * sizeof(int)];
            for (int i = 0; i < data.Length; i++)
            {
                BinaryPrimitives.WriteInt32BigEndian(span[(i * sizeof(int))..], data[i]);
            }
            Stream.Write(span);
        }
        public void WriteU64A(ReadOnlySpan<ulong> data)
        {
            Contract.Requires(CanWrite);
            Contract.Requires(data.Length * sizeof(ulong) <= int.MaxValue);
            Span<byte> span = stackalloc byte[data.Length * sizeof(ulong)];
            for (int i = 0; i < data.Length; i++)
            {
                BinaryPrimitives.WriteUInt64BigEndian(span[(i * sizeof(ulong))..], data[i]);
            }
            Stream.Write(span);
        }
        public void WriteS64A(ReadOnlySpan<long> data)
        {
            Contract.Requires(CanWrite);
            Contract.Requires(data.Length * sizeof(long) <= int.MaxValue);
            Span<byte> span = stackalloc byte[data.Length * sizeof(long)];
            for (int i = 0; i < data.Length; i++)
            {
                BinaryPrimitives.WriteInt64BigEndian(span[(i * sizeof(long))..], data[i]);
            }
            Stream.Write(span);
        }
        public void WriteF32A(ReadOnlySpan<float> data)
        {
            Contract.Requires(CanWrite);
            Contract.Requires(data.Length * sizeof(float) <= int.MaxValue);
            Span<byte> span = stackalloc byte[data.Length * sizeof(float)];
            for (int i = 0; i < data.Length; i++)
            {
                BinaryPrimitives.WriteSingleBigEndian(span[(i * sizeof(float))..], data[i]);
            }
            Stream.Write(span);
        }
        public void WriteF64A(ReadOnlySpan<double> data)
        {
            Contract.Requires(CanWrite);
            Contract.Requires(data.Length * sizeof(double) <= int.MaxValue);
            Span<byte> span = stackalloc byte[data.Length * sizeof(double)];
            for (int i = 0; i < data.Length; i++)
            {
                BinaryPrimitives.WriteDoubleBigEndian(span[(i * sizeof(double))..], data[i]);
            }
            Stream.Write(span);
        }
        public void WriteU8(byte data)
        {
            Contract.Requires(CanWrite);
            Stream.Write(stackalloc byte[] { data });
        }
        public void WriteS8(sbyte data)
        {
            Contract.Requires(CanWrite);
            Stream.Write(MemoryMarshal.AsBytes(stackalloc sbyte[] { data }));
        }
        public void WriteU16(ushort data)
        {
            Contract.Requires(CanWrite);
            Span<byte> span = stackalloc byte[sizeof(ushort)];
            BinaryPrimitives.WriteUInt16BigEndian(span, data);
            Stream.Write(span);
        }
        public void WriteS16(short data)
        {
            Contract.Requires(CanWrite);
            Span<byte> span = stackalloc byte[sizeof(short)];
            BinaryPrimitives.WriteInt16BigEndian(span, data);
            Stream.Write(span);
        }
        public void WriteU32(uint data)
        {
            Contract.Requires(CanWrite);
            Span<byte> span = stackalloc byte[sizeof(uint)];
            BinaryPrimitives.WriteUInt32BigEndian(span, data);
            Stream.Write(span);
        }
        public void WriteS32(int data)
        {
            Contract.Requires(CanWrite);
            Span<byte> span = stackalloc byte[sizeof(int)];
            BinaryPrimitives.WriteInt32BigEndian(span, data);
            Stream.Write(span);
        }
        public void WriteU64(ulong data)
        {
            Contract.Requires(CanWrite);
            Span<byte> span = stackalloc byte[sizeof(ulong)];
            BinaryPrimitives.WriteUInt64BigEndian(span, data);
            Stream.Write(span);
        }
        public void WriteS64(long data)
        {
            Contract.Requires(CanWrite);
            Span<byte> span = stackalloc byte[sizeof(long)];
            BinaryPrimitives.WriteInt64BigEndian(span, data);
            Stream.Write(span);
        }
        public void WriteF32(float data)
        {
            Contract.Requires(CanWrite);
            Span<byte> span = stackalloc byte[sizeof(float)];
            BinaryPrimitives.WriteSingleBigEndian(span, data);
            Stream.Write(span);
        }
        public void WriteF64(double data)
        {
            Contract.Requires(CanWrite);
            Span<byte> span = stackalloc byte[sizeof(double)];
            BinaryPrimitives.WriteDoubleBigEndian(span, data);
            Stream.Write(span);
        }
        public void WriteU32V(uint data)
        {
            Contract.Requires(CanWrite);
            do
            {
                byte current = (byte)data;
                data >>= 7;
                if (data != 0) current |= 128;
                else current &= 127;
                Stream.WriteByte(current);
            }
            while (data != 0);
        }
        public void WriteS32V(int data)
        {
            Contract.Requires(CanWrite);
            uint value = (uint)data;
            do
            {
                byte current = (byte)value;
                value >>= 7;
                if (value != 0) current |= 128;
                else current &= 127;
                Stream.WriteByte(current);
            }
            while (value != 0);
        }
        public void WriteU64V(ulong data)
        {
            Contract.Requires(CanWrite);
            do
            {
                byte current = (byte)data;
                data >>= 7;
                if (data != 0) current |= 128;
                else current &= 127;
                Stream.WriteByte(current);
            }
            while (data != 0);
        }
        public void WriteS64V(long data)
        {
            Contract.Requires(CanWrite);
            do
            {
                byte current = (byte)data;
                data >>= 7;
                if (data != 0) current |= 128;
                else current &= 127;
                Stream.WriteByte(current);
            }
            while (data != 0);
        }
        public void WriteU32V(uint data, out int size)
        {
            Contract.Requires(CanWrite);
            size = 0;
            do
            {
                byte current = (byte)data;
                data >>= 7;
                if (data != 0) current |= 128;
                else current &= 127;
                Stream.WriteByte(current);
                size++;
            }
            while (data != 0);
        }
        public void WriteS32V(int data, out int size)
        {
            Contract.Requires(CanWrite);
            size = 0;
            do
            {
                byte current = (byte)data;
                data >>= 7;
                if (data != 0) current |= 128;
                else current &= 127;
                Stream.WriteByte(current);
                size++;
            }
            while (data != 0);
        }
        public void WriteU64V(ulong data, out int size)
        {
            Contract.Requires(CanWrite);
            size = 0;
            do
            {
                byte current = (byte)data;
                data >>= 7;
                if (data != 0) current |= 128;
                else current &= 127;
                Stream.WriteByte(current);
                size++;
            }
            while (data != 0);
        }
        public void WriteS64V(long data, out int size)
        {
            Contract.Requires(CanWrite);
            size = 0;
            do
            {
                byte current = (byte)data;
                data >>= 7;
                if (data != 0) current |= 128;
                else current &= 127;
                Stream.WriteByte(current);
                size++;
            }
            while (data != 0);
        }
        public void WriteU8AV(ReadOnlySpan<byte> buffer)
        {
            Contract.Requires(CanWrite);
            WriteS32V(buffer.Length);
            Stream.Write(buffer);
        }
        public void WriteS8AV(ReadOnlySpan<sbyte> buffer)
        {
            Contract.Requires(CanWrite);
            WriteS32V(buffer.Length);
            Stream.Write(MemoryMarshal.Cast<sbyte, byte>(buffer));
        }
        public void WriteU16AV(ReadOnlySpan<ushort> buffer)
        {
            Contract.Requires(CanWrite);
            WriteS32V(buffer.Length);
            WriteU16A(buffer);
        }
        public void WriteS16AV(ReadOnlySpan<short> buffer)
        {
            Contract.Requires(CanWrite);
            WriteS32V(buffer.Length);
            WriteS16A(buffer);
        }
        public void WriteU32AV(ReadOnlySpan<uint> buffer)
        {
            Contract.Requires(CanWrite);
            WriteS32V(buffer.Length);
            WriteU32A(buffer);
        }
        public void WriteS32AV(ReadOnlySpan<int> buffer)
        {
            Contract.Requires(CanWrite);
            WriteS32V(buffer.Length);
            WriteS32A(buffer);
        }
        public void WriteU64AV(ReadOnlySpan<ulong> buffer)
        {
            Contract.Requires(CanWrite);
            WriteS32V(buffer.Length);
            WriteU64A(buffer);
        }
        public void WriteS64AV(ReadOnlySpan<long> buffer)
        {
            Contract.Requires(CanWrite);
            WriteS32V(buffer.Length);
            WriteS64A(buffer);
        }
        public void WriteF32AV(ReadOnlySpan<float> buffer)
        {
            Contract.Requires(CanWrite);
            WriteS32V(buffer.Length);
            WriteF32A(buffer);
        }
        public void WriteF64AV(ReadOnlySpan<double> buffer)
        {
            Contract.Requires(CanWrite);
            WriteS32V(buffer.Length);
            WriteF64A(buffer);
        }
        public void WriteBool(bool data)
        {
            Contract.Requires(CanWrite);
            Stream.Write(MemoryMarshal.AsBytes(stackalloc bool[] { data }));
        }
        public void WriteGuid(Guid data)
        {
            Contract.Requires(CanWrite);
            ReadOnlySpan<byte> buffer = MemoryMarshal.AsBytes(stackalloc Guid[] { data });
            if (!BitConverter.IsLittleEndian)
            {
                Stream.Write(buffer);
                return;
            }
            Stream.Write(stackalloc byte[] { buffer[3], buffer[2], buffer[1], buffer[0], buffer[5], buffer[4], buffer[7], buffer[6], buffer[8], buffer[9], buffer[10], buffer[11], buffer[12], buffer[13], buffer[14], buffer[15] });
        }
        public void WriteString32V(string data)
        {
            Contract.Requires(CanWrite);
            WriteU8AV(Encoding.UTF8.GetBytes(data));
        }
        public void WriteString16(string data)
        {
            Contract.Requires(CanWrite);
            ReadOnlySpan<byte> buffer = Encoding.UTF8.GetBytes(data);
            WriteU16((ushort)buffer.Length);
            Stream.Write(buffer);
        }
        public Span<byte> ReadU8A(int size)
        {
            Contract.Requires(CanRead);
            Span<byte> data = new(new byte[size]);
            int position = 0;
            while (position < size)
            {
                int read = Stream.Read(data[position..]);
                position += read;
                if (read == 0) throw new EndOfStreamException();
            }
            return data;
        }
        public Span<sbyte> ReadS8A(int size)
        {
            Contract.Requires(CanRead);
            return MemoryMarshal.Cast<byte, sbyte>(ReadU8A(size));
        }
        public Span<ushort> ReadU16A(int size)
        {
            Contract.Requires(CanRead);
            Contract.Requires(size * sizeof(ushort) <= int.MaxValue);
            Span<byte> buffer = ReadU8A(size * sizeof(ushort));
            Span<ushort> span = new ushort[size];
            for (int i = 0; i < size; i++)
            {
                span[i] = BinaryPrimitives.ReadUInt16BigEndian(buffer[(i * sizeof(ushort))..]);
            }
            return span;
        }
        public Span<short> ReadS16A(int size)
        {
            Contract.Requires(CanRead);
            Contract.Requires(size * sizeof(short) <= int.MaxValue);
            Span<byte> buffer = ReadU8A(size * sizeof(short));
            Span<short> span = new short[size];
            for (int i = 0; i < size; i++)
            {
                span[i] = BinaryPrimitives.ReadInt16BigEndian(buffer[(i * sizeof(short))..]);
            }
            return span;
        }
        public Span<uint> ReadU32A(int size)
        {
            Contract.Requires(CanRead);
            Contract.Requires(size * sizeof(uint) <= int.MaxValue);
            Span<byte> buffer = ReadU8A(size * sizeof(uint));
            Span<uint> span = new uint[size];
            for (int i = 0; i < size; i++)
            {
                span[i] = BinaryPrimitives.ReadUInt32BigEndian(buffer[(i * sizeof(uint))..]);
            }
            return span;
        }
        public Span<int> ReadS32A(int size)
        {
            Contract.Requires(CanRead);
            Contract.Requires(size * sizeof(int) <= int.MaxValue);
            Span<byte> buffer = ReadU8A(size * sizeof(int));
            Span<int> span = new int[size];
            for (int i = 0; i < size; i++)
            {
                span[i] = BinaryPrimitives.ReadInt32BigEndian(buffer[(i * sizeof(int))..]);
            }
            return span;
        }
        public Span<ulong> ReadU64A(int size)
        {
            Contract.Requires(CanRead);
            Contract.Requires(size * sizeof(ulong) <= int.MaxValue);
            Span<byte> buffer = ReadU8A(size * sizeof(ulong));
            Span<ulong> span = new ulong[size];
            for (int i = 0; i < size; i++)
            {
                span[i] = BinaryPrimitives.ReadUInt64BigEndian(buffer[(i * sizeof(ulong))..]);
            }
            return span;
        }
        public Span<long> ReadS64A(int size)
        {
            Contract.Requires(CanRead);
            Contract.Requires(size * sizeof(long) <= int.MaxValue);
            Span<byte> buffer = ReadU8A(size * sizeof(long));
            Span<long> span = new long[size];
            for (int i = 0; i < size; i++)
            {
                span[i] = BinaryPrimitives.ReadInt64BigEndian(buffer[(i * sizeof(long))..]);
            }
            return span;
        }
        public Span<float> ReadF32A(int size)
        {
            Contract.Requires(CanRead);
            Contract.Requires(size * sizeof(float) <= int.MaxValue);
            Span<byte> buffer = ReadU8A(size * sizeof(float));
            Span<float> span = new float[size];
            for (int i = 0; i < size; i++)
            {
                span[i] = BinaryPrimitives.ReadSingleBigEndian(buffer[(i * sizeof(float))..]);
            }
            return span;
        }
        public Span<double> ReadF64A(int size)
        {
            Contract.Requires(CanRead);
            Contract.Requires(size * sizeof(double) <= int.MaxValue);
            Span<byte> buffer = ReadU8A(size * sizeof(double));
            Span<double> span = new double[size];
            for (int i = 0; i < size; i++)
            {
                span[i] = BinaryPrimitives.ReadDoubleBigEndian(buffer[(i * sizeof(double))..]);
            }
            return span;
        }
        public byte ReadU8()
        {
            Contract.Requires(CanRead);
            return ReadU8A(1)[0];
        }
        public sbyte ReadS8()
        {
            Contract.Requires(CanRead);
            return ReadS8A(1)[0];
        }
        public ushort ReadU16()
        {
            Contract.Requires(CanRead);
            Span<byte> buffer = ReadU8A(sizeof(ushort));
            return BinaryPrimitives.ReadUInt16BigEndian(buffer);
        }
        public short ReadS16()
        {
            Contract.Requires(CanRead);
            Span<byte> buffer = ReadU8A(sizeof(short));
            return BinaryPrimitives.ReadInt16BigEndian(buffer);
        }
        public uint ReadU32()
        {
            Contract.Requires(CanRead);
            Span<byte> buffer = ReadU8A(sizeof(uint));
            return BinaryPrimitives.ReadUInt32BigEndian(buffer);
        }
        public int ReadS32()
        {
            Contract.Requires(CanRead);
            Span<byte> buffer = ReadU8A(sizeof(int));
            return BinaryPrimitives.ReadInt32BigEndian(buffer);
        }
        public ulong ReadU64()
        {
            Contract.Requires(CanRead);
            Span<byte> buffer = ReadU8A(sizeof(ulong));
            return BinaryPrimitives.ReadUInt64BigEndian(buffer);
        }
        public long ReadS64()
        {
            Contract.Requires(CanRead);
            Span<byte> buffer = ReadU8A(sizeof(long));
            return BinaryPrimitives.ReadInt64BigEndian(buffer);
        }
        public float ReadF32()
        {
            Contract.Requires(CanRead);
            Span<byte> buffer = ReadU8A(sizeof(float));
            return BinaryPrimitives.ReadSingleBigEndian(buffer);
        }
        public double ReadF64()
        {
            Contract.Requires(CanRead);
            Span<byte> buffer = ReadU8A(sizeof(double));
            return BinaryPrimitives.ReadDoubleBigEndian(buffer);
        }
        public uint ReadU32V(out int size)
        {
            Contract.Requires(CanRead);
            size = 0;
            uint data = 0;
            while (true)
            {
                byte current = ReadU8();
                data <<= 7;
                data |= current;
                size++;
                if ((current & 128) != 1) return data;
            }
        }
        public int ReadS32V(out int size)
        {
            Contract.Requires(CanRead);
            size = 0;
            int data = 0;
            while (true)
            {
                byte current = ReadU8();
                data <<= 7;
                data |= current;
                size++;
                if ((current & 128) != 1) return data;
            }
        }
        public ulong ReadU64V(out int size)
        {
            Contract.Requires(CanRead);
            size = 0;
            ulong data = 0;
            while (true)
            {
                byte current = ReadU8();
                data <<= 7;
                data |= current;
                size++;
                if ((current & 128) == 0) return data;
            }
        }
        public long ReadS64V(out int size)
        {
            Contract.Requires(CanRead);
            size = 0;
            long data = 0;
            while (true)
            {
                byte current = ReadU8();
                data <<= 7;
                data |= current;
                size++;
                if ((current & 128) == 0) return data;
            }
        }
        public uint ReadU32V()
        {
            Contract.Requires(CanRead);
            uint data = 0;
            while (true)
            {
                byte current = ReadU8();
                data <<= 7;
                data |= current;
                data |= current & 127u;
                if ((current & 128) == 0) return data;
            }
        }
        public int ReadS32V()
        {
            Contract.Requires(CanRead);
            int data = 0;
            while (true)
            {
                byte current = ReadU8();
                data <<= 7;
                data |= current & 127;
                if ((current & 128) == 0) return data;
            }
        }
        public ulong ReadU64V()
        {
            Contract.Requires(CanRead);
            ulong data = 0;
            while (true)
            {
                byte current = ReadU8();
                data <<= 7;
                data |= current & 127ul;
                if ((current & 128) != 1) return data;
            }
        }
        public long ReadS64V()
        {
            Contract.Requires(CanRead);
            long data = 0;
            while (true)
            {
                byte current = ReadU8();
                data <<= 7;
                data |= current & 127L;
                if ((current & 128) != 1) return data;
            }
        }
        public Span<byte> ReadU8AV()
        {
            Contract.Requires(CanRead);
            return ReadU8A(ReadS32V());
        }
        public Span<sbyte> ReadS8AV()
        {
            Contract.Requires(CanRead);
            return ReadS8A(ReadS32V());
        }
        public Span<ushort> ReadU16AV()
        {
            Contract.Requires(CanRead);
            return ReadU16A(ReadS32V());
        }
        public Span<short> ReadS16AV()
        {
            Contract.Requires(CanRead);
            return ReadS16A(ReadS32V());
        }
        public Span<uint> ReadU32AV()
        {
            Contract.Requires(CanRead);
            return ReadU32A(ReadS32V());
        }
        public Span<int> ReadS32AV()
        {
            Contract.Requires(CanRead);
            return ReadS32A(ReadS32V());
        }
        public Span<ulong> ReadU64AV()
        {
            Contract.Requires(CanRead);
            return ReadU64A(ReadS32V());
        }
        public Span<long> ReadS64AV()
        {
            Contract.Requires(CanRead);
            return ReadS64A(ReadS32V());
        }
        public bool ReadBool()
        {
            return ReadU8() != 0;
        }
        public Guid ReadGuid()
        {
            Contract.Requires(CanRead);
            ReadOnlySpan<byte> buffer = ReadU8A(16);
            if (BitConverter.IsLittleEndian) buffer = new(new byte[] { buffer[3], buffer[2], buffer[1], buffer[0], buffer[5], buffer[4], buffer[7], buffer[6], buffer[8], buffer[9], buffer[10], buffer[11], buffer[12], buffer[13], buffer[14], buffer[15] });
            return MemoryMarshal.Cast<byte, Guid>(buffer)[0];
        }
        public string ReadString32V(int size)
        {
            Contract.Requires(CanRead);
            string data = Encoding.UTF8.GetString(ReadU8AV());
            if (data.Length > size) throw new ProtocolViolationException("String too large!");
            return data;
        }
        public string ReadString16(int size)
        {
            Contract.Requires(CanRead);
            string data = Encoding.UTF8.GetString(ReadU8A(ReadS16()));
            if (data.Length > ushort.MaxValue) throw new ProtocolViolationException("String too large!");
            return data;
        }
        public void WriteGuidN(Guid? data)
        {
            Contract.Requires(CanWrite);
            WriteBool(data.HasValue);
            if (!data.HasValue) return;
            ReadOnlySpan<byte> buffer = MemoryMarshal.AsBytes(stackalloc Guid[] { data.Value });
            if (!BitConverter.IsLittleEndian)
            {
                Stream.Write(buffer);
                return;
            }
            Stream.Write(stackalloc byte[] { buffer[3], buffer[2], buffer[1], buffer[0], buffer[5], buffer[4], buffer[7], buffer[6], buffer[8], buffer[9], buffer[10], buffer[11], buffer[12], buffer[13], buffer[14], buffer[15] });
        }
        public void WriteString32VN(string? data)
        {
            Contract.Requires(CanWrite);
            WriteBool(data is not null);
            if (data is null) return;
            Contract.Assert(data.Length < int.MaxValue);
            WriteU8AV(Encoding.UTF8.GetBytes(data));
        }
        public void WriteString16N(string? data)
        {
            Contract.Requires(CanWrite);
            WriteBool(data is not null);
            if (data is null) return;
            Contract.Assert(data.Length < ushort.MaxValue);
            ReadOnlySpan<byte> buffer = Encoding.UTF8.GetBytes(data);
            WriteU16((ushort)buffer.Length);
            Stream.Write(buffer);
        }
        public Guid? ReadGuidN()
        {
            Contract.Requires(CanRead);
            if (!ReadBool()) return null;
            ReadOnlySpan<byte> buffer = ReadU8A(16);
            if (BitConverter.IsLittleEndian) buffer = new(new byte[] { buffer[3], buffer[2], buffer[1], buffer[0], buffer[5], buffer[4], buffer[7], buffer[6], buffer[8], buffer[9], buffer[10], buffer[11], buffer[12], buffer[13], buffer[14], buffer[15] });
            return MemoryMarshal.Cast<byte, Guid>(buffer)[0];
        }
        public string? ReadString32VN()
        {
            Contract.Requires(CanRead);
            if (!ReadBool()) return null;
            return Encoding.UTF8.GetString(ReadU8AV());
        }
        public string? ReadString16N()
        {
            Contract.Requires(CanRead);
            if (!ReadBool()) return null;
            return Encoding.UTF8.GetString(ReadU8A(ReadS16()));
        }
    }
}
