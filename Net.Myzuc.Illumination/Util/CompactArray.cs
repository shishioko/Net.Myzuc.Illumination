using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Me.Shishioko.Illumination.Util
{
    public sealed class CompactArray : IEnumerable<int>
    {
        private sealed class Enumerator : IEnumerator<int>
        {
            private readonly CompactArray Data;
            private int Index;
            private int Last;
            public int Current => Last;
            object IEnumerator.Current => Last;
            public Enumerator(CompactArray data)
            {
                Data = data;
                Index = -1;
                Last = 0;
            }
            public void Dispose()
            {

            }
            public bool MoveNext()
            {
                if (Index + 1 == Data.Length) return false;
                Index++;
                Last = Data[Index];
                return true;
            }
            public void Reset()
            {
                Index = -1;
                Last = 0;
            }
        }
        /// <summary>
        /// Compact Level
        /// </summary>
        public readonly byte Bits;
        /// <summary>
        /// Size
        /// </summary>
        public readonly int Length;
        /// <summary>
        /// Underlying Data
        /// </summary>
        public readonly ulong[] Data;
        /// <summary>
        /// Creates a new Compact Array.
        /// </summary>
        /// <param name="bits">Compact Level</param>
        /// <param name="length">Size</param>
        public CompactArray(byte bits, int length)
        {
            Contract.Requires(bits > 0);
            Contract.Requires(bits <= 32);
            Bits = bits;
            Length = length;
            Data = new ulong[(length - 1) / (64 / bits) + 1];
        }
        /// <summary>
        /// Deserializes a Compact Array.
        /// </summary>
        /// <param name="bits">Compact Level</param>
        /// <param name="data">Serialized Data</param>
        public CompactArray(byte bits, ulong[] data)
        {
            Contract.Requires(bits > 0);
            Contract.Requires(bits <= 32);
            Bits = bits;
            Length = data.Length * (64 / bits);
            Data = data;
        }
        public IEnumerator<int> GetEnumerator()
        {
            return new Enumerator(this);
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }
        /// <summary>
        /// Resizes a Compact Array.
        /// </summary>
        public CompactArray Resize(byte bits, int length)
        {
            CompactArray arr = new(bits, length);
            for(int i = 0; i < length && i < Length; i++) arr[i] = this[i] & ((1 << bits) - 1);
            return arr;
        }
        public int this[int index]
        {
            get
            {
                Contract.Requires(index >= 0);
                Contract.Requires(index < Length);
                int pack = 64 / Bits;
                return (int)(Data[index / pack] >> index % pack * Bits & (1UL << Bits) - 1UL);
            }
            set
            {
                Contract.Requires(index >= 0);
                Contract.Requires(index < Length);
                Contract.Requires(value < 1 << Bits);
                int pack = 64 / Bits;
                int high = index / pack;
                int low = index % pack;
                Data[high] &= ~((1UL << Bits) - 1UL << low * Bits);
                Data[high] |= (ulong)value << low * Bits;
            }
        }
    }
}
