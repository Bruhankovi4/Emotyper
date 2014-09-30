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
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Xml;

namespace TechNewLogic.GraphIT.Hv.Legend
{
	class GridViewColumnWrapper : IColumn
	{
		public GridViewColumnWrapper(
			GridViewTable gridViewTable)
		{
			ID = Guid.NewGuid().ToString();
			_gridViewTable = gridViewTable;
			
			// HACK (ListView)
			var xaml = string.Format(@"
<DataTemplate 
		xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" 
		xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
		xmlns:Legend=""clr-namespace:TechNewLogic.GraphIT.Hv.Legend;assembly=TechNewLogic.GraphIT"">
	<Legend:TableCellControl 
			CellBackground=""{{Binding DynamicRulerValues[{0}].Background}}"" 
			Text=""{{Binding DynamicRulerValues[{0}].Value}}"" />
</DataTemplate>",
				ID);

			var stringReader = new StringReader(xaml);
			var xmlReader = XmlReader.Create(stringReader);
			var template = (DataTemplate)XamlReader.Load(xmlReader);

			GridViewColumn = new GridViewColumn
				{
					CellTemplate = template,
				};
		}

		private readonly GridViewTable _gridViewTable;

		public string ID { get; private set; }

		public void SetHeader(string value)
		{
			GridViewColumn.Header = value;
		}

		public void SetIsVisible(bool value)
		{
			_gridViewTable.SetVisibility(this, value);
		}

		public GridViewColumn GridViewColumn { get; private set; }
	}
}