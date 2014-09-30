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
using System.Windows.Media;
using Helper;
using TechNewLogic.GraphIT.Hv;
using TechNewLogic.GraphIT.Hv.Vertical;

namespace BinaryCurve
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
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

			// Create a new curve
			var curve = CurveDisplay.TimeDoublePlottingSystem.AddCurve(
				string.Empty, 0, 1, Colors.Yellow, RedrawTime.Ms500, AxisMatchingMode.None, 
				CurveDrawingMode.FilledRectangleHighest(),
				new FloatingCommaValueFormater(),
				new SimpleValueFetchStrategy(), 
				AxisFormat.Double,
				int.MaxValue);
			curve.DoubleAxis.SetBounds(-2, 2);

			// Create some data and add it to the curve
			var data = EntryGenerator.Generatedata(
				DateTime.UtcNow.AddMinutes(-40),
				TimeSpan.FromMinutes(40),
				15);
			curve.DataSeries.Append(data);
		}
	}
}
