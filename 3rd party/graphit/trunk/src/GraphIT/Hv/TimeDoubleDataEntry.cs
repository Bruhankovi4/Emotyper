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

namespace TechNewLogic.GraphIT.Hv
{
	/// <summary>
	/// Represents a double value in time.
	/// </summary>
	public sealed class TimeDoubleDataEntry
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TimeDoubleDataEntry"/> class.
		/// </summary>
		/// <param name="x">The moment in time of the entry.</param>
		/// <param name="y">The value of the entry.</param>
		public TimeDoubleDataEntry(DateTime x, double y)
		{
			X = x;
			Y = y;
		}

		/// <summary>
		/// The moment in time of the entry.
		/// </summary>
		public DateTime X { get; private set; }

		/// <summary>
		/// The value of the entry.
		/// </summary>
		public double Y { get; private set; }

		/// <summary>
		/// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </returns>
		/// <filterpriority>2</filterpriority>
		public override string ToString()
		{
			return "{x: " + X + " / Y: " + Y + "}";
		}
	}
}