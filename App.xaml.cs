using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace CSharpWpf
{
	/// <summary>
	/// Логика взаимодействия для App.xaml
	/// </summary>
	public partial class App : Application
	{
		public static string Arg;
		private void GetArgs(object sender, StartupEventArgs e)
		{
			if (e.Args.Length > 0)
			{
				if (File.Exists(e.Args[0]))
				{
					Arg = e.Args[0];
					this.StartupUri = new System.Uri(@"..\..\..\Extractor.xaml", System.UriKind.Relative);
				}
				else
				{
					this.StartupUri = new System.Uri(@"..\..\..\MainWindow.xaml", System.UriKind.Relative);
				}
			}
			else
			{
				this.StartupUri = new System.Uri(@"..\..\..\MainWindow.xaml", System.UriKind.Relative);
			}
		}
	}
}
