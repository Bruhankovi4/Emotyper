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
using System.Collections.Generic;
using System.Linq;

namespace TechNewLogic.GraphIT.Hv
{
	class ValueFetcher
	{
		public ValueFetcher(IValueFetchStrategy valueFetchStrategy)
		{
			_valueFetchStrategy = valueFetchStrategy;
		}

		private readonly IValueFetchStrategy _valueFetchStrategy;

		public double GetValueAtTime(DateTime dateTime, IList<TimeDoubleDataEntry> logicalEntries, GetValueMode mode, double undefinedValue)
		{
			TimeDoubleDataEntry leftValue = null;
			TimeDoubleDataEntry rightValue = null;

			if (!logicalEntries.Any())
				return double.NaN;

			var firstEntry = logicalEntries[0];
			var lastEntry = logicalEntries[logicalEntries.Count - 1];
			if (dateTime < firstEntry.X)
			{
				rightValue = firstEntry;
			}
			else if (dateTime == firstEntry.X)
			{
				leftValue = firstEntry;
				rightValue = firstEntry;
			}
			else if (dateTime == lastEntry.X)
			{
				leftValue = lastEntry;
				rightValue = lastEntry;
			}
			else if (dateTime > lastEntry.X)
			{
				leftValue = lastEntry;
			}
			else
			{
				// Der gefragte Zeitpunkt muss zwischen 2 Punkten liegen
				for (var i = 0; i < logicalEntries.Count; i++)
				{
					var entry = logicalEntries[i];
					// Es muss immer einen nextEntry geben, da ansonsten die Bedingungen von oben greifen müssten
					var nextEntry = logicalEntries[i + 1];
					if (entry.X == dateTime)
					{
						leftValue = entry;
						rightValue = entry;
						break;
					}
					if (entry.X < dateTime && nextEntry.X > dateTime)
					{
						leftValue = entry;
						rightValue = nextEntry;
						break;
					}
					//if (entry.X == dateTime)
					//{
					//    leftValue = entry;
					//    rightValue = entry;
					//    break;
					//}
					//if (entry.X > dateTime)
					//{
					//    leftValue = entry;
					//    if (i + 1 < _logicalEntries.Count)
					//        rightValue = _logicalEntries[i + 1];
					//    break;
					//}
				}
			}

			// Performance-Critical
			//leftValue = LogicalEntries.TakeWhile(it => it.X < dateTime).LastOrDefault();
			//rightValue = LogicalEntries.SkipWhile(it => it.X <= dateTime).FirstOrDefault();

			if (mode == GetValueMode.LeftValue)
				return leftValue == null ? double.NaN : leftValue.Y;
			if (mode == GetValueMode.RightValue)
				return rightValue == null ? double.NaN : rightValue.Y;
			return leftValue == null || rightValue == null || leftValue.Y.Equals(undefinedValue) || rightValue.Y.Equals(undefinedValue)
				? double.NaN
				: _valueFetchStrategy.GetMiddleValue(dateTime, leftValue, rightValue);
		}
	}
}