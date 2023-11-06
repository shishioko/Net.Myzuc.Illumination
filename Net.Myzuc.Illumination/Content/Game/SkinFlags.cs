namespace Net.Myzuc.Illumination.Content.Game
{
    public struct SkinFlags
    {
        public bool Cape
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
        public bool Jacket
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
        public bool LeftSleeve
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
        public bool RightSleeve
        {
            get
            {
                return (Value & 8) > 0;
            }
            set
            {
                Value &= 255 - 8;
                if (value) Value |= 8;
            }
        }
        public bool LeftLeg
        {
            get
            {
                return (Value & 16) > 0;
            }
            set
            {
                Value &= 255 - 16;
                if (value) Value |= 16;
            }
        }
        public bool RightLeg
        {
            get
            {
                return (Value & 32) > 0;
            }
            set
            {
                Value &= 255 - 32;
                if (value) Value |= 32;
            }
        }
        public bool Hat
        {
            get
            {
                return (Value & 64) > 0;
            }
            set
            {
                Value &= 255 - 64;
                if (value) Value |= 64;
            }
        }
        public byte Value { get; private set; }
        public SkinFlags()
        {
            Value = 0;
        }
    }
}
