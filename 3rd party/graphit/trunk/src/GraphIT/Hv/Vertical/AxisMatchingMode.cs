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
namespace TechNewLogic.GraphIT.Hv.Vertical
{
	/// <summary>
	/// Search mode that specifies how axes can be grouped together.
	/// </summary>
	public enum AxisMatchingMode
	{
		/// <summary>
		/// No grouping is possible.
		/// </summary>
		None,

		/// <summary>
		/// Grouping can be possible if the UOM is equal.
		/// </summary>
		UomOnly,
		
		/// <summary>
		/// Grouping is possible if the UOM and the upper and loewr bounds are equal.
		/// </summary>
		UomAndBounds
	}
}