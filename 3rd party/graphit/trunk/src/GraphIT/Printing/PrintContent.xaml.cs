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
using System.Windows.Controls;
using System.Windows.Media;

namespace TechNewLogic.GraphIT.Printing
{
	/// <summary>
	/// Wrapper for additional visual print content.
	/// </summary>
	public sealed class PrintContent
	{
		/// <summary>
		/// Initializes a new instance of <see cref="PrintContent"/>.
		/// </summary>
		/// <param name="visual">The visual that will be displayed on the printed page.</param>
		/// <param name="position">The position where the <paramref name="visual"/> will be placed.</param>
		public PrintContent(Visual visual, Dock position)
		{
			Visual = visual;
			Position = position;
		}

		/// <summary>
		/// Gets the visual that will be displayed on the printed page.
		/// </summary>
		public Visual Visual { get; private set; }

		/// <summary>
		/// Gets a value that indicates the position where the <see cref="Visual"/> will be placed.
		/// </summary>
		public Dock Position { get; private set; }
	}
}