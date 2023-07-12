namespace Simple.AMT.AMTPackets;

using System;

public class ReadMac : DataPacket
{
    public string MAC { get; set; }

    public static DataPacket Request()
    {
        return BuildPacket(Commands.GET_MAC, 0x00);
    }
    public override void Unpack(byte[] receivedBytes)
    {
        base.Unpack(receivedBytes);

        MAC = BitConverter.ToString(Data, 1, 6).Replace('-', ':');
    }
}
