using System.Runtime.InteropServices;

namespace Archiver
{
	public class HUFF
	{
		[DllImport("HUFF.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Compress(string input, string output);
		[DllImport("HUFF.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		public static extern void DeCompress(string input, string output);
	}
}