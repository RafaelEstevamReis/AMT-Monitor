using System;

namespace Simple.AMT.AMTModels
{
    public class ConnectionInfo
    {
        public string IP { get; set; }
        public int Port { get; set; }
        public string Password { get; set; }
        public string Account { get; set; }
        public string Mac { get; set; }

        public byte[] GetPassword()
        {
            byte[] bytes = new byte[6];
            for (int i = 0; i < Password.Length; i++)
            {
                if (Password[i] < '0') throw new InvalidOperationException();
                if (Password[i] > '9') throw new InvalidOperationException();

                bytes[i] = (byte)(Password[i] - '0');
                // I have no idea why I camptured 0x0a for a zero
                if (bytes[i] == 0) bytes[i] = 0x0a;
            }
            return bytes;
        }
    }
}
