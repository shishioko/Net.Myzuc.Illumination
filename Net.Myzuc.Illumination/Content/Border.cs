using Net.Myzuc.Illumination.Net;
using Net.Myzuc.Illumination.Util;
using System;

namespace Net.Myzuc.Illumination.Content
{
    public sealed class Border : Subscribeable<Client>, IUpdateable
    {
        private readonly Object Lock = new();
        public Updateable<double> X { get; }
        public Updateable<double> Z { get; }
        public Updateable<double> Diameter { get; }
        public Updateable<double> TargetDiameter { get; }
        public Updateable<TimeSpan> TargetTime { get; }
        public Updateable<int> WarningDistance { get; }
        public Updateable<int> WarningTime { get; }
        public Border()
        {
            X = new(0.0d, Lock);
            Z = new(0.0d, Lock);
            Diameter  = new(double.MaxValue, Lock);
            TargetDiameter = new(double.MaxValue, Lock);
            TargetTime = new(TimeSpan.Zero, Lock);
            WarningDistance = new(0, Lock);
            WarningTime = new(0, Lock);
        }
        public void Update()
        {
            lock (Lock)
            {
                if (X.Updated || Z.Updated)
                {
                    using ContentStream mso = new();
                    mso.WriteS32V(71);
                    mso.WriteF64(X.PostUpdate);
                    mso.WriteF64(Z.PostUpdate);
                    X.Update();
                    Z.Update();
                    byte[] msop = mso.Get().ToArray();
                    Iterate((Client client) =>
                    {
                        client.Send(msop);
                    });
                }
                if ((TargetDiameter.Updated || TargetTime.Updated) && Diameter.PostUpdate != TargetDiameter.PostUpdate && TargetTime.PostUpdate > TimeSpan.Zero)
                {
                    using ContentStream mso = new();
                    mso.WriteS32V(72);
                    mso.WriteF64(Diameter.PostUpdate);
                    mso.WriteF64(TargetDiameter.PostUpdate);
                    mso.WriteS64V((long)TargetTime.PostUpdate.TotalMilliseconds);
                    Diameter.Update();
                    TargetDiameter.Update();
                    TargetTime.Update();
                    byte[] msop = mso.Get().ToArray();
                    Iterate((Client client) =>
                    {
                        client.Send(msop);
                    });
                }
                else if(Diameter.Updated)
                {
                    using ContentStream mso = new();
                    mso.WriteS32V(73);
                    mso.WriteF64(Diameter.PostUpdate);
                    Diameter.Update();
                    byte[] msop = mso.Get().ToArray();
                    Iterate((Client client) =>
                    {
                        client.Send(msop);
                    });
                }
                if (WarningDistance.Updated)
                {
                    using ContentStream mso = new();
                    mso.WriteS32V(75);
                    mso.WriteS32V(WarningDistance.PostUpdate);
                    WarningDistance.Update();
                    byte[] msop = mso.Get().ToArray();
                    Iterate((Client client) =>
                    {
                        client.Send(msop);
                    });
                }
                if (WarningTime.Updated)
                {
                    using ContentStream mso = new();
                    mso.WriteS32V(74);
                    mso.WriteS32V(WarningTime.PostUpdate);
                    WarningTime.Update();
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
            client.Border = this;
            using ContentStream mso = new();
            mso.WriteS32V(34);
            lock (Lock)
            {
                mso.WriteF64(X.PreUpdate);
                mso.WriteF64(Z.PreUpdate);
                mso.WriteF64(Diameter.PreUpdate);
                mso.WriteF64(TargetDiameter.PreUpdate);
                mso.WriteS64V((long)TargetTime.PreUpdate.TotalMilliseconds);
                mso.WriteS32V(int.MaxValue);
                mso.WriteS32V(WarningDistance.PreUpdate);
                mso.WriteS32V(WarningTime.PreUpdate);
            }
            client.Send(mso.Get());
        }
        public override void Unsubscribe(Client client)
        {
            base.Unsubscribe(client);
            client.Border = null;
            using ContentStream mso = new();
            mso.WriteS32V(34);
            mso.WriteF64(0.0d);
            mso.WriteF64(0.0d);
            mso.WriteF64(double.MaxValue);
            mso.WriteF64(double.MaxValue);
            mso.WriteS64V(0L);
            mso.WriteS32V(0);
            mso.WriteS32V(0);
            mso.WriteS32V(0);
            client.Send(mso.Get());
        }
        internal override void UnsubscribeQuietly(Client client)
        {
            base.UnsubscribeQuietly(client);
            client.Border = null;
        }
    }
}
