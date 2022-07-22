﻿using AMT.Lib.AMTModels;
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

            var auth = Connection.Request(ConnectionInfo.GetPassword());
            //await sendPacketAsync(stream, auth);
            //var response = await receiveBytesAsync(stream);
            //var result = DataPacket.Unpack(response);
            var authResult = await sendReceiveAsync<Connection>(stream, auth);
            if (authResult.Data[0] != 0) throw new InvalidOperationException("Auth Error");

            var pointerResult = await sendReceiveAsync<EventPointer>(stream, EventPointer.Request());
            pointerResult = pointerResult;

            for (int i = 0; i < 32; i++)
            {
                var logResult = await sendReceiveAsync<EventLog>(stream, EventLog.Request(i, pointerResult.Index));
                logResult = logResult;
            }



            var sensorResult = await sendReceiveAsync<SensorConfiguration>(stream, SensorConfiguration.Request());
            sensorResult = sensorResult;

            var statusResult = await sendReceiveAsync<CentralStatus>(stream, CentralStatus.Request());
            statusResult = statusResult;
        }

        private static async Task<T> sendReceiveAsync<T>(NetworkStream stream, DataPacket toSend)
            where T : DataPacket
        {
            await sendPacketAsync(stream, toSend);
            var bytesReceived = await receiveBytesAsync(stream);
            var receivedPacket = Activator.CreateInstance<T>();
            receivedPacket.Unpack(bytesReceived);

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

            if (hdrLen != 8) { }
            if (dataLen != dataExpenctedLen) { }

            byte[] packet = new byte[totalLen];
            try
            {
                Buffer.BlockCopy(bytes, 0, packet, 0, totalLen);
            }
            catch(Exception ex) { }
            return packet;
        }

    }
}
