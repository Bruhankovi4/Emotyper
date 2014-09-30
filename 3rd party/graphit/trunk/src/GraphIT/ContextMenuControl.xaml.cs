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
using System.Windows;
using System.Windows.Controls;

namespace TechNewLogic.GraphIT
{
	partial class ContextMenuControl : IContextMenuRegistrar
	{
		internal ContextMenuControl(IDrawingAreaInfo drawingAreaInfo)
		{
			_drawingAreaInfo = drawingAreaInfo;
			InitializeComponent();
		}

		private readonly IDrawingAreaInfo _drawingAreaInfo;

		private Point _screenPosition;

		private readonly ObservableCollection<ContextMenuEntry> _menuEntries = new ObservableCollection<ContextMenuEntry>();
		public IEnumerable<ContextMenuEntry> MenuEntries { get { return _menuEntries; } }

		public void Show()
		{
			_screenPosition = _drawingAreaInfo.MousePosition;
			if (_menuEntries.Any())
				comboBox.IsDropDownOpen = true;
		}

		public void Hide()
		{
			comboBox.IsDropDownOpen = false;
		}

		public Point GetScreenPosition()
		{
			return _screenPosition;
		}

		public void AddMenuEntry(UIElement entry)
		{
			_menuEntries.Add(new ContextMenuEntry(entry));
		}

		public void AddMenuEntry(string header, Action handler)
		{
			var entry = new SimpleContextMenuAction(header, handler, Hide);
			AddMenuEntry(entry);
		}
	}
}
