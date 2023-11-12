using Net.Myzuc.Illumination.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using System.Text.Json;
using System.Threading;
using Net.Myzuc.Illumination.Chat;
using System.Collections.ObjectModel;
using Net.Myzuc.Illumination.Util;

namespace Net.Myzuc.Illumination.Base
{
    public sealed class LoginRequest
    {
        public event Func<bool> Encryption = () => false;
        public event Func<int> Compression = () => 256;
        public event Action<JsonElement> Failure = (_) => { };
        public event Func<(Guid id, string name, ReadOnlyCollection<Property> properties)> PreSuccess;
        public event Action<Client> PostSuccess = (_) => { };
        public Connection Connection { get; }
        public string Name { get; private set; } = string.Empty;
        public Guid Id { get; private set; } = Guid.Empty;
        public bool Encrypted { get; private set; } = false;
        public ReadOnlyCollection<Property> Properties { get; private set; } = new List<Property>().AsReadOnly();
        private bool Done = false;
        internal LoginRequest(Connection connection)
        {
            PreSuccess = () => (Id, Name, Properties);
            Connection = connection;
        }
        public void Disconnect(ChatComponent chat)
        {
            if (Done) Connection.Dispose();
            Done = true;
            using ContentStream mso = new();
            mso.WriteS32V(0);
            mso.WriteString32V(JsonConvert.SerializeObject(chat, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore }));
            Connection.Send(mso.Get());
            Thread.Sleep(1000);
            Connection.Dispose();
        }
        internal void Start()
        {
            using (ContentStream msi = new(Connection.Receive()))
            {
                int id = msi.ReadS32V();
                if (id != 0) throw new ProtocolViolationException($"Invalid login start '0x{id:X02}'!");
                Name = msi.ReadString32V(16);
                if (msi.ReadBool()) Id = msi.ReadGuid();
            }
            if (Encryption.Invoke())
            {
                if (Id == Guid.Empty) throw new ProtocolViolationException("No guid provided!");
                string server = Convert.ToBase64String(RandomNumberGenerator.GetBytes(15));
                byte[] verify = RandomNumberGenerator.GetBytes(4);
                using RSA rsa = RSA.Create();
                byte[] key = rsa.ExportSubjectPublicKeyInfo();
                using ContentStream writer = new();
                writer.WriteS32V(1);
                writer.WriteString32V(server);
                writer.WriteU8AV(key);
                writer.WriteU8AV(verify);
                Connection.Send(writer.Get());
                using ContentStream msi = new(Connection.Receive());
                int id = msi.ReadS32V();
                if (id != 1) throw new ProtocolViolationException($"Invalid encryption response '0x{id:X02}'!");
                byte[] secret = rsa.Decrypt(msi.ReadU8AV().ToArray(), RSAEncryptionPadding.Pkcs1);
                if (!rsa.Decrypt(msi.ReadU8AV().ToArray(), RSAEncryptionPadding.Pkcs1).SequenceEqual(verify)) throw new ProtocolViolationException("Encryption failure!");
                if (FastAesStream.Supported) Connection.Stream.Stream = new FastAesStream(Connection.Stream.Stream, secret);
                else Connection.Stream.Stream = new AesStream(Connection.Stream.Stream, secret);
                BigInteger number = new(SHA1.HashData(Encoding.ASCII.GetBytes(server).Concat(secret).Concat(key).ToArray()).Reverse().ToArray());
                string url = $"https://sessionserver.mojang.com/session/minecraft/hasJoined?username={Name}&serverId={(number < 0 ? "-" + (-number).ToString("x") : number.ToString("x"))}";
                using HttpClient http = new();
                using HttpRequestMessage request = new(HttpMethod.Get, url);
                using HttpResponseMessage response = http.Send(request);
                using Stream stream = response.Content.ReadAsStream();
                using JsonDocument auth = JsonDocument.Parse(stream);
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    Failure.Invoke(auth.RootElement);
                    Connection.Dispose();
                    return;
                }
                Name = auth.RootElement.GetProperty("name").GetString()!;
                Id = Guid.ParseExact(auth.RootElement.GetProperty("id").GetString()!, "N");
                List<Property> properties = new();
                foreach (JsonElement property in auth.RootElement.GetProperty("properties")!.EnumerateArray())
                {
                    properties.Add(new(property.GetProperty("name").GetString()!, property.GetProperty("value").GetString()!, property.GetProperty("signature").GetString()));
                }
                Properties = properties.AsReadOnly();
            }
            else
            {
                byte[] hash = MD5.HashData(Encoding.UTF8.GetBytes($"OfflinePlayer:{Name}"));
                hash[6] &= 0x0f;
                hash[6] |= 0x30;
                hash[8] &= 0x3f;
                hash[8] |= 0x80;
                Id = Guid.ParseExact(string.Concat(hash.Select(b => b.ToString("x2"))), "N");
            }
            int compression = Compression.Invoke();
            using (ContentStream mso = new())
            {
                mso.WriteS32V(3);
                mso.WriteS32V(compression);
                Connection.Send(mso.Get());
            }
            Connection.CompressionThreshold = compression;
            using (ContentStream mso = new())
            {
                mso.WriteS32V(2);
                mso.WriteGuid(Id);
                mso.WriteString32V(Name);
                mso.WriteS32V(Properties.Count);
                foreach (Property property in Properties)
                {
                    mso.WriteString32V(property.Name);
                    mso.WriteString32V(property.Value);
                    mso.WriteString32VN(property.Signature);
                }
                Connection.Send(mso.Get());
            }
            Done = true;
            Client client = new(this);
            ThreadPool.QueueUserWorkItem(client.KeepAlive);
            PostSuccess.Invoke(client);
            client.ProcessIncomingPackets();
        }
    }
}
