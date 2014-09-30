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
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Windows.Media;
using TechNewLogic.GraphIT.Helper;

namespace TechNewLogic.GraphIT.Hv.Legend
{
	sealed class TableLegendItemViewModel : NotifyPropertyChanged, IDisposable
	{
		public TableLegendItemViewModel(TimeDoubleCurve curve)
		{
			_dynamicRulerValues = new IndexedObservableCollection<DynamicRulerColumn>(
				(col, key) => col.FirstOrDefault(it => it.Name == key),
				key => new DynamicRulerColumn(key));

			Curve = curve;
			_curvePropChgObservable = Observable
				.FromEventPattern(Curve, "PropertyChanged")
				.ObserveOn(SynchronizationContext.Current)
				.Sample(TimeSpan.FromMilliseconds(100))
				.Subscribe(o => RefreshView());
			Curve.IsSelectedChanged += Curve_IsSelectedChanged;
		}

		private readonly IDisposable _curvePropChgObservable;

		public TimeDoubleCurve Curve { get; private set; }

		public string CurrentValue { get { return Curve.GetFormattedValue(Curve.CurrentValue); } }//, FormatDefinitions.FloatingComma); } }
		public string Uom { get { return Curve.DoubleAxis.Uom; } }
		public Color Stroke { get { return Curve.Stroke; } }

		public string Min { get { return Curve.GetFormattedValue(Curve.DoubleAxis.LowerBound); } }//, FormatDefinitions.FloatingComma); } }
		public string Max { get { return Curve.GetFormattedValue(Curve.DoubleAxis.UpperBound); } }//, FormatDefinitions.FloatingComma); } }

		public string Description1 { get { return Curve.Description.DescriptionText1; } }
		public string Description2 { get { return Curve.Description.DescriptionText2; } }
		public string Description3 { get { return Curve.Description.DescriptionText3; } }
		public string Description4 { get { return Curve.Description.DescriptionText4; } }
		public string Description5 { get { return Curve.Description.DescriptionText5; } }

		private readonly IndexedObservableCollection<DynamicRulerColumn> _dynamicRulerValues;
		public object DynamicRulerValues { get { return _dynamicRulerValues; } }

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

		public void RefreshView()
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

		public DynamicRulerColumn GetDynamnicColumn(string columnID)
		{
			return _dynamicRulerValues[columnID];
		}

		void Curve_IsSelectedChanged(object sender, EventArgs e)
		{
			RefreshView();
		}

		public void Dispose()
		{
			_curvePropChgObservable.Dispose();
			Curve.IsSelectedChanged -= Curve_IsSelectedChanged;
		}
	}
}