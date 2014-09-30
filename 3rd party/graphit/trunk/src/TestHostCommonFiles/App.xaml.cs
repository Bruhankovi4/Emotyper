// Copyright 2013 Ronald Schlenker and Andre Krämer.
// 
// This file is part of GraphIT.
// 
// GraphIT is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// GraphIT is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with GraphIT.  If not, see <http://www.gnu.org/licenses/>.
// 
// 

using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace TestHost
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		public App()
		{
			DispatcherUnhandledException += App_DispatcherUnhandledException;
			Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
		}

		private void Application_Startup(object sender, StartupEventArgs e)
		{
			var window = new DemoWindow();
			ShutdownMode = ShutdownMode.OnMainWindowClose;
			MainWindow = window;
			MainWindow.Show();
		}

		void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
		{
			MessageBox.Show(e.Exception.ToString());
		}
	}
}
