using Net.Myzuc.Illumination.Net;
using Net.Myzuc.Illumination.Util;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Net.Myzuc.Illumination.Content.Game.Chunks
{
    public sealed class BlockSection
    {
        public ushort NonZero { get; private set; }
        internal ushort[] Data { get; }
        internal Dictionary<int, ushort> Updates { get; }
        internal BlockSection()
        {
            NonZero = 0;
            Data = new ushort[4096];
            Updates = new();
        }
        public ushort this[int x, int y, int z]
        {
            get
            {
                Contract.Requires(x >= 0);
                Contract.Requires(y >= 0);
                Contract.Requires(z >= 0);
                Contract.Requires(x < 16);
                Contract.Requires(y < 16);
                Contract.Requires(z < 16);
                int i = y << 8 | z << 4 | x;
                lock (Data)
                {
                    return Data[i];
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
                int i = y << 8 | z << 4 | x;
                lock (Data)
                {
                    if (Data[i] == value) return;
                    if (Data[i] == 0) NonZero++; //TODO: void air and cave air 10546 and 10547 (?)
                    if (value == 0) NonZero--;
                    Data[i] = value;
                    Updates[x << 8 | z << 4 | y] = value;
                }
            }
        }
        internal void Serialize(ContentStream mso)
        {
            Dictionary<ushort, ushort> palette = new();
            for (int i = 0; i < 4096; i++)
            {
                ushort value = Data[i];
                if (!palette.ContainsKey(value)) palette.Add(value, 0);
                palette[value]++;
            }
            mso.WriteU16(NonZero);
            byte bits = 0;
            for (int i = palette.Count - 1; i != 0; i >>= 1) bits++;
            if (bits > 8)
            {
                mso.WriteU8(15);
                CompactArray data = new(15, 4096);
                for (int i = 0; i < 4096; i++) data[i] = Data[i];
                ulong[] arr = data.Data;
                mso.WriteS32V(arr.Length);
                for (int i = 0; i < arr.Length; i++) mso.WriteU64(arr[i]);
            }
            else
            if (bits > 0)
            {
                mso.WriteU8(bits < 4 ? (byte)4 : bits);
                CompactArray data = new(bits < 4 ? (byte)4 : bits, 4096);
                Dictionary<int, int> lookup = new();
                mso.WriteS32V(palette.Count);
                foreach (ushort id in palette.Keys)
                {
                    lookup[id] = lookup.Count;
                    mso.WriteS32V(id);
                }
                for (int i = 0; i < 4096; i++) data[i] = lookup[Data[i]];
                mso.WriteS32V(data.Data.Length);
                for (int i = 0; i < data.Data.Length; i++) mso.WriteU64(data.Data[i]);
            }
            else
            {
                mso.WriteU8(0);
                mso.WriteS32V(palette.First().Key);
                mso.WriteS32V(0);
            }
        }
    }
}
