using System;
using Net.Myzuc.Illumination.Net;
using System.Net;
using System.IO;
using System.Net.Sockets;
using System.IO.Compression;
using Newtonsoft.Json;
using Net.Myzuc.Illumination.Content.Status;
using Net.Myzuc.Illumination.Chat;

namespace Net.Myzuc.Illumination
{
    public sealed class Connection : IDisposable
    {
        public event Func<ServerStatus?> Status;
        public event Action<LoginRequest> Login;
        public event Action<Exception> Error;
        public event Action Disposed;
        internal event Action<byte[]> Received;
        public IPEndPoint Endpoint { get; }
        public int Version { get; private set; }
        public string Address { get; private set; }
        public ushort Port { get; private set; }
        public bool IsDisposed { get; private set; }
        internal readonly ContentStream Stream;
        internal int CompressionThreshold;
        private bool Thrown;
        internal Connection(Socket socket)
        {
            Error = (_) => { };
            Status = () => null;
            Login = (_) => { };
            Disposed = () => { };
            Received = (_) => { };
            Endpoint = (socket.RemoteEndPoint as IPEndPoint)!;
            Version = 0;
            Address = string.Empty;
            Port = 0;
            IsDisposed = false;
            Stream = new(new NetworkStream(socket));
            CompressionThreshold = -1;
            Thrown = false;
        }
        public void Dispose()
        {
            if (IsDisposed) return;
            IsDisposed = true;
            Stream.Dispose();
            Disposed.Invoke();
        }
        internal void Start(object? _)
        {
            try
            {
                int next;
                using (ContentStream msi = new(Receive()))
                {
                    int id = msi.ReadS32V();
                    if (id != 0) throw new ProtocolViolationException($"Invalid handshake id '0x{id:X02}'!");
                    Version = msi.ReadS32V();
                    Address = msi.ReadString32V(255);
                    Port = msi.ReadU16();
                    next = msi.ReadS32V();
                }
                if (next == 1)
                {
                    using (ContentStream msi = new(Receive()))
                    {
                        int id = msi.ReadS32V();
                        if (id != 0) throw new ProtocolViolationException($"Invalid status id '0x{id:X02}'!");
                    }
                    string status = JsonConvert.SerializeObject(Status.Invoke() ?? new ServerStatus()
                    {
                        Description = new ChatText("No Description.")
                        {
                            Color = "red"
                        },
                        Version = new("Net.Myzuc.Illumination 1.19.4", -1)
                    }, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    using (ContentStream mso = new())
                    {
                        mso.WriteS32V(0);
                        mso.WriteString32V(status);
                        Send(mso.Get());
                    }
                    long value;
                    using (ContentStream msi = new(Receive()))
                    {
                        int id = msi.ReadS32V();
                        if (id != 1) throw new ProtocolViolationException($"Invalid ping id '0x{id:X02}'!");
                        value = msi.ReadS64();
                    }
                    using (ContentStream mso = new())
                    {
                        mso.WriteS32V(1);
                        mso.WriteS64(value);
                        Send(mso.Get());
                    }
                    Dispose();
                    return;
                }
                if (next == 2)
                {
                    LoginRequest request = new(this);
                    Login.Invoke(request);
                    request.Start();
                    Dispose();
                    return;
                }
                throw new ProtocolViolationException($"Invalid state id '0x{next:02}'!");
            }
            catch (Exception ex)
            {
                Throw(ex);
            }
        }
        internal void Throw(Exception ex)
        {
            if (IsDisposed) return;
            if (Thrown) return;
            Thrown = true;
            Error.Invoke(ex);
            Dispose();
        }
        internal void Send(ReadOnlySpan<byte> data)
        {
            try
            {
                if (IsDisposed) throw new ObjectDisposedException(nameof(Connection));
                if (data.Length < 1) return;
                if (CompressionThreshold < 0)
                {
                    Stream.WriteU8AV(data);
                }
                else
                {
                    if (data.Length < CompressionThreshold)
                    {
                        Stream.WriteS32V(data.Length + 1);
                        Stream.WriteU8(0);
                        Stream.WriteU8A(data);
                    }
                    else
                    {
                        using MemoryStream mso = new();
                        using (ZLibStream zlib = new(mso, CompressionLevel.Optimal, true)) zlib.Write(data);
                        byte[] compressed = mso.ToArray();
                        int extra = 0;
                        for (int value = data.Length; value != 0; value >>= 7) extra++;
                        Stream.WriteS32V(compressed.Length + extra);
                        Stream.WriteS32V(data.Length);
                        Stream.WriteU8A(compressed);
                    }
                }
            }
            catch(Exception ex)
            {
                Throw(ex);
            }
        }
        internal Span<byte> Receive()
        {
            Span<byte> data = Stream.ReadU8AV();
            if (CompressionThreshold < 0) return data;
            using MemoryStream ms = new(data.ToArray());
            using ContentStream msi = new(ms);
            int size = msi.ReadS32V(out int extra);
            if (size <= 0) return msi.ReadU8A(data.Length - extra);
            using ContentStream zlib = new(new ZLibStream(ms, CompressionMode.Decompress, false));
            return zlib.ReadU8A(size);
        }
    }
}
