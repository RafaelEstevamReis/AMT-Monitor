using System.Linq;

namespace Simple.AMT.ListenerModels
{
    public class CentralInformation
    {
        public enum ConnectionType
        {
            Ethernet = 0x45, // 'E'
            GPRS = 0x47, // 'G'
            GPRS2 = 0x48, // 'H'
        }

        public ConnectionType Connection { get; set; }
        public byte[] MacAddress { get; set; }
        public byte[] PartialMacAddress { get; set; }
        public int AccountId { get; set; }

        public string PartialMacAddressString
            => buildMac(PartialMacAddress);
        public string MacAddressString
            => buildMac(MacAddress);

        private static string buildMac(byte[] macAddress)
        {
            if (macAddress is null) return null;
            return string.Join(':', macAddress.Select(m => m.ToString("X2")));
        }

    }
}
