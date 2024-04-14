using Me.Shishioko.Illumination.Base;
using Me.Shishioko.Illumination.Net;
using Me.Shishioko.Illumination.Util;
using System;

namespace Me.Shishioko.Illumination.Content.Entities
{
    public sealed class Player : LivingEntity
    {
        public sealed class PlayerFlags
        {
            public byte Bitmask = 0;
            public bool Cape
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
            public bool Body
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
            public bool LeftArm
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
            public bool RightArm
            {
                get
                {
                    return (Bitmask & 8) != 0;
                }
                set
                {
                    Bitmask = (byte)((Bitmask & 247) | (value ? 8 : 0));
                }
            }
            public bool LeftLeg
            {
                get
                {
                    return (Bitmask & 16) != 0;
                }
                set
                {
                    Bitmask = (byte)((Bitmask & 239) | (value ? 16 : 0));
                }
            }
            public bool RightLeg
            {
                get
                {
                    return (Bitmask & 32) != 0;
                }
                set
                {
                    Bitmask = (byte)((Bitmask & 223) | (value ? 32 : 0));
                }
            }
            public bool Head
            {
                get
                {
                    return (Bitmask & 64) != 0;
                }
                set
                {
                    Bitmask = (byte)((Bitmask & 191) | (value ? 64 : 0));
                }
            }
            public PlayerFlags(bool disable = false)
            {
                if (!disable) Bitmask = 127;
            }
        }
        public override double HitboxWidth => 0.6;
        public override double HitboxHeight => 1.8;
        public Updateable<float> Absorption { get; }
        public Updateable<int> Score { get; }
        public Updateable<PlayerFlags> SkinFlags { get; }
        public Updateable<bool> RightHanded { get; }
        //TODO: parrot left shoulder nbt reference entity
        //TODO: parrot right shoulder nbt reference entity
        public Player(Guid id) : base(id, 122)
        {
            Absorption = new(0.0f, Lock);
            Score = new(0, Lock);
            SkinFlags = new(new(), Lock);
            RightHanded = new(true, Lock);
        }
        protected override void Spawn(Client client)
        {
            int eid;
            lock (EntityIdLookup)
            {
                 eid = EntityIdLookup[client.Id];
            }
            using (ContentStream mso = new())
            {
                mso.WriteS32V(3);
                lock (EntityIdLookup)
                mso.WriteS32V(eid);
                mso.WriteGuid(Id);
                mso.WriteF64(X.PreUpdate);
                mso.WriteF64(Y.PreUpdate);
                mso.WriteF64(Z.PreUpdate);
                mso.WriteU8((byte)(Pitch.PreUpdate / 360.0f * 256.0f));
                mso.WriteU8((byte)(Yaw.PreUpdate / 360.0f * 256.0f));
                client.Send(mso.Get());
            }
            using (ContentStream mso = new())
            {
                mso.WriteS32V(66);
                mso.WriteS32V(eid);
                mso.WriteU8((byte)(HeadYaw.PostUpdate / 360.0f * 256.0f));
                client.Send(mso.Get());
            }
        }
        /*protected override void Despawn(Client client)
        {
            using ContentStream mso = new();
            mso.WriteS32V(82);
            client.Send(mso.Get());
        }*/
        protected override void Serialize(ContentStream stream, bool update)
        {
            lock (Lock)
            {
                base.Serialize(stream, update);
                if (Absorption.Updated || !update)
                {
                    if (update) Absorption.Update();
                    stream.WriteU8(15);
                    stream.WriteS32V(3);
                    stream.WriteF32(Absorption.PostUpdate);
                }
                if (Score.Updated || !update)
                {
                    if (update) Score.Update();
                    stream.WriteU8(16);
                    stream.WriteS32V(1);
                    stream.WriteS32V(Score.PostUpdate);
                }
                if (SkinFlags.Updated || !update)
                {
                    if (update) SkinFlags.Update();
                    stream.WriteU8(17);
                    stream.WriteS32V(0);
                    stream.WriteU8(SkinFlags.PostUpdate.Bitmask);
                }
                if (RightHanded.Updated || !update)
                {
                    if (update) RightHanded.Update();
                    stream.WriteU8(18);
                    stream.WriteS32V(0);
                    stream.WriteBool(RightHanded.PostUpdate);
                }
            }
        }
        protected override bool MetadataUpdated()
        {
            return base.MetadataUpdated() || Absorption.Updated || Score.Updated || SkinFlags.Updated || RightHanded.Updated;
        }
    }
}
