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
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Media;

namespace TechNewLogic.GraphIT.Drawing
{
	class LineDrawingVisual : WpfDrawingVisual
	{
		public LineDrawingVisual(bool fillAndClose)
		{
			_fillAndClose = fillAndClose;
		}

		private readonly bool _fillAndClose;

		public override void Draw(
			IList<Point> points,
			double thickness,
			int miterLimit,
			ILogicalToScreenMapper logicalToScreenMapper)
		{
			// First define the geometric shape
			var streamGeometry = new StreamGeometry();
			using (var gc = streamGeometry.Open())
			{
				gc.BeginFigure(points[0], _fillAndClose, _fillAndClose);
				points.RemoveAt(0);

				gc.PolyLineTo(points, true, true);
			}

			using (var dc = RenderOpen())
				dc.DrawGeometry(
					_fillAndClose ? Brushes.White : null,
					new Pen(
						Brushes.White,
						thickness == 1 ? 0.25 : thickness)
						{
							MiterLimit = miterLimit
						},
					streamGeometry);
		}
	}
}
