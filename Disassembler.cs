using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Windows;

namespace CSharpWpf
{
	class Disassembler
	{
		public Disassembler(string p, string o)
		{
			Path = p;
			outputPath = o;
		}
		public void Disassemble()
		{
			byte[] buf;

			string o_Sizes = File.ReadLines(Path).First();
			string o_Names = File.ReadLines(Path).Skip(1).First();
			string[] s_Sizes = o_Sizes.Split(new char[] { ',' });
			long[] Sizes = new long[s_Sizes.Length];
			string[] Names = o_Names.Split(new char[] { ',' });

			for (int i = 0; i < Sizes.Length; i++)
			{
				if (!Int64.TryParse(s_Sizes[i], out Sizes[i]))
				{
					MessageBox.Show("Corupted file", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}

			FileStream fs = new FileStream(Path, FileMode.Open);
			List<FileStream> outFiles = new List<FileStream>();

			foreach (string f in Names)
			{
				outFiles.Add(new FileStream(outputPath + @"\" + f, FileMode.Create));
			}

			fs.Position = o_Names.Length + o_Sizes.Length + 2;

			for (int i = 0; i < Names.Length; i++)
			{
				if (Sizes[i] < Max_Size)
				{
					buf = new byte[Sizes[i]];
					fs.Read(buf, 0, buf.Length);
					outFiles[i].Write(buf, 0, buf.Length);
				}
				else
				{
					buf = new byte[Max_Size];
					long o_Length = Sizes[i];
					while (o_Length > Max_Size)
					{
						fs.Read(buf, 0, buf.Length);
						outFiles[i].Write(buf, 0, buf.Length);
						o_Length -= buf.Length;
					}
					if (o_Length > 0)
					{
						buf = new byte[o_Length];
						fs.Read(buf, 0, buf.Length);
						outFiles[i].Write(buf, 0, buf.Length);
					}
				}
			}

			fs.Close();
			foreach (FileStream f in outFiles)
			{
				f.Close();
			}
		}

		private const int Max_Size = 1073741823;
		private string Path, outputPath;
	}
}
