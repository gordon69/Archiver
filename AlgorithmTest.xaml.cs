using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using Archiver;

namespace CSharpWpf
{
	/// <summary>
	/// Логика взаимодействия для AlgorithmTest.xaml
	/// </summary>
	public partial class AlgorithmTest : Window
	{
		public AlgorithmTest(List<string> ls)
		{
			InitializeComponent();
			this.Closing += WhenClosing;
			Files = ls;
		}

		private void WhenClosing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			App.Current.Windows.OfType<Compressor>().ElementAt(0).SetAllEnable();
			App.Current.Windows.OfType<Compressor>().ElementAt(0).Activate();
			e.Cancel = false;
		}

		private List<string> Files;
		private Thread Thr0;

		private void Btn_start_test_Click(object sender, RoutedEventArgs e)
		{
			Btn_start_test.IsEnabled = false;
			Thr0 = new Thread(new ParameterizedThreadStart(Starter));
			Thr0.IsBackground = false;
			Thr0.Start(Files);
		}

		void Starter(object o)
		{
			DateTime Start;
			DateTime Stop;
			TimeSpan Time = new TimeSpan();

			FileInfo fi1, fi2;
			double kof;
			List<string> ls = (List<string>)o;
			Assembler asm = new Assembler(ls);
			asm.assemble(@".\tmp\$tmp");

			Start = DateTime.Now;
			LZSS.Compress(@".\tmp\$tmp", @".\tmp\$tmp1");
			Stop = DateTime.Now;
			Time = Stop.Subtract(Start);
			fi1 = new FileInfo(@".\tmp\$tmp");
			fi2 = new FileInfo(@".\tmp\$tmp1");
			kof = (double)fi1.Length / (double)fi2.Length;
			Dispatcher.Invoke(() => {
				Lable_LZSS.Content = String.Format("{0:0.###}", kof);
			});
			if (Time.Seconds > 0)
				Dispatcher.Invoke(() => {
					Lable_LZSS_time.Content = Time.Seconds.ToString() + " s";
				});
			else
				Dispatcher.Invoke(() => {
					Lable_LZSS_time.Content = Time.Milliseconds.ToString() + " ms";
				});
			File.Delete(@".\tmp\$tmp1");

			Start = DateTime.Now;
			HUFF.Compress(@".\tmp\$tmp", @".\tmp\$tmp1");
			Stop = DateTime.Now;
			Time = Stop.Subtract(Start);
			fi1 = new FileInfo(@".\tmp\$tmp");
			fi2 = new FileInfo(@".\tmp\$tmp1");
			kof = (double)fi1.Length / (double)fi2.Length;
			Dispatcher.Invoke(() => {
				Lable_HUFF.Content = String.Format("{0:0.###}", kof);
			});
			if(Time.Seconds > 0)
				Dispatcher.Invoke(() => {
					Lable_HUFF_time.Content = Time.Seconds.ToString() + " s";
				});
			else
				Dispatcher.Invoke(() => {
					Lable_HUFF_time.Content = Time.Milliseconds.ToString() + " ms";
				});
			File.Delete(@".\tmp\$tmp1");

			Start = DateTime.Now;
			LZSS.Compress(@".\tmp\$tmp", @".\tmp\$tmp1");
			HUFF.Compress(@".\tmp\$tmp1", @".\tmp\$tmp2");
			Stop = DateTime.Now;
			Time = Stop.Subtract(Start);
			fi1 = new FileInfo(@".\tmp\$tmp");
			fi2 = new FileInfo(@".\tmp\$tmp2");
			kof = (double)fi1.Length / (double)fi2.Length;
			Dispatcher.Invoke(() => {
				Lable_LZSS_HUFF.Content = String.Format("{0:0.###}", kof);
			});
			if (Time.Seconds > 0)
				Dispatcher.Invoke(() => {
					Lable_LZSS_HUFF_time.Content = Time.Seconds.ToString() + " s";
				});
			else
				Dispatcher.Invoke(() => {
					Lable_LZSS_HUFF_time.Content = Time.Milliseconds.ToString() + " ms";
				});
			File.Delete(@".\tmp\$tmp1");
			File.Delete(@".\tmp\$tmp2");

			Start = DateTime.Now;
			HUFF.Compress(@".\tmp\$tmp", @".\tmp\$tmp1");
			LZSS.Compress(@".\tmp\$tmp1", @".\tmp\$tmp2");
			Stop = DateTime.Now;
			Time = Stop.Subtract(Start);
			fi1 = new FileInfo(@".\tmp\$tmp");
			fi2 = new FileInfo(@".\tmp\$tmp2");
			kof = (double)fi1.Length / (double)fi2.Length;
			Dispatcher.Invoke(() => {
				Lable_HUFF_LZSS.Content = String.Format("{0:0.###}", kof);
			});
			if (Time.Seconds > 0)
				Dispatcher.Invoke(() => {
					Lable_HUFF_LZSS_time.Content = Time.Seconds.ToString() + " s";
				});
			else
				Dispatcher.Invoke(() => {
					Lable_HUFF_LZSS_time.Content = Time.Milliseconds.ToString() + " ms";
				});
			File.Delete(@".\tmp\$tmp1");
			File.Delete(@".\tmp\$tmp2");

			File.Delete(@".\tmp\$tmp");
		}
	}
}
