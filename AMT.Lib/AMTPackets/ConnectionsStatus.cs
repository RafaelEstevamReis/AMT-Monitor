namespace Simple.AMT.AMTPackets
{
    public class ConnectionsStatus : DataPacket
    {
        public bool Eth_IP1 { get; set; }
        public bool Eth_IP2 { get; set; }

        public bool IsWiFi { get; set; }
        public bool IsEth { get; set; }

        public bool GPRS_IP1 { get; set; }
        public bool GPRS_IP2 { get; set; }

        public bool Cloud_Eth { get; set; }
        public bool Cloud_GRPS { get; set; }

        public static DataPacket Request()
        {
            return BuildPacket(Commands.CONNECTIONS_STATUS);
        }
        public override void Unpack(byte[] receivedBytes)
        {
            base.Unpack(receivedBytes);

            Eth_IP1 = IsBit(Data[0], 0);
            Eth_IP2 = IsBit(Data[0], 1);

            IsWiFi = IsBit(Data[0], 2);
            IsEth = !IsWiFi;

            GPRS_IP1 = IsBit(Data[0], 4);
            GPRS_IP2 = IsBit(Data[0], 5);

            Cloud_Eth = IsBit(Data[1], 0);
            Cloud_GRPS = IsBit(Data[1], 1);
        }
    }
}
