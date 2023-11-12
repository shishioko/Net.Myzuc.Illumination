using System.Collections.Generic;
using System;
using Net.Myzuc.Illumination.Net;
using Newtonsoft.Json;
using Net.Myzuc.Illumination.Chat;

namespace Net.Myzuc.Illumination.Content
{
    public sealed class Bossbar
    {
        public Guid Id { get; }
        public ChatComponent InternalTitle { get; set; }
        public float InternalHealth { get; set; }
        public int InternalColor { get; set; }
        public int InternalDivision { get; set; }
        public byte InternalFlags { get; set; }
        public ChatComponent Title
        {
            get
            {
                return InternalTitle;
            }
            set
            {
                InternalTitle = value;
                using ContentStream mso = new();
                mso.WriteS32V(11);
                mso.WriteGuid(Id);
                mso.WriteS32V(3);
                mso.WriteString32V(JsonConvert.SerializeObject(InternalTitle, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore }));
                lock (Subscribers)
                {
                    foreach (Client client in Subscribers.Values)
                    {
                        client.Send(mso.Get());
                    }
                }
            }
        }
        public float Health
        {
            get
            {
                return InternalHealth;
            }
            set
            {
                InternalHealth = value;
                using ContentStream mso = new();
                mso.WriteS32V(11);
                mso.WriteGuid(Id);
                mso.WriteS32V(2);
                mso.WriteF32(InternalHealth);
                lock (Subscribers)
                {
                    foreach (Client client in Subscribers.Values)
                    {
                        client.Send(mso.Get());
                    }
                }
            }
        }
        public int Color
        {
            get
            {
                return InternalColor;
            }
            set
            {
                InternalColor = value;
                using ContentStream mso = new();
                mso.WriteS32V(11);
                mso.WriteGuid(Id);
                mso.WriteS32V(4);
                mso.WriteS32V(InternalColor);
                mso.WriteS32V(InternalDivision);
                lock (Subscribers)
                {
                    foreach (Client client in Subscribers.Values)
                    {
                        client.Send(mso.Get());
                    }
                }
            }
        }
        public int Division
        {
            get
            {
                return InternalDivision;
            }
            set
            {
                InternalDivision = value;
                using ContentStream mso = new();
                mso.WriteS32V(11);
                mso.WriteGuid(Id);
                mso.WriteS32V(4);
                mso.WriteS32V(InternalColor);
                mso.WriteS32V(InternalDivision);
                lock (Subscribers)
                {
                    foreach (Client client in Subscribers.Values)
                    {
                        client.Send(mso.Get());
                    }
                }
            }
        }
        public bool DarkSky
        {
            get
            {
                return (InternalFlags & 1) != 0;
            }
            set
            {
                InternalDivision &= ~1;
                InternalDivision |= value ? 1 : 0;
                using ContentStream mso = new();
                mso.WriteS32V(11);
                mso.WriteGuid(Id);
                mso.WriteS32V(5);
                mso.WriteS32V(InternalFlags);
                lock (Subscribers)
                {
                    foreach (Client client in Subscribers.Values)
                    {
                        client.Send(mso.Get());
                    }
                }
            }
        }
        public bool EndMusic
        {
            get
            {
                return (InternalFlags & 2) != 0;
            }
            set
            {
                InternalDivision &= ~2;
                InternalDivision |= value ? 2 : 0;
                using ContentStream mso = new();
                mso.WriteS32V(11);
                mso.WriteGuid(Id);
                mso.WriteS32V(5);
                mso.WriteS32V(InternalFlags);
                lock (Subscribers)
                {
                    foreach (Client client in Subscribers.Values)
                    {
                        client.Send(mso.Get());
                    }
                }
            }
        }
        public bool Fog
        {
            get
            {
                return (InternalFlags & 4) != 0;
            }
            set
            {
                InternalDivision &= ~4;
                InternalDivision |= value ? 4 : 0;
                using ContentStream mso = new();
                mso.WriteS32V(11);
                mso.WriteGuid(Id);
                mso.WriteS32V(5);
                mso.WriteS32V(InternalFlags);
                lock (Subscribers)
                {
                    foreach (Client client in Subscribers.Values)
                    {
                        client.Send(mso.Get());
                    }
                }
            }
        }
        private Dictionary<Guid, Client> Subscribers { get; }
        public Bossbar(Guid id)
        {
            Id = id;
            InternalTitle = new ChatText(string.Empty);
            InternalColor = 0;
            InternalDivision = 0;
            InternalFlags = 0;
            Subscribers = new();
        }

        public void Subscribe(Client client)
        {
            lock (Subscribers)
            {
                Subscribers.Add(client.Login.Id, client);
            }
            lock (client.Bossbars)
            {
                client.Bossbars.Add(Id, this);
            }
            using ContentStream mso = new();
            mso.WriteS32V(11);
            mso.WriteGuid(Id);
            mso.WriteS32V(0);
            mso.WriteString32V(JsonConvert.SerializeObject(InternalTitle, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore }));
            mso.WriteF32(InternalHealth);
            mso.WriteS32V(InternalColor);
            mso.WriteS32V(InternalDivision);
            mso.WriteU8(InternalFlags);
            client.Send(mso.Get());
        }
        public void Unsubscribe(Client client)
        {
            lock (Subscribers)
            {
                if (!Subscribers.Remove(client.Login.Id)) return;
            }
            lock (client.Bossbars)
            {
                client.Bossbars.Remove(Id);
            }
            using ContentStream mso = new();
            mso.WriteS32V(11);
            mso.WriteGuid(Id);
            mso.WriteS32V(1);
            client.Send(mso.Get());
        }
        public void UnsubscribeQuietly(Client client)
        {
            lock (Subscribers)
            {
                if (!Subscribers.Remove(client.Login.Id)) return;
            }
            lock (client.Bossbars)
            {
                client.Bossbars.Remove(Id);
            }
        }
    }
}
