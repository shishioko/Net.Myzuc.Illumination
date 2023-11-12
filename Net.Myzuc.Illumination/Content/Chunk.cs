using Net.Myzuc.Illumination.Base;
using Net.Myzuc.Illumination.Content.Structs;
using Net.Myzuc.Illumination.Net;
using Net.Myzuc.Illumination.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Net.Myzuc.Illumination.Content
{
    public sealed class Chunk : Subscribeable<Client>, IUpdateable
    {
        public sealed class ChunkSection
        {
            public int X { get; }
            public int Y { get; }
            public int Z { get; }
            public BlockSection Blocks { get; }
            public BiomeSection Biomes { get; }
            public LightSection Skylight { get; }
            public LightSection Blocklight { get; }
            internal ChunkSection(int x, int y, int z, DimensionType type)
            {
                X = x;
                Y = y;
                Z = z;
                Blocks = new();
                Biomes = new(type);
                Skylight = new();
                Blocklight = new();
            }
        }
        public sealed class BlockSection
        {
            internal ushort[] Data { get; }
            public ushort NonZero { get; private set; }
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
        public sealed class BiomeSection
        {
            private DimensionType Type { get; }
            internal BiomeType[] Data { get; }
            internal bool Updated { get; set; }
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
        public sealed class LightSection
        {
            internal byte[] Data { get; }
            public ushort NonZero { get; private set; }
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
        public sealed class Heightmap
        {
            internal CompactArray Data { get; }
            internal bool Updated { get; set; }
            internal Heightmap(int height)
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
        public DimensionType Type { get; }
        public int X { get; }
        public int Z { get; }
        public object Lock { get; } = new();
        public Heightmap MotionBlocking { get; }
        private ChunkSection[] Sections { get; }
        public Chunk(DimensionType type, int x, int z)
        {
            Contract.Requires(x < 2097152 && x >= -2097152);
            Contract.Requires(z < 2097152 && z >= -2097152);
            Type = type;
            X = x;
            Z = z;
            MotionBlocking = new(Type.SectionHeight);
            Sections = new ChunkSection[Type.SectionHeight];
            for (int i = 0; i < Type.SectionHeight; i++) Sections[i] = new(X, Type.SectionDepth + i, Z, Type);
        }
        public ChunkSection this[int y]
        {
            get
            {
                Contract.Requires(y >= Type.SectionDepth);
                Contract.Requires(y < Type.SectionDepth + Type.SectionHeight);
                return Sections[y - Type.SectionDepth];
            }
        }
        public void Update()
        {
            lock (Sections)
            {
                if (Sections.Any(section => section.Biomes.Updated))
                {
                    using ContentStream mso = new();
                    mso.WriteS32V(13);
                    mso.WriteS32V(1);
                    mso.WriteS32(X);
                    mso.WriteS32(Z);
                    using ContentStream msob = new();
                    for (int i = 0; i < Sections.Length; i++)
                    {
                        BiomeSection biomes = Sections[i].Biomes;
                        lock (biomes.Data)
                        {
                            biomes.Updated = false;
                            biomes.Serialize(msob);
                        }
                    }
                    mso.WriteU8AV(msob.Get());
                    byte[] msop = mso.Get().ToArray();
                    Iterate((Client client) =>
                    {
                        client.Send(msop);
                    });
                }
                if (Sections.Any(section => section.Skylight.Updated || section.Blocklight.Updated))
                {
                    using ContentStream mso = new();
                    mso.WriteS32V(39);
                    mso.WriteS32(X);
                    mso.WriteS32(Z);
                    int lightSections = Sections.Length + 2;
                    CompactArray skylight = new(1, lightSections);
                    CompactArray skylight_empty = new(1, lightSections);
                    CompactArray blocklight = new(1, lightSections);
                    CompactArray blocklight_empty = new(1, lightSections);
                    for (int i = 1; i < lightSections - 2; i++)
                    {
                        LightSection skylights = Sections[i - 1].Skylight;
                        skylight[i] = skylights.Updated && skylights.NonZero > 0 ? 1 : 0;
                        skylight_empty[i] = skylights.Updated && skylights.NonZero < 1 ? 1 : 0;
                        LightSection blocklights = Sections[i - 1].Skylight;
                        blocklight[i] = blocklights.Updated && blocklights.NonZero > 0 ? 1 : 0;
                        blocklight_empty[i] = blocklights.Updated && blocklights.NonZero < 1 ? 1 : 0;
                    }
                    mso.WriteU64AV(skylight.Data);
                    mso.WriteU64AV(blocklight.Data);
                    mso.WriteU64AV(skylight_empty.Data);
                    mso.WriteU64AV(blocklight_empty.Data);
                    mso.WriteS32V(skylight.Count(v => v > 0));
                    for (int i = 1; i < lightSections - 2; i++)
                    {
                        if (skylight[i] < 1) continue;
                        LightSection lights = Sections[i - 1].Skylight;
                        lock (lights.Data)
                        {
                            lights.Updated = false;
                            mso.WriteU8AV(lights.Data);
                        }
                    }
                    mso.WriteS32V(blocklight.Count(v => v > 0));
                    for (int i = 1; i < lightSections - 2; i++)
                    {
                        if (blocklight[i] < 1) continue;
                        LightSection lights = Sections[i - 1].Blocklight;
                        lock (lights.Data)
                        {
                            lights.Updated = false;
                            mso.WriteU8AV(lights.Data);
                        }
                    }
                    byte[] msop = mso.Get().ToArray();
                    Iterate((Client client) =>
                    {
                        client.Send(msop);
                    });
                }
                for (int i = 0; i < Sections.Length; i++)
                {
                    BlockSection blocks = Sections[i].Blocks;
                    if (blocks.Updates.Count < 1) continue;
                    lock (blocks.Data)
                    {
                        using ContentStream mso = new();
                        if (blocks.Updates.Count > 1)
                        {
                            mso.WriteS32V(67);
                            mso.WriteU64((ulong)(long)(i + Type.SectionDepth) & 1048575 | ((ulong)(long)Z & 4194303) << 20 | ((ulong)(long)X & 4194303) << 42);
                            mso.WriteS32V(blocks.Updates.Count);
                            foreach (KeyValuePair<int, ushort> kvp in blocks.Updates)
                            {
                                mso.WriteU64V((ulong)(kvp.Key | kvp.Value << 12));
                            }
                        }
                        else
                        {
                            KeyValuePair<int, ushort> kvp = blocks.Updates.First();
                            mso.WriteS32V(10);
                            mso.WriteU64(new Position((kvp.Key >> 8) + X * 16, (kvp.Key & 15) + i * 16, (kvp.Key >> 4 & 15) + Z * 16).Value);
                            mso.WriteS32V(kvp.Value);
                        }
                        blocks.Updates.Clear();
                        byte[] msop = mso.Get().ToArray();
                        Iterate((Client client) =>
                        {
                            client.Send(msop);
                        });
                    }
                }
            }
        }
        public override void Subscribe(Client client)
        {
            Contract.Requires(client.Dimension?.Type == Type);
            base.Subscribe(client);
            lock (client.Chunks)
            {
                if (client.Chunks.TryGetValue((X, Z), out Chunk? chunk)) chunk.UnsubscribeQuietly(client);
                client.Chunks[(X, Z)] = this;
                using ContentStream mso = new();
                mso.WriteS32V(36);
                mso.WriteS32(X);
                mso.WriteS32(Z);
                mso.WriteU8(10);
                mso.WriteString16(string.Empty);
                mso.WriteU8(12);
                mso.WriteString16("MOTION_BLOCKING");
                mso.WriteS32(MotionBlocking.Data.Data.Length);
                mso.WriteU64A(MotionBlocking.Data.Data);
                mso.WriteU8(0);
                using ContentStream msob = new();
                for (int i = 0; i < Sections.Length; i++)
                {
                    Sections[i].Blocks.Serialize(msob);
                    Sections[i].Biomes.Serialize(msob);
                }
                mso.WriteU8AV(msob.Get());
                mso.WriteS32V(0); //TODO: block entities
                int lightSections = Sections.Length + 2;
                CompactArray skylight = new(1, lightSections);
                CompactArray skylight_empty = new(1, lightSections);
                CompactArray blocklight = new(1, lightSections);
                CompactArray blocklight_empty = new(1, lightSections);
                for (int i = 1; i < lightSections - 2; i++)
                {
                    LightSection skylights = Sections[i - 1].Skylight;
                    skylight[i] = skylights.Updated && skylights.NonZero > 0 ? 1 : 0;
                    skylight_empty[i] = skylights.Updated && skylights.NonZero < 1 ? 1 : 0;
                    LightSection blocklights = Sections[i - 1].Skylight;
                    blocklight[i] = blocklights.Updated && blocklights.NonZero > 0 ? 1 : 0;
                    blocklight_empty[i] = blocklights.Updated && blocklights.NonZero < 1 ? 1 : 0;
                }
                mso.WriteU64AV(skylight.Data);
                mso.WriteU64AV(blocklight.Data);
                mso.WriteU64AV(skylight_empty.Data);
                mso.WriteU64AV(blocklight_empty.Data);
                mso.WriteS32V(skylight.Count(v => v > 0));
                for (int i = 1; i < lightSections - 2; i++)
                {
                    if (skylight[i] < 1) continue;
                    mso.WriteU8AV(Sections[i - 1].Skylight.Data);
                }
                mso.WriteS32V(blocklight.Count(v => v > 0));
                for (int i = 1; i < lightSections - 2; i++)
                {
                    if (blocklight[i] < 1) continue;
                    mso.WriteU8AV(Sections[i - 1].Blocklight.Data);
                }
                client.Send(mso.Get());
            }
        }
        public override void Unsubscribe(Client client)
        {
            base.Unsubscribe(client);
            lock (client.Chunks)
            {
                client.Chunks.Remove((X, Z), out Chunk? chunk);
                using ContentStream mso = new();
                mso.WriteS32V(30);
                mso.WriteS32(X);
                mso.WriteS32(Z);
                client.Send(mso.Get());
            }
        }
        internal override void UnsubscribeQuietly(Client client)
        {
            base.UnsubscribeQuietly(client);
            lock (client.Chunks)
            {
                client.Chunks.Remove((X, Z), out Chunk? chunk);
            }
        }
    }
}
