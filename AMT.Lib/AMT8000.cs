using AMT.Lib.AMTModels;
using AMT.Lib.AMTPackets;
using System.Net.Sockets;

namespace AMT.Lib
{
    public class AMT8000
    {

        public ConnectionInfo ConnectionInfo { get; }

        private readonly TcpClient tcpClient;

        public AMT8000(ConnectionInfo connectionInfo)
        {
            ConnectionInfo = connectionInfo;
            tcpClient = new TcpClient();
        }

        public async Task ConnectAsync()
        {
            byte[] bytes;

            await tcpClient.ConnectAsync(ConnectionInfo.IP, ConnectionInfo.Port);
            if (!tcpClient.Connected) throw new InvalidOperationException("Not connected");
            var stream = tcpClient.GetStream();

            var auth = DataPacket.BuildAuthenticationPacket(ConnectionInfo.GetPassword());
            //await sendPacketAsync(stream, auth);
            //var response = await receiveBytesAsync(stream);
            //var result = DataPacket.Unpack(response);
            var authResult = await sendReceiveAsync(stream, auth);
            if (authResult.Data[0] != 0) throw new InvalidOperationException("Auth Error");

            var statusResult = await sendReceiveAsync(stream, DataPacket.BuildCentralStatus());
            statusResult = statusResult;
        }

        private static async Task<DataPacket> sendReceiveAsync(NetworkStream stream, DataPacket toSend)
        {
            await sendPacketAsync(stream, toSend);
            var bytesReceived = await receiveBytesAsync(stream);
            var receivedPacket = DataPacket.Unpack(bytesReceived);

            if (receivedPacket.Header[6] != toSend.Header[6]
                || receivedPacket.Header[7] != toSend.Header[7])
            {

            }
            return receivedPacket;
        }

        private static async Task sendPacketAsync(NetworkStream stream, DataPacket packet)
        {
            await stream.WriteAsync(packet.GetBytes());
            await stream.FlushAsync();
        }
        public static async Task<byte[]> receiveBytesAsync(NetworkStream stream)
        {
            byte[] bytes = new byte[512];
            
            // read 8 (header)
            int hdrLen = await stream.ReadAsync(bytes, 0, 8);
            // unpack len
            int dataExpenctedLen = bytes[4] * 256 + bytes[5] - 2;
            // read len
            int dataLen = await stream.ReadAsync(bytes, 8, dataExpenctedLen);

            int totalLen = 8 + dataLen;
            // read checksum
            int bCheck = stream.ReadByte();

            byte[] packet = new byte[totalLen];
            Buffer.BlockCopy(bytes, 0, packet, 0, totalLen);
            return packet;
        }

    }
}
