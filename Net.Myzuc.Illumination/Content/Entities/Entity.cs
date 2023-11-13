using Net.Myzuc.Illumination.Base;
using Net.Myzuc.Illumination.Chat;
using Net.Myzuc.Illumination.Net;
using Net.Myzuc.Illumination.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security.AccessControl;

namespace Net.Myzuc.Illumination.Content.Entities
{
    public abstract class Entity : Subscribeable<Client>, IIdentifiable, IUpdateable
    {
        public enum EntityPose : int
        {
            Standing = 0,
            FallFlying = 1,
            Sleeping = 2,
            Swimming = 3,
            SpinAttack = 4,
            Snealing = 5,
            LongJumping = 6,
            Dying = 7,
            Croaking = 8,
            UsingTongue = 9,
            Sitting = 10,
            Roaring = 11,
            Sniffing = 12,
            Emerging = 13,
            Digging = 14,
        }
        public sealed class EntityFlags
        {
            public byte Bitmask = 0;
            public bool OnFire
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
            public bool Crouching
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
            public bool Sprinting
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
            public bool Swimming
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
            public bool Invisible
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
            public bool Glowing
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
            public bool ElytraFlying
            {
                get
                {
                    return (Bitmask & 128) != 0;
                }
                set
                {
                    Bitmask = (byte)((Bitmask & 127) | (value ? 128 : 0));
                }
            }
            public EntityFlags()
            {

            }
        }
        public abstract int TypeId { get; }
        public abstract double HitboxWidth { get; }
        public abstract double HitboxHeight { get; }
        public Guid Id { get; }
        private object Lock { get; } = new();
        public Updateable<double> X { get; }
        public Updateable<double> Y { get; }
        public Updateable<double> Z { get; }
        public Updateable<float> Pitch { get; }
        public Updateable<float> Yaw { get; }
        public Updateable<float> HeadYaw { get; }
        protected Updateable<int> ObjectData { get; }
        public Updateable<EntityFlags> Flags { get; }
        public Updateable<TimeSpan> Air { get; }
        public Updateable<ChatComponent?> Display { get; }
        public Updateable<bool> Silent { get; }
        public Updateable<bool> NoGravity { get; }
        public Updateable<EntityPose> Pose { get; }
        public Updateable<TimeSpan> Frozen { get; }

        private Dictionary<Guid, int> EntityIdLookup { get; } = new();
        internal Entity(Guid id)
        {
            Id = id;
            X = new(0.0d, Lock);
            Y = new(0.0d, Lock);
            Z = new(0.0d, Lock);
            Pitch = new(0.0f, Lock);
            Yaw = new(0.0f, Lock);
            HeadYaw = new(0.0f, Lock);
            ObjectData = new(0, Lock);
            Flags = new(new(), Lock);
            Air = new(TimeSpan.FromSeconds(15), Lock);
            Display = new(null, Lock);
            Silent = new(false, Lock);
            NoGravity = new(false, Lock);
            Pose = new(EntityPose.Standing, Lock);
            Frozen = new(TimeSpan.Zero, Lock);
        }
        public void Update()
        {
            lock (Lock)
            {
                if (Math.Abs(X.PreUpdate - X.PostUpdate) > 8.0d || Math.Abs(Y.PreUpdate - Y.PostUpdate) > 8.0d || Math.Abs(Z.PreUpdate - Z.PostUpdate) > 8.0d)
                {
                    Iterate((Client client) =>
                    {
                        int eid;
                        lock (EntityIdLookup)
                        {
                            eid = EntityIdLookup[client.Id];
                        }
                        using ContentStream mso = new();
                        mso.WriteS32V(104);
                        mso.WriteS32V(eid);
                        mso.WriteF64(X.PostUpdate);
                        mso.WriteF64(Y.PostUpdate);
                        mso.WriteF64(Z.PostUpdate);
                        mso.WriteU8((byte)(Yaw.PostUpdate / 360.0f * 256.0f));
                        mso.WriteU8((byte)(Pitch.PostUpdate / 360.0f * 256.0f));
                        mso.WriteBool(false);
                        client.Send(mso.Get());
                    });
                    X.Update();
                    Y.Update();
                    Z.Update();
                    Yaw.Update();
                    Pitch.Update();
                }
                if ((X.Updated || Y.Updated || Z.Updated) && (Pitch.Updated || Yaw.Updated || HeadYaw.Updated))
                {
                    Iterate((Client client) =>
                    {
                        int eid;
                        lock (EntityIdLookup)
                        {
                            eid = EntityIdLookup[client.Id];
                        }
                        using ContentStream mso = new();
                        mso.WriteS32V(44);
                        mso.WriteS32V(eid);
                        mso.WriteS32((ushort)((X.PostUpdate * 32.0d - X.PreUpdate * 32.0d) * 128.0d));
                        mso.WriteS32((ushort)((Y.PostUpdate * 32.0d - Y.PreUpdate * 32.0d) * 128.0d));
                        mso.WriteS32((ushort)((Z.PostUpdate * 32.0d - Z.PreUpdate * 32.0d) * 128.0d));
                        mso.WriteU8((byte)(Yaw.PostUpdate / 360.0f * 256.0f));
                        mso.WriteU8((byte)(Pitch.PostUpdate / 360.0f * 256.0f));
                        mso.WriteBool(false);
                        client.Send(mso.Get());
                    });
                    X.Update();
                    Y.Update();
                    Z.Update();
                    Yaw.Update();
                    Pitch.Update();
                }
                if (X.Updated || Y.Updated || Z.Updated)
                {
                    Iterate((Client client) =>
                    {
                        int eid;
                        lock (EntityIdLookup)
                        {
                            eid = EntityIdLookup[client.Id];
                        }
                        using ContentStream mso = new();
                        mso.WriteS32V(43);
                        mso.WriteS32V(eid);
                        mso.WriteS32((ushort)((X.PostUpdate * 32.0d - X.PreUpdate * 32.0d) * 128.0d));
                        mso.WriteS32((ushort)((Y.PostUpdate * 32.0d - Y.PreUpdate * 32.0d) * 128.0d));
                        mso.WriteS32((ushort)((Z.PostUpdate * 32.0d - Z.PreUpdate * 32.0d) * 128.0d));
                        mso.WriteBool(false);
                        client.Send(mso.Get());
                    });
                    X.Update();
                    Y.Update();
                    Z.Update();
                }
                if (Pitch.Updated || Yaw.Updated || HeadYaw.Updated)
                {
                    Iterate((Client client) =>
                    {
                        int eid;
                        lock (EntityIdLookup)
                        {
                            eid = EntityIdLookup[client.Id];
                        }
                        using ContentStream mso = new();
                        mso.WriteS32V(45);
                        mso.WriteS32V(eid);
                        mso.WriteU8((byte)(Yaw.PostUpdate / 360.0f * 256.0f));
                        mso.WriteU8((byte)(Pitch.PostUpdate / 360.0f * 256.0f));
                        mso.WriteBool(false);
                        client.Send(mso.Get());
                    });
                    Yaw.Update();
                    Pitch.Update();
                }
                if (HeadYaw.Updated)
                {
                    Iterate((Client client) =>
                    {
                        int eid;
                        lock (EntityIdLookup)
                        {
                            eid = EntityIdLookup[client.Id];
                        }
                        using ContentStream mso = new();
                        mso.WriteS32V(66);
                        mso.WriteS32V(eid);
                        mso.WriteU8((byte)(HeadYaw.PostUpdate / 360.0f * 256.0f));
                        client.Send(mso.Get());
                    });
                    HeadYaw.Update();
                }
                if (MetadataUpdated())
                {
                    Iterate((Client client) =>
                    {
                        int eid;
                        lock (EntityIdLookup)
                        {
                            eid = EntityIdLookup[client.Id];
                        }
                        using ContentStream mso = new();
                        mso.WriteS32V(82);
                        mso.WriteS32V(eid);
                        Serialize(mso);
                        client.Send(mso.Get());
                    });
                }
            }
        }
        public override void Subscribe(Client client)
        {
            base.Subscribe(client);
            int eid;
            lock (client.SubscribedEntities)
            {
                do
                {
                    eid = Random.Shared.Next();
                }
                while (client.SubscribedEntities.ContainsKey(eid));
                client.SubscribedEntities.Add(eid, this);
            }
            lock (EntityIdLookup)
            {
                EntityIdLookup.Add(client.Id, eid);
            }
            using (ContentStream mso = new())
            {
                mso.WriteS32V(1);
                mso.WriteS32V(eid);
                mso.WriteGuid(Id);
                mso.WriteS32V(TypeId);
                lock (Lock)
                {
                    mso.WriteF64(X.PreUpdate);
                    mso.WriteF64(Y.PreUpdate);
                    mso.WriteF64(Z.PreUpdate);
                    mso.WriteU8((byte)(Pitch.PreUpdate / 360.0f * 256.0f));
                    mso.WriteU8((byte)(Yaw.PreUpdate / 360.0f * 256.0f));
                    mso.WriteU8((byte)(HeadYaw.PreUpdate / 360.0f * 256.0f));
                    mso.WriteS32V(ObjectData.PreUpdate);
                }
                mso.WriteU16(0);
                mso.WriteU16(0);
                mso.WriteU16(0);
                client.Send(mso.Get());
            }
            using (ContentStream mso = new())
            {
                mso.WriteS32V(1);
                mso.WriteS32V(eid);
                mso.WriteGuid(Id);
                mso.WriteS32V(TypeId);
                lock (Lock)
                {
                    mso.WriteF64(X.PreUpdate);
                    mso.WriteF64(Y.PreUpdate);
                    mso.WriteF64(Z.PreUpdate);
                    mso.WriteU8((byte)(Pitch.PreUpdate / 360.0f * 256.0f));
                    mso.WriteU8((byte)(Yaw.PreUpdate / 360.0f * 256.0f));
                    mso.WriteU8((byte)(HeadYaw.PreUpdate / 360.0f * 256.0f));
                    mso.WriteS32V(ObjectData.PreUpdate);
                }
                mso.WriteU16(0);
                mso.WriteU16(0);
                mso.WriteU16(0);
                client.Send(mso.Get());
            }
            using (ContentStream mso = new())
            {
                mso.WriteS32V(82);
                mso.WriteS32V(eid);
                Serialize(mso);
                client.Send(mso.Get());
            }
        }
        public override void Unsubscribe(Client client)
        {
            base.Unsubscribe(client);
            using ContentStream mso = new();
            mso.WriteS32V(62);
            mso.WriteS32V(1);
            lock (client.SubscribedEntities)
            {
                int eid = EntityIdLookup[client.Id];
                lock (EntityIdLookup)
                {
                    mso.WriteS32V(eid);
                    client.Send(mso.Get());
                    client.SubscribedEntities.Remove(eid);
                }
                EntityIdLookup.Remove(client.Id);
            }
        }
        internal override void UnsubscribeQuietly(Client client)
        {
            base.UnsubscribeQuietly(client);
            lock (client.SubscribedEntities)
            {
                lock (EntityIdLookup)
                {
                    client.SubscribedEntities.Remove(EntityIdLookup[client.Id]);
                }
                EntityIdLookup.Remove(client.Id);
            }
        }
        public virtual void Serialize(ContentStream stream)
        {
            lock (Lock)
            {
                if (Flags.Updated)
                {
                    stream.WriteU8(0);
                    stream.WriteU8(0);
                    stream.WriteU8(Flags.PostUpdate.Bitmask);
                    Flags.Update();
                }
                if (Air.Updated)
                {
                    stream.WriteU8(1);
                    stream.WriteU8(1);
                    stream.WriteS32V((int)(Air.PostUpdate.TotalSeconds * 20.0d));
                    Air.Update();
                }
                if (Display.Updated)
                {
                    if (Display.PostUpdate is not null)
                    {
                        stream.WriteU8(2);
                        stream.WriteU8(6);
                        stream.WriteString32V(JsonConvert.SerializeObject(Display.PostUpdate, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore }));
                    }
                    stream.WriteU8(3);
                    stream.WriteU8(8);
                    stream.WriteBool(Display.PostUpdate is not null);
                    Display.Update();
                }
                if (Silent.Updated)
                {
                    stream.WriteU8(4);
                    stream.WriteU8(8);
                    stream.WriteBool(Silent.PostUpdate);
                    Silent.Update();
                }
                if (NoGravity.Updated)
                {
                    stream.WriteU8(5);
                    stream.WriteU8(8);
                    stream.WriteBool(NoGravity.PostUpdate);
                    NoGravity.Update();
                }
                if (Pose.Updated)
                {
                    stream.WriteU8(6);
                    stream.WriteU8(20);
                    stream.WriteS32V((int)Pose.PostUpdate);
                    Pose.Update();
                }
                if (Frozen.Updated)
                {
                    stream.WriteU8(7);
                    stream.WriteU8(1);
                    stream.WriteS32V((int)(Frozen.PostUpdate.TotalSeconds * 20.0d));
                    Frozen.Update();
                }
            }
        }
        public virtual bool MetadataUpdated()
        {
            return Flags.Updated || Air.Updated || Display.Updated || Silent.Updated || NoGravity.Updated || Pose.Updated || Frozen.Updated;
        }
    }
}
