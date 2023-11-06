using Net.Myzuc.Illumination.Content.Chat;
using Net.Myzuc.Illumination.Content.Entities;
using Net.Myzuc.Illumination.Content.Game;
using Net.Myzuc.Illumination.Content.World;
using Net.Myzuc.Illumination.Content.World.Chunks;
using Net.Myzuc.Illumination.Net;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Net.Myzuc.Illumination
{
    public sealed class Client : IDisposable
    {
        public event Action Disposed;
        public LoginRequest Login { get; }
        public DateTime LastKeepAlive { get; private set; }
        public bool Hardcore { get; set; }
        public byte Gamemode { get; set; } //TODO: updateable
        public bool ReducedDebugInfo { get; set; }
        internal readonly ConcurrentDictionary<Entity, int> SubscribedEntities;
        internal readonly ConcurrentDictionary<int, Guid> SubscribedEntityIds;
        internal Dictionary<(int x, int z), Chunk> Chunks { get; }
        internal Dictionary<Guid, Bossbar> Bossbars { get; }
        internal Dimension? Dimension { get; set; }
        internal Border? Border { get; set; }
        internal Tablist? Tablist { get; set; }
        private ConcurrentQueue<byte[]> PacketQueue { get; }
        private SemaphoreSlim PacketSemaphore { get; }
        internal Client(LoginRequest login)
        {
            Disposed = () => { };
            Login = login;
            LastKeepAlive = DateTime.Now;
            SubscribedEntities = new();
            SubscribedEntityIds = new();
            Chunks = new();
            Bossbars = new();
            PacketQueue = new();
            PacketSemaphore = new(1, 1);
            Login.Connection.Disposed += Dispose;
        }
        public void Message(ChatComponent chat, bool overlay = false)
        {
            using ContentStream mso = new();
            mso.WriteS32V(100);
            mso.WriteString32V(JsonConvert.SerializeObject(chat, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore }));
            mso.WriteBool(overlay);
            Send(mso.Get());
        }
        public void Disconnect(ChatComponent chat)
        {
            using ContentStream mso = new();
            mso.WriteS32V(26);
            mso.WriteString32V(JsonConvert.SerializeObject(chat, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore }));
            Send(mso.Get());
            Thread.Sleep(1000);
            Dispose();
        }
        public void Dispose()
        {
            if (Login.Connection.IsDisposed) return;
            Login.Connection.Disposed -= Dispose;
            while (!SubscribedEntities.IsEmpty)
            {
                Entity entity = SubscribedEntities.First().Key;
                entity.Subscribers.TryRemove(Login.Id, out _);
                SubscribedEntities.TryRemove(entity, out int eid);
                SubscribedEntityIds.TryRemove(eid, out _);
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
            Login.Connection.Dispose();
            Disposed();
        }
        internal void Send(Span<byte> data)
        {
            if (Login.Connection.IsDisposed) throw new ObjectDisposedException(nameof(Client));
            PacketQueue.Enqueue(data.ToArray());
            if (PacketSemaphore.Wait(0)) ThreadPool.QueueUserWorkItem(ProcessOutgoingPackets);
        }
        internal void KeepAlive(object? _)
        {
            while (!Login.Connection.IsDisposed)
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
                while (!Login.Connection.IsDisposed)
                {
                    Span<byte> data = Login.Connection.Receive();
                    ContentStream msi = new(data);
                    int d = msi.ReadS32V();
                    switch (d)
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
            catch(Exception ex)
            {
                Login.Connection.Throw(ex);
            }
        }
        private void ProcessOutgoingPackets(object? _)
        {
            try
            {
                while (PacketQueue.TryDequeue(out byte[]? data))
                {
                    Login.Connection.Send(data);
                }
                PacketSemaphore.Release();
            }
            catch(Exception ex)
            {
                Login.Connection.Throw(ex);
            }
        }
    }
}
