using Net.Myzuc.Illumination.Base;
using Net.Myzuc.Illumination.Chat;
using Net.Myzuc.Illumination.Content.Structs;
using Net.Myzuc.Illumination.Net;
using Net.Myzuc.Illumination.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Net.Myzuc.Illumination.Content
{
    public sealed class TablistEntry : Subscribeable<Tablist>, IUpdateable, IIdentifiable
    {
        public object Lock { get; } = new();
        public Guid Id { get; }
        public string Name { get; }
        public IReadOnlyCollection<Property> Properties { get; }
        public Updateable<Gamemode> Gamemode { get; }
        public Updateable<bool> Visible { get; }
        public Updateable<int> Latency { get; }
        public Updateable<ChatComponent?> Display { get; }
        public TablistEntry(Guid id, string name, ReadOnlyCollection<Property>? properties = null)
        {
            Id = id;
            Name = name;
            Properties = properties ?? new List<Property>().AsReadOnly();
            Gamemode = new(Structs.Gamemode.Survival, Lock);
            Visible = new(true, Lock);
            Latency = new(-1, Lock);
            Display = new(null, Lock);
        }
        public void Update()
        {
            lock (Lock)
            {
                if (!Gamemode.Updated && !Latency.Updated && !Display.Updated) return;
                using ContentStream mso = new();
                mso.WriteS32V(58);
                mso.WriteU8((byte)((Gamemode.Updated ? 4 : 0) | (Visible.Updated ? 8 : 0) | (Latency.Updated ? 16 : 0) | (Display.Updated ? 32 : 0)));
                mso.WriteS32V(1);
                mso.WriteGuid(Id);
                if (Gamemode.Updated)
                {
                    mso.WriteS32V((int)Gamemode.PostUpdate);
                    Gamemode.Update();
                }
                if (Visible.Updated)
                {
                    mso.WriteBool(Visible.PostUpdate);
                    Visible.Update();
                }
                if (Latency.Updated)
                {
                    mso.WriteS32V((int)Latency.PostUpdate);
                    Latency.Update();
                }
                if (Display.Updated)
                {
                    mso.WriteBool(Display.PostUpdate is not null);
                    if (Display.PostUpdate is not null)
                    {
                        mso.WriteString32V(JsonConvert.SerializeObject(Display.PostUpdate, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore }));
                    }
                    Display.Update();
                }
                byte[] msop = mso.Get().ToArray();
                Iterate((Tablist tablist) =>
                {
                    tablist.Iterate((Client client) =>
                    {
                        client.Send(msop);
                    });
                });
            }
        }
        public override void Subscribe(Tablist tablist)
        {
            base.Subscribe(tablist);
            lock (tablist.Entries)
            {
                tablist.Entries.Add(Id, this);
            }
            using ContentStream mso = new();
            mso.WriteS32V(58);
            mso.WriteU8(61);
            mso.WriteS32V(1);
            mso.WriteGuid(Id);
            lock (Lock)
            {
                mso.WriteString32V(Name);
                mso.WriteS32V(Properties.Count);
                foreach (Property property in Properties)
                {
                    mso.WriteString32V(property.Name);
                    mso.WriteString32V(property.Value);
                    mso.WriteString32VN(property.Signature);
                }
                mso.WriteS32V((int)Gamemode.PreUpdate);
                mso.WriteBool(true);
                mso.WriteS32V(Latency.PreUpdate);
                mso.WriteBool(Display.PreUpdate is not null);
                if (Display.PreUpdate is not null)
                {
                    mso.WriteString32V(JsonConvert.SerializeObject(Display.PreUpdate, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore }));
                }
            }
            byte[] msop = mso.Get().ToArray();
            tablist.Iterate((Client client) =>
            {
                client.Send(msop);
            });
        }
        public override void Unsubscribe(Tablist tablist)
        {
            base.Unsubscribe(tablist);
            lock (tablist.Entries)
            {
                tablist.Entries.Remove(Id);
            }
            using ContentStream mso = new();
            mso.WriteS32V(57);
            mso.WriteS32V(1);
            mso.WriteGuid(Id);
            byte[] msop = mso.Get().ToArray();
            tablist.Iterate((Client client) =>
            {
                client.Send(msop);
            });
        }
        internal override void UnsubscribeQuietly(Tablist tablist)
        {
            base.UnsubscribeQuietly(tablist);
            lock (tablist.Entries)
            {
                tablist.Entries.Remove(Id);
            }
        }
    }
}
