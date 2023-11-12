using Net.Myzuc.Illumination.Content.Chat;
using Net.Myzuc.Illumination.Net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Net.Myzuc.Illumination.Content
{
    public sealed class TablistEntry
    {
        public Guid Id { get; }


        public string Name
        {
            get
            {
                return InternalName;
            }
            set
            {
                InternalName = value;
                using ContentStream mso = new();
                mso.WriteS32V(58);
                mso.WriteU8(1);
                mso.WriteS32V(1);
                mso.WriteGuid(Id);
                mso.WriteString32V(InternalName);
                lock (Properties)
                {
                    mso.WriteS32V(Properties.Count);
                    foreach ((string name, string pvalue, string? signature) in Properties)
                    {
                        mso.WriteString32V(name);
                        mso.WriteString32V(pvalue);
                        mso.WriteString32VN(signature);
                    }
                }
                Span<byte> span = mso.Get();
                lock (Subscribers)
                {
                    foreach (Tablist subscriber in Subscribers)
                    {
                        lock (subscriber.Subscribers)
                        {
                            foreach (Client client in subscriber.Subscribers.Values)
                            {
                                client.Send(span);
                            }
                        }
                    }
                }
            }
        }
        public List<(string name, string value, string? signature)> Properties { get; set; }
        public byte Gamemode
        {
            get
            {
                return InternalGamemode;
            }
            set
            {
                InternalGamemode = value;
                using ContentStream mso = new();
                mso.WriteS32V(58);
                mso.WriteU8(4);
                mso.WriteS32V(1);
                mso.WriteGuid(Id);
                mso.WriteS32V(InternalGamemode);
                Span<byte> span = mso.Get();
                lock (Subscribers)
                {
                    foreach (Tablist subscriber in Subscribers)
                    {
                        lock (subscriber.Subscribers)
                        {
                            foreach (Client client in subscriber.Subscribers.Values)
                            {
                                client.Send(span);
                            }
                        }
                    }
                }
            }
        }
        public bool Visible
        {
            get
            {
                return InternalVisible;
            }
            set
            {
                InternalVisible = value;
                using ContentStream mso = new();
                mso.WriteS32V(58);
                mso.WriteU8(8);
                mso.WriteS32V(1);
                mso.WriteGuid(Id);
                mso.WriteBool(InternalVisible);
                Span<byte> span = mso.Get();
                lock (Subscribers)
                {
                    foreach (Tablist subscriber in Subscribers)
                    {
                        lock (subscriber.Subscribers)
                        {
                            foreach (Client client in subscriber.Subscribers.Values)
                            {
                                client.Send(span);
                            }
                        }
                    }
                }
            }
        }
        public int Latency
        {
            get
            {
                return InternalLatency;
            }
            set
            {
                InternalLatency = value;
                using ContentStream mso = new();
                mso.WriteS32V(58);
                mso.WriteU8(16);
                mso.WriteS32V(1);
                mso.WriteGuid(Id);
                mso.WriteS32V(InternalLatency);
                Span<byte> span = mso.Get();
                lock (Subscribers)
                {
                    foreach (Tablist subscriber in Subscribers)
                    {
                        lock (subscriber.Subscribers)
                        {
                            foreach (Client client in subscriber.Subscribers.Values)
                            {
                                client.Send(span);
                            }
                        }
                    }
                }
            }
        }
        public ChatComponent? Display
        {
            get
            {
                return InternalDisplay;
            }
            set
            {
                InternalDisplay = value;
                using ContentStream mso = new();
                mso.WriteS32V(58);
                mso.WriteU8(32);
                mso.WriteS32V(1);
                mso.WriteGuid(Id);
                ChatComponent? display = InternalDisplay;
                mso.WriteBool(display is not null);
                if (display is not null)
                {
                    mso.WriteString32V(JsonConvert.SerializeObject(display, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore }));
                }
                Span<byte> span = mso.Get();
                lock (Subscribers)
                {
                    foreach (Tablist subscriber in Subscribers)
                    {
                        lock (subscriber.Subscribers)
                        {
                            foreach (Client client in subscriber.Subscribers.Values)
                            {
                                client.Send(span);
                            }
                        }
                    }
                }
            }
        }
        internal string InternalName { get; set; }
        internal byte InternalGamemode { get; set; }
        internal bool InternalVisible { get; set; }
        internal int InternalLatency { get; set; }
        internal ChatComponent? InternalDisplay { get; set; }
        internal List<Tablist> Subscribers { get; }
        public TablistEntry(Guid id)
        {
            Id = id;
            InternalName = string.Empty;
            Properties = new();
            InternalGamemode = 0;
            InternalVisible = true;
            InternalLatency = 0;
            InternalDisplay = null;
            Subscribers = new();
        }
        public void Subscribe(Tablist subscriber)
        {
            lock (Subscribers)
            {
                Subscribers.Add(subscriber);
            }
            lock (subscriber.Entries)
            {
                subscriber.Entries.Add(Id, this);
            }
            using ContentStream mso = new();
            mso.WriteS32V(58);
            mso.WriteU8(61);
            mso.WriteS32V(1);
            mso.WriteGuid(Id);
            mso.WriteString32V(InternalName);
            lock (Properties)
            {
                mso.WriteS32V(Properties.Count);
                foreach ((string name, string value, string? signature) in Properties)
                {
                    mso.WriteString32V(name);
                    mso.WriteString32V(value);
                    mso.WriteString32VN(signature);
                }
            }
            mso.WriteS32V(InternalGamemode);
            mso.WriteBool(InternalVisible);
            mso.WriteS32V(InternalLatency);
            ChatComponent? display = InternalDisplay;
            mso.WriteBool(display is not null);
            if (display is not null)
            {
                mso.WriteString32V(JsonConvert.SerializeObject(display, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore }));
            }
            Span<byte> span = mso.Get();
            lock (subscriber.Subscribers)
            {
                foreach (Client client in subscriber.Subscribers.Values)
                {
                    client.Send(span);
                }
            }
        }
        public void Unsubscribe(Tablist subscriber)
        {
            lock (Subscribers)
            {
                if (!Subscribers.Remove(subscriber)) return;
            }
            lock (subscriber.Entries)
            {
                subscriber.Entries.Remove(Id);
            }
            using ContentStream mso = new();
            mso.WriteS32V(57);
            mso.WriteS32V(1);
            mso.WriteGuid(Id);
            Span<byte> span = mso.Get();
            lock (subscriber.Subscribers)
            {
                foreach (Client client in subscriber.Subscribers.Values)
                {
                    client.Send(span);
                }
            }
        }
        public void UnsubscribeQuietly(Tablist subscriber)
        {
            lock (Subscribers)
            {
                if (!Subscribers.Remove(subscriber)) return;
            }
            lock (subscriber.Entries)
            {
                subscriber.Entries.Remove(Id);
            }
        }
        public void RefreshProperties()
        {
            using ContentStream mso = new();
            mso.WriteS32V(58);
            mso.WriteU8(1);
            mso.WriteS32V(1);
            mso.WriteGuid(Id);
            mso.WriteString32V(InternalName);
            lock (Properties)
            {
                mso.WriteS32V(Properties.Count);
                foreach ((string name, string pvalue, string? signature) in Properties)
                {
                    mso.WriteString32V(name);
                    mso.WriteString32V(pvalue);
                    mso.WriteString32VN(signature);
                }
            }
            Span<byte> span = mso.Get();
            lock (Subscribers)
            {
                foreach (Tablist subscriber in Subscribers)
                {
                    lock (subscriber.Subscribers)
                    {
                        foreach (Client client in subscriber.Subscribers.Values)
                        {
                            client.Send(span);
                        }
                    }
                }
            }
        }
    }
}
