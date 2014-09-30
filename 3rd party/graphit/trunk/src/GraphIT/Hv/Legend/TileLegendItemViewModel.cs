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
using System;
using System.Reactive.Linq;
using System.Threading;
using System.Windows.Media;
using TechNewLogic.GraphIT.Helper;
using TechNewLogic.GraphIT.Hv.Horizontal;

namespace TechNewLogic.GraphIT.Hv.Legend
{
	sealed class TileLegendItemViewModel : NotifyPropertyChanged, IDisposable
	{
		public TileLegendItemViewModel(
			TimeDoubleCurve curve,
			TimeAxis timeAxis)
		{
			_timeAxis = timeAxis;
			Curve = curve;
			//Curve.PropertyChanged += Curve_PropertyChanged;
			_curvePropChgObservable = Observable
				.FromEventPattern(Curve, "PropertyChanged")
				.Sample(TimeSpan.FromMilliseconds(100))
				.ObserveOn(SynchronizationContext.Current)
				.Subscribe(o => RefreshView());
			Curve.IsSelectedChanged += Curve_IsSelectedChanged;
		}

		private readonly IDisposable _curvePropChgObservable;
		private readonly TimeAxis _timeAxis;

		public TimeDoubleCurve Curve { get; private set; }

		public string CurrentValue
		{
			get
			{
				// TODO: Aus siesem Compiler-Switch ein setzbares Property machen
#if TILE_CURVAL_IS_TIMEAXIS
				return Curve.GetFormattedValue(
					Curve.DataSeries.GetValueAtTime(_timeAxis.ActualUpperBound, GetValueMode.LeftValue));
					//FormatDefinitions.FloatingComma);
#else
				return Curve.GetFormattedValue(Curve.CurrentValue);
#endif
			}
		}

		public string Uom { get { return Curve.DoubleAxis.Uom; } }
		public Color Stroke { get { return Curve.Stroke; } }

		public string Description1 { get { return Curve.Description.DescriptionText1; } }
		public string Description2 { get { return Curve.Description.DescriptionText2; } }
		public string Description3 { get { return Curve.Description.DescriptionText3; } }
		public string Description4 { get { return Curve.Description.DescriptionText4; } }

		public bool IsVisible
		{
			get { return Curve.IsVisible; }
			set { Curve.IsVisible = value; }
		}

		public bool IsSelected { get { return Curve.IsSelected; } }

		public void SelectCurve()
		{
			Curve.Select(false);
		}

		public void UnselectCurve()
		{
			Curve.Unselect();
		}

		public void TogglePin()
		{
			Curve.TogglePin();
		}

		private void RefreshView()
		{
			OnPropertyChanged("CurrentValue");
			OnPropertyChanged("Uom");
			OnPropertyChanged("IsSelected");
			OnPropertyChanged("Stroke");
			OnPropertyChanged("IsVisible");
			OnPropertyChanged("Description1");
			OnPropertyChanged("Description2");
			OnPropertyChanged("Description3");
			OnPropertyChanged("Description4");
			OnPropertyChanged("RulerDiff1");
			OnPropertyChanged("RulerDiff2");
		}

		void Curve_IsSelectedChanged(object sender, EventArgs e)
		{
			RefreshView();
		}

		//void Curve_PropertyChanged(object sender, PropertyChangedEventArgs e)
		//{
		//    RefreshView();
		//}

		public void Dispose()
		{
			_curvePropChgObservable.Dispose();
			//Curve.PropertyChanged -= Curve_PropertyChanged;
			Curve.IsSelectedChanged -= Curve_IsSelectedChanged;

			// Das muss gemacht werden, weil aus irgend einem WPF internen Grund nicht richtig abgebaut wird
			Curve = null;
		}
	}
}
