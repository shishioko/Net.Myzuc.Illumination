using Net.Myzuc.Illumination.Util;
using System;
using System.Diagnostics.Contracts;

namespace Net.Myzuc.Illumination.Content.World.Chunks
{
    public sealed class Heightmap
    {
        internal CompactArray Data { get; }
        internal bool Updated { get; set; }
        public Heightmap(int height)
        {
            byte bits = 0;
            for (int i = (height / 16 << 4) - 1; i != 0; i >>= 1) bits++;
            Data = new(bits, 256);
            Updated = false;
        }
        public int this[int x, int z]
        {
            get
            {
                Contract.Requires(x >= 0);
                Contract.Requires(z >= 0);
                Contract.Requires(x < 16);
                Contract.Requires(z < 16);
                int i = x << 4 | z;
                lock (Data)
                {
                    return Data[i];
                }
            }
            set
            {
                Contract.Requires(x >= 0);
                Contract.Requires(z >= 0);
                Contract.Requires(x < 16);
                Contract.Requires(z < 16);
                Contract.Requires(value < 16);
                int i = x << 4 | z;
                lock (Data)
                {
                    if (Data[i] == value) return;
                    Data[i] = value;
                    Updated = true;
                }
            }
        }
    }
}
