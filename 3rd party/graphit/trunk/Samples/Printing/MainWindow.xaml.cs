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
using System.Windows.Controls;
using System.Windows.Media;
using Helper;
using TechNewLogic.GraphIT.Hv;
using TechNewLogic.GraphIT.Hv.Vertical;
using TechNewLogic.GraphIT.Printing;

namespace Printing
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			Loaded += (s, e) => new InfoControl(this).Show();
			SplashScreenHelper.Show();
			InitializeComponent();
			SplashScreenHelper.Close();

			// Set the bounds of the time axis
			CurveDisplay.TimeDoublePlottingSystem.TimeAxis.SetBounds(
				DateTime.UtcNow.AddMinutes(-50),
				DateTime.UtcNow.AddMinutes(10));

			AddCurve();
		}

		private void AddCurve()
		{
			// Create a new curve
			var curve = CurveDisplay.TimeDoublePlottingSystem.AddCurve(
				"kg", -100, 100, Colors.Blue, RedrawTime.Ms500, AxisMatchingMode.None, 
				CurveDrawingMode.SimpleLine(), 
				new FloatingCommaValueFormater(), 
				new InterpolationValueFetchStrategy(),
				AxisFormat.Double,
				int.MaxValue);

			// Create some data and add it to the curve
			var data = EntryGenerator.Generatedata(
				90,
				0,
				DateTime.UtcNow.AddMinutes(-40),
				TimeSpan.FromMinutes(40),
				2000);
			curve.DataSeries.Append(data);
		}

		private void PrintPreview_Click(object sender, RoutedEventArgs e)
		{
			// We add a dummy textbox as header for the print page
			var dummyTb = new TextBlock
			{
				Text = "Date: " + DateTime.Now,
				Margin = new Thickness(20),
				FontSize = 20,
				TextAlignment = TextAlignment.Center
			};
			CurveDisplay.ShowPrintPreview(new PrintContent(dummyTb, Dock.Top));
		}

		private void Print_Click(object sender, RoutedEventArgs e)
		{
			// We add a dummy textbox as header for the print page
			var dummyTb = new TextBlock
			{
				Text = "Date: " + DateTime.Now,
				Margin = new Thickness(20),
				FontSize = 20,
				TextAlignment = TextAlignment.Center
			};
			CurveDisplay.Print(20, new PrintContent(dummyTb, Dock.Top));
		}
	}
}
