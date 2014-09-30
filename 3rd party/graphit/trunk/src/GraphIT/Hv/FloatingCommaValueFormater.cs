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
using TechNewLogic.GraphIT.MultiLanguage;

namespace TechNewLogic.GraphIT.Hv
{
	/// <summary>
	/// Formats a given double value. The decimal count depends on how large the given number is.
	/// </summary>
	public class FloatingCommaValueFormater : IValueFormater
	{
		/// <summary>
		/// Gets a string representation of a given value.
		/// </summary>
		/// <param name="value">The original value.</param>
		/// <returns>The string representation.</returns>
		public string GetFormattedValue(object value)
		{
			if(value == null)
				return MlResources.NoData;
			if (!(value is double))
				return value.ToString();

			var d = (double)value;
			if (double.IsNaN(d))
				return MlResources.NoData;
			if (d > -0.1 && d < 0.1)
				return d.ToString("0.###");
			if (d > -10 && d < 10)
				return d.ToString("0.##");
			if (d > -100 && d < 100)
				return d.ToString("0.#");
			return d.ToString("0");
		}
	}
}