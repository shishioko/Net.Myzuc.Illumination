using System;
using System.Drawing;

namespace Me.Shishioko.Illumination.Content
{
    public sealed class BiomeType
    {
        public readonly struct BiomeMusic
        {
            public readonly string Sound { get; }
            public readonly bool ReplaceCurrentMusic { get; init; } = true;
            public readonly TimeSpan MaxDelay { get; init; } = TimeSpan.Zero;
            public readonly TimeSpan MinDelay { get; init; } = TimeSpan.Zero;
            public BiomeMusic(string sound)
            {
                Sound = sound;
            }
        }
        public readonly struct BiomeRandomSound//TODO: does this actually work?
        {
            public readonly string Sound { get; }
            public readonly double Chance { get; init; } = 1.0d / 60.0d;
            public BiomeRandomSound(string sound)
            {
                Sound = sound;
            }
        }
        public readonly struct BiomeIntervalSound //TODO: does this actually work?
        {
            public readonly string Sound { get; }
            public readonly TimeSpan Delay { get; init; } = TimeSpan.FromSeconds(10);
            public readonly double Offset { get; init; } = 0.0d;
            public readonly int BlockSearchExtent { get; init; } = 0;
            public BiomeIntervalSound(string sound)
            {
                Sound = sound;
            }
        }
        public string Name { get; }
        public bool Precipitation { get; init; } = true;
        public Color SkyColor { get; init; } = Color.CornflowerBlue;
        public Color WaterFogColor { get; init; } = Color.DarkBlue;
        public Color FogColor { get; init; } = Color.WhiteSmoke;
        public Color WaterColor { get; init; } = Color.Blue;
        public Color? FoliageColor { get; init; } = Color.LimeGreen;
        public Color? GrassColor { get; init; } = Color.LimeGreen;
        public BiomeMusic? Music { get; init; } = null;
        public string? AmbientSound { get; init; } = null;
        public BiomeRandomSound? AdditionsSound { get; init; } = null;
        public BiomeIntervalSound? MoodSound { get; init; } = null;
        public BiomeType(string name)
        {
            Name = name;
        }
        //TODO: particle (float:probability,compound:properties+type:particle_type)
    }
}
