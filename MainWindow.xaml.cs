using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using System.Collections.ObjectModel;
using System.IO;

namespace CSharpWpf
{
	/// <summary>
	/// Логика взаимодействия для MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private ObservableCollection<FileList> Files = new ObservableCollection<FileList> { };
		public MainWindow()
		{
			InitializeComponent();
			Lst.ItemsSource = Files;
		}

		private void DragEnterF(object sender, DragEventArgs e)
		{
			BorderDrop.Visibility = Visibility.Visible;
			TxtDrop.Visibility = Visibility.Visible;
		}

		private void DragLeaveF(object sender, DragEventArgs e)
		{
			BorderDrop.Visibility = Visibility.Hidden;
			TxtDrop.Visibility = Visibility.Hidden;
		}

		private void DragOverF(object sender, DragEventArgs e)
		{
			e.Effects = DragDropEffects.None;
			e.Handled = true;
		}

		private void DragOverP(object sender, DragEventArgs e)
		{

			if (e.Data.GetDataPresent(DataFormats.FileDrop, true) == true)
			{
				string[] filenames = (string[])e.Data.GetData(DataFormats.FileDrop, true);
				foreach (string filename in filenames)
				{
					if (File.Exists(filename))
					{
						bool find = false;
						for (int a = 0; a < Files.Count; a++)
						{
							if (Files[a].F_Name == filename)
							{
								find = true;
							}
						}
						if (find)
						{
							e.Effects = DragDropEffects.None;
							break;
						}
						else
						{
							e.Effects = DragDropEffects.All;
						}
					}
					else
					{
						e.Effects = DragDropEffects.None;
						break;
					}
				}
			}
			else
				e.Effects = DragDropEffects.None;
			e.Handled = true;
		}

		private void DropP(object sender, DragEventArgs e)
		{
			string[] filenames = (string[])e.Data.GetData(DataFormats.FileDrop, true);
			foreach (string filename in filenames)
			{
				FileInfo f = new FileInfo(filename);
				Files.Add(new FileList() { Name = f.Name, Size = GetFileSize(f.Length), F_Name = f.FullName });
			}
			
			BorderDrop.Visibility = Visibility.Hidden;
			TxtDrop.Visibility = Visibility.Hidden;
			BtnProceed.IsEnabled = true;
			
			e.Handled = true;
		}

		private string GetFileSize(long sz)
		{
			string type = "Byte";
			double buf = sz;
			if (buf > 1000)
			{
				buf /= 1024;
				type = "KB";
			}
			if (buf > 1000)
			{
				buf /= 1024;
				type = "MB";
			}
			if (buf > 1000)
			{
				buf /= 1024;
				type = "GB";
			}
			return buf.ToString("#.##") + @" " + type;
		}

		private void Lst_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (Lst.SelectedIndex != -1)
			{
				BtnDelete.IsEnabled = true;
			}
			else
			{
				BtnDelete.IsEnabled = false;
			}
		}

		private void BtnDelete_Click(object sender, RoutedEventArgs e)
		{
			Files.Remove((FileList)Lst.SelectedItem);
			if (Files.Count == 0)
			{
				BtnProceed.IsEnabled = false;
			}
		}

		private void SetAllDisabled()
		{
			Lst.IsEnabled = false;
			BtnDelete.IsEnabled = false;
			BtnProceed.IsEnabled = false;
			this.AllowDrop = false;
		}

		public void SetAllEnabled()
		{
			Lst.IsEnabled = true;
			BtnDelete.IsEnabled = true;
			BtnProceed.IsEnabled = true;
			this.AllowDrop = true;
		}

		private void BtnCompr_Click(object sender, RoutedEventArgs e)
		{
			SetAllDisabled();
			Compressor CompressWin = new Compressor(new List<FileList>(Files));
			CompressWin.Owner = this;
			CompressWin.Show();
		}
	}
}
