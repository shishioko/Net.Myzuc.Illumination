using Net.Myzuc.Illumination.Chat;
using Net.Myzuc.Illumination.Content;
using Net.Myzuc.Illumination.Content.Entities;
using Net.Myzuc.Illumination.Content.Structs;
using Net.Myzuc.Illumination.Net;
using Net.Myzuc.Illumination.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;

namespace Net.Myzuc.Illumination.Base
{
    public sealed class Client : IDisposable, IIdentifiable
    {
        public event Action Disposed;
        public Guid Id { get; }
        public string Name { get; }
        public bool Encrypted { get; }
        public ReadOnlyCollection<Property> Properties { get; }

        public bool Hardcore { get; set; }
        public byte Gamemode { get; set; } //TODO: updateable
        public bool ReducedDebugInfo { get; set; }
        internal readonly Dictionary<int, Entity> SubscribedEntities;
        internal Dictionary<(int x, int z), Chunk> Chunks { get; }
        internal Dictionary<Guid, Bossbar> Bossbars { get; }
        internal Dimension? Dimension { get; set; }
        internal Border? Border { get; set; }
        internal Tablist? Tablist { get; set; }

        private Connection Connection { get; }
        private ConcurrentQueue<byte[]> PacketQueue { get; }
        private SemaphoreSlim PacketSemaphore { get; }
        public DateTime LastKeepAlive { get; private set; }
        internal Client(LoginRequest login)
        {
            Disposed = () => { };
            Id = login.Id;
            Name = login.Name;
            Encrypted = login.Encrypted;
            Properties = login.Properties;

            SubscribedEntities = new();
            Chunks = new();
            Bossbars = new();

            Connection = login.Connection;
            PacketQueue = new();
            PacketSemaphore = new(1, 1);
            Connection.Disposed += Dispose;
            LastKeepAlive = DateTime.Now;
        }
        public void Message(ChatComponent chat, bool overlay = false)
        {
            using ContentStream mso = new();
            mso.WriteS32V(100);
            mso.WriteString32V(JsonConvert.SerializeObject(chat, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore }));
            mso.WriteBool(overlay);
            Send(mso.Get());
        }
        public void Disconnect(ChatComponent chat, TimeSpan? delay = null)
        {
            using ContentStream mso = new();
            mso.WriteS32V(26);
            mso.WriteString32V(JsonConvert.SerializeObject(chat, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore }));
            Send(mso.Get());
            Thread.Sleep(delay ?? TimeSpan.FromSeconds(1));
            Dispose();
        }
        public void Dispose()
        {
            if (Connection.IsDisposed) return;
            Connection.Disposed -= Dispose;
            lock (SubscribedEntities)
            {
                while (SubscribedEntities.Count > 0)
                {
                    KeyValuePair<int,Entity> kvp = SubscribedEntities.First();
                    kvp.Value.UnsubscribeQuietly(this);
                }
            }
            lock (Chunks)
            {
                while (Chunks.Count > 0)
                {
                    Chunk chunk = Chunks.Values.First();
                    chunk.UnsubscribeQuietly(this);
                }
            }
            lock (Bossbars)
            {
                while (Bossbars.Count > 0)
                {
                    Bossbar bossbar = Bossbars.Values.First();
                    bossbar.UnsubscribeQuietly(this);
                }
            }
            Dimension?.Unsubscribe(this);
            Border?.UnsubscribeQuietly(this);
            Tablist?.UnsubscribeQuietly(this);
            PacketSemaphore.Dispose();
            Connection.Dispose();
            Disposed();
        }
        internal void Send(Span<byte> data)
        {
            if (Connection.IsDisposed) return;
            PacketQueue.Enqueue(data.ToArray());
            if (PacketSemaphore.Wait(0)) ThreadPool.QueueUserWorkItem(ProcessOutgoingPackets);
        }
        internal void KeepAlive(object? _)
        {
            while (!Connection.IsDisposed)
            {
                using ContentStream mso = new();
                mso.WriteS32V(35);
                mso.WriteS64(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                Send(mso.Get());
                TimeSpan span = LastKeepAlive + TimeSpan.FromSeconds(15) - DateTime.Now;
                if (span.Ticks > 0) Thread.Sleep(span);
                if (DateTime.Now < LastKeepAlive + TimeSpan.FromSeconds(30)) continue;
                Disconnect(new ChatText("Timed out!"));
            }
        }
        internal void ProcessIncomingPackets()
        {
            try
            {
                while (!Connection.IsDisposed)
                {
                    Span<byte> data = Connection.Receive();
                    ContentStream msi = new(data);
                    switch (msi.ReadS32V())
                    {
                        default:
                            {
                                break;
                            }
                        case 18:
                            {
                                LastKeepAlive = DateTime.Now;
                                break;
                            }
                    }
                }
            }
            catch (Exception ex)
            {
                Connection.Throw(ex);
            }
        }
        private void ProcessOutgoingPackets(object? _)
        {
            try
            {
                while (PacketQueue.TryDequeue(out byte[]? data))
                {
                    Connection.Send(data);
                }
                PacketSemaphore.Release();
            }
            catch (Exception ex)
            {
                Connection.Throw(ex);
            }
        }
    }
}
