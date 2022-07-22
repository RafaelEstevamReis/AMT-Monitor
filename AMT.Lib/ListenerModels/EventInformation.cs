namespace AMT.Lib.ListenerModels
{
    public class EventInformation
    {
        // 0x11 Ethernet IP1, 0x12 IP2, 0x21 GPRS IP1, 0x22 IP2
        public enum ChannelType
        {
            Eth_IP1 = 0x11,
            Eth_IP2 = 0x12,
            GPRS_IP1 = 0x21,
            GPRS_IP2 = 0x22,
        }

        public ChannelType Channel { get; set; }
        public int ContactId { get; set; }
        public int MessageType { get; set; } // 18: ContactId
        // 1: Any/Open | 3: Reset
        public int Qualifier { get; set; }
        public int Code { get; set; }
        public string CodeName
            => Code switch
            {
                // 1xx: ALARMS
                100 => "Medical",
                110 => "Fire",
                120 => "Panic",
                121 => "Duress",
                122 => "Silent",
                130 => "Burglary",
                133 => "24 Hour (Safe)",

                140 => "General Alarm",
                145 => "Expansion module tamper",
                146 => "Silent Burglary",
                147 => "Sensor Supervision Failure",
                // 200: SUPERVISORY
                // 300: TROUBLES
                300 => "System Trouble",
                301 => "AC Loss",
                302 => "Low system battery",
                305 => "System reset",
                306 => "Panel programming changed",
                311 => "Battery Missing/Dead",

                351 => "Line Loss",
                354 => "Event Failure",

                380 => "Sensor trouble",
                383 => "Sensor tamper",
                384 => "RF low battery",
                393 => "Maintenance Alert",

                // 400: OPEN/CLOSE REMOTE ACCESS
                400 => "Arm/disarm",
                401 => "User arm/disarm",
                402 => "Group arm/disarm",
                403 => "Automatic arm/disarm",

                407 => "Remote arm/disarm",
                408 => "Keyswitch arm/disarm",

                410 => "Remote Access",
                461 => "Invalid Password",

                // 500: BYPASSES/DISABLES
                570 => "Zone Bypass",

                // 600: TEST/MISC
                601 => "Manual Test",
                602 => "Periodic Test",
                616 => "Maintenance Request",
                621 => "EventLog Reset",
                622 => "EventLog 50% Full",
                623 => "EventLog 90% Full",
                625 => "Date/Time reset",
                627 => "Program Mode Entry",
                628 => "Program Mode Exit",

                _ => "",
            };

        public int Partition { get; set; }
        public int Zone { get; set; }
        public byte CheckSum { get; set; }

        public short PhotoIndex { get; set; }
        public byte PhotoCount { get; set; }


    }
}
