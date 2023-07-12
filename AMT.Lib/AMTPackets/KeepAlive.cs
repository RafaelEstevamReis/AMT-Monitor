namespace Simple.AMT.AMTPackets;

internal class KeepAlive : DataPacket
{
    public bool Result { get; set; }
    public static DataPacket Request()
    {
        return BuildPacket(Commands.KEEP_ALIVE, null);
    }
    public override void Unpack(byte[] receivedBytes)
    {
        base.Unpack(receivedBytes);

        Result = Data[0] == 0xF0
              && Data[1] == 0xFE;
    }

}
