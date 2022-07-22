namespace AMT.Lib.AMTPackets
{
    public class DataPacket
    {
        // Based on captured data with Wireshark
        public enum Commands : ushort
        {
            CONNECT_PASSWORD = 0xF0F0,
            CENTRAL_STATUS = 0x0B4A,
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
            var packet = new DataPacket();

            packet.Header = new byte[8];
            int dLen = (data?.Length ?? 0) + 2;
            packet.Header[4] = (byte)(dLen >> 8);
            packet.Header[5] = (byte)dLen;
            packet.Header[6] = (byte)(command >> 8);
            packet.Header[7] = (byte)command;

            packet.Data = new byte[data?.Length ?? 0];
            if (packet.Data.Length > 0)
            {
                Array.Copy(data, 0, packet.Data, 0, data.Length);
            }

            return packet;
        }
        public static DataPacket Unpack(byte[] data)
        {
            var packet = new DataPacket();
            packet.Header = new byte[8];
            Array.Copy(data, 0, packet.Header, 0, packet.Header.Length);

            int dataLen = packet.Header[4] * 256 + packet.Header[5] - 2;
            packet.Data = new byte[dataLen];
            if (dataLen > 0)
            {
                Array.Copy(data, 8, packet.Data, 0, dataLen);
            }
            return packet;
        }

        public static DataPacket BuildAuthenticationPacket(byte[] password)
        {
            var data = new byte[9];
            Buffer.BlockCopy(password, 0, data, 1, password.Length);
            // captured a header and a trailer
            data[0] = 1;
            data[7] = 0x10; 

            return BuildPacket(Commands.CONNECT_PASSWORD, data);
        }
        public static DataPacket BuildCentralStatus()
        {
            return BuildPacket(Commands.CENTRAL_STATUS, null);
        }


    }
}
