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
	/// Specifies the method of how a value should be retrived from a <see cref="TimeDoubleDataSeries"/>.
	/// </summary>
	public enum GetValueMode
	{
		/// <summary>
		/// Retrives a value by linear interpolating between the values of the left and right neighbours for a given point in time.
		/// </summary>
		/// <remarks>
		/// If the given point in time does not lie between two neighbours, <see cref="double.NaN"/> will be returned.
		/// </remarks>
		MiddleValue,

		/// <summary>
		/// Retrive a value by getting the value of the left neighbour for a given point in time.
		/// </summary>
		/// <remarks>
		/// If the given point in time does not have a left neighbour, <see cref="double.NaN"/> will be returned.
		/// </remarks>
		LeftValue,

		/// <summary>
		/// Retrive a value by getting the value of the right neighbour for a given point in time.
		/// </summary>
		/// <remarks>
		/// If the given point in time does not have a right neighbour, <see cref="double.NaN"/> will be returned.
		/// </remarks>
		RightValue
	}
}