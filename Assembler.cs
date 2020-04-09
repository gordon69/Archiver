using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace CSharpWpf
{
	class Assembler
	{
		public Assembler(List<string> fls)
		{
			pth = fls;
		}

		public void assemble(string path)
		{
			files = new List<FileStream> { };
			for(int i = 0; i < pth.Count; i++)
			{
				files.Add(new FileStream(pth[i], FileMode.Open));
			}

			FileStream fs = new FileStream(path, FileMode.Create);

			string str = "";
			for (int i = 0; i < files.Count - 1; i++)
			{
				str += (files[i].Length.ToString() + ",");
			}
			str += (files[files.Count - 1].Length.ToString() + "\n");

			byte[] buf = Encoding.UTF8.GetBytes(str);

			fs.Write(buf, 0, buf.Length);
			/////////////////////////////////////////////////////////////////
			str = "";
			for (int i = 0; i < files.Count - 1; i++)
			{
				str += (Path.GetFileName(files[i].Name) + ",");
			}
			str += (Path.GetFileName(files[files.Count - 1].Name) + "\n");

			buf = Encoding.UTF8.GetBytes(str);

			fs.Write(buf, 0, buf.Length);
			/////////////////////////////////////////////////////////////////

			foreach (FileStream f in files)
			{
				if (f.Length < Max_Size)
				{
					buf = new byte[f.Length];
					f.Read(buf, 0, Convert.ToInt32(f.Length));
					fs.Write(buf, 0, buf.Length);
				}
				else
				{
					long o_length = f.Length;
					buf = new byte[Max_Size];
					while (o_length > Max_Size)
					{
						f.Read(buf, 0, buf.Length);
						fs.Write(buf, 0, buf.Length);
						o_length -= buf.Length;
					}
					if (o_length > 0)
					{
						buf = new byte[o_length];
						f.Read(buf, 0, buf.Length);
						fs.Write(buf, 0, buf.Length);
					}
				}
			}
			
			fs.Close();
			for(int i = files.Count - 1; i >= 0; i--)
			{
				files[i].Close();
			}
		}

		private List<FileStream> files;

		private List<string> pth;

		private const int Max_Size = 1073741823;
	}
}
