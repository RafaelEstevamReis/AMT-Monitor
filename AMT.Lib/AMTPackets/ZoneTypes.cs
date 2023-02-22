namespace Simple.AMT.AMTPackets
{
    public class ZoneTypes : DataPacket
    {
        public ZoneType[] Types { get; set; }
        public static DataPacket Request()
        {
            return BuildPacket(Commands.ZONE_TYPES, 0xFF);
        }
        public override void Unpack(byte[] receivedBytes)
        {
            base.Unpack(receivedBytes);

            Types = new ZoneType[64];
            for (int i = 0; i < 64; i++)
            {
                Types[i] = new ZoneType
                {
                    Id = i,
                    Name = (i + 1).ToString(),
                    Type = Data[i],
                };
            }
        }

        public class ZoneType
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public byte Type { get; set; } 

        }
    }
}
