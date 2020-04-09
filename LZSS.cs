using System.Runtime.InteropServices;


namespace Archiver
{
	public class LZSS
	{
		[DllImport("LZSS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Compress(string input, string output);
		[DllImport("LZSS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		public static extern void DeCompress(string input, string output);
	}
}
