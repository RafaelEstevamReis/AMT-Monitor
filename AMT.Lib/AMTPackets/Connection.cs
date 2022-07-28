using System;

namespace Simple.AMT.AMTPackets
{
    public class Connection : DataPacket
    {
        public bool Success
            => Data != null && Data.Length > 0 && Data[0] == 0;

        public static DataPacket Request(byte[] password)
        {
            var data = new byte[9];
            Buffer.BlockCopy(password, 0, data, 1, password.Length);
            // captured a header and a trailer
            data[0] = 1;
            data[7] = 0x10;

            return BuildPacket(Commands.CONNECT_PASSWORD, data);
        }

    }
}
