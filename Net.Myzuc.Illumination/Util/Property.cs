namespace Net.Myzuc.Illumination.Util
{
    public readonly struct Property
    {
        public readonly string Name;
        public readonly string Value;
        public readonly string? Signature;
        public Property(string name, string value, string? signature)
        {
            Name = name;
            Value = value;
            Signature = signature;
        }
    }
}
