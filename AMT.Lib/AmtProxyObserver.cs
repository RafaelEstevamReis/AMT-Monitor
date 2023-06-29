using Simple.AMT.AMTPackets;
using System;
using System.Net.Sockets;
using System.Threading;

namespace Simple.AMT
{
    public class AmtProxyObserver : IDisposable
    {
        private readonly TcpListener listener;
        private readonly TcpClient remote;
        private readonly string AmtIP;
        private readonly int AmtPort;
        private readonly Thread thread;

        private TcpClient localClient;

        public event EventHandler<string> LogEvents;
        public event EventHandler<ProxyTraffic> DataTraffic;

        public AmtProxyObserver(int localPort, string amtIP, int amtPort)
        {
            listener = new TcpListener(System.Net.IPAddress.Any, localPort);
            remote = new TcpClient();
            AmtIP = amtIP;
            AmtPort = amtPort;

            thread = new Thread(processSocket);
        }

        public void Start()
        {
            listener.Start();
            listener.BeginAcceptTcpClient(new AsyncCallback(callBack), null);
        }

        private void callBack(IAsyncResult ar)
        {
            remote.Connect(AmtIP, AmtPort);
            localClient = listener.EndAcceptTcpClient(ar);

            LogEvents?.Invoke(this, $"New connection {localClient.Client.RemoteEndPoint}->{remote.Client.RemoteEndPoint}");

            thread.Start();
        }

        private void processSocket(object obj)
        {
            var remoteStream = remote.GetStream();
            var localStream = localClient.GetStream();
            byte[] buffer = new byte[2048];

            while (true)
            {
                if (!remote.Connected) break;
                if (!localClient.Connected) break;

                if (remote.Available > 0)
                {
                    Thread.Sleep(10); // wait to more data arrive
                    int len = remoteStream.Read(buffer, 0, buffer.Length);
                    localStream.Write(buffer, 0, len);

                    var td = ProxyTraffic.Create(false, buffer, len);
                    DataTraffic?.Invoke(this, td);

                    continue;
                }
                if (localClient.Available > 0)
                {
                    Thread.Sleep(10); // wait to more data arrive
                    int len = localStream.Read(buffer, 0, buffer.Length);
                    remoteStream.Write(buffer, 0, len);

                    var td = ProxyTraffic.Create(true, buffer, len);
                    DataTraffic?.Invoke(this, td);

                    continue;
                }

                Thread.Sleep(50); // no data
            }

            Dispose();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

    }
    public class ProxyTraffic
    {
        public bool SentToCentral { get; set; }
        public byte[] Data { get; set; }

        public static ProxyTraffic Create(bool toCentral, byte[] src, int len)
        {
            var traffic = new ProxyTraffic()
            {
                SentToCentral = toCentral,
                Data = new byte[len],
            };

            Buffer.BlockCopy(src, 0, traffic.Data, 0, len);

            return traffic;
        }
    }
    public class TrafficDecoder
    {
        private readonly ProxyTraffic traffic;

        public ushort PacketLen => traffic.Data.Length > 8 ? (ushort)(GetUShort(traffic.Data, 4) - 2) : (ushort)0;
        public ushort PacketCommand => traffic.Data.Length > 8 ? GetUShort(traffic.Data, 6) : (ushort)0;

        public TrafficDecoder(ProxyTraffic traffic)
        {
            this.traffic = traffic;
        }

        public byte[] GetPacketData()
        {
            if (PacketLen == 0) return traffic.Data;

            byte[] b = new byte[PacketLen];
            Buffer.BlockCopy(traffic.Data, 8, b, 0, PacketLen);
            return b;
        }

        public ushort GetUShort(byte[] data, int start)
        {
            return (ushort)(data[start] * 256 + data[start + 1]);
        }

        public T UnpackResponse<T>()
            where T : DataPacket
        {
            var instance = Activator.CreateInstance<T>();
            instance.Unpack(traffic.Data);
            return instance;
        }

    }
}
