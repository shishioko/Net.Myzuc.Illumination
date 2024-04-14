using Me.Shishioko.Illumination.Chat;
using Me.Shishioko.Illumination.Content.Structs;
using Me.Shishioko.Illumination.Net;
using Me.Shishioko.Illumination.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Me.Shishioko.Illumination.Base;

namespace Me.Shishioko.Illumination.Content
{
    public sealed class Tablist : Subscribeable<Client>, IIdentifiable, IUpdateable
    {
        public Guid Id { get; } = Guid.NewGuid();
        private object Lock { get; } = new();
        public Updateable<ChatComponent> Header { get; set; }
        public Updateable<ChatComponent> Footer { get; set; }
        internal Dictionary<Guid, TablistEntry> Entries { get; }
        public Tablist()
        {
            Header = new(new ChatText(string.Empty), Lock);
            Footer = new(new ChatText(string.Empty), Lock);
            Entries = new();
        }
        public void Update()
        {
            lock (Lock)
            {
                if (!Header.Updated && !Footer.Updated) return;
                using ContentStream mso = new();
                mso.WriteS32V(101);
                mso.WriteString32V(JsonConvert.SerializeObject(Header.PostUpdate, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore }));
                mso.WriteString32V(JsonConvert.SerializeObject(Footer.PostUpdate, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore }));
                byte[] msop = mso.Get().ToArray();
                Iterate((Client client) =>
                {
                    client.Send(msop);
                });
                Header.Update();
                Footer.Update();
            }
        }
        public override void Subscribe(Client client)
        {
            base.Subscribe(client);
            client.Tablist = this;
            using (ContentStream mso = new())
            {
                mso.WriteS32V(101);
                lock (Lock)
                {
                    mso.WriteString32V(JsonConvert.SerializeObject(Header.PreUpdate, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore }));
                    mso.WriteString32V(JsonConvert.SerializeObject(Footer.PreUpdate, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore }));
                }
                client.Send(mso.Get());
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
                        mso.WriteString32V(entry.Name);
                        mso.WriteS32V(entry.Properties.Count);
                        foreach (Property property in entry.Properties)
                        {
                            mso.WriteString32V(property.Name);
                            mso.WriteString32V(property.Value);
                            mso.WriteString32VN(property.Signature);
                        }
                        lock (entry.Lock)
                        {
                            mso.WriteS32V((int)entry.Gamemode.PreUpdate);
                            mso.WriteBool(true);
                            mso.WriteS32V(entry.Latency.PreUpdate);
                            mso.WriteBool(entry.Display.PreUpdate is not null);
                            if (entry.Display.PreUpdate is not null)
                            {
                                mso.WriteString32V(JsonConvert.SerializeObject(entry.Display.PreUpdate, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore }));
                            }
                        }
                    }
                }
                client.Send(mso.Get());
            }
        }
        public override void Unsubscribe(Client client)
        {

            using (ContentStream mso = new())
            {
                lock (Entries)
                {
                    base.Unsubscribe(client);
                    client.Tablist = null;
                    mso.WriteS32V(57);
                    mso.WriteS32V(Entries.Count);
                    foreach (TablistEntry entry in Entries.Values)
                    {
                        mso.WriteGuid(entry.Id);
                    }
                }
                client.Send(mso.Get());
            }
            using (ContentStream mso = new())
            {
                mso.WriteS32V(101);
                mso.WriteString32V("{\"text\":\"\"}");
                mso.WriteString32V("{\"text\":\"\"}");
                client.Send(mso.Get());
            }
        }
        internal override void UnsubscribeQuietly(Client client)
        {
            base.UnsubscribeQuietly(client);
            client.Tablist = null;
        }
    }
}
