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
// 
// 

using System;
using System.Collections.Generic;
using TechNewLogic.GraphIT.Hv;

namespace Helper
{
	public static class EntryGenerator
	{
		private static readonly Random Random = new Random((int)DateTime.Now.Ticks);

		public static IEnumerable<TimeDoubleDataEntry> Generatedata(
			double amplitude,
			double yOffset,
			DateTime startTime,
			TimeSpan timeSpan,
			long numOfPoints)
		{
			var list = new List<TimeDoubleDataEntry>();

			for (var i = startTime.Ticks; i < (startTime.Ticks + timeSpan.Ticks); i += timeSpan.Ticks / numOfPoints)
			{
				var currentTime = new DateTime(i);
				var value = Math.Sin((currentTime.Ticks - startTime.Ticks) / 1000000000d) * amplitude;
				list.Add(
					new TimeDoubleDataEntry(
						currentTime,
						value + yOffset));
			}

			return list;
		}

		public static IEnumerable<TimeDoubleDataEntry> Generatedata(
			DateTime startTime,
			TimeSpan timeSpan,
			long numOfPoints)
		{
			var list = new List<TimeDoubleDataEntry>();

			var lastValue = 1;
			for (var i = startTime.Ticks; i < (startTime.Ticks + timeSpan.Ticks); i += timeSpan.Ticks / numOfPoints)
			{
				var currentTime = new DateTime(i);
				lastValue = lastValue == 1 ? 0 : 1;
				list.Add(
					new TimeDoubleDataEntry(
						currentTime,
						lastValue));
			}

			return list;
		}

		public static IEnumerable<TimeDoubleDataEntry> GenerateRandomData(
			DateTime startTime,
			TimeSpan timeRange,
			long numOfPoints,
			double maxValue,
			double minValue,
			double rippleFactor)
		{
			var points = new List<TimeDoubleDataEntry>();
			var point = new TimeDoubleDataEntry(startTime, 0);
			var stepSize = timeRange.TotalSeconds / numOfPoints;
			for (long i = 0; i < numOfPoints; i++)
			{
				// distance +- 5 from previous point
				var delta = Random.NextDouble() * rippleFactor;
				// pos or neg?
				if (Random.Next(0, 2) == 0)
					delta = -delta;

				var y = point.Y + delta;
				if (y > maxValue)
					y = maxValue;
				else if (y < minValue)
					y = minValue;

				point = new TimeDoubleDataEntry(point.X.AddSeconds(stepSize), y);
				points.Add(point);
			}
			return points;
		}
	}
}
