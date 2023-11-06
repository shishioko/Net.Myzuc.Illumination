namespace Net.Myzuc.Illumination.Content.Entities.Structs
{
    public struct EntityFlags
    {
        public bool IsOnFire
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
        public bool IsCrouching
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
        public bool IsSprinting
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
        public bool IsSwimming
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
        public bool IsInvisible
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
        public bool IsGlowing
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
        public bool IsFallFlying
        {
            get
            {
                return (Value & 128) > 0;
            }
            set
            {
                Value &= 255 - 128;
                if (value) Value |= 128;
            }
        }
        public byte Value { get; private set; }
        public EntityFlags()
        {
            Value = 0;
        }
    }
}
