using System.Diagnostics.Contracts;
using System.Drawing;
using System.Text.RegularExpressions;

namespace Net.Myzuc.Illumination.Content.World
{
    public sealed class BiomeType
    {
        public string Name { get; }
        public bool Precipitation { get; }
        public Color SkyColor { get; }
        public Color WaterFogColor { get; }
        public Color FogColor { get; }
        public Color WaterColor { get; }
        public Color? FoliageColor { get; }
        public Color? GrassColor { get; }
        public (bool, string, int, int)? Music { get; }
        public string? AmbientSound { get; }
        public (string, double)? AdditionsSound { get; }
        public (string, int, double, int)? MoodSound { get; }
        public BiomeType(string name, bool precipitation, Color skyColor, Color waterFogColor, Color fogColor, Color waterColor, Color? foliageColor, Color? grassColor, (bool, string, int, int)? music, string? ambientSound, (string, double)? additionsSound, (string, int, double, int)? moodSound)
        {
            Contract.Requires(Regex.IsMatch(name, "[A-z:._-]"));
            Name = name;
            Precipitation = precipitation;
            SkyColor = skyColor;
            WaterFogColor = waterFogColor;
            FogColor = fogColor;
            WaterColor = waterColor;
            FoliageColor = foliageColor;
            GrassColor = grassColor;
            Music = music;
            AmbientSound = ambientSound;
            AdditionsSound = additionsSound;
            MoodSound = moodSound;
        }
        //TODO: particle (float:probability,compound:properties+type:particle_type)
    }
}
