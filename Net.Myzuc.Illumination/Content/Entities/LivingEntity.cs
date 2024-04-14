using Me.Shishioko.Illumination.Content.Structs;
using Me.Shishioko.Illumination.Net;
using Me.Shishioko.Illumination.Util;
using System;
using System.Drawing;

namespace Me.Shishioko.Illumination.Content.Entities
{
    public abstract class LivingEntity : Entity
    {
        public sealed class LivingEntityFlags
        {
            public byte Bitmask = 0;
            public bool HandActive
            {
                get
                {
                    return (Bitmask & 1) != 0;
                }
                set
                {
                    Bitmask = (byte)((Bitmask & 254) | (value ? 1 : 0));
                }
            }
            public bool OffhandActive
            {
                get
                {
                    return (Bitmask & 2) != 0;
                }
                set
                {
                    Bitmask = (byte)((Bitmask & 253) | (value ? 2 : 0));
                }
            }
            public bool Spinning
            {
                get
                {
                    return (Bitmask & 4) != 0;
                }
                set
                {
                    Bitmask = (byte)((Bitmask & 251) | (value ? 4 : 0));
                }
            }
            public LivingEntityFlags()
            {

            }
        }

        public Updateable<LivingEntityFlags> HandFlags { get; }
        public Updateable<float> Health { get; }
        public Updateable<Color> PotionEffectColor { get; }
        public Updateable<bool> PotionEffectAmbient { get; }
        public Updateable<int> ArrowCount { get; } //TODO: necessary?
        public Updateable<int> StingCount { get; }
        public Updateable<Position?> SleepingAt { get; }
        internal LivingEntity(Guid id, int type) : base(id, type)
        {
            HandFlags = new(new(), Lock);
            Health = new(1.0f, Lock);
            PotionEffectColor = new(Color.Transparent, Lock);
            PotionEffectAmbient = new(false, Lock);
            ArrowCount = new(0, Lock);
            StingCount = new(0, Lock);
            SleepingAt = new(null, Lock);
        }
        protected override void Serialize(ContentStream stream, bool update)
        {
            lock (Lock)
            {
                base.Serialize(stream, update);
                if (HandFlags.Updated || !update)
                {
                    if (update) HandFlags.Update();
                    stream.WriteU8(8);
                    stream.WriteS32V(0);
                    stream.WriteU8(HandFlags.PostUpdate.Bitmask);
                }
                if (Health.Updated || !update)
                {
                    if (update) Health.Update();
                    stream.WriteU8(9);
                    stream.WriteS32V(3);
                    stream.WriteF32(Health.PostUpdate);
                }
                if (PotionEffectColor.Updated || !update)
                {
                    if (update) PotionEffectColor.Update();
                    stream.WriteU8(10);
                    stream.WriteS32V(1);
                    stream.WriteS32V(PotionEffectColor.PostUpdate.ToArgb());
                }
                if (PotionEffectAmbient.Updated || !update)
                {
                    if (update) PotionEffectAmbient.Update();
                    stream.WriteU8(11);
                    stream.WriteS32V(8);
                    stream.WriteBool(PotionEffectAmbient.PostUpdate);
                }
                if (ArrowCount.Updated || !update)
                {
                    if (update) ArrowCount.Update();
                    stream.WriteU8(12);
                    stream.WriteS32V(1);
                    stream.WriteS32V(ArrowCount.PostUpdate);
                }
                if (StingCount.Updated || !update)
                {
                    if (update) StingCount.Update();
                    stream.WriteU8(13);
                    stream.WriteS32V(1);
                    stream.WriteS32V(StingCount.PostUpdate);
                }
                if (SleepingAt.Updated || !update)
                {
                    if (update) SleepingAt.Update();
                    stream.WriteU8(14);
                    stream.WriteS32V(11);
                    stream.WriteBool(SleepingAt.PostUpdate.HasValue);
                    if (SleepingAt.PostUpdate.HasValue) stream.WriteU64(SleepingAt.PostUpdate.Value.Value);
                }
            }
        }
        protected override bool MetadataUpdated()
        {
            return base.MetadataUpdated() || HandFlags.Updated || Health.Updated || PotionEffectColor.Updated || PotionEffectAmbient.Updated || ArrowCount.Updated || StingCount.Updated || SleepingAt.Updated;
        }
    }
}
