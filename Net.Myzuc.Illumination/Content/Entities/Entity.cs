using Net.Myzuc.Illumination.Base;
using Net.Myzuc.Illumination.Chat;
using Net.Myzuc.Illumination.Content.Entities.Structs;
using Net.Myzuc.Illumination.Net;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Net.Myzuc.Illumination.Content.Entities
{
    public abstract class Entity : IDisposable
    {
        public abstract int TypeId { get; }
        public abstract double HitboxWidth { get; }
        public abstract double HitboxHeight { get; }
        public Guid EntityGuid { get; }
        private float InternalHeadYaw;
        public float HeadYaw
        {
            get
            {
                return InternalHeadYaw;
            }
            set
            {
                InternalHeadYaw = value;
                foreach (Client client in Subscribers.Values)
                {
                    if (!client.SubscribedEntities.TryGetValue(this, out int eid)) continue;
                    using ContentStream mso = new();
                    mso.WriteS32V(66);
                    mso.WriteS32V(eid);
                    mso.WriteU8((byte)(HeadYaw / 360.0f * 256.0f));
                    client.Send(mso.Get());
                }
            }
        }
        public EntityPosition Position
        {
            get
            {
                return InternalPosition;
            }
            set
            {
                InternalPosition = value;
                foreach (Client client in Subscribers.Values)
                {
                    if (!client.SubscribedEntities.TryGetValue(this, out int eid)) continue;
                    using ContentStream mso = new();
                    mso.WriteS32V(104);
                    mso.WriteS32V(eid);
                    mso.WriteF64(Position.X);
                    mso.WriteF64(Position.Y);
                    mso.WriteF64(Position.Z);
                    mso.WriteU8((byte)(Position.Pitch / 360.0f * 256.0f));
                    mso.WriteU8((byte)(Position.Yaw / 360.0f * 256.0f));
                    mso.WriteBool(true);
                    client.Send(mso.Get());
                }
            }
        }
        private EntityPosition InternalPosition;

        public int ObjectData { get; set; }
        public EntityFlags Flags { get; }
        public int AirTicks { get; set; }
        public ChatComponent? CustomName { get; set; }
        public bool IsCustomNameVisible { get; set; }
        public bool IsSilent { get; set; }
        public bool NoGravity { get; set; }
        public Pose Pose { get; set; }
        public int FrozenTicks { get; set; }
        internal ConcurrentDictionary<Guid, Client> Subscribers;
        internal Entity()
        {
            EntityGuid = Guid.NewGuid();
            Subscribers = new();
        }
        public void Subscribe(Client client)
        {
            if (!Subscribers.TryAdd(client.Id, client)) return;
            int eid = 0;
            while (client.SubscribedEntityIds.ContainsKey(eid))
            {
                eid = Random.Shared.Next();
            }
            client.SubscribedEntities.TryAdd(this, eid);
            client.SubscribedEntityIds.TryAdd(eid, EntityGuid);
            using ContentStream mso = new();
            mso.WriteS32V(1);
            mso.WriteS32V(eid);
            mso.WriteGuid(EntityGuid);
            mso.WriteS32V(TypeId);
            mso.WriteF64(Position.X);
            mso.WriteF64(Position.Y);
            mso.WriteF64(Position.Z);
            mso.WriteU8((byte)(Position.Pitch / 360.0f * 256.0f));
            mso.WriteU8((byte)(Position.Yaw / 360.0f * 256.0f));
            mso.WriteU8((byte)(HeadYaw / 360.0f * 256.0f));
            Serialize(mso);
            mso.WriteS32V(ObjectData);
            mso.WriteU16(0);
            mso.WriteU16(0);
            mso.WriteU16(0);
            client.Send(mso.Get());
        }
        public void Unsubscribe(Client client)
        {
            if (!Subscribers.TryRemove(client.Id, out _)) return;
            if (client.SubscribedEntities.TryRemove(this, out int eid))
            {
                client.SubscribedEntityIds.TryRemove(eid, out _);
                using ContentStream mso = new();
                mso.WriteS32V(62);
                mso.WriteS32V(1);
                mso.WriteS32V(eid);
                client.Send(mso.Get());
            }
        }
        public void Update()
        {
            foreach (Client client in Subscribers.Values)
            {
                if (!client.SubscribedEntities.TryGetValue(this, out int eid)) continue;
                using ContentStream mso = new();
                mso.WriteS32V(82);
                mso.WriteS32V(eid);
                Serialize(mso);
                client.Send(mso.Get());
            }
        }
        public void Dispose()
        {
            while (!Subscribers.IsEmpty)
            {
                Unsubscribe(Subscribers.First().Value);
            }
            GC.SuppressFinalize(this);
        }
        public virtual void Serialize(ContentStream stream)
        {
            stream.WriteU8(0);
            stream.WriteU8(0);
            stream.WriteU8(Flags.Value);
            stream.WriteU8(1);
            stream.WriteU8(1);
            stream.WriteS32V(AirTicks);
            stream.WriteU8(2);
            stream.WriteU8(6);
            stream.WriteString32VN(CustomName is null ? JsonConvert.SerializeObject(CustomName, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore }) : null);
            stream.WriteU8(3);
            stream.WriteU8(8);
            stream.WriteBool(IsCustomNameVisible);
            stream.WriteU8(4);
            stream.WriteU8(8);
            stream.WriteBool(IsSilent);
            stream.WriteU8(5);
            stream.WriteU8(8);
            stream.WriteBool(NoGravity);
            stream.WriteU8(6);
            stream.WriteU8(20);
            stream.WriteS32V(Pose.GetHashCode());
            stream.WriteU8(7);
            stream.WriteU8(1);
            stream.WriteS32V(FrozenTicks);
        }
    }
}
