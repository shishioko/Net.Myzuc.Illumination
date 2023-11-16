using Net.Myzuc.Illumination.Base;
using Net.Myzuc.Illumination.Content.Structs;
using Net.Myzuc.Illumination.Net;
using Net.Myzuc.Illumination.Util;
using System;
using System.Drawing;
using System.Security.Cryptography;

namespace Net.Myzuc.Illumination.Content.Entities
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
                    stream.WriteU8(8);
                    stream.WriteS32V(0);
                    stream.WriteU8(HandFlags.PostUpdate.Bitmask);
                    if (update) HandFlags.Update();
                }
                if (Health.Updated || !update)
                {
                    stream.WriteU8(9);
                    stream.WriteS32V(3);
                    stream.WriteF32(Health.PostUpdate);
                    if (update) Health.Update();
                }
                if (PotionEffectColor.Updated || !update)
                {
                    stream.WriteU8(10);
                    stream.WriteS32V(1);
                    stream.WriteS32V(PotionEffectColor.PostUpdate.ToArgb());
                    if (update) PotionEffectColor.Update();
                }
                if (PotionEffectAmbient.Updated || !update)
                {
                    stream.WriteU8(11);
                    stream.WriteS32V(8);
                    stream.WriteBool(PotionEffectAmbient.PostUpdate);
                    if (update) PotionEffectAmbient.Update();
                }
                if (ArrowCount.Updated || !update)
                {
                    stream.WriteU8(12);
                    stream.WriteS32V(1);
                    stream.WriteS32V(ArrowCount.PostUpdate);
                    if (update) ArrowCount.Update();
                }
                if (StingCount.Updated || !update)
                {
                    stream.WriteU8(13);
                    stream.WriteS32V(1);
                    stream.WriteS32V(StingCount.PostUpdate);
                    if (update) StingCount.Update();
                }
                if (SleepingAt.Updated || !update)
                {
                    stream.WriteU8(14);
                    stream.WriteS32V(11);
                    stream.WriteBool(SleepingAt.PostUpdate.HasValue);
                    if (SleepingAt.PostUpdate.HasValue) stream.WriteU64(SleepingAt.PostUpdate.Value.Value);
                    if (update) SleepingAt.Update();
                }
            }
        }
        protected override bool MetadataUpdated()
        {
            return base.MetadataUpdated() || HandFlags.Updated || Health.Updated || PotionEffectColor.Updated || PotionEffectAmbient.Updated || ArrowCount.Updated || StingCount.Updated || SleepingAt.Updated;
        }
    }
}
