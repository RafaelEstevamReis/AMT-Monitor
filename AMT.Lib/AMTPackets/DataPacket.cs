namespace Simple.AMT.AMTPackets;

using System;

public class DataPacket
{
    // Based on captured data with Wireshark
    public enum Commands : ushort
    {
        KEEP_ALIVE = 0xF0F7, // Sent every 10s
        KEEP_ACK = 0xF0FE,
        KEEP_NACK = 0xF0FD,

        CONNECT_PASSWORD = 0xF0F0,
        DISCONNECT = 0xF0F1,
        CENTRAL_STATUS = 0x0B4A,
        GET_MAC = 0x3FAA,
        GET_WEEK_DAY = 0x34A2,
        GET_FIRMWRE_UPDATE = 0x4994,
        GET_FIRMWRE_UPDATE_VERSION = 0x5994,

        UPDATE_DATE = 0x24A1,
        UPDATE_TIME = 0x24AA,

        USER_NAMES = 0x32E0,
        ZONE_TYPES = 0x33A0,
        ZONE_NAMES = 0x33E0,
        ZONE_CONFIGURATION = 0x3660,
        EVENT_POINTER = 0x3003,
        EVENT_LOG = 0x3900,
        CONNECTIONS_STATUS = 0x0B71,

        OPENED_ZONES = 0x0B11,
        TRIGGERED_ZONES = 0x0B12,
        BYPASSED_ZONES = 0x0B13,
        GPRS_SIGNAL_LEVEL = 0x0B72,
        DEVICE_SIGNAL_LEVEL = 0x0B73,
        DEVICE_SENSOR_TYPE = 0x0B74,
        DEVICE_VERSION = 0x0B76,
    }

    public byte[] Header { get; set; }
    public byte[] Data { get; set; }

    public byte[] GetBytes()
    {
        byte[] bytes = new byte[Header.Length + Data.Length + 1];

        Array.Copy(Header, 0, bytes, 0, Header.Length);
        Array.Copy(Data, 0, bytes, Header.Length, Data.Length);

        bytes[^1] = Helpers.ErrorCorrection.CalculateChecksum(bytes);

        return bytes;
    }

    public static DataPacket BuildPacket(Commands command, params byte[] data)
        => BuildPacket((ushort)command, data);
    public static DataPacket BuildPacket(ushort command, byte[] data)
    {
        int dLen = (data?.Length ?? 0) + 2;
        var packet = new DataPacket
        {
            Header = new byte[8]
            {
                0, 0, 0, 0,
                (byte)(dLen >> 8),    // [4]
                (byte)(dLen & 0xFF),  // [5]
                (byte)(command >> 8), // [6]
                (byte)(command & 0xFF)// [7]
            },
            Data = new byte[data?.Length ?? 0]
        };

        if (packet.Data.Length > 0)
        {
            Array.Copy(data, 0, packet.Data, 0, data.Length);
        }

        return packet;
    }

    public virtual void Unpack(byte[] receivedBytes)
    {
        Header = new byte[8];
        Array.Copy(receivedBytes, 0, Header, 0, Header.Length);

        int dataLen = Header[4] * 256 + Header[5] - 2;
        Data = new byte[dataLen];
        if (dataLen > 0)
        {
            Array.Copy(receivedBytes, 8, Data, 0, dataLen);
        }

    }

    protected static bool IsBit(byte value, byte index)
    {
        int mask = 1 << index;
        return (value & mask) != 0;
    }
    protected static byte hexToDec(byte hex)
    {
        var h = hex >> 4;
        var l = hex & 0x0F;

        if (h == 0x0a) h = 0;
        if (l == 0x0a) l = 0;

        return (byte)(h * 10 + l);
    }

}
