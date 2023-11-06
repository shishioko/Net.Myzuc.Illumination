using Net.Myzuc.Illumination.Content.Game.Chunks;
using Net.Myzuc.Illumination.Net;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text.RegularExpressions;

namespace Net.Myzuc.Illumination.Content.Game.World
{
    public sealed class Dimension
    {
        public string Name { get; }
        public DimensionType Type { get; }
        public int SeedHash { get; }
        public bool Flat { get; }
        private Dictionary<Guid, Client> Subscribers { get; }
        public Dimension(string name, DimensionType type, int seedHash, bool flat)
        {
            Contract.Requires(Regex.IsMatch(name, "[A-z:._-]"));
            Name = name;
            Type = type;
            SeedHash = seedHash;
            Flat = flat;
            Subscribers = new();
        }
        public void Subscribe(Client client)
        {
            lock (Subscribers)
            {
                Subscribers.Add(client.Login.Id, client);
            }
            client.Dimension = this;
            lock (client.Chunks)
            {
                while (client.Chunks.Count > 0)
                {
                    Chunk chunk = client.Chunks.Values.First();
                    chunk.UnsubscribeQuietly(client);
                }
            }
            using (ContentStream mso = new())
            {
                mso.WriteS32V(40);
                mso.WriteS32(-1);
                mso.WriteBool(client.Hardcore);
                mso.WriteU8(client.Gamemode);
                mso.WriteU8(255);
                mso.WriteS32V(1);
                mso.WriteString32V(Name);
                mso.WriteU8(10);
                mso.WriteU16(0);
                mso.WriteU8(10);
                mso.WriteString16("minecraft:damage_type");
                mso.WriteU8(8);
                mso.WriteString16("type");
                mso.WriteString16("minecraft:damage_type");
                mso.WriteU8(9);
                mso.WriteString16("value");
                mso.WriteU8(10);
                mso.WriteS32(30);
                string[] damages = {
                "minecraft:generic_kill",
                "minecraft:dragon_breath",
                "minecraft:outside_border",
                "minecraft:freeze",
                "minecraft:stalagmite",
                "minecraft:in_fire",
                "minecraft:wither",
                "minecraft:generic",
                "minecraft:cactus",
                "minecraft:cramming",
                "minecraft:drown",
                "minecraft:fall",
                "minecraft:falling_anvil",
                "minecraft:fireworks",
                "minecraft:in_wall",
                "minecraft:indirect_magic",
                "minecraft:lava",
                "minecraft:lightning_bolt",
                "minecraft:magic",
                "minecraft:mob_attack",
                "minecraft:mob_attack_no_aggro",
                "minecraft:on_fire",
                "minecraft:out_of_world",
                "minecraft:player_explosion",
                "minecraft:starve",
                "minecraft:string",
                "minecraft:sweet_berry_bush",
                "minecraft:dry_out",
                "minecraft:hot_floor",
                "minecraft:fly_into_wall",
                };
                for (int i = 0; i < damages.Length; i++)
                {
                    mso.WriteU8(8);
                    mso.WriteString16("name");
                    mso.WriteString16(damages[i]);
                    mso.WriteU8(3);
                    mso.WriteString16("id");
                    mso.WriteS32(i);
                    mso.WriteU8(10);
                    mso.WriteString16("element");
                    mso.WriteU8(5);
                    mso.WriteString16("exhaustion");
                    mso.WriteF32(0.0f);
                    mso.WriteU8(8);
                    mso.WriteString16("scaling");
                    mso.WriteString16("always");
                    mso.WriteU8(8);
                    mso.WriteString16("message_id");
                    mso.WriteString16("arrow");
                    mso.WriteU8(0);
                    mso.WriteU8(0);
                }
                mso.WriteU8(0);
                mso.WriteU8(10);
                mso.WriteString16("minecraft:dimension_type");
                mso.WriteU8(8);
                mso.WriteString16("type");
                mso.WriteString16("minecraft:dimension_type");
                mso.WriteU8(9);
                mso.WriteString16("value");
                mso.WriteU8(10);
                mso.WriteS32(1);
                mso.WriteU8(8);
                mso.WriteString16("name");
                mso.WriteString16(Type.Name);
                mso.WriteU8(3);
                mso.WriteString16("id");
                mso.WriteS32(0);
                mso.WriteU8(10);
                mso.WriteString16("element");
                mso.WriteU8(5);
                mso.WriteString16("ambient_light");
                mso.WriteF32(Type.AmbientLight);
                mso.WriteU8(1);
                mso.WriteString16("bed_works");
                mso.WriteBool(true);
                mso.WriteU8(6);
                mso.WriteString16("coordinate_scale");
                mso.WriteF64(1.0);
                mso.WriteU8(8);
                mso.WriteString16("effects");
                mso.WriteString16(Type.Effects);
                mso.WriteU8(1);
                mso.WriteString16("has_ceiling");
                mso.WriteBool(false);
                mso.WriteU8(1);
                mso.WriteString16("has_raids");
                mso.WriteBool(true);
                mso.WriteU8(1);
                mso.WriteString16("has_skylight");
                mso.WriteBool(Type.HasSkylight);
                mso.WriteU8(3);
                mso.WriteString16("height");
                mso.WriteS32(Type.SectionHeight * 16);
                mso.WriteU8(8);
                mso.WriteString16("infiniburn");
                mso.WriteString16("#minecraft:infiniburn_overworld");
                mso.WriteU8(3);
                mso.WriteString16("logical_height");
                mso.WriteS32(Type.SectionHeight);
                mso.WriteU8(3);
                mso.WriteString16("min_y");
                mso.WriteS32(Type.SectionDepth * 16);
                mso.WriteU8(3);
                mso.WriteString16("monster_spawn_block_light_limit");
                mso.WriteS32(0);
                mso.WriteU8(3);
                mso.WriteString16("monster_spawn_light_level");
                mso.WriteS32(0);
                mso.WriteU8(1);
                mso.WriteString16("natural");
                mso.WriteBool(Type.Natural);
                mso.WriteU8(1);
                mso.WriteString16("piglin_safe");
                mso.WriteBool(false);
                mso.WriteU8(1);
                mso.WriteString16("respawn_anchor_works");
                mso.WriteBool(true);
                mso.WriteU8(1);
                mso.WriteString16("ultrawarm");
                mso.WriteBool(false);
                mso.WriteU8(0);
                mso.WriteU8(0);
                mso.WriteU8(0);
                mso.WriteU8(10);
                mso.WriteString16("minecraft:worldgen/biome");
                mso.WriteU8(8);
                mso.WriteString16("type");
                mso.WriteString16("minecraft:worldgen/biome");
                mso.WriteU8(9);
                mso.WriteString16("value");
                mso.WriteU8(10);
                mso.WriteS32(Type.BiomeTypes.Count);
                uint bid = 0;
                foreach (BiomeType biome in Type.BiomeTypes)
                {
                    mso.WriteU8(8);
                    mso.WriteString16("name");
                    mso.WriteString16(biome.Name);
                    mso.WriteU8(3);
                    mso.WriteString16("id");
                    mso.WriteU32(bid++);
                    mso.WriteU8(10);
                    mso.WriteString16("element");
                    mso.WriteU8(1);
                    mso.WriteString16("has_precipitation");
                    mso.WriteBool(biome.Precipitation);
                    mso.WriteU8(5);
                    mso.WriteString16("temperature");
                    mso.WriteF32(0.0f);
                    mso.WriteU8(5);
                    mso.WriteString16("downfall");
                    mso.WriteF32(0.0f);
                    mso.WriteU8(10);
                    mso.WriteString16("effects");
                    mso.WriteU8(3);
                    mso.WriteString16("sky_color");
                    mso.WriteU8(biome.SkyColor.A);
                    mso.WriteU8(biome.SkyColor.R);
                    mso.WriteU8(biome.SkyColor.G);
                    mso.WriteU8(biome.SkyColor.B);
                    mso.WriteU8(3);
                    mso.WriteString16("water_fog_color");
                    mso.WriteU8(biome.WaterFogColor.A);
                    mso.WriteU8(biome.WaterFogColor.R);
                    mso.WriteU8(biome.WaterFogColor.G);
                    mso.WriteU8(biome.WaterFogColor.B);
                    mso.WriteU8(3);
                    mso.WriteString16("fog_color");
                    mso.WriteU8(biome.FogColor.A);
                    mso.WriteU8(biome.FogColor.R);
                    mso.WriteU8(biome.FogColor.G);
                    mso.WriteU8(biome.FogColor.B);
                    mso.WriteU8(3);
                    mso.WriteString16("water_color");
                    mso.WriteU8(biome.WaterColor.A);
                    mso.WriteU8(biome.WaterColor.R);
                    mso.WriteU8(biome.WaterColor.G);
                    mso.WriteU8(biome.WaterColor.B);
                    if (biome.FoliageColor.HasValue)
                    {
                        mso.WriteU8(3);
                        mso.WriteString16("foliage_color");
                        mso.WriteU8(biome.FoliageColor.Value.A);
                        mso.WriteU8(biome.FoliageColor.Value.R);
                        mso.WriteU8(biome.FoliageColor.Value.G);
                        mso.WriteU8(biome.FoliageColor.Value.B);
                    }
                    if (biome.GrassColor.HasValue)
                    {
                        mso.WriteU8(3);
                        mso.WriteString16("grass_color");
                        mso.WriteU8(biome.GrassColor.Value.A);
                        mso.WriteU8(biome.GrassColor.Value.R);
                        mso.WriteU8(biome.GrassColor.Value.G);
                        mso.WriteU8(biome.GrassColor.Value.B);
                    }
                    if (biome.Music.HasValue)
                    {
                        mso.WriteU8(10);
                        mso.WriteString16("music");
                        mso.WriteU8(1);
                        mso.WriteString16("replace_current_music");
                        mso.WriteBool(biome.Music.Value.Item1);
                        mso.WriteU8(8);
                        mso.WriteString16("sound");
                        mso.WriteString16(biome.Music.Value.Item2);
                        mso.WriteU8(3);
                        mso.WriteString16("max_delay");
                        mso.WriteS32(biome.Music.Value.Item3);
                        mso.WriteU8(3);
                        mso.WriteString16("min_delay");
                        mso.WriteS32(biome.Music.Value.Item4);
                        mso.WriteU8(0);
                    }
                    if (biome.AmbientSound is not null)
                    {
                        mso.WriteU8(8);
                        mso.WriteString16("ambient_sound");
                        mso.WriteString16(biome.AmbientSound);
                    }
                    if (biome.AdditionsSound.HasValue)
                    {
                        mso.WriteU8(10);
                        mso.WriteString16("additions_sound");
                        mso.WriteU8(8);
                        mso.WriteString16("sound");
                        mso.WriteString16(biome.AdditionsSound.Value.Item1);
                        mso.WriteU8(6);
                        mso.WriteString16("tick_chance");
                        mso.WriteF64(biome.AdditionsSound.Value.Item2);
                        mso.WriteU8(0);
                    }
                    if (biome.MoodSound.HasValue)
                    {
                        mso.WriteU8(10);
                        mso.WriteString16("additions_sound");
                        mso.WriteU8(8);
                        mso.WriteString16("sound");
                        mso.WriteString16(biome.MoodSound.Value.Item1);
                        mso.WriteU8(3);
                        mso.WriteString16("tick_delay");
                        mso.WriteS32(biome.MoodSound.Value.Item2);
                        mso.WriteU8(6);
                        mso.WriteString16("offset");
                        mso.WriteF64(biome.MoodSound.Value.Item3);
                        mso.WriteU8(3);
                        mso.WriteString16("block_search_extent");
                        mso.WriteS32(biome.MoodSound.Value.Item4);
                        mso.WriteU8(0);
                    }
                    //TODO: particle
                    mso.WriteU8(0);
                    mso.WriteU8(0);
                    mso.WriteU8(0);
                }
                mso.WriteU8(0);
                mso.WriteU8(0);
                mso.WriteString32V(Type.Name);
                mso.WriteString32V(Name);
                mso.WriteS64(SeedHash);
                mso.WriteS32V(0);
                mso.WriteS32V(64);
                mso.WriteS32V(64);
                mso.WriteBool(client.ReducedDebugInfo);
                mso.WriteBool(true);
                mso.WriteBool(false);
                mso.WriteBool(Flat);
                mso.WriteBool(false);
                /*if (DeathLocation.HasValue)
                {
                    mso.WriteVarString(DeathLocation.Value.DimensionName);
                    mso.WriteUInt64(DeathLocation.Value.Location.Data);
                }*/ //TODO: deatch location updateable seperate
                mso.WriteS32V(0);
                client.Send(mso.Get());
            }

            //TODO:
            using (ContentStream mso = new())
            {
                mso.WriteS32V(0x50);
                mso.WriteU64(new Position(0, 64, 0).Value);
                mso.WriteF32(0);
                client.Send(mso.Get());
            }
            using (ContentStream mso = new())
            {
                mso.WriteS32V(78);
                mso.WriteS32V(0);
                mso.WriteS32V(0);
                client.Send(mso.Get());
            }
        }
        public void Unsubscribe(Client client)
        {
            lock (Subscribers)
            {
                if (!Subscribers.Remove(client.Login.Id)) return;
            }
            client.Dimension = null;
        }
    }
}

