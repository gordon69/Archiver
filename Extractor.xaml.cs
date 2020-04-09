using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Archiver;

namespace CSharpWpf
{
    /// <summary>
    /// Логика взаимодействия для Extractor.xaml
    /// </summary>
    public partial class Extractor : Window
    {
		private string InPath;

		private bool Running = false, Done = false;

		private Thread Thr;

		public Extractor()
        {
			InPath = App.Arg;
			
            InitializeComponent();
			V_F.Visibility = Visibility.Hidden;
			BtnExtr.IsEnabled = false;
			Closing += WhenClosing;
        }

		private void WhenClosing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (!Done)
			{
				if(System.Windows.MessageBox.Show("Are you sure?", "Canceling", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
				{
					e.Cancel = false;
				}
				else
				{
					e.Cancel = true;
				}
			}
			else
			{
				e.Cancel = false;
			}
		}

		private void TextChanged(object sender, TextChangedEventArgs e)
		{
			if (Directory.Exists(TxtFolder.Text))
			{
				V_F.Visibility = Visibility.Visible;
				X_F.Visibility = Visibility.Hidden;

				BtnExtr.IsEnabled = true;
				
			}
			else
			{
				V_F.Visibility = Visibility.Hidden;
				X_F.Visibility = Visibility.Visible;

				BtnExtr.IsEnabled = false;
			}
		}

		private void BtnCancel_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void BtnFolder_Click(object sender, RoutedEventArgs e)
		{
			FolderBrowserDialog Dialog = new FolderBrowserDialog() { ShowNewFolderButton = true };
			if (Dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				TxtFolder.Text = Dialog.SelectedPath;
			}
		}

		private void BtnExtr_Click(object sender, RoutedEventArgs e)
		{

			Progr.Visibility = Visibility.Visible;
			BtnExtr.IsEnabled = false;
			BtnCancel.IsEnabled = false;
			BtnFolder.IsEnabled = false;
			TxtFolder.IsEnabled = false;

			Running = true;
			FileStream FInp = new FileStream(InPath, FileMode.Open, FileAccess.Read);
			
			FileInfo fi = new FileInfo(InPath);
			string Pth = fi.DirectoryName + @"$tmps";
			FileStream FOut = new FileStream(Pth, FileMode.Create, FileAccess.Write);

			for(long i = 0; i < FInp.Length - 1; i++)
				FOut.WriteByte(Convert.ToByte(FInp.ReadByte()));
			Pack_ p = new Pack_ { Pth = Pth, Ptr = TxtFolder.Text, Algo = Convert.ToByte(FInp.ReadByte()) };
			FOut.Close();
			FInp.Close();

			Thr = new Thread(new ParameterizedThreadStart(ThreadHandl));
			Thr.IsBackground = false;
			Thr.Start(p);
			Udt();
		}

		void ThreadHandl(object o)
		{
			Pack_ pk = (Pack_)o;

			if(pk.Algo == 0)
			{
				Disassembler ds = new Disassembler(pk.Pth, pk.Ptr);
				ds.Disassemble();
			}
			if(pk.Algo == 1)
			{
				LZSS.DeCompress(pk.Pth, pk.Ptr + @"\$tmp");
				Disassembler ds = new Disassembler(pk.Ptr + @"\$tmp", pk.Ptr);
				ds.Disassemble();
				File.Delete(pk.Ptr + @"\$tmp");
			}
			if (pk.Algo == 2)
			{
				HUFF.DeCompress(pk.Pth, pk.Ptr + @"\$tmp");
				Disassembler ds = new Disassembler(pk.Ptr + @"\$tmp", pk.Ptr);
				ds.Disassemble();
				File.Delete(pk.Ptr + @"\$tmp");
			}
			if (pk.Algo == 3)
			{
				HUFF.DeCompress(pk.Pth, pk.Ptr + @"\$tmp");
				LZSS.DeCompress(pk.Ptr + @"\$tmp", pk.Ptr + @"\$tmph");
				Disassembler ds = new Disassembler(pk.Ptr + @"\$tmph", pk.Ptr);
				ds.Disassemble();
				File.Delete(pk.Ptr + @"\$tmp");
				File.Delete(pk.Ptr + @"\$tmph");
			}
			if (pk.Algo == 4)
			{
				LZSS.DeCompress(pk.Pth, pk.Ptr + @"\$tmp");
				HUFF.DeCompress(pk.Ptr + @"\$tmp", pk.Ptr + @"\$tmph");
				Disassembler ds = new Disassembler(pk.Ptr + @"\$tmph", pk.Ptr);
				ds.Disassemble();
				File.Delete(pk.Ptr + @"\$tmp");
				File.Delete(pk.Ptr + @"\$tmph");
			}
			/*C:\Users\Gordon\Desktop\dsm_test\test1.xyz*/
			File.Delete(pk.Pth);

			Running = false;
		}

		async void Udt()
		{
			await Task.Run(() => {
				DateTime Start = DateTime.Now;
				DateTime Stop;
				TimeSpan Time = new TimeSpan();
				while (Running)
				{
					Task.Delay(5);
				}
				Dispatcher.Invoke(() => { Progr.Visibility = Visibility.Hidden; });
				Stop = DateTime.Now;
				Time = Stop.Subtract(Start);
				System.Windows.MessageBox.Show("Done in " + Convert.ToString(Time.Seconds) + " s.", "Extraction", MessageBoxButton.OK, MessageBoxImage.Information);

				Dispatcher.Invoke(() => {
					Done = true;
					Close();
				});
			});
		}
	}
}
