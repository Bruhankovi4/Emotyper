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

namespace CustomControls
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		private TimeDoubleCurve _curve;

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

			CreateCurveWithGap();

			// We add a custom control to the surface (fixed time-axis and fixed y-screen-coordinate)
			CurveDisplay.TimeDoublePlottingSystem.AddCustomControl(
				DateTime.UtcNow.AddMinutes(-40),
				150,
				new CustomControl
					{
						Text = "I'm a custom control which is placed at absolute date and fixed y-screen-coordinate",
						Width = 150
					});

			// We add a custom control to the surface (pinned to a curve with fixed y-coordinate)
			_curve.AddCustomControl(
				DateTime.UtcNow.AddMinutes(-30),
				350,
				new CustomControl
					{
						Text = "I'm a custom control which is pinned at a curve and fixed y-coordinate",
						Width = 150
					});

			// We add a custom control to the surface (pinned to a curve)
			_curve.AddCustomControl(
				DateTime.UtcNow.AddMinutes(-5),
				new CustomControl
					{
						Text = "I'm a custom control which is pinned at a curve",
						Width = 150
					});

			// We add a custom control to the surface (pinned to a curve at a gap)
			_curve.AddCustomControl(
				DateTime.UtcNow.AddMinutes(-15),
				new CustomControl
					{
						Text = "I'm a custom control which is pinned at a curve at a gap",
						Width = 150
					});
		}

		private void CreateCurveWithGap()
		{
			// Create a new curve
			_curve = CurveDisplay.TimeDoublePlottingSystem.AddCurve(
				"kg", -20, 20, Colors.Blue, RedrawTime.Ms500, AxisMatchingMode.None,
				CurveDrawingMode.SimpleLine(), 
				new FloatingCommaValueFormater(), 
				new InterpolationValueFetchStrategy(),
				AxisFormat.Double,
				int.MaxValue);

			// Create some data and add it to the curve
			var startTime = DateTime.UtcNow.AddMinutes(-40);
			var timeSpan = TimeSpan.FromMinutes(20);
			var data = EntryGenerator.GenerateRandomData(
				startTime,
				timeSpan,
				5000,
				15,
				-15,
				0.2);
			_curve.DataSeries.Append(data);

			// Not, we append a gap
			var latestTime = startTime.Add(timeSpan);
			var undefinedValue = _curve.DataSeries.UndefinedValue;
			var gapEnd = latestTime.AddMinutes(10);
			_curve.DataSeries.Append(
				new[]
					{
						new TimeDoubleDataEntry(latestTime.AddMilliseconds(1), undefinedValue),
						new TimeDoubleDataEntry(gapEnd, _curve.DataSeries.UndefinedValue),
					});

			// Create some additional data after the gap
			var additionalData = EntryGenerator.GenerateRandomData(
				gapEnd.AddMilliseconds(1),
				TimeSpan.FromMinutes(10),
				5000,
				15,
				-15,
				0.2);
			_curve.DataSeries.Append(additionalData);
		}
	}
}
