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
using System.Windows;
using System.Windows.Media;

namespace Demo
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		private void CompleteSample_Click(object sender, RoutedEventArgs e)
		{
			ShowWindow(() => new CompleteSample.MainWindow(), "Complete Sample");
		}

		private void RectangleCurve_Click(object sender, RoutedEventArgs e)
		{
			ShowWindow(() => new RectangleCurve.MainWindow(), "Rectangle Curve");
		}

		private void BinaryCurve_Click(object sender, RoutedEventArgs e)
		{
			ShowWindow(() => new BinaryCurve.MainWindow(), "Binary Curve");
		}

		private void Legend_Click(object sender, RoutedEventArgs e)
		{
			ShowWindow(() => new TableAndTileLegend.MainWindow(), "Table and Tile Legend");
		}

		private void Printing_Click(object sender, RoutedEventArgs e)
		{
			ShowWindow(() => new Printing.MainWindow(), "Printing");
		}

		private void CurveGap_Click(object sender, RoutedEventArgs e)
		{
			ShowWindow(() => new CurveGaps.MainWindow(), "Curve Gaps");
		}

		private void StaticRuler_Click(object sender, RoutedEventArgs e)
		{
			ShowWindow(() => new Rulers.MainWindow(), "Rulers");
		}

		private void CustomControls_Click(object sender, RoutedEventArgs e)
		{
			ShowWindow(() => new CustomControls.MainWindow(), "Custom Controls");
		}

		private void ShowWindow(Func<Window> createWindow, string useCaseDescription)
		{
			var window = createWindow();
			window.Title = "TechNewLogic GraphIT - " + useCaseDescription;
			window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
			window.Height = 800;
			window.Width = 1000;
			window.Background = Brushes.Black;

			window.ShowDialog();
		}
	}
}
