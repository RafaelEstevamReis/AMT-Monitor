namespace Simple.AMT.AMTPackets
{
    public class SensorConfiguration : DataPacket
    {
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
                    Sensitivity = Data[offset++],
                    LedMode = Data[offset++],
                    OperationMode = Data[offset++],
                };
            }
        }

        public class Sensor
        {
            public byte Id { get; set; }
            public byte Sensitivity { get; set; }
            public byte LedMode { get; set; }
            public byte OperationMode { get; set; }
            public override string ToString()
            {
                return $"Id: {Id} Sensitivity: {Sensitivity} LedMode: {LedMode} OperationMode: {OperationMode}";
            }
        }
    }
}
