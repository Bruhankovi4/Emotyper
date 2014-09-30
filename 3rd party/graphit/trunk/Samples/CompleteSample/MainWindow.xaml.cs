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

using System;
using System.ComponentModel;
using System.Windows;
using Helper;

namespace CompleteSample
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, IResetDemo
	{
		public MainWindow()
		{
			Loaded += (s, e) => new InfoControl(this).Show();
			SplashScreenHelper.Show();
			InitializeComponent();
			SplashScreenHelper.Close();

			Reset();
		}

		public void Reset()
		{
			DisposeCurveDisplay();

			Content = new DemoControl(this);
			GC.Collect();
		}

		private void DisposeCurveDisplay()
		{
			if (Content is DemoControl)
				((DemoControl)Content).Dispose();
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);
			DisposeCurveDisplay();
		}
	}
}
