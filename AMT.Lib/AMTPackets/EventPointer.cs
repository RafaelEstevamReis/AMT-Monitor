namespace Simple.AMT.AMTPackets
{
    public class EventPointer: DataPacket
    {
        public int Index 
            => ((Data[1] & 0xFF) << 8 | Data[2] & 0xFF) - 1;

        public static DataPacket Request()
        {
            return BuildPacket(Commands.EVENT_POINTER, 0x00);
        }

    }
}
