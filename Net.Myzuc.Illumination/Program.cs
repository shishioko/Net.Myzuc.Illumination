using Microsoft.VisualStudio.Threading;
using Net.Myzuc.Illumination.Content.Chat;
using Net.Myzuc.Illumination.Content.Game;
using Net.Myzuc.Illumination.Content.Game.Chunks;
using Net.Myzuc.Illumination.Content.Game.World;
using Net.Myzuc.Illumination.Content.Status;
using Net.Myzuc.Illumination.Net;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace Net.Myzuc.Illumination
{
    internal class Program
    {
        static void Main(string[] args)
        {
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, 62913);
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
                        Version = new("Net.Myzuc.Illumination", 763),
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
                    auth.Success += (Client client) =>
                    {
                        Console.WriteLine($"[{endpoint}] [{connection.Endpoint}] Logged in.");
                        List<BiomeType> biomes = new()
                        {
                            new("minecraft:plains", false, Color.Black, Color.MediumPurple, Color.DarkBlue, Color.Yellow, null, null, null, null, null, null),
                            new("myzuc:nexusbiome", false, Color.MediumPurple, Color.MediumPurple, Color.Blue, Color.Yellow, null, null, null, null, null, null)
                        };
                        DimensionType dimensiontype = new("myzuc:subspace", 16, 0, biomes.AsReadOnly());
                        Dimension dimension = new("myzuc:subspace_0", dimensiontype, 0, false);
                        dimension.Subscribe(client);
                        Tablist tab = new();
                        tab.Subscribe(client);
                        tab.Header = new ChatText("mayonaise is superior!!1");
                        TablistEntry entry0 = new(Guid.NewGuid())
                        {
                            InternalName = "mate27",
                            Gamemode = 3,
                            Display = new ChatText("[healer] mate27"),
                            Latency = -1,
                            Visible = true,
                        };
                        entry0.Subscribe(tab);
                        Bossbar bossbar = new(Guid.NewGuid())
                        {
                            Title = new ChatText("baguet'"),
                            DarkSky = true,
                        };
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
                        foreach (Chunk chunk in chunks)
                        {
                            for (int x = 0; x < 16; x++)
                            {
                                for (int z = 0; z < 16; z++)
                                {
                                    chunk[3].Blocks[x, 15, z] = 2;
                                }
                            }
                            chunk.Tick();
                        }
                        client.Message(new ChatText("System Test Message"));
                        Border border = new();
                        border.Diameter = 200;
                        border.TargetDiameter = 200;
                        border.TargetTime = DateTime.UnixEpoch;
                        border.WarningDistance = 5;
                        border.WarningTime = 5;
                        border.Subscribe(client);
                        border.TargetDiameter = 5;
                        border.TargetTime = DateTime.Now.AddSeconds(5);
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