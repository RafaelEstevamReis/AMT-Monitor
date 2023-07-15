using Simple.AMT.AMTModels;
using Simple.AMT.AMTPackets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Simple.AMT
{
    /// <summary>
    /// AMT-8000 Client
    /// </summary>
    public class AMT8000
    {
        public ConnectionInfo ConnectionInfo { get; }
        public DateTime LastCommunication { get; private set; }

        private TcpClient tcpClient;
        public bool IsConnected => tcpClient?.Connected ?? false;

        public AMT8000(ConnectionInfo connectionInfo)
        {
            ConnectionInfo = connectionInfo;
        }

        public async Task<bool> ConnectAsync()
        {
            tcpClient = new TcpClient();
            await tcpClient.ConnectAsync(ConnectionInfo.IP, ConnectionInfo.Port);
            if (!tcpClient.Connected) throw new InvalidOperationException("Not connected");
            var stream = tcpClient.GetStream();

            var auth = Connection.Request(ConnectionInfo.GetPassword());
            var authResult = await sendReceiveAsync<Connection>(stream, auth);
            if (!authResult.Success) return false;

            return true;
        }
        public void Disconnect()
        {
            tcpClient.Close();
        }
        public async Task<bool> KeepAliveAsync()
        {
            var ka = await sendReceiveAsync<KeepAlive>(KeepAlive.Request());
            return ka.Result;
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
        public async Task<ConnectionsStatus> GetConnectionsAsync()
        {
            var result = await sendReceiveAsync<ConnectionsStatus>(ConnectionsStatus.Request());
            return result;
        }

        long busy = 0;
        private async Task<T> sendReceiveAsync<T>(DataPacket toSend) where T : DataPacket
        {
            for (int i = 0; i < 10 * 2; i++) // max 2s
            {
                if (Interlocked.Read(ref busy) == 0) break;
                await Task.Delay(100);
            }
            Interlocked.Increment(ref busy);

            LastCommunication = DateTime.UtcNow;
            var result = await sendReceiveAsync<T>(tcpClient.GetStream(), toSend);

            Interlocked.Exchange(ref  busy, 0); // reset
            return result;
        }
        private static async Task<T> sendReceiveAsync<T>(NetworkStream stream, DataPacket toSend)
            where T : DataPacket
        {
            while (stream.DataAvailable)
            {
                // empty previous bytes
                var oldBytes = await receiveBytesAsync(stream);
                oldBytes = oldBytes;
            }

            await sendPacketAsync(stream, toSend);
            // Wait first bytes to arrive
            await Task.Delay(50);

            int limit = 0;
            while (!stream.DataAvailable)
            {
                await Task.Delay(100);
                if(limit++ > 10) throw new TimeoutException("Central did not respond");
            }

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
            byte[] bytes = new byte[4096];
            // read 8 (header)
            await Task.Delay(50); // wait for packet completion
            int hdrLen = await stream.ReadAsync(bytes, 0, 8);
            // unpack len
            int dataExpenctedLen = bytes[4] * 256 + bytes[5] - 2;
            if (dataExpenctedLen < 0) dataExpenctedLen = 0;
            // read len
            int dataToRead = Math.Min(bytes.Length - 8, dataExpenctedLen);
            int dataLen = await stream.ReadAsync(bytes, 8, dataToRead);

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
