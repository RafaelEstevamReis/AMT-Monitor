using System;
using System.Collections.Generic;
using System.Text;

namespace Simple.AMT.AMTPackets
{
    public class DeviceSignalLevel : DataPacket
    {
        public static DataPacket Request()
        {
            return BuildPacket(Commands.DEVICE_SIGNAL_LEVEL, 0x06);
        }
    }
}
