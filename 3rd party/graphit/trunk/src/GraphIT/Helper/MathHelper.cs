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
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace TechNewLogic.GraphIT.Helper
{
	static class MathHelper
	{
		/// <summary>
		/// Bildet einen Wert von einer Skala auf eine andere Skala ab.
		/// </summary>
		/// <remarks>
		/// (s, t): Wertebereich der Quell-Skala
		/// (a, b): Min / Max-Bereich der Ziel-Skala
		///
		/// Daraus folgt: P1(s, a) und P2(t, b)
		///
		/// Punktsteigungsform:
		/// y = (b - a) / (t - s) * (x - s) + a
		/// </remarks>
		/// <param name="a">Anfang des Wertebereichs der Ziel-Skala</param>
		/// <param name="b">Ende des Wertebereichs der Ziel-Skala</param>
		/// <param name="s">Anfang des Wertebereichs der Quell-Skala</param>
		/// <param name="t">Ende des Wertebereichs der Quell-Skala</param>
		/// <param name="value">Zu übersetzender Wert</param>
		/// <returns></returns>
		public static double MapPoint(double a, double b, double s, double t, double value)
		{
			return (b - a) / (t - s) * (value - s) + a;
		}

		// TODO: Doku (siehe oben)
		public static double LinearInterpolation(double x1, double y1, double x2, double y2, double xCurrent)
		{
			return (y2 - y1) / (x2 - x1) * (xCurrent - x1) + y1;
		}

		public static void Zoom(
			double lowerBound,
			double upperBound,
			double factor,
			out double newLowerBound,
			out double newUpperBound,
			double relativeCenter = 0.5)
		{
			var center = lowerBound + (upperBound - lowerBound) * relativeCenter;
			var matrix = new Matrix();
			matrix.ScaleAt(
				factor, 
				factor, 
				center,
				center);
			var p1 = matrix.Transform(new Point(lowerBound, 0));
			var p2 = matrix.Transform(new Point(upperBound, 0));
			newLowerBound = p1.X;
			newUpperBound = p2.X;
		}

		public static int GetStride(int width, int bpp)
		{
			return ((width * bpp + 31) & ~31) / 8;			
		}
	}
}
