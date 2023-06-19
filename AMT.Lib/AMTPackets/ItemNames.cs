using System;
using System.Text;

namespace Simple.AMT.AMTPackets
{
    public class ItemNames : DataPacket
    {
        private static readonly Encoding enc8859 = Encoding.GetEncoding("ISO-8859-1");
        private const int ENTRIES_PER_BLOCK = 16;
        public NameEntry[] Names { get; set; }

        public static DataPacket Request(int block, Commands command)
        {
            int entries = ENTRIES_PER_BLOCK;

            if (command == Commands.USER_NAMES && block == 6) entries = 4;

            byte[] data = new byte[entries];
            for (int i = 0; i < entries; i++)
            {
                data[i] = (byte)((block * ENTRIES_PER_BLOCK) + i);
            }

            return BuildPacket(command, data);
        }

        public override void Unpack(byte[] receivedBytes)
        {
            base.Unpack(receivedBytes);
            Names = new NameEntry[ENTRIES_PER_BLOCK];

            int offset = 0;

            for (int i = 0; i < ENTRIES_PER_BLOCK; i++)
            {
                if (offset + 15 > Data.Length) return;

                var str = enc8859.GetString(Data, offset + 1, 14);
                var devId = Data[offset];
                offset += 15;

                Names[i] = new NameEntry
                {
                    Id = devId,
                    Name = str.Replace("\0", "").Trim()
                };
            }
        }
        public class NameEntry
        {
            public enum EntryType
            {
                Text,
            }
            public byte Id { get; set; }
            public string Name { get; set; }
            public EntryType Type { get; set; }

            public override string ToString()
            {
                return $"{Id:00} {Name}";
            }
        }
    }
}
