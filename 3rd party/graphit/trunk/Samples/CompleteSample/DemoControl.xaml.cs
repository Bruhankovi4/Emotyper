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
using TechNewLogic.GraphIT.Hv.Vertical;
using TechNewLogic.GraphIT.Printing;

namespace CompleteSample
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

			try
			{
				InitializeComponent();

				CurveDisplay.TimeDoublePlottingSystem.TimeAxis.SetBounds(
					DateTime.Now.AddMinutes(-30), DateTime.Now.AddMinutes(30));

				_onlineOfflineLegend = new OnlineOfflineLegend(
					CurveDisplay,
					"AKZ",
					"Einbau Ort",
					"Aggregat",
					"Medium",
					"Messgröße");
				legendHolder.Content = _onlineOfflineLegend;

				_refreshCurveDataTimer = new DispatcherTimer();
				_refreshCurveDataTimer.Interval = TimeSpan.FromSeconds(3);
				_refreshCurveDataTimer.Tick += refreshCurveDataTimer_Tick;
				_refreshCurveDataTimer.Start();

				AddAutoCurve();
				AddAutoCurve();
				AddAutoCurve();
				AddAutoCurve();
			}
			catch (Exception e)
			{
				MessageBox.Show(e.ToString());
			}
		}

		private void AddAutoCurve()
		{
			switch (_globalCurvesCounter)
			{
				case 0:
					AddCurve("%", 0, 100, Colors.Green, RedrawTime.Ms500, "R2033", "Haus 20", "BHKW", "Istwert", "Wirkungsgrad", AxisMatchingMode.UomOnly, 
						CurveDrawingMode.SimpleLine(),
						new FloatingCommaValueFormater(),
						new InterpolationValueFetchStrategy(),
						AxisFormat.Double,
						1);
					break;
				case 1:
					AddCurve("kW", 0, 20, Colors.Violet, RedrawTime.Ms500, "U2060", "Haus 18", "BHKW", "el.-Energie", "Wirkleistung aktuell", AxisMatchingMode.UomOnly, 
						CurveDrawingMode.SimpleLine(),
						new FloatingCommaValueFormater(),
						new InterpolationValueFetchStrategy(),
						AxisFormat.Double,
						1);
					break;
				case 2:
					var curve = AddCurve("m³/h", 0, 10, Colors.Yellow, RedrawTime.Ms500, "U2031", "Haus 20", "BHKW", "Gas", "Gasmenge", AxisMatchingMode.UomOnly, 
						CurveDrawingMode.FilledRectangleBaseline(3),
						new FloatingCommaValueFormater(),
						new InterpolationValueFetchStrategy(),
						AxisFormat.Double,
						0.1);
					curve.DoubleAxis.SetBounds(0, 30);
					break;
				case 3:
					AddCurve("°C", -10, 2, Colors.Blue, RedrawTime.Ms500, "T2051", "Haus 20", "Aussentemp.", "Istwert", "Temperatur", AxisMatchingMode.UomOnly, 
						CurveDrawingMode.FilledRectangleLowest(),
						new BinaryValueFormater(), 
						new SimpleValueFetchStrategy(), 
						AxisFormat.Double,
						1);
					break;
				default:
					MessageBox.Show("Bitte zuerst Kurven entfernen!");
					break;
			}
		}

		private readonly IResetDemo _resetDemo;
		private readonly DispatcherTimer _refreshCurveDataTimer;

		private readonly DateTime _startTime = DateTime.Now;
		private readonly Dictionary<TimeDoubleCurve, TimeDoubleEntryFactory2> _curves
			= new Dictionary<TimeDoubleCurve, TimeDoubleEntryFactory2>();

		private readonly OnlineOfflineLegend _onlineOfflineLegend;

		private int _globalCurvesCounter;

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
			AxisMatchingMode axisMatchMode,
			CurveDrawingMode curveDrawingMode,
			IValueFormater valueFormater,
			IValueFetchStrategy valueFetchStrategy,
			AxisFormat axisFormat,
			double volatileFactor)
		{
			_globalCurvesCounter++;

			var curve = CurveDisplay.TimeDoublePlottingSystem.AddCurve(
				uom,
				min,
				max,
				color,
				redrawTime,
				axisMatchMode,
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

			if (valueFormater is FloatingCommaValueFormater)
			{
				var firstCurve = _curves.Count == 0;

				var pf = TimeDoubleEntryFactory2.Attach(
					curve,
					_startTime.AddMinutes(-50),
					min,
					max,
					1,
					(max - min) / 500 / volatileFactor,
					(max - min) / 100 / volatileFactor,
					(max - min) / 2);

				if (firstCurve)
				{
					// Insg.: 50min
					var first = _random.Next(5, 30);
					var second = _random.Next(5, 50 - first - 5);
					second = second > 15 ? 15 : second;
					var third = 50 - first - second;
					//pf.AppendPoints(TimeSpan.FromMinutes(100));
					pf.AppendPoints(TimeSpan.FromMinutes(first));
					pf.AppendGap(TimeSpan.FromMinutes(second));
					pf.AppendPoints(TimeSpan.FromMinutes(third));
				}
				else
				{
					pf.AppendPoints(TimeSpan.FromMinutes(50));
				}

				// Boundaries
				if (firstCurve)
				{
					var entries = curve.DataSeries.LogicalEntries.Select(it => it.Y).Where(it => !double.IsNaN(it));
					var maxValue = entries.Max();
					var minValue = entries.Min();
					var diff = max - min;

					curve.MinMinBelt = minValue + 0.025 * diff;
					curve.MinBelt = minValue + 0.05 * diff;
					curve.MaxBelt = maxValue - 0.05 * diff;
					curve.MaxMaxBelt = maxValue - 0.025 * diff;
				}

				_curves.Add(curve, pf);
			}
			else
			{
				var min1 = curve.DoubleAxis.ActualLowerBound;
				var max1 = curve.DoubleAxis.ActualUpperBound;
				var dataFactory = DataFactory.Attach(
					curve,
					_startTime.AddMinutes(-50),
					min1,
					max1,
					0.01,
					(max1 - min1) / 500,
					(max1 - min1) / 100,
					(max1 - min1) / 2);

				// all in all: 50min
				dataFactory.AppendBinary(TimeSpan.FromMinutes(50));

				//curve.MinMinBelt = -0.9 + dataFactory.Offset;
				//curve.MinBelt = -0.4 + dataFactory.Offset;
				//curve.MaxBelt = 0.3 + dataFactory.Offset;
				//curve.MaxMaxBelt = 0.7 + dataFactory.Offset;

				//_curves.Add(curve, dataFactory);
			}

			return curve;
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

		private void AddCurve_Click(object sender, RoutedEventArgs e)
		{
			AddAutoCurve();
		}

		private void Print_Click(object sender, RoutedEventArgs e)
		{
			var dummyTb = new TextBlock
			{
				Text = "Datum: " + DateTime.Now,
				Margin = new Thickness(20),
				FontSize = 20,
				TextAlignment = TextAlignment.Center
			};
			CurveDisplay.Print(30, new PrintContent(dummyTb, Dock.Top));
		}

		private void PrintPreview_Click(object sender, RoutedEventArgs e)
		{
			var dummyTb = new TextBlock
			{
				Text = "Datum: " + DateTime.Now,
				Margin = new Thickness(20),
				FontSize = 20,
				TextAlignment = TextAlignment.Center
			};
			CurveDisplay.ShowPrintPreview(new PrintContent(dummyTb, Dock.Top));
		}

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
			try
			{
				var diff = (int)(CurveDisplay.TimeDoublePlottingSystem.TimeAxis.ActualUpperBound - CurveDisplay.TimeDoublePlottingSystem.TimeAxis.ActualLowerBound).Ticks;
				var rnd = _random.Next(0, diff);
				var time = CurveDisplay.TimeDoublePlottingSystem.TimeAxis.ActualLowerBound.AddTicks(rnd);
				return time;
			}
			catch (Exception e)
			{
				return DateTime.Now;
			}
		}

		#endregion

		private void RemoveCurve_Click(object sender, RoutedEventArgs e)
		{
			if (_curves.Count == 0)
				return;
			var curve = _curves.Last();
			CurveDisplay.TimeDoublePlottingSystem.RemoveCurve(curve.Key);
			_curves.Remove(curve.Key);
			_globalCurvesCounter--;
		}

		private void ToggleOnline_Click(object sender, RoutedEventArgs e)
		{
			var plottingSystem = CurveDisplay.TimeDoublePlottingSystem;
			if (plottingSystem.IsOnline)
				plottingSystem.DisableOnlineMode();
			else
			{
				plottingSystem.EnableOnlineMode(DateTime.Now);
				plottingSystem.TimeAxis.SetBounds(
					plottingSystem.TimeAxis.ActualUpperBound.AddMinutes(-2.5),
					plottingSystem.TimeAxis.ActualUpperBound);
			}
		}

		private void AppendData_Click(object sender, RoutedEventArgs e)
		{
			if (_curves.Count == 0)
				return;
			var curve = _curves.First();
			curve.Value.AppendPoints(TimeSpan.FromMinutes(1));
		}

		private void ToggleVisibility_Click(object sender, RoutedEventArgs e)
		{
			CurveDisplay.Curves.OfType<TimeDoubleCurve>().ToList().ForEach(it => it.IsVisible = !it.IsVisible);
		}

		private void Reset_Click(object sender, RoutedEventArgs e)
		{
			_resetDemo.Reset();
		}

		private void ShowAxes_Click(object sender, RoutedEventArgs e)
		{
			CurveDisplay.TimeDoublePlottingSystem.SetScalesVisibility(true, true, true);
		}

		private void HideAxes_Click(object sender, RoutedEventArgs e)
		{
			CurveDisplay.TimeDoublePlottingSystem.SetScalesVisibility(false, false, false);
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
	}
}
