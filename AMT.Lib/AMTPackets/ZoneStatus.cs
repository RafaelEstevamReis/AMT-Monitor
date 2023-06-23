
namespace Simple.AMT.AMTPackets
{
    public class ZoneStatus : DataPacket
    {
        public bool[] SensorsValue { get; set; }

        public static DataPacket Request_Openned()
        {
            return BuildPacket(Commands.OPENED_ZONES, 0xFF);
        }
        public static DataPacket Request_Triggered()
        {
            return BuildPacket(Commands.TRIGGERED_ZONES, 0xFF);
        }
        public static DataPacket Request_Bypassed()
        {
            return BuildPacket(Commands.BYPASSED_ZONES, 0xFF);
        }

        public override void Unpack(byte[] receivedBytes)
        {
            base.Unpack(receivedBytes);

            SensorsValue = new bool[64];
            int offset = 1; // skip first byte
            for (int i = 0; i < 8; i++)
            {
                var b = Data[offset++];
                for (byte bit = 0; bit < 8; bit++)
                {
                    int idx = i * 8 + bit;
                    SensorsValue[idx] = IsBit(b, bit);
                }
            }
        }
    }
}
