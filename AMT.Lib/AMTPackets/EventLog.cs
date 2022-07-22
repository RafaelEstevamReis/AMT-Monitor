using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMT.Lib.AMTPackets
{
    public class EventLog : DataPacket
    {
        public Event[] Events { get; set; }


        public static DataPacket Request(int block, int eventPointer)
        {
            byte[] data = new byte[32];

            int start = block * 16;
            int end = start + 16;

            int offset = 0;
            for (int idx = start; idx < end; idx++)
            {
                int eventIndex = (idx <= eventPointer) ? (eventPointer - idx) : (512 + eventPointer - idx);
                data[offset++] = (byte)(eventIndex >> 8);
                data[offset++] = (byte)eventIndex;
            }

            return BuildPacket(Commands.EVENT_LOG, data);
        }

        public override void Unpack(byte[] receivedBytes)
        {
            base.Unpack(receivedBytes);

            int offset = 0;
            Events = new Event[16];
            for (int i = 0; i < 16; i++)
            {
                Events[i] = new Event();

                Events[i].EventId = (ushort)(Data[offset++] * 256 + Data[offset++]);
                Events[i].DateTime = new DateTime(
                    year: hexToDec(Data[offset++]) + 2000,
                    month: hexToDec(Data[offset++]),
                    day: hexToDec(Data[offset++]),
                    hour: hexToDec(Data[offset++]),
                    minute: hexToDec(Data[offset++]),
                    second: hexToDec(Data[offset++]));

                Events[i].EventType = (ushort)(hexToDec(Data[offset++]) * 100 + hexToDec(Data[offset++]));

                var unk = hexToDec(Data[offset++]);
                var part = Data[offset++];
                var topPart = part >> 4;
                var bottom = part & 0x0F;

                Events[i].UnkownData = (ushort)((unk << 4) + topPart);
                Events[i].ZoneUser = (ushort)(bottom * 100 + hexToDec(Data[offset++]));
                Events[i].Partition = hexToDec(Data[offset++]);
                Events[i].Photo = hexToDec(Data[offset++]);
            }
        }

        public class Event
        {
            public ushort EventId { get; set; }
            public DateTime DateTime { get; set; }
            public ushort EventType { get; set; }
            public ushort UnkownData { get; set; }
            public ushort ZoneUser { get; set; }
            public byte Partition { get; set; }
            public byte Photo { get; set; }
        }
    }
}
