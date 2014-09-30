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

namespace TechNewLogic.GraphIT.Hv.Legend
{
	class GridViewTable : ITable
	{
		public GridViewTable(
			Func<IEnumerable<TableLegendItemViewModel>> getViewModels,
			ExtendedGridView extendedGridView)
		{
			_getViewModels = getViewModels;
			_extendedGridView = extendedGridView;
		}

		private readonly Func<IEnumerable<TableLegendItemViewModel>> _getViewModels;
		private readonly ExtendedGridView _extendedGridView;

		public IColumn CreateColumn()
		{
			var column = new GridViewColumnWrapper(this);
			_extendedGridView.AddColumn(column.GridViewColumn);
			return column;
		}

		public void RemoveColumn(IColumn column)
		{
			if (!(column is GridViewColumnWrapper))
				throw new Exception("Only GridViewColumnWrapper are allowed.");
			var gridViewColumnWrapper = (GridViewColumnWrapper)column;
			if (!_extendedGridView.RemoveColumn(gridViewColumnWrapper.GridViewColumn))
				throw new Exception("The given GridViewColumnWrapper was not an element of the collection.");
		}

		public IEnumerable<TableLegendItemViewModel> ViewModels
		{
			get { return _getViewModels().ToList(); }
		}

		public void SetVisibility(GridViewColumnWrapper column, bool isVisible)
		{
			_extendedGridView.SetColumnVisibility(column.GridViewColumn, isVisible);
		}
	}
}