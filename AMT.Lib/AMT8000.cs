using Simple.AMT.AMTModels;
using Simple.AMT.AMTPackets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Simple.AMT
{
    /// <summary>
    /// AMT-8000 Client
    /// </summary>
    public class AMT8000
    {
        public ConnectionInfo ConnectionInfo { get; }

        private readonly TcpClient tcpClient;

        public AMT8000(ConnectionInfo connectionInfo)
        {
            ConnectionInfo = connectionInfo;
            tcpClient = new TcpClient();
        }

        public async Task<bool> ConnectAsync()
        {
            await tcpClient.ConnectAsync(ConnectionInfo.IP, ConnectionInfo.Port);
            if (!tcpClient.Connected) throw new InvalidOperationException("Not connected");
            var stream = tcpClient.GetStream();

            var auth = Connection.Request(ConnectionInfo.GetPassword());
            var authResult = await sendReceiveAsync<Connection>(stream, auth);
            if (!authResult.Success) return false;

            return true;
        }

        public async Task<CentralStatus> GetCentralStatusAsync()
        {
            return await sendReceiveAsync<CentralStatus>(CentralStatus.Request());
        }
        public async Task<EventLog.Event[]> GetEventsAsync()
        {
            var events = new List<EventLog.Event>();
            var pointerResult = await sendReceiveAsync<EventPointer>(EventPointer.Request());
            for (int i = 0; i < 32; i++)
            {
                var logResult = await sendReceiveAsync<EventLog>(EventLog.Request(i, pointerResult.Index));
                events.AddRange(logResult.Events);
            }
            return events.ToArray();
        }
        public async Task<SensorConfiguration.Sensor[]> GetSensorConfigurationAsync()
        {
            var result = await sendReceiveAsync<SensorConfiguration>(SensorConfiguration.Request());
            return result.Sensors;
        }

        public async Task<ItemNames.NameEntry[]> GetZonesNamesAsync()
        {
            var names = new List<ItemNames.NameEntry>();
            for (int page = 0; page < 4; page++)
            {
                var namesResult = await sendReceiveAsync<ItemNames>(ItemNames.Request(page, DataPacket.Commands.ZONE_NAMES));
                names.AddRange(namesResult.Names);
            }
            return names.ToArray();
        }
        public async Task<ItemNames.NameEntry[]> GetUserNamesAsync()
        {
            var names = new List<ItemNames.NameEntry>();
            for (int page = 0; page < 7; page++)
            {
                var namesResult = await sendReceiveAsync<ItemNames>(ItemNames.Request(page, DataPacket.Commands.USER_NAMES));
                names.AddRange(namesResult.Names);
            }
            return names.Where(n => n != null).ToArray();
        }
        public async Task<ZoneTypes.ZoneType[]> GetZonesTypesAsync()
        {
            var result = await sendReceiveAsync<ZoneTypes>(ZoneTypes.Request());

            return result.Types;
        }
        public async Task<bool[]> GetOpenedZonesAsync()
        {
            var result = await sendReceiveAsync<ZoneStatus>(ZoneStatus.Request_Openned());
            return result.SensorsValue;
        }
        public async Task<bool[]> GetBypassedZonesAsync()
        {
            var result = await sendReceiveAsync<ZoneStatus>(ZoneStatus.Request_Bypassed());
            return result.SensorsValue;
        }
        public async Task<bool[]> GetTriggeredZonesAsync()
        {
            var result = await sendReceiveAsync<ZoneStatus>(ZoneStatus.Request_Triggered());
            return result.SensorsValue;
        }
        public async Task<string> GetMacAsync()
        {
            var result = await sendReceiveAsync<ReadMac>(ReadMac.Request());
            return result.MAC;
        }


        private async Task<T> sendReceiveAsync<T>(DataPacket toSend) where T : DataPacket
            => await sendReceiveAsync<T>(tcpClient.GetStream(), toSend);
        private static async Task<T> sendReceiveAsync<T>(NetworkStream stream, DataPacket toSend)
            where T : DataPacket
        {
            await sendPacketAsync(stream, toSend);
            var bytesReceived = await receiveBytesAsync(stream);
            if (bytesReceived.Length == 9 && bytesReceived[8] == 31)
            {
                throw new Exception("Invalid password");
            }
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
            catch { }
            return packet;
        }

    }
}
