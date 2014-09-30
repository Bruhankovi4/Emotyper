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
using System.Reflection;
using System.Windows;
using System.Windows.Media;

namespace TechNewLogic.GraphIT.Drawing
{
	class RectangleWpfDrawingVisual : WpfDrawingVisual
	{
		#region Factory

		private RectangleWpfDrawingVisual() { }

		public static RectangleWpfDrawingVisual Auto()
		{
			return new RectangleWpfDrawingVisual();
		}

		public static RectangleWpfDrawingVisual Baseline(double baselineValue)
		{
			return new RectangleWpfDrawingVisual { _baselineValue = baselineValue, _useBaseline = true };
		}

		public static RectangleWpfDrawingVisual Highest()
		{
			return new RectangleWpfDrawingVisual { _useHighest = true };
		}

		public static RectangleWpfDrawingVisual Lowest()
		{
			return new RectangleWpfDrawingVisual { _useLowest = true };
		}

		#endregion

		private double _baselineValue;
		private bool _useBaseline;
		private bool _useHighest;
		private bool _useLowest;

		public override void Draw(
			IList<Point> points,
			double thickness,
			int miterLimit,
			ILogicalToScreenMapper logicalToScreenMapper)
		{
			var pointsCount = points.Count;
			var firstPoint = points[0];
			var lastPoint = points[pointsCount - 1];
			var highestValue = _useHighest ? points.Max(it => it.Y) : 0;
			var lowestValue = _useLowest ? points.Min(it => it.Y) : 0;
			double baselineValue;

			// Auf gleiche Höhe bringen
			if (_useBaseline)
				baselineValue = logicalToScreenMapper.MapY(_baselineValue);
			else if (_useHighest)
				baselineValue = highestValue;
			else if (_useLowest)
				baselineValue = lowestValue;
			else
			{
				baselineValue = firstPoint.Y > lastPoint.Y
					? lastPoint.Y
					: firstPoint.Y;
			}

			using (var dc = RenderOpen())
			{
				for (var i = 1; i < points.Count; i++)
				{
					var previousPoint = points[i - 1];
					var currentPoint = points[i];
					var previousY = previousPoint.Y;
					// -1 weil: 1 Pixel nach links verschieben bewirkt, dass die Löcher zwischen den Rechtecken nicht mehr auftauchen
					// nicht mehr nach 1 verschieben, weil der Fall bei Kriko (kleine Löcher in den Kurven) sowieso nicht vorkommt
					// var previousX = previousPoint.X - 1;
					var previousX = previousPoint.X;
					// Rect kann mit negativen Höhen nicht umgehen, deshalb die komischen Expressions
					var height = previousY > baselineValue ? previousY - baselineValue : baselineValue - previousY;
					var y = previousY > baselineValue ? baselineValue : previousY;
					var rectangle = new Rect(
						previousX,
						height == 0 ? y - 1 : y, // Linie um 1 nach oben verschieben, damit die Linien nicht optisch nach unten versetzt sind
						currentPoint.X - previousX,
						height == 0d ? 1 : height);
					dc.DrawRectangle(
						Brushes.White,
						new Pen(
							Brushes.White,
							0)
						// brauchen wir nicht mehr (siehe oben - height == 0): thickness == 1 ? 0.25 : thickness)
						{
							MiterLimit = miterLimit
						},
						rectangle);
				}
			}
		}
	}
}
