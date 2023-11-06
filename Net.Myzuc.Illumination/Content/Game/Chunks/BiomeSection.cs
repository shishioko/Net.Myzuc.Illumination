using Net.Myzuc.Illumination.Content.Game.World;
using Net.Myzuc.Illumination.Net;
using Net.Myzuc.Illumination.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using static System.Collections.Specialized.BitVector32;

namespace Net.Myzuc.Illumination.Content.Game.Chunks
{
    public sealed class BiomeSection
    {
        internal BiomeType[] Data { get; }
        internal bool Updated { get; set; }
        private DimensionType Type { get; }
        internal BiomeSection(DimensionType type)
        {
            Data = new BiomeType[64];
            Updated = false;
            Type = type;
            Array.Fill(Data, Type.BiomeTypes.First());
        }
        public BiomeType this[int x, int y, int z]
        {
            get
            {
                Contract.Requires(x >= 0);
                Contract.Requires(y >= 0);
                Contract.Requires(z >= 0);
                Contract.Requires(x < 4);
                Contract.Requires(y < 4);
                Contract.Requires(z < 4);
                int i = y << 4 | z << 2 | x;
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
                Contract.Requires(x < 4);
                Contract.Requires(y < 4);
                Contract.Requires(z < 4);
                int i = y << 4 | z << 2 | x;
                lock (Data)
                {
                    Data[i] = value;
                    Updated = true;
                }
            }
        }
        internal void Serialize(ContentStream mso)
        {
            Dictionary<int, ushort> palette = new();
            for (int i = 0; i < 64; i++)
            {
                int value = Type.BiomeTypes.IndexOf(Data[i]);
                if (!palette.ContainsKey(value)) palette.Add(value, 0);
                palette[value]++;
            }
            byte bits = 0;
            for (int i = palette.Count - 1; i != 0; i >>= 1) bits++;
            byte biomebits = 0;
            for (int i = Type.BiomeTypes.Count - 1; i != 0; i >>= 1) biomebits++;
            if (bits >= 4)
            {
                mso.WriteU8(biomebits);
                CompactArray data = new(biomebits, 64);
                for (int i = 0; i < 64; i++) data[i] = Type.BiomeTypes.IndexOf(Data[i]);
                ulong[] arr = data.Data;
                mso.WriteS32V(arr.Length);
                for (int i = 0; i < arr.Length; i++) mso.WriteU64(arr[i]);
            }
            else
            if (bits > 0)
            {
                mso.WriteU8(bits);
                CompactArray data = new(bits < 4 ? (byte)4 : bits, 64);
                Dictionary<int, int> lookup = new();
                mso.WriteS32V(palette.Count);
                foreach (int id in palette.Keys)
                {
                    lookup[id] = lookup.Count;
                    mso.WriteS32V(id);
                }
                for (int i = 0; i < 64; i++) data[i] = lookup[Type.BiomeTypes.IndexOf(Data[i])];
                ulong[] arr = data.Data;
                mso.WriteS32V(arr.Length);
                for (int i = 0; i < arr.Length; i++) mso.WriteU64(arr[i]);
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
