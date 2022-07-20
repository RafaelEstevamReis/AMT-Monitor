namespace AMT.Lib.Models
{
    public class EventInformation
    {
        // 0x11 Ethernet IP1, 0x12 IP2, 0x21 GPRS IP1, 0x22 IP2
        public enum ChannelType
        {
            Ethernet_IP1 = 0x11,
            Ethernet_IP2 = 0x12,
            GPRS_IP1 = 0x21,
            GPRS_IP2 = 0x22,
        }

        public ChannelType Channel { get; set; }
        public int ContactId { get; set; }
        public int MessageType { get; set; }
        // 1: Any/Open | 3: Reset
        public int Qualifier { get; set; }
        public int Code { get; set; }
        public string CodeName
            => Code switch
            {
                // 1xx: ALARMS
                // Medical
                100 => "Medical",
                101 => "Personal Emergency",
                102 => "Failt to report in",
                // Fire
                110 => "Fire",
                111 => "Smoke",
                112 => "Combustion",
                113 => "Water Flow",
                114 => "Heat",
                115 => "Pull Station",
                116 => "Duct",
                117 => "Flame",
                118 => "Near Alarm",
                // Panic
                120 => "Panic",
                121 => "Duress",
                122 => "Silent",
                123 => "Audible",
                124 => "Duress – Access granted ",
                125 => "Duress – Egress granted",
                // Burglar
                130 => "Burglary",
                131 => "Perimeter",
                132 => "Interior",
                133 => "24 Hour (Safe)",
                134 => "Entry/Exit",
                135 => "Day/night",
                136 => "Outdoor",
                137 => "Tamper",
                138 => "Near alarm",
                139 => "Intrusion Verifier",

                140 => "General Alarm",
                141 => "Polling loop open",
                142 => "Polling loop short",
                143 => "Expansion module failure",
                144 => "Sensor tamper",
                145 => "Expansion module tamper",
                146 => "Silent Burglary",
                147 => "Sensor Supervision Failure",

                // 200: SUPERVISORY
                // 300: TROUBLES
                300 => "System Trouble",
                301 => "AC Loss",
                302 => "Low system battery",
                303 => "RAM Checksum bad",
                304 => "ROM checksum bad",
                305 => "System reset",
                306 => "Panel programming changed",
                307 => "Self-test failure",
                308 => "System shutdown",
                309 => "Battery test failure",
                310 => "Ground fault",
                311 => "Battery Missing/Dead",
                312 => "Power Supply Overcurrent",
                313 => "Engineer Reset",
                
                380 => "Sensor trouble",
                383 => "Sensor tamper",
                384 => "RF low battery",
                387 => "Intrusion detector Hi sensitivity",
                388 => "Intrusion detector Low sensitivity",
                389 => "Sensor self-test failure",
                393 => "Maintenance Alert",

                // 400: OPEN/CLOSE REMOTE ACCESS
                400 => "Open/Close",
                401 => "O/C by user",
                402 => "Group O/C",
                403 => "Automatic O/C",

                407 => "Remote arm/disarm",


                // 500: BYPASSES/DISABLES
                // 600: TEST/MISC
                625 => "Date/Time reset",

                _ => "",
            };

        public int Partition { get; set; }
        public int Zone { get; set; }
        public byte CheckSum { get; set; }

        public short PhotoIndex { get; set; }
        public byte PhotoCount { get; set; }


    }
}
