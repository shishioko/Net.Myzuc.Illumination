using Net.Myzuc.Illumination.Net;
using System;
using System.Collections.Generic;

namespace Net.Myzuc.Illumination.Content
{
    public sealed class Border
    {
        public double X
        {
            get
            {
                return InternalX;
            }
            set
            {
                InternalX = value;
                using ContentStream mso = new();
                mso.WriteS32V(71);
                mso.WriteF64(InternalX);
                mso.WriteF64(InternalZ);
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
        public double Z
        {
            get
            {
                return InternalZ;
            }
            set
            {
                InternalZ = value;
                using ContentStream mso = new();
                mso.WriteS32V(71);
                mso.WriteF64(InternalX);
                mso.WriteF64(InternalZ);
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
        public double Diameter
        {
            get
            {
                return InternalDiameter;
            }
            set
            {
                InternalDiameter = value;
                if (InternalDiameter == InternalTargetDiameter || InternalTargetTime < DateTime.Now)
                {
                    using ContentStream mso = new();
                    mso.WriteS32V(73);
                    mso.WriteF64(InternalDiameter);
                    Span<byte> span = mso.Get();
                    lock (Subscribers)
                    {
                        foreach (Client client in Subscribers.Values)
                        {
                            client.Send(span);
                        }
                    }
                }
                else
                {
                    using ContentStream mso = new();
                    mso.WriteS32V(72);
                    mso.WriteF64(InternalDiameter);
                    mso.WriteF64(InternalTargetDiameter);
                    long ms = (long)(InternalTargetTime - DateTime.Now).TotalMilliseconds;
                    mso.WriteS64V(ms < 0 ? 0 : ms);
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
        }
        public double TargetDiameter
        {
            get
            {
                return InternalTargetDiameter;
            }
            set
            {
                InternalTargetDiameter = value;
                if (InternalDiameter == InternalTargetDiameter || InternalTargetTime < DateTime.Now)
                {
                    using ContentStream mso = new();
                    mso.WriteS32V(73);
                    mso.WriteF64(InternalDiameter);
                    Span<byte> span = mso.Get();
                    lock (Subscribers)
                    {
                        foreach (Client client in Subscribers.Values)
                        {
                            client.Send(span);
                        }
                    }
                }
                else
                {
                    using ContentStream mso = new();
                    mso.WriteS32V(72);
                    mso.WriteF64(InternalDiameter);
                    mso.WriteF64(InternalTargetDiameter);
                    long ms = (long)(InternalTargetTime - DateTime.Now).TotalMilliseconds;
                    mso.WriteS64V(ms < 0 ? 0 : ms);
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
        }
        public DateTime TargetTime
        {
            get
            {
                return InternalTargetTime;
            }
            set
            {
                InternalTargetTime = value;
                if (InternalDiameter == InternalTargetDiameter || InternalTargetTime < DateTime.Now)
                {
                    using ContentStream mso = new();
                    mso.WriteS32V(73);
                    mso.WriteF64(InternalDiameter);
                    Span<byte> span = mso.Get();
                    lock (Subscribers)
                    {
                        foreach (Client client in Subscribers.Values)
                        {
                            client.Send(span);
                        }
                    }
                }
                else
                {
                    using ContentStream mso = new();
                    mso.WriteS32V(72);
                    mso.WriteF64(InternalDiameter);
                    mso.WriteF64(InternalTargetDiameter);
                    long ms = (long)(InternalTargetTime - DateTime.Now).TotalMilliseconds;
                    mso.WriteS64V(ms < 0 ? 0 : ms); //TODO: fix
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
        }
        public int WarningDistance
        {
            get
            {
                return InternalWarningDistance;
            }
            set
            {
                InternalWarningDistance = value;
                using ContentStream mso = new();
                mso.WriteS32V(75);
                mso.WriteS32V(InternalWarningDistance);
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
        public int WarningTime
        {
            get
            {
                return InternalWarningTime;
            }
            set
            {
                InternalWarningTime = value;
                using ContentStream mso = new();
                mso.WriteS32V(74);
                mso.WriteS32V(InternalWarningTime);
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
        private double InternalX { get; set; }
        private double InternalZ { get; set; }
        private double InternalDiameter { get; set; }
        private double InternalTargetDiameter { get; set; }
        private DateTime InternalTargetTime { get; set; }
        private int InternalWarningDistance { get; set; }
        private int InternalWarningTime { get; set; }
        private Dictionary<Guid, Client> Subscribers { get; }
        public Border()
        {
            InternalX = 0;
            InternalZ = 0;
            InternalDiameter = double.MaxValue;
            InternalTargetDiameter = double.MaxValue;
            InternalTargetTime = DateTime.UnixEpoch;
            InternalWarningDistance = 0;
            InternalWarningTime = 0;
            Subscribers = new();
        }
        public void Subscribe(Client client)
        {
            lock (Subscribers)
            {
                Subscribers.Add(client.Login.Id, client);
            }
            client.Border = this;
            using ContentStream mso = new();
            mso.WriteS32V(34);
            mso.WriteF64(InternalX);
            mso.WriteF64(InternalZ);
            mso.WriteF64(InternalDiameter);
            mso.WriteF64(InternalTargetDiameter);
            long ms = (long)(InternalTargetTime - DateTime.Now).TotalMilliseconds;
            mso.WriteS64V(ms < 0 ? 0 : ms);
            mso.WriteS32V(int.MaxValue);
            mso.WriteS32V(InternalWarningDistance);
            mso.WriteS32V(InternalWarningTime);
            client.Send(mso.Get());
        }
        public void Unsubscribe(Client client)
        {
            lock (Subscribers)
            {
                if (!Subscribers.Remove(client.Login.Id)) return;
            }
            client.Border = null;
            using ContentStream mso = new();
            mso.WriteS32V(34);
            mso.WriteF64(0);
            mso.WriteF64(0);
            mso.WriteF64(double.MaxValue);
            mso.WriteF64(double.MaxValue);
            mso.WriteS64V(0);
            mso.WriteS32V(0);
            mso.WriteS32V(0);
            mso.WriteS32V(0);
            client.Send(mso.Get());
        }
        public void UnsubscribeQuietly(Client client)
        {
            lock (Subscribers)
            {
                if (!Subscribers.Remove(client.Login.Id)) return;
            }
            client.Border = null;
        }
    }
}
