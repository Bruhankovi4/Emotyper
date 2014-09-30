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
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using TechNewLogic.GraphIT.Hv;
using TechNewLogic.GraphIT.Hv.Legend;
using TechNewLogic.GraphIT.Hv.Vertical;
using TechNewLogic.GraphIT.Printing;

namespace TestHost
{
	/// <summary>
	/// Interaction logic for DemoControl.xaml
	/// </summary>
	public partial class DemoControl : IDisposable
	{
		private static readonly Random _random;

		static DemoControl()
		{
			_random = new Random((int)DateTime.Now.Ticks);
		}

		public DemoControl(IResetDemo resetDemo)
		{
			_resetDemo = resetDemo;

			InitializeComponent();

			//GuardReleaseVersion();

			_startTime = new DateTime(2012, 3, 25, 0, 0, 0);
			//_startTime = DateTime.UtcNow;
			CurveDisplay.TimeDoublePlottingSystem.TimeAxis.SetBounds(
				//new DateTime(2012, 3, 25, 0, 0, 0),
				//new DateTime(2012, 3, 25, 5, 0, 0));
				_startTime,
				_startTime.AddHours(5));

			_onlineOfflineLegend = new OnlineOfflineLegend(
				CurveDisplay,
				"AKZ",
				"Location",
				"Aggregate",
				"Medium",
				"Physical");
			legendHolder.Content = _onlineOfflineLegend;

			_refreshCurveDataTimer = new DispatcherTimer();
			_refreshCurveDataTimer.Interval = TimeSpan.FromSeconds(3);
			_refreshCurveDataTimer.Tick += refreshCurveDataTimer_Tick;
			_refreshCurveDataTimer.Start();

			var curve = AddCurve("%", 0, 100, Colors.Green, RedrawTime.Ms500, "R2033", "House 20", "BHKW", "Istwert", "Power Calc.", CurveDrawingMode.SimpleLine(), new FloatingCommaValueFormater(), new InterpolationValueFetchStrategy(), AxisFormat.Double);
			GenerateData(curve, false);
			curve = AddCurve("m³/h", 0, 10, Colors.Yellow, RedrawTime.Ms500, "U2031", "House 20", "BHKW", "Gas", "Quantity", CurveDrawingMode.SimpleLine(), new FloatingCommaValueFormater(), new InterpolationValueFetchStrategy(), AxisFormat.Double);
			GenerateData(curve, false);
			curve = AddCurve("kW", 0, 100, Colors.Red, RedrawTime.Ms500, "U2002", "House 20", "BHKW", "Heater", "Heating Actual", CurveDrawingMode.SimpleLine(), new FloatingCommaValueFormater(), new InterpolationValueFetchStrategy(), AxisFormat.Double);
			GenerateData(curve, false);
			curve = AddCurve("°C", -20, 20, Colors.Blue, RedrawTime.Ms500, "T2051", "House 20", "Outer Temp.", "Curr. Value", "Temperature", CurveDrawingMode.FilledRectangleAuto(), new FloatingCommaValueFormater(), new InterpolationValueFetchStrategy(), AxisFormat.Double);
			GenerateData(curve, false);
			curve = AddCurve("°C", 20, 60, Colors.Orange, RedrawTime.Ms500, "T2051", "House 20", "Outer Temp.", "Curr. Value", "Temperature", CurveDrawingMode.FilledRectangleHighest(), new FloatingCommaValueFormater(), new InterpolationValueFetchStrategy(), AxisFormat.Double);
			GenerateData(curve, false);
			curve = AddCurve("°C", -0.2, 1.2, Colors.PaleTurquoise, RedrawTime.Ms500, "T2051", "House 20", "Outer Temp.", "Curr. Value", "Temperature", CurveDrawingMode.FilledRectangleHighest(), new BinaryValueFormater(), new SimpleValueFetchStrategy(), AxisFormat.Double);
			GenerateBinaryData(curve);
			CurveDisplay.TimeDoublePlottingSystem.AxesGroups.First(it => it.DoubleAxes.Contains(curve.DoubleAxis)).IgnoreCanvasMovementOrZoom = true;

			//var vScaleByAxis = CurveDisplay.TimeDoublePlottingSystem.GetVScaleByAxis(curve.DoubleAxis);
			//vScaleByAxis.AxesGroup.IgnoreCanvasMovementOrZoom = true;
			//vScaleByAxis.EnableUserInput = false;

			//CurveDisplay.TimeDoublePlottingSystem.TimeAxis.BoundsChangedThrottled += (s, e) => Thread.Sleep(5000);
			//CurveDisplay.TimeDoublePlottingSystem.TimeAxis.BoundsChangedThrottled += (s, e) => 
			//    CurveDisplay.Curves.OfType<TimeDoubleCurve>().First().DataSeries.Append(new[]
			//    {
			//        new TimeDoubleDataEntry(_startTime, 100),
			//        new TimeDoubleDataEntry(_startTime.AddMinutes(10), 90),
			//        new TimeDoubleDataEntry(_startTime.AddMinutes(15), double.NaN),
			//        new TimeDoubleDataEntry(_startTime.AddMinutes(20), 80),
			//        new TimeDoubleDataEntry(_startTime.AddMinutes(30), 70),
			//    });
		}

		private readonly IResetDemo _resetDemo;
		private readonly DispatcherTimer _refreshCurveDataTimer;

		private readonly DateTime _startTime;
		private readonly Dictionary<TimeDoubleCurve, DataFactory> _curves
			= new Dictionary<TimeDoubleCurve, DataFactory>();

		private readonly OnlineOfflineLegend _onlineOfflineLegend;

		private int _globalCurvesCounter;

		//private void GuardReleaseVersion()
		//{
		//    var type = typeof(CurveDisplay);
		//    var releaseBgName = "Release_BG";
		//    var isReleaseBg = type.Assembly
		//        .GetCustomAttributes(false)
		//        .OfType<AssemblyConfigurationAttribute>()
		//        .Where(it => it.Configuration == releaseBgName)
		//        .Any();
		//    if (!isReleaseBg)
		//        MessageBox.Show("It seems that the GraphIT assembly was not built using the corrent '" + releaseBgName + "' configuration.");
		//}

		private TimeDoubleCurve AddCurve(
			string uom,
			double min,
			double max,
			Color color,
			RedrawTime redrawTime,
			string descriptionText1,
			string descriptionText2,
			string descriptionText3,
			string descriptionText4,
			string descriptionText5,
			CurveDrawingMode curveDrawingMode,
			IValueFormater valueFormater,
			IValueFetchStrategy valueFetchStrategy,
			AxisFormat axisFormat)
		{
			const int maxCurves = 20;
			if (_curves.Count >= maxCurves)
			{
				MessageBox.Show("You cannot add more than " + maxCurves + " curves.");
				return null;
			}

			_globalCurvesCounter++;

			var curve = CurveDisplay.TimeDoublePlottingSystem.AddCurve(
				uom,
				min,
				max,
				color,
				redrawTime,
				AxisMatchingMode.UomOnly,
				curveDrawingMode,
				valueFormater,
				valueFetchStrategy,
				axisFormat,
				5000);

			curve.Description.DescriptionText1 = descriptionText1;
			curve.Description.DescriptionText2 = descriptionText2;
			curve.Description.DescriptionText3 = descriptionText3;
			curve.Description.DescriptionText4 = descriptionText4;
			curve.Description.DescriptionText5 = descriptionText5;

			return curve;
		}

		private void GenerateData(TimeDoubleCurve curve, bool randomData)
		{
			var min = curve.DoubleAxis.ActualLowerBound;
			var max = curve.DoubleAxis.ActualUpperBound;
			var dataFactory = DataFactory.Attach(
				curve,
				_startTime,
				min,
				max,
				1,
				(max - min) / 500,
				(max - min) / 100,
				(max - min) / 2);

			// all in all: 50min
			var first = _random.Next(5, 30);
			var second = _random.Next(5, 50 - first - 5);
			second = second > 15 ? 15 : second;
			var third = 50 - first - second;
			if (!randomData)
				dataFactory.AppendSine(TimeSpan.FromMinutes(50), (_curves.Count + 1) / 15d);
			else
			{
				dataFactory.AppendPoints(TimeSpan.FromMinutes(first));
				dataFactory.AppendGap(TimeSpan.FromMinutes(second));
				dataFactory.AppendPoints(TimeSpan.FromMinutes(third));
			}

			curve.MinMinBelt = -0.9 + dataFactory.Offset;
			curve.MinBelt = -0.4 + dataFactory.Offset;
			curve.MaxBelt = 0.3 + dataFactory.Offset;
			curve.MaxMaxBelt = 0.7 + dataFactory.Offset;

			//// Belt
			//if (_curves.Count == 1)
			//{
			//    curve.MinMinBelt = -0.9 + dataFactory.Offset;
			//    curve.MinBelt = -0.4 + dataFactory.Offset;
			//    curve.MaxBelt = 0.3 + dataFactory.Offset;
			//    curve.MaxMaxBelt = 0.7 + dataFactory.Offset;
			//}

			//_curves.Add(curve, dataFactory);
		}

		private void GenerateBinaryData(TimeDoubleCurve curve)
		{
			var min = curve.DoubleAxis.ActualLowerBound;
			var max = curve.DoubleAxis.ActualUpperBound;
			var dataFactory = DataFactory.Attach(
				curve,
				_startTime,
				min,
				max,
				0.01,
				(max - min) / 500,
				(max - min) / 100,
				(max - min) / 2);

			// all in all: 50min
			dataFactory.AppendBinary(TimeSpan.FromMinutes(50));

			//curve.MinMinBelt = -0.9 + dataFactory.Offset;
			//curve.MinBelt = -0.4 + dataFactory.Offset;
			//curve.MaxBelt = 0.3 + dataFactory.Offset;
			//curve.MaxMaxBelt = 0.7 + dataFactory.Offset;

			//_curves.Add(curve, dataFactory);
		}

		private void GenerateSimpleData(TimeDoubleCurve curve)
		{
			curve.DataSeries.Append(new[]
				{
					new TimeDoubleDataEntry(_startTime, 100),
					new TimeDoubleDataEntry(_startTime.AddMinutes(10), 90),
					new TimeDoubleDataEntry(_startTime.AddMinutes(15), double.NaN),
					new TimeDoubleDataEntry(_startTime.AddMinutes(20), 80),
					new TimeDoubleDataEntry(_startTime.AddMinutes(30), 70),
				});
		}

		public string ApplicationTitle
		{
			get { return "CLR Runtime:   " + Environment.Version; }
		}

		void refreshCurveDataTimer_Tick(object sender, EventArgs e)
		{
			if (!CurveDisplay.TimeDoublePlottingSystem.IsOnline)
				return;
			var upperBound = CurveDisplay.TimeDoublePlottingSystem.TimeAxis.ActualUpperBound;
			foreach (var it in _curves)
			{
				var lastEntry = it.Key.DataSeries.LogicalEntries.LastOrDefault();
				if (lastEntry == null)
					continue;
				var diff = upperBound - lastEntry.X;
				if (diff <= TimeSpan.Zero)
					continue;
				it.Value.AppendPoints(TimeSpan.FromSeconds(diff.TotalSeconds));
			}
		}

		private void Print_Click(object sender, RoutedEventArgs e)
		{
			var dummyTb = new TextBlock
			{
				Text = "Date: " + DateTime.Now,
				Margin = new Thickness(20),
				FontSize = 20,
				TextAlignment = TextAlignment.Center
			};

			var legend = new TableLegend(
				CurveDisplay,
				"AKZ",
				"Location",
				"Aggregate",
				"Medium",
				"Physical");
			legend.ColumnConfiguration.ShowAvgColumn = _onlineOfflineLegend.TableLegend.ColumnConfiguration.ShowAvgColumn;
			legend.ColumnConfiguration.ShowDeltaTColumn = _onlineOfflineLegend.TableLegend.ColumnConfiguration.ShowDeltaTColumn;
			legend.ColumnConfiguration.ShowDeltaYColumn = _onlineOfflineLegend.TableLegend.ColumnConfiguration.ShowDeltaYColumn;
			legend.ColumnConfiguration.ShowMaxColumn = _onlineOfflineLegend.TableLegend.ColumnConfiguration.ShowMaxColumn;
			legend.ColumnConfiguration.ShowMinColumn = _onlineOfflineLegend.TableLegend.ColumnConfiguration.ShowMinColumn;

			CurveDisplay.Print(
				30,
				new PrintContent(dummyTb, Dock.Top));
			//new PrintContent(legend, Dock.Bottom));
		}

		private void PrintPreview_Click(object sender, RoutedEventArgs e)
		{
			var dummyTb = new TextBlock
			{
				Text = "Date: " + DateTime.Now,
				Margin = new Thickness(20),
				FontSize = 20,
				TextAlignment = TextAlignment.Center
			};

			var legend = new TableLegend(
				CurveDisplay,
				"AKZ",
				"Location",
				"Aggregate",
				"Medium",
				"Physical");
			legend.ColumnConfiguration.ShowAvgColumn = _onlineOfflineLegend.TableLegend.ColumnConfiguration.ShowAvgColumn;
			legend.ColumnConfiguration.ShowDeltaTColumn = _onlineOfflineLegend.TableLegend.ColumnConfiguration.ShowDeltaTColumn;
			legend.ColumnConfiguration.ShowDeltaYColumn = _onlineOfflineLegend.TableLegend.ColumnConfiguration.ShowDeltaYColumn;
			legend.ColumnConfiguration.ShowMaxColumn = _onlineOfflineLegend.TableLegend.ColumnConfiguration.ShowMaxColumn;
			legend.ColumnConfiguration.ShowMinColumn = _onlineOfflineLegend.TableLegend.ColumnConfiguration.ShowMinColumn;

			CurveDisplay.ShowPrintPreview(
				new PrintContent(dummyTb, Dock.Top));
			//new PrintContent(legend, Dock.Bottom));
		}

		private void ToggleOnline_Click(object sender, RoutedEventArgs e)
		{
			if (CurveDisplay.TimeDoublePlottingSystem.IsOnline)
				CurveDisplay.TimeDoublePlottingSystem.DisableOnlineMode();
			else
				CurveDisplay.TimeDoublePlottingSystem.EnableOnlineMode(DateTime.Now);
		}

		private void Reset_Click(object sender, RoutedEventArgs e)
		{
			_resetDemo.Reset();
		}

		#region Implementation of IDisposable

		public void Dispose()
		{
			_refreshCurveDataTimer.Stop();
			_refreshCurveDataTimer.Tick -= refreshCurveDataTimer_Tick;
			CurveDisplay.Dispose();
			_onlineOfflineLegend.Dispose();
		}

		#endregion

		#region Custom Controls

		private readonly IDictionary<UIElement, TimeDoubleCurve> _curveCustomControls
			= new Dictionary<UIElement, TimeDoubleCurve>();
		private readonly List<UIElement> _plottingSystemCustomControls
			= new List<UIElement>();

		private void AddCustomControl_Click(object sender, RoutedEventArgs e)
		{
			var curve = CurveDisplay.Curves.OfType<TimeDoubleCurve>().LastOrDefault();
			if (curve == null)
				return;
			var customControl = new CustomControl();
			var time = GetMiddleAxisTime();
			curve.AddCustomControl(time, customControl);
			_curveCustomControls.Add(customControl, curve);
		}

		private void AddCustomControl1_Click(object sender, RoutedEventArgs e)
		{
			var curve = CurveDisplay.Curves.OfType<TimeDoubleCurve>().LastOrDefault();
			if (curve == null)
				return;
			var customControl = new CustomControl();
			var time = GetMiddleAxisTime();
			curve.AddCustomControl(time, 100, customControl);
			_curveCustomControls.Add(customControl, curve);
		}

		private void RemoveCustomControl_Click(object sender, RoutedEventArgs e)
		{
			if (_curveCustomControls.Count == 0)
				return;
			var customControl = _curveCustomControls.FirstOrDefault();
			_curveCustomControls.Remove(customControl);
			customControl.Value.RemoveCustomControl(customControl.Key);
		}


		private void AddCustomControl2_Click(object sender, RoutedEventArgs e)
		{
			var customControl = new CustomControl();
			var time = GetMiddleAxisTime();
			CurveDisplay.TimeDoublePlottingSystem.AddCustomControl(time, 200, customControl);
			_plottingSystemCustomControls.Add(customControl);
		}

		private void RemoveCustomControl2_Click(object sender, RoutedEventArgs e)
		{
			var customControl = _plottingSystemCustomControls.FirstOrDefault();
			_plottingSystemCustomControls.Remove(customControl);
			CurveDisplay.TimeDoublePlottingSystem.RemoveCustomControl(customControl);
		}

		private DateTime GetMiddleAxisTime()
		{
			var diff = (CurveDisplay.TimeDoublePlottingSystem.TimeAxis.ActualUpperBound - CurveDisplay.TimeDoublePlottingSystem.TimeAxis.ActualLowerBound).TotalMilliseconds;
			var rnd = _random.Next(0, (int)diff);
			var time = CurveDisplay.TimeDoublePlottingSystem.TimeAxis.ActualLowerBound.AddMilliseconds(rnd);
			return time;
		}

		#endregion

		private void RemoveCurve_Click(object sender, RoutedEventArgs e)
		{
			if (_curves.Count == 0)
				return;
			var curve = _curves.First();
			CurveDisplay.TimeDoublePlottingSystem.RemoveCurve(curve.Key);
			_curves.Remove(curve.Key);
		}

		private void AppendData_Click(object sender, RoutedEventArgs e)
		{
			if (_curves.Count == 0)
				return;
			var curve = _curves.First();
			curve.Value.AppendPoints(TimeSpan.FromSeconds(10));
		}

		private void PrependData_Click(object sender, RoutedEventArgs e)
		{
			throw new NotImplementedException();
			//if (_curves.Count == 0)
			//    return;
			//var curve = _curves.First();
			//curve.Value.PrependPoints(TimeSpan.FromSeconds(10));
		}

		private void ToggleVisibility_Click(object sender, RoutedEventArgs e)
		{
			CurveDisplay.Curves.OfType<TimeDoubleCurve>().ToList().ForEach(it => it.IsVisible = !it.IsVisible);
		}

		private void ShowAxes_Click(object sender, RoutedEventArgs e)
		{
			CurveDisplay.TimeDoublePlottingSystem.SetScalesVisibility(true, true, true);
		}

		private void HideAxes_Click(object sender, RoutedEventArgs e)
		{
			CurveDisplay.TimeDoublePlottingSystem.SetScalesVisibility(false, false, false);
		}
	}
}
