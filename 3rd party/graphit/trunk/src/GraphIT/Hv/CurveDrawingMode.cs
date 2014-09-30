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
namespace TechNewLogic.GraphIT.Hv
{
	/// <summary>
	/// Specifies how curves shall be drawn and interpolated.
	/// </summary>
	public class CurveDrawingMode
	{
		private CurveDrawingMode() { }

		/// <summary>
		/// COnnects the data entries with a simple line.
		/// </summary>
		public static CurveDrawingMode SimpleLine()
		{
			return new CurveDrawingMode { UseSimpleLine = true };
		}

		/// <summary>
		/// The data entries are interpreted as rectangle values. The baseline is the y-coordinate of the first entry.
		/// </summary>
		public static CurveDrawingMode FilledRectangleAuto()
		{
			return new CurveDrawingMode { UseFilledRectangleAuto = true };
		}

		/// <summary>
		/// The data entries are interpreted as rectangle values.
		/// </summary>
		/// <param name="baselineValue">The baseline value.</param>
		public static CurveDrawingMode FilledRectangleBaseline(double baselineValue)
		{
			return new CurveDrawingMode { BaselineValue = baselineValue, UseFilledRectangleBaseline = true };
		}

		/// <summary>
		/// The data entries are interpreted as rectangle values. The baseline is the largest y-coordinate of all entries.
		/// </summary>
		public static CurveDrawingMode FilledRectangleHighest()
		{
			return new CurveDrawingMode { UseFilledRectangleHighest = true };
		}

		/// <summary>
		/// The data entries are interpreted as rectangle values. The baseline is the smallest y-coordinate of all entries.
		/// </summary>
		public static CurveDrawingMode FilledRectangleLowest()
		{
			return new CurveDrawingMode { UseFilledRectangleLowest = true };
		}

		internal bool UseSimpleLine { get; private set; }
		internal bool UseFilledRectangleAuto { get; private set; }
		internal double BaselineValue { get; private set; }
		internal bool UseFilledRectangleBaseline { get; private set; }
		internal bool UseFilledRectangleHighest { get; private set; }
		internal bool UseFilledRectangleLowest { get; private set; }
	}
}