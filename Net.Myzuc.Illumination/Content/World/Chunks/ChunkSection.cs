using Net.Myzuc.Illumination.Content.World;

namespace Net.Myzuc.Illumination.Content.World.Chunks
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
}
