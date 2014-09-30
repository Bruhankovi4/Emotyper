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
using System.Text;
using System.Windows.Controls;

namespace TechNewLogic.GraphIT.Hv.Legend
{
	class ExtendedGridView : GridView
	{
		private readonly List<GridViewColumn> _internalColumns = new List<GridViewColumn>();

		public void AddColumn(GridViewColumn gridViewColumn)
		{
			_internalColumns.Add(gridViewColumn);
			Columns.Add(gridViewColumn);
		}

		public bool RemoveColumn(GridViewColumn gridViewColumn)
		{
			Columns.Remove(gridViewColumn);
			return _internalColumns.Remove(gridViewColumn);
		}

		public void SetColumnVisibility(GridViewColumn gridViewColumn, bool isVisible)
		{
			if (!_internalColumns.Contains(gridViewColumn))
				throw new Exception("GridViewColumn is not a member of the internal children.");
			if (!isVisible)
				Columns.Remove(gridViewColumn);
			else
			{
				if (Columns.Contains(gridViewColumn))
					return;

				var matchingColumn = _internalColumns
					.TakeWhile(it => it != gridViewColumn)
					.Intersect(Columns)
					.LastOrDefault();
				if (matchingColumn == null)
					Columns.Add(gridViewColumn);
				else
					Columns.Insert(
						Columns.IndexOf(matchingColumn) + 1,
						gridViewColumn);
			}
		}
	}
}
