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
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using TechNewLogic.GraphIT.Helper;

namespace TechNewLogic.GraphIT.Hv.Vertical
{
	class DragScaleAdorner : BaseContainerAdorner
	{
		public DragScaleAdorner(
			DoubleScale adornedElement,
			DragScaleControl dragScaleControl)
			: base(adornedElement)
		{
			_dragScaleControl = dragScaleControl;
			VisualChildren.Add(_dragScaleControl);
		}

		private readonly DragScaleControl _dragScaleControl;

		private Point _position;
		public Point Position
		{
			get { return _position; }
			set
			{
				_position = value;
				InvalidateVisual();
			}
		}

		protected override Size ArrangeOverride(Size finalSize)
		{
			// Die Offset-Werte im Rect ergeben sich aus dem ControlTemplate für ValidationAdornerControl
			// (konkret: BorderThickness)
			var currentMousePosition = Mouse.GetPosition(this);
			var reservedSpace = ActualWidth - 10;
			var offsetX = currentMousePosition.X < reservedSpace ? 0 : reservedSpace;
			_dragScaleControl.Arrange(
				new Rect(Position.X - offsetX, Position.Y, finalSize.Width, finalSize.Height));
			return finalSize;
		}

		protected override Size MeasureOverride(Size constraint)
		{
			_dragScaleControl.Measure(constraint);
			return _dragScaleControl.DesiredSize;
		}
	}
}
