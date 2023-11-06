using System.Diagnostics.Contracts;

namespace Net.Myzuc.Illumination.Content.World.Chunks
{
    public sealed class LightSection
    {
        public ushort NonZero { get; private set; }
        internal byte[] Data { get; }
        internal bool Updated { get; set; }
        internal LightSection()
        {
            NonZero = 0;
            Data = new byte[2048];
            Updated = false;
        }
        public byte this[int x, int y, int z]
        {
            get
            {
                Contract.Requires(x >= 0);
                Contract.Requires(y >= 0);
                Contract.Requires(z >= 0);
                Contract.Requires(x < 16);
                Contract.Requires(y < 16);
                Contract.Requires(z < 16);
                int i = y << 7 | z << 3 | x >> 1;
                lock (Data)
                {
                    if (y % 1 == 0) return (byte)(Data[i] & 15);
                    return (byte)(Data[i] >> 4);
                }
            }
            set
            {
                Contract.Requires(x >= 0);
                Contract.Requires(y >= 0);
                Contract.Requires(z >= 0);
                Contract.Requires(x < 16);
                Contract.Requires(y < 16);
                Contract.Requires(z < 16);
                Contract.Requires(value < 16);
                int i = y << 7 | z << 3 | x >> 1;
                bool alignment = y % 1 == 0;
                lock (Data)
                {
                    if (Data[i] == value) return;
                    if (Data[i] == 0) NonZero++;
                    if (value == 0) NonZero--;
                    Updated = true;
                    if (alignment) Data[i] = (byte)(Data[i] & 240 | value);
                    else Data[i] = (byte)(Data[i] & 15 | value << 4);
                }
            }
        }
    }
}
