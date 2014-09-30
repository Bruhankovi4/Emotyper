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
using TechNewLogic.GraphIT.Helper;

namespace TechNewLogic.GraphIT.Hv
{
	/// <summary>
	/// Linear interpolation between two points.
	/// </summary>
	public class InterpolationValueFetchStrategy : IValueFetchStrategy
	{
		/// <summary>
		/// Gets a value for a given point in time.
		/// </summary>
		/// <param name="dateTime">The point in time.</param>
		/// <param name="leftValue">The value in a series that lies directly left of the time.</param>
		/// <param name="rightValue">The value in a series that lies directly right of the time.</param>
		/// <returns>The value.</returns>
		public double GetMiddleValue(DateTime dateTime, TimeDoubleDataEntry leftValue, TimeDoubleDataEntry rightValue)
		{
			return MathHelper.LinearInterpolation(
				leftValue.X.Ticks,
				leftValue.Y,
				rightValue.X.Ticks,
				rightValue.Y,
				dateTime.Ticks);
		}
	}
}