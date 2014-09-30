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

namespace TechNewLogic.GraphIT.Hv.Vertical
{
	/// <summary>
	/// Represents a vertical scale.
	/// </summary>
	/// <remarks>
	/// A scale is a visual object that aggregates one or more axes. It can accept usse input such as drag / drop, panning, moving, etc.
	/// </remarks>
	public interface IVScale
	{
		/// <summary>
		/// Geta a value that indicates the placement relative to the drawing area.
		/// </summary>
		ScalePosition Position { get; }

		/// <summary>
		/// Gets the <see cref="DoubleAxesGroup"/> for this <see cref="IVScale"/>.
		/// </summary>
		DoubleAxesGroup AxesGroup { get; }

		/// <summary>
		/// Gets or sets whether the user can modify (move or scale) this <see cref="IVScale"/>.
		/// </summary>
		bool EnableUserInput { get; set; }
	}
}