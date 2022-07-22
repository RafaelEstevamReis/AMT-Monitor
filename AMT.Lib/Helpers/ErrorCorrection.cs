namespace AMT.Lib.Helpers
{
    public class ErrorCorrection
    {
		public static byte CalculateChecksum(byte[] data)
		{
			byte value = 0;
			foreach (byte d in data)
			{
				value = (byte)(value ^ d);
			}
			return (byte)(value ^ 0xFFu);
		}

	}
}
