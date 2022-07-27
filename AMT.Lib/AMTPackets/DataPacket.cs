using System;

namespace AMT.Lib.AMTPackets
{
    public class DataPacket
    {
        // Based on captured data with Wireshark
        public enum Commands : ushort
        {
            CONNECT_PASSWORD = 0xF0F0,
            CENTRAL_STATUS = 0x0B4A,
            SENSOR_CONFIGURATION = 0x3660,
            EVENT_POINTER = 0x3003,
            EVENT_LOG = 0x3900,
        }

        public byte[] Header { get; set; }
        public byte[] Data { get; set; }

        public byte[] GetBytes()
        {
            byte[] bytes = new byte[Header.Length + Data.Length + 1];

            Array.Copy(Header, 0, bytes, 0, Header.Length);
            Array.Copy(Data, 0, bytes, Header.Length, Data.Length);

            bytes[^1] = Helpers.ErrorCorrection.CalculateChecksum(bytes);

            return bytes;
        }

        public static DataPacket BuildPacket(Commands command, params byte[] data)
            => BuildPacket((ushort)command, data);
        public static DataPacket BuildPacket(ushort command, byte[] data)
        {
            int dLen = (data?.Length ?? 0) + 2;
            var packet = new DataPacket
            {
                Header = new byte[8]
                {
                    0, 0, 0, 0,
                    (byte)(dLen >> 8),    // [4]
                    (byte)(dLen & 0xFF),  // [5]
                    (byte)(command >> 8), // [6]
                    (byte)(command & 0xFF)// [7]
                },
                Data = new byte[data?.Length ?? 0]
            };

            if (packet.Data.Length > 0)
            {
                Array.Copy(data, 0, packet.Data, 0, data.Length);
            }

            return packet;
        }

        public virtual void Unpack(byte[] receivedBytes)
        {
            Header = new byte[8];
            Array.Copy(receivedBytes, 0, Header, 0, Header.Length);

            int dataLen = Header[4] * 256 + Header[5] - 2;
            Data = new byte[dataLen];
            if (dataLen > 0)
            {
                Array.Copy(receivedBytes, 8, Data, 0, dataLen);
            }

        }

        protected static bool IsBit(byte value, byte index)
        {
            int mask = 1 << index;
            return (value & mask) != 0;
        }
        protected static byte hexToDec(byte hex)
        {
            var h = hex >> 4;
            var l = hex & 0x0F;

            if (h == 0x0a) h = 0;
            if (l == 0x0a) l = 0;

            return (byte)(h * 10 + l);
        }

    }
}
