using System;
using Me.Shishioko.Illumination.Net;
using Newtonsoft.Json;
using Me.Shishioko.Illumination.Chat;
using Me.Shishioko.Illumination.Util;
using Me.Shishioko.Illumination.Base;

namespace Me.Shishioko.Illumination.Content
{
    public sealed class Bossbar : Subscribeable<Client>, IUpdateable, IIdentifiable
    {
        public enum BossbarColor : int
        {
            Pink = 0,
            Blue = 1,
            Red = 2,
            Green = 3,
            Yellow = 4,
            Purple = 5,
            White = 6
        }
        public enum BossbarDivision : int
        {
            None = 0,
            Notches6 = 1,
            Notches10 = 2,
            Notches12 = 3,
            Notches20 = 4
        }
        public class BossbarFlags
        {
            internal byte Bitmap = 0;
            public bool DarkenSky
            {
                get
                {
                    return (Bitmap & 1) != 0;
                }
                set
                {
                    Bitmap = (byte)((Bitmap & 254) | (value ? 1 : 0));
                }
            }
            public bool EndMusic
            {
                get
                {
                    return (Bitmap & 2) != 0;
                }
                set
                {
                    Bitmap = (byte)((Bitmap & 253) | (value ? 2 : 0));
                }
            }
            public bool Fog
            {
                get
                {
                    return (Bitmap & 4) != 0;
                }
                set
                {
                    Bitmap = (byte)((Bitmap & 251) | (value ? 4 : 0));
                }
            }
            internal BossbarFlags()
            {

            }
        }
        private object Lock { get; } = new();
        public Guid Id { get; }
        public Updateable<ChatComponent> Title { get; }
        public Updateable<float> Health { get; }
        public Updateable<BossbarColor> Color { get; }
        public Updateable<BossbarDivision> Division { get; }
        public Updateable<BossbarFlags> Flags { get; }
        public Bossbar(Guid id)
        {
            Id = id;
            Health = new(0.0f, Lock);
            Title = new(new ChatText(string.Empty), Lock);
            Color = new(BossbarColor.Pink, Lock);
            Division = new(BossbarDivision.None, Lock);
            Flags = new(new BossbarFlags(), Lock);
        }
        public void Update()
        {
            lock (Lock)
            {
                if (Health.Updated)
                {
                    using ContentStream mso = new();
                    mso.WriteS32V(11);
                    mso.WriteGuid(Id);
                    mso.WriteS32V(2);
                    mso.WriteF32(Health.PostUpdate);
                    Health.Update();
                    byte[] msop = mso.Get().ToArray();
                    Iterate((Client client) =>
                    {
                        client.Send(msop);
                    });
                }
                if (Title.Updated)
                {
                    using ContentStream mso = new();
                    mso.WriteS32V(11);
                    mso.WriteGuid(Id);
                    mso.WriteS32V(3);
                    mso.WriteString32V(JsonConvert.SerializeObject(Title.PostUpdate, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore }));
                    Title.Update();
                    byte[] msop = mso.Get().ToArray();
                    Iterate((Client client) =>
                    {
                        client.Send(msop);
                    }); 
                }
                if (Color.Updated || Division.Updated)
                {
                    using ContentStream mso = new();
                    mso.WriteS32V(11);
                    mso.WriteGuid(Id);
                    mso.WriteS32V(4);
                    mso.WriteS32V((int)Color.PostUpdate);
                    mso.WriteS32V((int)Division.PostUpdate);
                    Color.Update();
                    Division.Update();
                    byte[] msop = mso.Get().ToArray();
                    Iterate((Client client) =>
                    {
                        client.Send(msop);
                    });
                }
                if (Flags.Updated)
                {
                    using ContentStream mso = new();
                    mso.WriteS32V(11);
                    mso.WriteGuid(Id);
                    mso.WriteS32V(5);
                    mso.WriteU8(Flags.PostUpdate.Bitmap);
                    Flags.Update();
                    byte[] msop = mso.Get().ToArray();
                    Iterate((Client client) =>
                    {
                        client.Send(msop);
                    });
                }
            }
        }
        public override void Subscribe(Client client)
        {
            base.Subscribe(client);
            lock (client.Bossbars)
            {
                client.Bossbars.Add(Id, this);
            }
            using ContentStream mso = new();
            mso.WriteS32V(11);
            mso.WriteGuid(Id);
            mso.WriteS32V(0);
            lock (Lock)
            {
                mso.WriteString32V(JsonConvert.SerializeObject(Title.PreUpdate, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore }));
                mso.WriteF32(Health.PreUpdate);
                mso.WriteS32V((int)Color.PreUpdate);
                mso.WriteS32V((int)Division.PreUpdate);
                mso.WriteU8(Flags.PreUpdate.Bitmap);
            }
            client.Send(mso.Get());
        }
        public override void Unsubscribe(Client client)
        {
            base.Unsubscribe(client);
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
        internal override void UnsubscribeQuietly(Client client)
        {
            base.UnsubscribeQuietly(client);
            lock (client.Bossbars)
            {
                client.Bossbars.Remove(Id);
            }
        }
    }
}
