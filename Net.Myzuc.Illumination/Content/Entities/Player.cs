using Net.Myzuc.Illumination.Content.Entities.Structs;
using Net.Myzuc.Illumination.Net;

namespace Net.Myzuc.Illumination.Content.Entities
{
    public sealed class Player : LivingEntity
    {
        public override int TypeId => 122;
        public override double HitboxWidth => 0.6;
        public override double HitboxHeight => 1.8;
        public float Absorption { get; set; }
        public int Score { get; set; }
        public SkinFlags SkinParts { get; }
        public bool RightHanded { get; }
        internal Client? Link; //TODO: link or exclude
        //TODO: parrot left shoulder nbt reference entity
        //TODO: parrot right shoulder nbt reference entity
        public Player(Client? link = null)
        {
            RightHanded = true;
            Link = link;
        }
        public override void Serialize(ContentStream stream)
        {
            base.Serialize(stream);
            stream.WriteU8(15);
            stream.WriteU8(3);
            stream.WriteF32(Absorption);
            stream.WriteU8(16);
            stream.WriteU8(1);
            stream.WriteS32V(Score);
            stream.WriteU8(17);
            stream.WriteU8(0);
            stream.WriteU8(SkinParts.Value);
            stream.WriteU8(18);
            stream.WriteU8(8);
            stream.WriteBool(RightHanded);
        }
    }
}
