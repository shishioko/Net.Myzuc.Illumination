using Me.Shishioko.Illumination.Base;
using Me.Shishioko.Illumination.Chat;
using Me.Shishioko.Illumination.Content;
using Me.Shishioko.Illumination.Content.Entities;
using Me.Shishioko.Illumination.Content.Structs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Text.Json;
using System.Threading;
using static Me.Shishioko.Illumination.Status.ServerStatus;

namespace Me.Shishioko.Illumination.Bin.Test
{
    internal class Program
    {
        static void Main(string[] args)
        {
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, 4646);
            Listener listener = new(endpoint);
            listener.Accept += (Connection connection) =>
            {
                Console.WriteLine($"[{endpoint}] [{connection.Endpoint}] Connected.");
                connection.Status += () =>
                {
                    return new()
                    {
                        Description = new ChatText("")
                        {
                            Extra = new ChatComponent[]
                            {
                                new ChatText("MYZUC.NET\n")
                                {
                                    Font = "minecraft:uniform",
                                    Color = "#80C080",
                                    Bold = true
                                },
                                new ChatText("Procrastinating since 1921")
                                {
                                    Font = "minecraft:uniform",
                                    Color = "#A0E0A0",
                                    Italic = true
                                }
                            }
                        },
                        Version = new("Me.Shishioko.Illumination", 763),
                        Players = new()
                        {
                            Max = 0,
                            Online = 0,
                            Sample = new StatusPlayer[]
                            {
                                new("None", Guid.Empty)
                            }
                        },
                        EnforcesSecureChat = false,
                    };
                };
                connection.Login += (LoginRequest auth) =>
                {
                    Console.WriteLine($"[{endpoint}] [{connection.Endpoint}] Logging in.");
                    auth.Encryption += () =>
                    {
                        return false;
                    };
                    auth.Compression += () =>
                    {
                        return 256;
                    };
                    auth.Failure += (JsonElement json) =>
                    {
                        Console.WriteLine($"[{endpoint}] [{connection.Endpoint}] Failed to log in.");
                        Console.WriteLine(json.ToString());
                        auth.Disconnect(new ChatText("Authentication failure!"));
                        return;
                    };
                    auth.PreSuccess += () =>
                    {
                        return (Guid.NewGuid(), auth.Name, new List<Property>()
                        {
                            new("textures", "ewogICJ0aW1lc3RhbXAiIDogMTY1MzIwMTM1OTk5MCwKICAicHJvZmlsZUlkIiA6ICJiMjdjMjlkZWZiNWU0OTEyYjFlYmQ5NDVkMmI2NzE0YSIsCiAgInByb2ZpbGVOYW1lIiA6ICJIRUtUMCIsCiAgInNpZ25hdHVyZVJlcXVpcmVkIiA6IHRydWUsCiAgInRleHR1cmVzIiA6IHsKICAgICJTS0lOIiA6IHsKICAgICAgInVybCIgOiAiaHR0cDovL3RleHR1cmVzLm1pbmVjcmFmdC5uZXQvdGV4dHVyZS82ZDE4NGJkYTRkZDllYWEwOWZmMDMxYzU5ZTkzZTIzN2ZhY2E0MjQzODUxMDYwMTliYjNhMzMxOGZmNTk4ODlmIiwKICAgICAgIm1ldGFkYXRhIiA6IHsKICAgICAgICAibW9kZWwiIDogInNsaW0iCiAgICAgIH0KICAgIH0KICB9Cn0=", "s1r8R8zvhqcgQ+iWl83oSn3ewxlPYIL8z09Z9oqhFVSNeMyq0GZc9NuHWtgrvjRPnxMUkEe4H5yyXACNg+L9S9lyPFcOh8Zl9E8mjD2NscXgTFj/mbO1N+gtgS/b+sLrVebPih72x/rnjoVqOLdJNbAWxQLZH5slo1vbiU9Njx3BZSJBQhKvOoBFfvzg+FXjEfTNiJkWU7yAeecPJN5mj4gsVYCyDGK5IWN81apeGTNfAJheEWFonuvmOnivbVqCQex1CREWIrAFwN+xSgM7Pu0r8DecdGtHihftOz3A/7bFfnoNIGvVuV14U70Hfw8x2UlAOxOlVK2pX6HpxL4b4cq7BZ6ja16pJtwOplfFunQAEGAA11idITtdsN+Q1y2EDKTGtF1n33TacXeJSqGoUDV8MYblDg53HfdvFbI02rnIZpy6A7Wmn9ithUO4D8Bu9EHOs54ei9mANxkfjU0RJ12f/aEhzz+kRCxU6qLBTL7LFaauJbkoAvReCK+F0xZh6TTo39EZfwScWlhzutV3pBvEYXKinJ3t8r9eLbmY7lW169ppT9t9y2IjFlVMrtrVEztXq9NW9DozkHKOxn4rNVmUrPLBH1m0BWo6xheiR+lKIqQSBX7rmDNQeLn8kvMfODWJFhEMksICPU7I7u3wWirxJHVu50oW6v440tfYYEM=")
                        }.AsReadOnly());
                    };
                    auth.PostSuccess += (Client client) =>
                    {
                        Console.WriteLine($"[{endpoint}] [{connection.Endpoint}] Logged in.");
                        List<BiomeType> biomes = new()
                        {
                            new("minecraft:plains")
                            {
                                Precipitation = false,
                                SkyColor = Color.Black,
                                WaterFogColor = Color.MediumPurple,
                                FogColor = Color.DarkBlue,
                                WaterColor = Color.Yellow,
                                AmbientSound = "minecraft:music.creative"
                            },
                        };
                        DimensionType dimensiontype = new("myzuc:subspace", 16, 0, biomes.AsReadOnly());
                        Dimension dimension = new("myzuc:subspace_0", dimensiontype);
                        dimension.Subscribe(client);

                        Tablist tab = new();
                        tab.Subscribe(client);
                        tab.Header.PostUpdate = new ChatText("mayonaise is superior!!1");

                        TablistEntry entry0 = new(Guid.NewGuid(), "mate27", null);
                        entry0.Gamemode.PostUpdate = Gamemode.Spectator;
                        entry0.Display.PostUpdate = new ChatText("[healer] mate27");
                        entry0.Latency.PostUpdate = -1;
                        entry0.Update();
                        entry0.Subscribe(tab);

                        TablistEntry entry1 = new(Guid.NewGuid(), "gay", null);
                        entry1.Gamemode.PostUpdate = Gamemode.Creative;
                        entry1.Display.PostUpdate = new ChatText("ur gay");
                        entry1.Latency.PostUpdate = 20;
                        entry1.Update();
                        entry1.Subscribe(tab);

                        Bossbar bossbar = new(Guid.NewGuid());
                        bossbar.Title.PostUpdate = new ChatText("baguet'");
                        bossbar.Flags.PostUpdate.DarkenSky = true;
                        bossbar.Update();
                        bossbar.Subscribe(client);

                        List<Chunk> chunks = new();
                        for (int cx = -5; cx <= 5; cx++)
                        {
                            for (int cz = -5; cz <= 5; cz++)
                            {
                                Chunk chunk = new(dimensiontype, cx, cz);
                                for (int x = 0; x < 16; x++)
                                {
                                    for (int z = 0; z < 16; z++)
                                    {
                                        chunk[3].Blocks[x, 15, z] = 1;
                                    }
                                }
                                chunks.Add(chunk);
                                chunk.Subscribe(client);
                            }
                        }
                        Player player = new(Guid.NewGuid());
                        player.X.PostUpdate = 0;
                        player.Y.PostUpdate = 66;
                        player.Z.PostUpdate = 0;
                        player.Pose.PostUpdate = Entity.EntityPose.Sleeping;
                        //player.Display.PostUpdate = new ChatText("gay 2");
                        TablistEntry tabplayer = new(player.Id, "gay", new List<Property>()
                        {
                            new("textures", "ewogICJ0aW1lc3RhbXAiIDogMTY1MzIwMTM1OTk5MCwKICAicHJvZmlsZUlkIiA6ICJiMjdjMjlkZWZiNWU0OTEyYjFlYmQ5NDVkMmI2NzE0YSIsCiAgInByb2ZpbGVOYW1lIiA6ICJIRUtUMCIsCiAgInNpZ25hdHVyZVJlcXVpcmVkIiA6IHRydWUsCiAgInRleHR1cmVzIiA6IHsKICAgICJTS0lOIiA6IHsKICAgICAgInVybCIgOiAiaHR0cDovL3RleHR1cmVzLm1pbmVjcmFmdC5uZXQvdGV4dHVyZS82ZDE4NGJkYTRkZDllYWEwOWZmMDMxYzU5ZTkzZTIzN2ZhY2E0MjQzODUxMDYwMTliYjNhMzMxOGZmNTk4ODlmIiwKICAgICAgIm1ldGFkYXRhIiA6IHsKICAgICAgICAibW9kZWwiIDogInNsaW0iCiAgICAgIH0KICAgIH0KICB9Cn0=", "s1r8R8zvhqcgQ+iWl83oSn3ewxlPYIL8z09Z9oqhFVSNeMyq0GZc9NuHWtgrvjRPnxMUkEe4H5yyXACNg+L9S9lyPFcOh8Zl9E8mjD2NscXgTFj/mbO1N+gtgS/b+sLrVebPih72x/rnjoVqOLdJNbAWxQLZH5slo1vbiU9Njx3BZSJBQhKvOoBFfvzg+FXjEfTNiJkWU7yAeecPJN5mj4gsVYCyDGK5IWN81apeGTNfAJheEWFonuvmOnivbVqCQex1CREWIrAFwN+xSgM7Pu0r8DecdGtHihftOz3A/7bFfnoNIGvVuV14U70Hfw8x2UlAOxOlVK2pX6HpxL4b4cq7BZ6ja16pJtwOplfFunQAEGAA11idITtdsN+Q1y2EDKTGtF1n33TacXeJSqGoUDV8MYblDg53HfdvFbI02rnIZpy6A7Wmn9ithUO4D8Bu9EHOs54ei9mANxkfjU0RJ12f/aEhzz+kRCxU6qLBTL7LFaauJbkoAvReCK+F0xZh6TTo39EZfwScWlhzutV3pBvEYXKinJ3t8r9eLbmY7lW169ppT9t9y2IjFlVMrtrVEztXq9NW9DozkHKOxn4rNVmUrPLBH1m0BWo6xheiR+lKIqQSBX7rmDNQeLn8kvMfODWJFhEMksICPU7I7u3wWirxJHVu50oW6v440tfYYEM=")
                        }.AsReadOnly());
                        tabplayer.Visible.PostUpdate = true;
                        tabplayer.Gamemode.PostUpdate = Gamemode.Creative;
                        //tabplayer.Display.PostUpdate = new ChatText("gay 1");
                        tabplayer.Latency.PostUpdate = 500;
                        tabplayer.Update();
                        tabplayer.Subscribe(tab);
                        player.Update();
                        player.Subscribe(client);

                        TablistEntry playertab = new(client.Id, "<client>", new List<Property>()
                        {
                            new("textures", "ewogICJ0aW1lc3RhbXAiIDogMTY1MzIwMTM1OTk5MCwKICAicHJvZmlsZUlkIiA6ICJiMjdjMjlkZWZiNWU0OTEyYjFlYmQ5NDVkMmI2NzE0YSIsCiAgInByb2ZpbGVOYW1lIiA6ICJIRUtUMCIsCiAgInNpZ25hdHVyZVJlcXVpcmVkIiA6IHRydWUsCiAgInRleHR1cmVzIiA6IHsKICAgICJTS0lOIiA6IHsKICAgICAgInVybCIgOiAiaHR0cDovL3RleHR1cmVzLm1pbmVjcmFmdC5uZXQvdGV4dHVyZS82ZDE4NGJkYTRkZDllYWEwOWZmMDMxYzU5ZTkzZTIzN2ZhY2E0MjQzODUxMDYwMTliYjNhMzMxOGZmNTk4ODlmIiwKICAgICAgIm1ldGFkYXRhIiA6IHsKICAgICAgICAibW9kZWwiIDogInNsaW0iCiAgICAgIH0KICAgIH0KICB9Cn0=", "s1r8R8zvhqcgQ+iWl83oSn3ewxlPYIL8z09Z9oqhFVSNeMyq0GZc9NuHWtgrvjRPnxMUkEe4H5yyXACNg+L9S9lyPFcOh8Zl9E8mjD2NscXgTFj/mbO1N+gtgS/b+sLrVebPih72x/rnjoVqOLdJNbAWxQLZH5slo1vbiU9Njx3BZSJBQhKvOoBFfvzg+FXjEfTNiJkWU7yAeecPJN5mj4gsVYCyDGK5IWN81apeGTNfAJheEWFonuvmOnivbVqCQex1CREWIrAFwN+xSgM7Pu0r8DecdGtHihftOz3A/7bFfnoNIGvVuV14U70Hfw8x2UlAOxOlVK2pX6HpxL4b4cq7BZ6ja16pJtwOplfFunQAEGAA11idITtdsN+Q1y2EDKTGtF1n33TacXeJSqGoUDV8MYblDg53HfdvFbI02rnIZpy6A7Wmn9ithUO4D8Bu9EHOs54ei9mANxkfjU0RJ12f/aEhzz+kRCxU6qLBTL7LFaauJbkoAvReCK+F0xZh6TTo39EZfwScWlhzutV3pBvEYXKinJ3t8r9eLbmY7lW169ppT9t9y2IjFlVMrtrVEztXq9NW9DozkHKOxn4rNVmUrPLBH1m0BWo6xheiR+lKIqQSBX7rmDNQeLn8kvMfODWJFhEMksICPU7I7u3wWirxJHVu50oW6v440tfYYEM=")
                        }.AsReadOnly());
                        playertab.Visible.PostUpdate = true;
                        playertab.Update();
                        playertab.Subscribe(tab);

                        client.Message(new ChatText("System Test Message"));

                        Border border = new();
                        border.Diameter.PostUpdate = 200;
                        border.TargetDiameter.PostUpdate = 200;
                        border.TargetTime.PostUpdate = TimeSpan.Zero;
                        border.WarningDistance.PostUpdate = 20;
                        border.WarningTime.PostUpdate = 0;
                        border.Update();
                        border.Subscribe(client);
                        border.TargetDiameter.PostUpdate = 5;
                        border.TargetTime.PostUpdate = TimeSpan.FromSeconds(30);
                        border.Update();
                        Thread.Sleep(2000);
                        ThreadPool.QueueUserWorkItem((object? _) =>
                        {
                            while (true)
                            {
                                for (double i = 0.0; i < Math.PI * 2; i += 0.2)
                                {
                                    player.X.PostUpdate = Math.Sin(i) * 2.0;
                                    player.Z.PostUpdate = Math.Cos(i) * 2.0;
                                    player.Update();
                                    Thread.Sleep(100);
                                }
                            }
                        });
                    };
                    connection.Disposed += () =>
                    {
                        Console.WriteLine($"[{endpoint}] [{connection.Endpoint}] Logging out.");
                    };
                    connection.Error += (ex) =>
                    {
                        ChatText chat = new("")
                        {
                            Color = "red",
                            Font = "minecraft:uniform",
                            Extra = new ChatComponent[]
                            {
                                new ChatText("mits.uk\n")
                                {
                                    Color = "#80C080",
                                    Bold = true
                                },
                                new ChatText("YOU HAVE BEEN BANNED!\n\n")
                                {
                                    Color = "#E00000"
                                },
                                new ChatText("admin rights go brrr\n\n")
                                {
                                    Color = "#FFFFFF"
                                },
                                new ChatText("xgamer27\n\n")
                                {
                                    Color = "#C0C000"
                                },
                                new ChatText("https://vir.us/F3bQsN3\n")
                                {
                                    Color = "#0563C1",
                                    Underlined = true
                                },
                                new ChatText("https://mits.uk/appeal\n")
                                {
                                    Color = "#0563C1",
                                    Underlined = true
                                },
                            }
                        };
                        auth.Disconnect(chat);
                    };
                };
                connection.Disposed += () =>
                {
                    Console.WriteLine($"[{endpoint}] [{connection.Endpoint}] Disconnected.");
                };
                connection.Error += (Exception ex) =>
                {
                    Console.WriteLine($"[{endpoint}] [{connection.Endpoint}] An Error occurred.");
                    Console.WriteLine($"{ex}");
                };
            };
            listener.Start();
            Console.WriteLine($"[{endpoint}] Started listening.");
        }
    }
}