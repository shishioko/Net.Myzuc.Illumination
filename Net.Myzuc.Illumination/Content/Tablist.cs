using Net.Myzuc.Illumination.Chat;
using Net.Myzuc.Illumination.Content.Chat;
using Net.Myzuc.Illumination.Net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Net.Myzuc.Illumination.Content
{
    public sealed class Tablist
    {
        public ChatComponent Header
        {
            get
            {
                return InternalHeader;
            }
            set
            {
                InternalHeader = value;
                using ContentStream mso = new();
                mso.WriteS32V(101);
                mso.WriteString32V(JsonConvert.SerializeObject(InternalHeader, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore }));
                mso.WriteString32V(JsonConvert.SerializeObject(InternalFooter, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore }));
                Span<byte> span = mso.Get();
                lock (Subscribers)
                {
                    foreach (Client client in Subscribers.Values)
                    {
                        client.Send(span);
                    }
                }
            }
        }
        public ChatComponent Footer
        {
            get
            {
                return InternalFooter;
            }
            set
            {
                InternalFooter = value;
                using ContentStream mso = new();
                mso.WriteS32V(101);
                mso.WriteString32V(JsonConvert.SerializeObject(InternalHeader, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore }));
                mso.WriteString32V(JsonConvert.SerializeObject(InternalFooter, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore }));
                Span<byte> span = mso.Get();
                lock (Subscribers)
                {
                    foreach (Client client in Subscribers.Values)
                    {
                        client.Send(span);
                    }
                }
            }
        }
        private ChatComponent InternalHeader { get; set; }
        private ChatComponent InternalFooter { get; set; }
        internal Dictionary<Guid, TablistEntry> Entries { get; }
        internal Dictionary<Guid, Client> Subscribers { get; }
        public Tablist()
        {
            InternalHeader = new ChatText(string.Empty);
            InternalFooter = new ChatText(string.Empty);
            Entries = new();
            Subscribers = new();
        }
        public void Subscribe(Client client)
        {
            lock (Subscribers)
            {
                Subscribers.Add(client.Login.Id, client);
            }
            client.Tablist = this;
            using (ContentStream mso = new())
            {
                mso.WriteS32V(101);
                mso.WriteString32V(JsonConvert.SerializeObject(InternalHeader, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore }));
                mso.WriteString32V(JsonConvert.SerializeObject(InternalFooter, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore }));
                Span<byte> span = mso.Get();
                client.Send(span);
            }
            using (ContentStream mso = new())
            {
                mso.WriteS32V(58);
                mso.WriteU8(61);
                lock (Entries)
                {
                    mso.WriteS32V(Entries.Count);
                    foreach (TablistEntry entry in Entries.Values)
                    {
                        mso.WriteGuid(entry.Id);
                        mso.WriteString32V(entry.InternalName);
                        lock (entry.Properties)
                        {
                            mso.WriteS32V(entry.Properties.Count);
                            foreach ((string name, string value, string? signature) in entry.Properties)
                            {
                                mso.WriteString32V(name);
                                mso.WriteString32V(value);
                                mso.WriteString32VN(signature);
                            }
                        }
                        mso.WriteS32V(entry.InternalGamemode);
                        mso.WriteBool(entry.InternalVisible);
                        mso.WriteS32V(entry.InternalLatency);
                        ChatComponent? display = entry.InternalDisplay;
                        mso.WriteBool(display is not null);
                        if (display is not null)
                        {
                            mso.WriteString32V(JsonConvert.SerializeObject(display, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore }));
                        }
                        client.Send(mso.Get());
                    }
                }
            }
        }
        public void Unsubscribe(Client client)
        {
            lock (Subscribers)
            {
                if (!Subscribers.Remove(client.Login.Id)) return;
            }
            client.Tablist = null;
            using (ContentStream mso = new())
            {
                mso.WriteS32V(101);
                mso.WriteString32V("{\"text\":\"\"}");
                mso.WriteString32V("{\"text\":\"\"}");
                client.Send(mso.Get());
            }
            using (ContentStream mso = new())
            {
                mso.WriteS32V(57);
                lock (Entries)
                {
                    mso.WriteS32V(Entries.Count);
                    foreach (TablistEntry entry in Entries.Values)
                    {
                        mso.WriteGuid(entry.Id);
                    }
                }
                client.Send(mso.Get());
            }
        }
        public void UnsubscribeQuietly(Client client)
        {
            lock (Subscribers)
            {
                if (!Subscribers.Remove(client.Login.Id)) return;
            }
            client.Tablist = null;
        }
    }
}
