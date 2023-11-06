using Net.Myzuc.Illumination.Content.Entities.Structs;
using Net.Myzuc.Illumination.Content.World;
using Net.Myzuc.Illumination.Net;

namespace Net.Myzuc.Illumination.Content.Entities
{
    public abstract class LivingEntity : Entity
    {
        public EntityHandFlags HandFlags { get; }
        public float Health { get; set; }
        public int PotionEffectColor { get; set; }
        public bool PotionEffectAmbient { get; set; }
        public int ArrowCount { get; set; }
        public int StingCount { get; set; }
        public Position? SleepingAt { get; set; }
        internal LivingEntity()
        {
            Health = 1.0f;
            SleepingAt = null;
        }
        public override void Serialize(ContentStream stream)
        {
            base.Serialize(stream);
            stream.WriteU8(8);
            stream.WriteU8(0);
            stream.WriteU8(HandFlags.Value);
            stream.WriteU8(9);
            stream.WriteU8(3);
            stream.WriteF32(Health);
            stream.WriteU8(10);
            stream.WriteU8(1);
            stream.WriteS32V(PotionEffectColor);
            stream.WriteU8(11);
            stream.WriteU8(8);
            stream.WriteBool(PotionEffectAmbient);
            stream.WriteU8(12);
            stream.WriteU8(1);
            stream.WriteS32V(ArrowCount);
            stream.WriteU8(13);
            stream.WriteU8(1);
            stream.WriteS32V(StingCount);
            stream.WriteU8(14);
            stream.WriteU8(11);
            stream.WriteBool(SleepingAt.HasValue);
            if (SleepingAt.HasValue) stream.WriteU64(SleepingAt.Value.Value);
        }
    }
}
