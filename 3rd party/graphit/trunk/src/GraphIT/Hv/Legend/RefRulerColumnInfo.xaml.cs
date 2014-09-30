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
using System.Linq;
using System.Windows.Media;
using TechNewLogic.GraphIT.Helper;
using TechNewLogic.GraphIT.Hv.Horizontal;

namespace TechNewLogic.GraphIT.Hv.Legend
{
	class RefRulerColumnInfo : NotifyPropertyChanged
	{
		public RefRulerColumnInfo(
			IStaticRulerManager staticRulerManager,
			ITable table,
			IColumn column)
		{
			_staticRulerManager = staticRulerManager;
			_table = table;

			_column = column;
			_column.SetIsVisible(false);
			_column.SetHeader("REF");
		}

		private readonly IStaticRulerManager _staticRulerManager;
		private readonly ITable _table;
		private readonly IColumn _column;

		public void RefreshVisibility()
		{
			_column.SetIsVisible(GetRuler() != null);
		}

		public void RefreshValue(TimeDoubleCurve curve)
		{
			var ruler = GetRuler();
			var itemViewModel = _table.ViewModels.Single(it => it.Curve == curve);
			var dynamicColumn = itemViewModel.GetDynamnicColumn(_column.ID);
			dynamicColumn.Value = curve.GetFormattedValue(
				curve.DataSeries.GetValueAtTime(ruler.Position, GetValueMode.MiddleValue));
				//FormatDefinitions.FloatingComma);
			dynamicColumn.Background = new SolidColorBrush(Color.FromArgb(100, 255, 255, 0));
		}

		private HStaticRuler GetRuler()
		{
			return _staticRulerManager.StaticRulers.FirstOrDefault(it => it.IsReference);
		}
	}
}