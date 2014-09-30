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
using System.Windows.Controls;
using TechNewLogic.GraphIT.MultiLanguage;

namespace TechNewLogic.GraphIT.Hv.Legend
{
	class RulerColumnCollection
	{
		public RulerColumnCollection(
			IColumnConfiguration columnConfiguration,
			IStaticRulerManager staticRulerManager,
			ITable table,
			int rulerIndex)
		{
			_columnConfiguration = columnConfiguration;
			_table = table;
			_rulerColumns = new List<RulerColumnInfo>();

			// Value
			_valueColumn = new RulerColumnInfo(
				(curve, ruler) => curve.GetFormattedValue(
					curve.DataSeries.GetValueAtTime(ruler.Position, GetValueMode.MiddleValue)),
					//FormatDefinitions.FloatingComma),
				staticRulerManager,
				_table,
				_table.CreateColumn(),
				rulerIndex,
				MlResources.RulerShortText,
				false);
			AddColumn(_valueColumn);

			// Y-Delta
			_yDeltaColumn = new RulerColumnInfo(
				(curve, ruler) => curve.GetFormattedValue(
					ruler.GetDiff(curve.DataSeries).CompareValue),
					//FormatDefinitions.FloatingComma),
				staticRulerManager,
				_table,
				_table.CreateColumn(),
				rulerIndex,
				MlResources.DeltaY,
				true);
			AddColumn(_yDeltaColumn);

			// T-Delta
			_tDeltaColumn = new RulerColumnInfo(
				(curve, ruler) => ruler
					.GetDiff(curve.DataSeries)
					.TimeDiff.Abs().GetFormattedValue(),
				staticRulerManager,
				_table,
				_table.CreateColumn(),
				rulerIndex,
				MlResources.DeltaT,
				true);
			AddColumn(_tDeltaColumn);

			// Min
			_minColumn = new RulerColumnInfo(
				(curve, ruler) => curve.GetFormattedValue(
					ruler
						.GetAggregate(curve.DataSeries, list => list.Select(it => it.Y).MinOrFallback(double.NaN))
						.CompareValue),
					//FormatDefinitions.FloatingComma),
				staticRulerManager,
				_table,
				_table.CreateColumn(),
				rulerIndex,
				MlResources.Min,
				true);
			AddColumn(_minColumn);

			// Max
			_maxColumn = new RulerColumnInfo(
				(curve, ruler) => curve.GetFormattedValue(
					ruler
						.GetAggregate(curve.DataSeries, list => list.Select(it => it.Y).MaxOrFallback(double.NaN))
						.CompareValue),
					//FormatDefinitions.FloatingComma),
				staticRulerManager,
				_table,
				_table.CreateColumn(),
				rulerIndex,
				MlResources.Max,
				true);
			AddColumn(_maxColumn);

			// Avg
			_avgColumn = new RulerColumnInfo(
				(curve, ruler) => curve.GetFormattedValue(
					ruler
						.GetAggregate(curve.DataSeries, list => list.Select(it => it.Y).AverageOrFallback(double.NaN))
						.CompareValue),
					//FormatDefinitions.FloatingComma),
				staticRulerManager,
				_table,
				_table.CreateColumn(),
				rulerIndex,
				MlResources.Avg,
				true);
			AddColumn(_avgColumn);

			//// Sum
			//_rulerColumns.Add(
			//    new RulerColumnInfo(
			//        (curve, ruler) => ruler
			//            .GetAggregate(curve, Sum)
			//            .CompareValue.GetFormattedValue(FormatDefinitions.FloatingComma),
			//        _staticRulerManager,
			//        i,
			//        dataGrid,
			//        "Σ (minute) " + (i + 1) + "-" + nextRulerLabel));
		}

		private readonly IColumnConfiguration _columnConfiguration;
		private readonly ITable _table;

		private readonly List<RulerColumnInfo> _rulerColumns;

		private readonly RulerColumnInfo _valueColumn;
		private readonly RulerColumnInfo _yDeltaColumn;
		private readonly RulerColumnInfo _tDeltaColumn;
		private readonly RulerColumnInfo _minColumn;
		private readonly RulerColumnInfo _maxColumn;
		private readonly RulerColumnInfo _avgColumn;

		private void AddColumn(RulerColumnInfo rulerColumn)
		{
			_rulerColumns.Add(rulerColumn);
		}

		public void RemoveMe()
		{
			_rulerColumns.ForEach(it => _table.RemoveColumn(it.Column));
		}

		//private static double Sum(IEnumerable<TimeDoubleDataEntry> timeDoubleDataEntries)
		//{
		//    return timeDoubleDataEntries.Aggregate(
		//        new
		//        {
		//            LastEntry = new TimeDoubleDataEntry(new DateTime(), double.NaN),
		//            CurrentSum = 0d,
		//            IsSeed = true
		//        },
		//        (acc, entry) => new
		//        {
		//            LastEntry = entry,
		//            CurrentSum = acc.CurrentSum + (
		//                acc.IsSeed
		//                    ? 0d
		//                    : (entry.Y - acc.LastEntry.Y) * (entry.X - acc.LastEntry.X).TotalSeconds / 2),
		//            IsSeed = false
		//        },
		//        acc => acc.CurrentSum);
		//}

		public void Refresh(TimeDoubleCurve curve, bool isLastColumnCollection)
		{
			_rulerColumns.ForEachElement(it => it.RefreshValue(curve));

			_yDeltaColumn.SetIsVisible(_columnConfiguration.ShowDeltaYColumn && !isLastColumnCollection);
			_tDeltaColumn.SetIsVisible(_columnConfiguration.ShowDeltaTColumn && !isLastColumnCollection);
			_minColumn.SetIsVisible(_columnConfiguration.ShowMinColumn && !isLastColumnCollection);
			_maxColumn.SetIsVisible(_columnConfiguration.ShowMaxColumn && !isLastColumnCollection);
			_avgColumn.SetIsVisible(_columnConfiguration.ShowAvgColumn && !isLastColumnCollection);
		}
	}
}