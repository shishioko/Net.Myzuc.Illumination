using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Text.RegularExpressions;

namespace Net.Myzuc.Illumination.Content.Game.World
{
    public sealed class DimensionType
    {
        public string Name { get; }
        public string Effects { get; }
        public int SectionHeight { get; }
        public bool Natural { get; }
        public int SectionDepth { get; }
        public bool HasSkylight { get; }
        public float AmbientLight { get; }
        public ReadOnlyCollection<BiomeType> BiomeTypes { get; }
        public DimensionType(string name, int sectionHeight, int sectionDepth, ReadOnlyCollection<BiomeType> biomeTypes, string effects = "minecraft:overworld", float ambientlight = 0.0f, bool skylight = true, bool natural = true)
        {
            Contract.Requires(Regex.IsMatch(name, "[A-z:._-]"));
            Contract.Requires(sectionDepth >= -128);
            Contract.Requires(sectionHeight > 0);
            Contract.Requires(sectionDepth + sectionHeight < 128);
            Name = name;
            SectionHeight = (sectionHeight + 15) / 16 * 16;
            SectionDepth = sectionDepth / 16 * 16;
            BiomeTypes = biomeTypes;
            Effects = effects;
            AmbientLight = ambientlight;
            HasSkylight = skylight;
            Natural = natural;
            if (BiomeTypes.Count < 1) throw new InvalidOperationException("No biome specified!");
        }
    }
}
