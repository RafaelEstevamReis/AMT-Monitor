namespace Simple.AMT.AMTPackets;

public class SensorConfiguration : DataPacket
{
    public enum SensitivityMode : byte
    {
        MINIMAL = 0,
        DEFAULT = 1,
        MEDIUM = 2,
        MAXIMUM = 3,
    }
    public enum LedMode : byte
    {
        ALWAYS = 0,
        TRIGGER = 1,
    }
    public enum OperationMode : byte
    {
        ECONOMY = 0,
        CONTINUOUS = 1,
    }

    public Sensor[] Sensors { get; set; }

    public static DataPacket Request()
    {
        return BuildPacket(Commands.ZONE_CONFIGURATION, 0xFF);
    }

    public override void Unpack(byte[] receivedBytes)
    {
        base.Unpack(receivedBytes);

        Sensors = new Sensor[64];
        int offset = 1; // skip first byte
        for (int i = 0; i < 64; i++)
        {
            Sensors[i] = new Sensor()
            {
                Id = (byte)(i + 1),
                Sensitivity = (SensitivityMode)Data[offset++],
                LedMode = (LedMode)Data[offset++],
                OperationMode = (OperationMode)Data[offset++],
            };
        }
    }

    public class Sensor
    {
        public byte Id { get; set; }
        public SensitivityMode Sensitivity { get; set; }
        public LedMode LedMode { get; set; }
        public OperationMode OperationMode { get; set; }
        public override string ToString()
        {
            return $"Id: {Id} Sensitivity: {Sensitivity} LedMode: {LedMode} OperationMode: {OperationMode}";
        }
    }
}
