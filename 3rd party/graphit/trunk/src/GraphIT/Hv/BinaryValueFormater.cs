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
	/// Maps any given value to '0' or '1'.
	/// </summary>
	public class BinaryValueFormater : IValueFormater
	{
		/// <summary>
		/// Gets a string representation of a given value.
		/// </summary>
		/// <param name="value">The original value.</param>
		/// <returns>The string representation.</returns>
		public string GetFormattedValue(object value)
		{
			if (value == null)
				return MlResources.NoData;
			if (value is double)
			{
				var d = (double)value;
				if (d == 0)
					return "0";
				if (d == 1)
					return "1";
				return d.ToString();
			}
			return value.ToString();
		}
	}
}