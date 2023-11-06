namespace Net.Myzuc.Illumination.Content.Game
{
    public struct EntityHandFlags
    {
        public bool IsHandActive
        {
            get
            {
                return (Value & 1) > 0;
            }
            set
            {
                Value &= 255 - 1;
                if (value) Value |= 1;
            }
        }
        public bool IsOffhandActive
        {
            get
            {
                return (Value & 2) > 0;
            }
            set
            {
                Value &= 255 - 2;
                if (value) Value |= 2;
            }
        }
        public bool IsRiptideSpinning
        {
            get
            {
                return (Value & 4) > 0;
            }
            set
            {
                Value &= 255 - 4;
                if (value) Value |= 4;
            }
        }
        public byte Value { get; private set; }
        public EntityHandFlags()
        {
            Value = 0;
        }
    }
}
