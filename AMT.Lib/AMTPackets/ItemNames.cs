﻿using System.Text;

namespace Simple.AMT.AMTPackets
{
    public class ItemNames : DataPacket
    {
        private const int ENTRIES_PER_BLOCK = 16;
        public NameEntry[] RegiteredNames { get; set; }

        public static DataPacket Request(int block, Commands command)
        {
            byte[] data = new byte[ENTRIES_PER_BLOCK];
            for (int i = 0; i < ENTRIES_PER_BLOCK; i++)
            {
                data[i] = (byte)((block * ENTRIES_PER_BLOCK) + i);
            }

            return BuildPacket(command, data);
        }

        public override void Unpack(byte[] receivedBytes)
        {
            base.Unpack(receivedBytes);
            RegiteredNames = new NameEntry[ENTRIES_PER_BLOCK];

            int offset = 0;
            var enc = Encoding.GetEncoding("ISO-8859-1");

            for (int i = 0; i < ENTRIES_PER_BLOCK; i++)
            {
                var str = enc.GetString(Data, offset + 1, 14);
                var devId = Data[offset];
                offset += 15;

                RegiteredNames[i] = new NameEntry
                {
                    Id = devId,
                    Name = str.Replace("\0", "").Trim()
                };
            }
        }
        public class NameEntry
        {
            public byte Id { get; set; }
            public string Name { get; set; }

            public override string ToString()
            {
                return $"{Id:00} {Name}";
            }
        }
    }
}