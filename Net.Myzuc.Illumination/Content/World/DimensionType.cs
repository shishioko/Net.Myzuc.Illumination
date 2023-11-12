using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text.RegularExpressions;

namespace Net.Myzuc.Illumination.Content.World
{
    public sealed class DimensionType
    {
        public enum VanillaDimension
        {
            Overworld,
            Nether,
            End
        }
        public string Name { get; }
        public int SectionHeight { get; }
        public int SectionDepth { get; }
        public VanillaDimension Effects { get; init; } = VanillaDimension.Overworld;
        public bool Natural { get; init; } = true;
        public bool HasSkylight { get; init; } = true;
        public float AmbientLight { get; init; } = 0.0f;
        public ReadOnlyCollection<BiomeType> BiomeTypes { get; } = new List<BiomeType>().AsReadOnly();
        public DimensionType(string name = "minecraft:overworld", int sectionHeight = 16, int sectionDepth = 0, ReadOnlyCollection<BiomeType>? biomes = null)
        {
            Contract.Requires(Regex.IsMatch(name, "[A-z:._-]"));
            Contract.Requires(sectionDepth >= -128);
            Contract.Requires(sectionHeight > 0);
            Contract.Requires(sectionDepth + sectionHeight < 128);
            Name = name;
            SectionHeight = sectionHeight;
            SectionDepth = sectionDepth;
            List<BiomeType> biomeList = biomes is not null ? new(biomes) : new();
            if (!biomeList.Any(biome => biome.Name == "minecraft:plains")) biomeList.Add(new("minecraft:plains"));
            BiomeTypes = biomeList.AsReadOnly();
        }
    }
}
