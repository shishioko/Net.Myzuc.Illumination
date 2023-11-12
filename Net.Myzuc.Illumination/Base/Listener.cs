using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Net.Myzuc.Illumination.Base
{
    public sealed class Listener : IDisposable
    {
        public event Action<Connection> Accept;
        public event Action<Exception> Stop;
        public bool IsDisposed { get; private set; }
        public bool IsListening { get; private set; }
        private readonly Socket Socket;
        public Listener(IPEndPoint endpoint)
        {
            Accept = (_) => { };
            Stop = (_) => { };
            IsDisposed = true;
            Socket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Socket.Bind(endpoint);
        }
        public void Dispose()
        {
            if (IsDisposed) return;
            IsDisposed = true;
            IsListening = false;
            Socket.Dispose();
        }
        public void Start()
        {
            if (IsListening) throw new InvalidOperationException("Already listening!");
            try
            {
                IsListening = true;
                Socket.Listen();
                while (true)
                {
                    Connection client = new(Socket.Accept());
                    ThreadPool.QueueUserWorkItem(client.Start);
                    Accept.Invoke(client);
                }
            }
            catch (Exception ex)
            {
                Stop(ex);
            }
        }
    }
}
