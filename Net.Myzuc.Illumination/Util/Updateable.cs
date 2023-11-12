namespace Net.Myzuc.Illumination.Util
{
    public sealed class Updateable<T>
    {
        internal readonly object Lock;
        public T PostUpdate
        {
            get
            {
                lock (Lock)
                {
                    return InternalPostUpdate;
                }
            }
            set
            {
                lock (Lock)
                {
                    InternalPostUpdate = value;
                }
            }
        }
        public T PreUpdate
        {
            get
            {
                lock (Lock)
                {
                    return InternalPreUpdate;
                }
            }
        }
        internal bool Updated
        {
            get
            {
                lock (Lock)
                {
                    if (InternalPreUpdate is null) return InternalPostUpdate is not null;
                    if (!InternalPreUpdate.Equals(InternalPostUpdate)) return false;
                    return InternalPreUpdate.GetHashCode() == InternalPostUpdate.GetHashCode();
                }
            }
        }
        private T InternalPostUpdate;
        private T InternalPreUpdate;
        internal Updateable(T value, object? lockObject = null)
        {
            InternalPostUpdate = value;
            InternalPreUpdate = value;
            Lock = lockObject ?? new();
        }
        internal void Update()
        {
            lock (Lock)
            {
                InternalPreUpdate = InternalPostUpdate;
            }
        }
    }
}
