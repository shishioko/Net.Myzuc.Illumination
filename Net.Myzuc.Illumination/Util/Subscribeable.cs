using System;
using System.Collections.Generic;

namespace Net.Myzuc.Illumination.Util
{
    public abstract class Subscribeable<SubscriberType> where SubscriberType : IIdentifiable
    {
        private readonly Dictionary<Guid, SubscriberType> Subscribers = new();
        internal Subscribeable()
        {

        }
        public void Iterate(Action<SubscriberType> action)
        {
            lock (Subscribers)
            {
                foreach (SubscriberType subscriber in Subscribers.Values)
                {
                    action(subscriber);
                }
            }
        }
        public virtual void Subscribe(SubscriberType subscriber)
        {
            lock (Subscribers)
            {
                Subscribers.Add(subscriber.Id, subscriber);
            }
        }
        public virtual void Unsubscribe(SubscriberType subscriber)
        {
            lock (Subscribers)
            {
                Subscribers.Remove(subscriber.Id);
            }
        }
        internal virtual void UnsubscribeQuietly(SubscriberType subscriber)
        {
            lock (Subscribers)
            {
                Subscribers.Remove(subscriber.Id);
            }
        }
    }
}
