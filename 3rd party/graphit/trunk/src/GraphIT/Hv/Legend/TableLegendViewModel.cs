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
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using TechNewLogic.GraphIT.Helper;

namespace TechNewLogic.GraphIT.Hv.Legend
{
	sealed class TableLegendViewModel : NotifyPropertyChanged, IDisposable, IColumnConfiguration
	{
		public TableLegendViewModel(
			ICurvePool curvePool,
			IStaticRulerManager staticRulerManager,
			RefRulerColumnCollection refRulerColumnCollection,
			Func<int, RulerColumnCollection> createRulerColumnCollection)
		{
			_curvePool = curvePool;
			// TODO: Wenn sich die Kurven-Daten ändern, müssen die Rulers ebenfalls upgedatet werden

			_curvePool.CurveAdded += CurveAdded;
			_curvePool.CurveRemoved += CurveRemoved;

			_curvePool.Curves
				.OfType<TimeDoubleCurve>()
				.ForEachElement(it => _curves.Add(new TableLegendItemViewModel(it)));

			_staticRulerManager = staticRulerManager;
			_staticRulerManager.RulersChanged += ResetRulerColumns;
			_staticRulerManager.ReferenceRulerChanged += ResetRulerColumns;

			_updateRulerObservable = _updateRulerValuesSubject
				.Sample(TimeSpan.FromMilliseconds(100))
				.ObserveOn(SynchronizationContext.Current)
				.Subscribe(o => UpdateRulerColumns());

			_refRulerColumnCollection = refRulerColumnCollection;
			_createRulerColumnCollection = createRulerColumnCollection;
		}

		private readonly ICurvePool _curvePool;
		private readonly IStaticRulerManager _staticRulerManager;
		private readonly IDisposable _updateRulerObservable;
		private readonly Subject<object> _updateRulerValuesSubject = new Subject<object>();

		private bool _columnsCreatedForReferenceRuler;

		private readonly ObservableCollection<TableLegendItemViewModel> _curves
			= new ObservableCollection<TableLegendItemViewModel>();
		public IEnumerable<TableLegendItemViewModel> Curves { get { return _curves; } }

		#region IColumnConfiguration Implementation

		private bool _showDeltaYColumn;
		public bool ShowDeltaYColumn
		{
			get { return _showDeltaYColumn; }
			set
			{
				_showDeltaYColumn = value;
				OnPropertyChanged("ShowDeltaYColumn");
				UpdateRulerColumns();
			}
		}

		private bool _showDeltaTColumn;
		public bool ShowDeltaTColumn
		{
			get { return _showDeltaTColumn; }
			set
			{
				_showDeltaTColumn = value;
				OnPropertyChanged("ShowDeltaTColumn");
				UpdateRulerColumns();
			}
		}

		private bool _showMinColumn;
		public bool ShowMinColumn
		{
			get { return _showMinColumn; }
			set
			{
				_showMinColumn = value;
				OnPropertyChanged("ShowMinColumn");
				UpdateRulerColumns();
			}
		}

		private bool _showMaxColumn;
		public bool ShowMaxColumn
		{
			get { return _showMaxColumn; }
			set
			{
				_showMaxColumn = value;
				OnPropertyChanged("ShowMaxColumn");
				UpdateRulerColumns();
			}
		}

		private bool _showAvgColumn;
		public bool ShowAvgColumn
		{
			get { return _showAvgColumn; }
			set
			{
				_showAvgColumn = value;
				OnPropertyChanged("ShowAvgColumn");
				UpdateRulerColumns();
			}
		}

		#endregion

		void CurveAdded(object sender, EventArgs<Curve> curve)
		{
			var tdc = (curve.Arg as TimeDoubleCurve);
			if (tdc == null)
				return;
			_curves.Add(new TableLegendItemViewModel(tdc));

			ResetRulerColumns();
			tdc.DataSeries.LogicalEntriesChanged += DataSeries_LogicalEntriesChanged;
		}

		void CurveRemoved(object sender, EventArgs<Curve> curve)
		{
			var tdc = (curve.Arg as TimeDoubleCurve);
			if (tdc == null)
				return;

			var item = _curves.FirstOrDefault(it => it.Curve == tdc);
			if (item != null)
			{
				_curves.Remove(item);
				item.Dispose();
			}

			ResetRulerColumns();
			tdc.DataSeries.LogicalEntriesChanged -= DataSeries_LogicalEntriesChanged;
		}

		#region Ruler Columns

		private readonly RefRulerColumnCollection _refRulerColumnCollection;
		private readonly Func<int, RulerColumnCollection> _createRulerColumnCollection;

		private readonly List<RulerColumnCollection> _rulerColumnCollections = new List<RulerColumnCollection>();

		private void DataSeries_LogicalEntriesChanged()
		{
			RaiseUpdateStaticRulerValues();
		}

		public void ResetRulerColumns()
		{
			// Set the visibility of the reference ruler
			_refRulerColumnCollection.RefreshVisibility();

			// a change in reference ruler leads to a complete refresh of all columns
			if (_staticRulerManager.HasReferenceRuler != _columnsCreatedForReferenceRuler)
			{
				_rulerColumnCollections
					.ToArray()
					.ForEachElement(RemoveColumnCollection);
			}
			_columnsCreatedForReferenceRuler = _staticRulerManager.HasReferenceRuler;

			var currentRulersCount = _rulerColumnCollections.Count;
			var countDiff = _staticRulerManager.StaticRulers.Count(it => !it.IsReference) - currentRulersCount;

			// Create additional Rulers
			for (var i = 0; i < countDiff; i++)
				_rulerColumnCollections.Add(
					_createRulerColumnCollection(i + currentRulersCount));

			// Remove un-needed Rulers
			_rulerColumnCollections
				.ToArray()
				.Reverse()
				.Take(-countDiff)
				.ForEachElement(RemoveColumnCollection);

			// TODO: Events von nicht mehr vorhandendn Rulern wieder abhängen
			DeregisterRulerEvents();
			RegisterRulerEvents();
			UpdateRulerColumns();
		}

		private void RegisterRulerEvents()
		{
			_staticRulerManager.StaticRulers
				.ForEachElement(it => { it.PositionUpdated += it_PositionUpdated; });
		}

		private void DeregisterRulerEvents()
		{
			_staticRulerManager.StaticRulers
				.ForEachElement(it => { it.PositionUpdated -= it_PositionUpdated; });
		}

		private void RaiseUpdateStaticRulerValues()
		{
			_updateRulerValuesSubject.OnNext(null);
		}

		private void UpdateRulerColumns()
		{
			var timeDoubleCurves = _curvePool.Curves
				.OfType<TimeDoubleCurve>();

			if (_staticRulerManager.HasReferenceRuler)
			{
				timeDoubleCurves
					.ForEachElement(it =>
					{
						_rulerColumnCollections.ForEachElement(it2 => it2.Refresh(it, false));
						_refRulerColumnCollection.RefreshValue(it);
					});
			}
			else
			{
				timeDoubleCurves
					.ForEachElement(
						it =>
						{
							_rulerColumnCollections
								.Take(_rulerColumnCollections.Count - 1)
								.ForEachElement(it2 => it2.Refresh(it, false));
							_rulerColumnCollections
								.Skip(_rulerColumnCollections.Count - 1)
								.ForEachElement(it2 => it2.Refresh(it, true));
						});
			}
		}

		private void RemoveColumnCollection(RulerColumnCollection columnCollection)
		{
			columnCollection.RemoveMe();
			_rulerColumnCollections.Remove(columnCollection);
		}

		void it_PositionUpdated(object sender, EventArgs e)
		{
			RaiseUpdateStaticRulerValues();
		}

		#endregion

		public void Dispose()
		{
			DeregisterRulerEvents();

			_curvePool.CurveAdded -= CurveAdded;
			_curvePool.CurveRemoved -= CurveRemoved;

			_staticRulerManager.RulersChanged -= ResetRulerColumns;
			_staticRulerManager.ReferenceRulerChanged -= ResetRulerColumns;

			_updateRulerObservable.Dispose();
			_updateRulerValuesSubject.Dispose();

			_curves.ForEachElement(it => it.Dispose());
		}
	}
}
