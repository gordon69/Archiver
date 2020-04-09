using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Archiver;

namespace CSharpWpf
{
	/// <summary>
	/// Логика взаимодействия для Compressor.xaml
	/// </summary>
	public partial class Compressor : Window
	{
		private List<FileList> files;
		private bool Running = false;

		private Thread Thr;

		public Compressor(List<FileList> fs)
		{
			InitializeComponent();
			this.Closing += WhenClosing;
			files = fs;
			V_F.Visibility = Visibility.Hidden;
			V_N.Visibility = Visibility.Hidden;
			BtnArch.IsEnabled = false;
		}

		private void WhenClosing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			App.Current.Windows.OfType<MainWindow>().ElementAt(0).SetAllEnabled();
			App.Current.Windows.OfType<MainWindow>().ElementAt(0).Activate();
			e.Cancel = false;
		}

		private void TextChanged(object sender, TextChangedEventArgs e)
		{
			if (Directory.Exists(TxtFolder.Text))
			{
				V_F.Visibility = Visibility.Visible;
				X_F.Visibility = Visibility.Hidden;

				if (!File.Exists(TxtFolder.Text + @"\" + TxtName.Text + @".xyz") && TxtName.Text != null && TxtName.Text != "")
				{
					V_N.Visibility = Visibility.Visible;
					X_N.Visibility = Visibility.Hidden;

					BtnArch.IsEnabled = true;
				}
				else
				{
					V_N.Visibility = Visibility.Hidden;
					X_N.Visibility = Visibility.Visible;

					BtnArch.IsEnabled = false;
				}
			}
			else
			{
				V_F.Visibility = Visibility.Hidden;
				X_F.Visibility = Visibility.Visible;

				V_N.Visibility = Visibility.Hidden;
				X_N.Visibility = Visibility.Visible;

				BtnArch.IsEnabled = false;
			}
		}

		private void BtnCancel_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void BtnArch_Click(object sender, RoutedEventArgs e)
		{
			Progr.Visibility = Visibility.Visible;
			BtnArch.IsEnabled = false;
			BtnCancel.IsEnabled = false;
			BtnFolder.IsEnabled = false;
			TxtFolder.IsEnabled = false;
			TxtName.IsEnabled = false;
			SliderCompress.IsEnabled = false;

			Running = true;

			List<string> fs = new List<string> { };
			for (int i = 0; i < files.Count; i++)
			{
				fs.Add(files[i].F_Name);
			}
			Pack p = new Pack { Pth = TxtFolder.Text + @"\" + TxtName.Text, Ptr = fs };

			if (SliderCompress.Value < 1)
			{
				p.Algo = 0;
			}
			else if (SliderCompress.Value > 0 && SliderCompress.Value < 2)
			{
				p.Algo = 1;
			}
			else if (SliderCompress.Value > 1 && SliderCompress.Value < 3)
			{
				p.Algo = 2;
			}
			else if (SliderCompress.Value > 2 && SliderCompress.Value < 4)
			{
				p.Algo = 3;
			}
			else
			{
				p.Algo = 4;
			}

			Thr = new Thread(new ParameterizedThreadStart(ThreadHandl));
			Thr.IsBackground = false;
			Thr.Start(p);
			Udt();
		}

		private void BtnFolder_Click(object sender, RoutedEventArgs e)
		{
			FolderBrowserDialog Dialog = new FolderBrowserDialog() { ShowNewFolderButton = true };
			if (Dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				TxtFolder.Text = Dialog.SelectedPath;
			}
		}

		void ThreadHandl(object o)
		{
			Pack pk = (Pack)o;
			Assembler a = new Assembler(pk.Ptr);

			if(pk.Algo == 0)
			{
				a.assemble(pk.Pth + ".xyz");
			}
			if(pk.Algo == 1)
			{
				a.assemble(pk.Pth);

				LZSS.Compress(pk.Pth, pk.Pth + ".xyz");

				File.Delete(pk.Pth);
			}
			if (pk.Algo == 2)
			{
				a.assemble(pk.Pth);

				HUFF.Compress(pk.Pth, pk.Pth + ".xyz");

				File.Delete(pk.Pth);
			}
			if (pk.Algo == 3)
			{
				a.assemble(pk.Pth);

				LZSS.Compress(pk.Pth, pk.Pth + ".x");
				HUFF.Compress(pk.Pth + ".x", pk.Pth + ".xyz");

				File.Delete(pk.Pth);
				File.Delete(pk.Pth + ".x");
			}
			if (pk.Algo == 4)
			{
				a.assemble(pk.Pth);

				HUFF.Compress(pk.Pth, pk.Pth + ".x");
				LZSS.Compress(pk.Pth + ".x", pk.Pth + ".xyz");

				File.Delete(pk.Pth);
				File.Delete(pk.Pth + ".x");
			}

			FileStream ou = new FileStream(pk.Pth + ".xyz", FileMode.Append);
			ou.WriteByte(pk.Algo);
			ou.Close();

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
				System.Windows.MessageBox.Show("Done in " + Convert.ToString(Time.Seconds) + " s.", "Compression", MessageBoxButton.OK, MessageBoxImage.Information);

				Dispatcher.Invoke(() => { Close(); });
			});
		}

		private void SliderCompress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if(e.NewValue < 1)
			{
				LableCompress.Content = "Without compress";
			}
			else if(e.NewValue > 0 && e.NewValue < 2)
			{
				LableCompress.Content = "LZSS algorithm";
			}
			else if(e.NewValue > 1 && e.NewValue < 3)
			{
				LableCompress.Content = "Huffman algorithm";
			}
			else if(e.NewValue > 2 && e.NewValue < 4)
			{
				LableCompress.Content = "LZSS + Huffman algorithm";
			}
			else
			{
				LableCompress.Content = "Huffman + LZSS algorithm";
			}
		}

		private void Btn_TestAlgorithm_Click(object sender, RoutedEventArgs e)
		{
			BtnArch.IsEnabled = false;
			BtnCancel.IsEnabled = false;
			BtnFolder.IsEnabled = false;
			TxtFolder.IsEnabled = false;
			TxtName.IsEnabled = false;
			SliderCompress.IsEnabled = false;

			List<string> fs = new List<string> { };
			for (int i = 0; i < files.Count; i++)
			{
				fs.Add(files[i].F_Name);
			}

			AlgorithmTest wf = new AlgorithmTest(fs);
			wf.Show();
		}

		public void SetAllEnable()
		{
			BtnArch.IsEnabled = true;
			BtnCancel.IsEnabled = true;
			BtnFolder.IsEnabled = true;
			TxtFolder.IsEnabled = true;
			TxtName.IsEnabled = true;
			SliderCompress.IsEnabled = true;
		}
	}
}