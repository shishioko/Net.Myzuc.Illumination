using Net.Myzuc.Illumination.Content.Game;
using Net.Myzuc.Illumination.Content.World;
using Net.Myzuc.Illumination.Net;
using Net.Myzuc.Illumination.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Net.Myzuc.Illumination.Content.World.Chunks
{
    public sealed class Chunk
    {
        public DimensionType Type { get; }
        public int X { get; }
        public int Z { get; }
        public Heightmap MotionBlocking { get; }
        private Dictionary<Guid, Client> Subscribers { get; }
        private ChunkSection[] Sections { get; }
        public Chunk(DimensionType type, int x, int z)
        {
            Contract.Requires(x < 2097152 && x >= -2097152);
            Contract.Requires(z < 2097152 && z >= -2097152);
            Type = type;
            X = x;
            Z = z;
            MotionBlocking = new(Type.SectionHeight);
            Subscribers = new();
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
        public void Subscribe(Client client)
        {
            Contract.Requires(client.Dimension?.Type == Type);
            lock (Subscribers)
            {
                if (!Subscribers.TryAdd(client.Login.Id, client)) return;
            }
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
        public void Unsubscribe(Client client)
        {
            lock (Subscribers)
            {
                if (!Subscribers.Remove(client.Login.Id)) return;
            }
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
        public void UnsubscribeQuietly(Client client)
        {
            lock (Subscribers)
            {
                if (!Subscribers.Remove(client.Login.Id)) return;
            }
            lock (client.Chunks)
            {
                client.Chunks.Remove((X, Z), out Chunk? chunk);
            }
        }
        public void Tick()
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
                    Span<byte> span = mso.Get();
                    lock (Subscribers)
                    {
                        foreach (Client client in Subscribers.Values)
                        {
                            client.Send(span);
                        }
                    }
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
                    Span<byte> span = mso.Get();
                    lock (Subscribers)
                    {
                        foreach (Client client in Subscribers.Values)
                        {
                            client.Send(span);
                        }
                    }
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
                        Span<byte> span = mso.Get();
                        lock (Subscribers)
                        {
                            foreach (Client client in Subscribers.Values)
                            {
                                client.Send(span);
                            }
                        }
                    }
                }
            }
        }
    }
}
