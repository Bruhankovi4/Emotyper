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
namespace TechNewLogic.GraphIT.Printing
{
	sealed class PaperFormat
	{
		#region Constants

		private static readonly PaperFormat _a4 = new PaperFormat(1123, 794);
		public static PaperFormat A4 { get { return _a4; } }

		private static readonly PaperFormat _a3 = new PaperFormat(1588, 1123);
		public static PaperFormat A3 { get { return _a3; } }

		#endregion

		private PaperFormat(int longSide, int shortSide)
		{
			LongSide = longSide;
			ShortSide = shortSide;
		}

		public int LongSide { get; private set; }
		public int ShortSide { get; private set; }

		internal void GetWidthAndHeight(PaperOrientation paperOrientation, out int width, out int height)
		{
			width = paperOrientation == PaperOrientation.Landscape
				? LongSide : ShortSide;
			height = paperOrientation == PaperOrientation.Landscape
				? ShortSide : LongSide;
		}
	}
}