using System;
using System.Collections.Generic;

namespace Simple.AMT.AMTPackets
{
    public enum BatteryLevel : byte
    {
        UNKOWN = 0,
        NoBattery = 1,
        LowBattery = 2,
        MidBattery = 3,
        FullBattery = 4,
    }
    public enum StatusType
    {
        Disarmed = 0,
        Armed = 2
    }

    public class CentralStatus : DataPacket
    {
        public string Firmware { get; set; }
        public bool IsEth { get; set; }
        public bool IsWifi { get; set; }
        public bool IsGprs { get; set; }

        public bool HasProblems { get; set; }
        public bool HasSiren { get; set; }
        public bool AllZonesClosed { get; set; }
        public bool AnyZoneTriggered { get; set; }
        public bool AnyZoneByPassed { get; set; }
        public bool StayMode { get; set; }
        public int Status { get; set; }
        public BatteryLevel BatteryLevel { get; set; }

        public DateTime CentralDateTime { get; set; }
        public DateTime PacketBuiltTime { get; set; }
        public ZoneInfo[] Zones { get; set; }

        public static DataPacket Request()
        {
            return BuildPacket(Commands.CENTRAL_STATUS, null);
        }

        public override void Unpack(byte[] receivedBytes)
        {
            base.Unpack(receivedBytes);

            /*
    00000011 [--] 00 00 00 00 00 91 0b 4a  01 02 00 06 0d 00 62 03   .......J ......b.
    00000021 [08] 01 00 00 00 07 00 00 00  00 00 00 00 64 91 00 00   ........ ....d...
    00000031 [24] 00 00 00 00 00 00 00 00  00 00 00 00 00 00 00 00   ........ ........
    00000041 [40] 00 00 00 00 00 00 00 00  00 00 00 00 00 00 00 00   ........ ........
    00000051 [56] 00 00 00 00 00 00 00 00  21 07 22 20 34 52 00 20   ........ !." 4R. 
    00000061 [72] 01 00 00 00 00 00 00 00  00 00 00 00 00 00 00 00   ........ ........
    00000071 [88] 00 00 00 00 00 00 00 00  00 00 00 00 00 00 00 00   ........ ........
    00000081 [04] 00 00 00 00 00 00 00 00  00 00 00 00 00 00 00 00   ........ ........
    00000091 [20] 00 00 00 00 00 00 00 00  00 00 00 00 00 00 04 00   ........ ........
    000000A1 [36] 00 00 00 00 00 a4 10 66                            .......f 
            */
            Firmware = $"{Data[1]}.{Data[2]}.{Data[3]}";
            #region Capabilities
            //                             7654 3210
            // Data[4] capabilities [0x0d]=0000.1101
            // [3] eth
            IsEth = IsBit(Data[4], 3);
            // [2] wifi
            IsWifi = IsBit(Data[4], 2);
            // [1] gprs
            IsGprs = IsBit(Data[4], 1);
            #endregion
            #region Statuses
            //                        7654 3210
            // Data[20] Status [0x64]=0110.0100
            // [7] stayMode
            StayMode = IsBit(Data[20], 7);
            // [6] StatusA
            // [5] StatusB
            if (IsBit(Data[20], 6)) Status = 2;
            else
            {
                Status = IsBit(Data[20], 5) ? 1 : 0;
            }
            // [4] Bypassed
            AnyZoneByPassed = IsBit(Data[20], 4);
            // [3] Triggered
            AnyZoneTriggered = IsBit(Data[20], 3);
            // [2] AllClosed
            AllZonesClosed = IsBit(Data[20], 2);
            // [1] Siren
            HasSiren = IsBit(Data[20], 1);
            // [0] Problems
            HasProblems = IsBit(Data[20], 0);
            #endregion

            PacketBuiltTime = DateTime.Now;
            // data[64..69] dateTime

            CentralDateTime = new DateTime(
                year: hexToDec(Data[66]) + 2000,
                month: hexToDec(Data[65]),
                day: hexToDec(Data[64]),
                hour: hexToDec(Data[67]),
                minute: hexToDec(Data[68]),
                second: hexToDec(Data[69]));
            BatteryLevel = Data[134];

            int idxOpen = 38;
            int idxTrigger = 46;
            int idxByPass = 54;
            List<ZoneInfo> lstZones = new List<ZoneInfo>();
            for (int block = 0; block < 8; block++) // 8 byte-blocks
            {
                var bytesOpen = Data[idxOpen++];
                var bytesTrigger = Data[idxTrigger++];
                var bytesBypass = Data[idxByPass++];

                for (byte i = 0; i < 8; i++)
                {
                    lstZones.Add(new ZoneInfo()
                    {
                        Open = IsBit(bytesOpen, i),
                        Trigger = IsBit(bytesTrigger, i),
                        ByPass = IsBit(bytesBypass, i),
                    });
                }
            }
            Zones = lstZones.ToArray();

        }

        public class ZoneInfo
        {
            public bool Open { get; set; }
            public bool Trigger { get; set; }
            public bool ByPass { get; set; }

            public override string ToString()
            {
                return $"Open: {Open} Trigger: {Trigger} ByPass: {ByPass}";
            }
        }
    }
}
