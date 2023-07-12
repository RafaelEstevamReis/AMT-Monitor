namespace Simple.AMT.AMTPackets;

public class DeviceSignalLevel : DataPacket
{
    public enum DeviceId : byte
    {
        Controller = 0,
        Sensor = 1,
        PGM = 6,
        Keyboard = 9,
        Siren = 10,
        Repeater = 11,
    }

    public static DataPacket Request(DeviceId id, byte index)
    {
        if (id == DeviceId.Sensor)
        {
            return BuildPacket(Commands.DEVICE_SIGNAL_LEVEL, index);
        }
        else
        {
            byte deviceId = (byte)id;
            return BuildPacket(Commands.DEVICE_SIGNAL_LEVEL, deviceId, index);
        }
    }

    public override void Unpack(byte[] receivedBytes)
    {
        base.Unpack(receivedBytes);

        receivedBytes = receivedBytes;
    }
}
