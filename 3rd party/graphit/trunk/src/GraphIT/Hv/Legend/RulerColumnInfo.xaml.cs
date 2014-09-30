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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using TechNewLogic.GraphIT.Helper;
using TechNewLogic.GraphIT.Hv.Horizontal;

namespace TechNewLogic.GraphIT.Hv.Legend
{
	class RulerColumnInfo : NotifyPropertyChanged
	{
		public RulerColumnInfo(
			Func<TimeDoubleCurve, HStaticRuler, string> getValueMethod,
			IStaticRulerManager staticRulerManager,
			ITable table,
			IColumn column,
			int rulerIndex,
			string columnHeader,
			bool isDiffColumn)
		{
			_getValueMethod = getValueMethod;
			_staticRulerManager = staticRulerManager;
			_table = table;
			_rulerIndex = rulerIndex;
			_columnHeader = columnHeader + " ";
			IsDiffColumn = isDiffColumn;

			Column = column;

			//UpdateColumnBackground();
			UpdateColumnHeader();
		}

		private readonly Func<TimeDoubleCurve, HStaticRuler, string> _getValueMethod;
		private readonly IStaticRulerManager _staticRulerManager;
		private readonly ITable _table;
		private readonly int _rulerIndex;
		private readonly string _columnHeader;

		public IColumn Column { get; private set; }
		
		public bool IsDiffColumn { get; private set; }

		public void SetIsVisible(bool isVisible)
		{
			//_column.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
			Column.SetIsVisible(isVisible);
		}

		private void UpdateColumnHeader()
		{
			var header = IsDiffColumn
				? (_columnHeader
					+ (_rulerIndex + 1) + ".."
					+ (_staticRulerManager.HasReferenceRuler
						? "REF"
						: (_rulerIndex + 2).ToString()))
				: _columnHeader + (_rulerIndex + 1);

			Column.SetHeader(header);
		}

		public void RefreshValue(TimeDoubleCurve curve)
		{
			var ruler = GetRuler();
			var itemViewModel = _table.ViewModels.Single(it => it.Curve == curve);
			var dynamicColumn = itemViewModel.GetDynamnicColumn(Column.ID);
			dynamicColumn.Value = _getValueMethod(curve, ruler);
			dynamicColumn.Background = _rulerIndex % 2 == 1
				? new SolidColorBrush(Color.FromArgb(25, 255, 255, 255))
				: new SolidColorBrush(Color.FromArgb(50, 255, 255, 255));

			UpdateColumnHeader();
			//UpdateColumnBackground();
		}

		private HStaticRuler GetRuler()
		{
			return _staticRulerManager.StaticRulers
				.Where(it => !it.IsReference)
				.OrderBy(it => it.Position)
				.ElementAt(_rulerIndex);
		}
	}
}