// Copyright 2013 Ronald Schlenker and Andre Kr�mer.
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
using System.Linq;
using TechNewLogic.GraphIT.Hv;

namespace CompleteSample
{
	public class DataFactory
	{
		public static DataFactory Attach(
			TimeDoubleCurve curve,
			DateTime startTime,
			double minValue,
			double maxValue,
			double numberOfPointsPerSecond,
			double minYStepSize,
			double maxYStepSize,
			double offset)
		{
			return new DataFactory(
				curve,
				startTime,
				minValue,
				maxValue,
				numberOfPointsPerSecond,
				maxYStepSize,
				minYStepSize,
				offset);
		}

		private DataFactory(
			TimeDoubleCurve curve,
			DateTime startTime,
			double minValue,
			double maxValue,
			double numberOfPointsPerSecond,
			double maxYStepSize,
			double minYStepSize,
			double offset)
		{
			_curve = curve;
			StartTime = startTime;
			MinValue = minValue;
			MaxValue = maxValue;
			NumberOfPointsPerSecond = numberOfPointsPerSecond;
			MinYStepSize = minYStepSize;
			Offset = offset;
			MaxYStepSize = maxYStepSize;

			_yStepSize = (_random.NextDouble() * (MaxYStepSize - MinYStepSize)) + MinYStepSize;
		}

		private readonly TimeDoubleCurve _curve;
		private readonly Random _random = new Random((int)DateTime.Now.Ticks);
		private readonly double _yStepSize;

		private DateTime StartTime { get; set; }
		private double MinValue { get; set; }
		private double MaxValue { get; set; }
		private double NumberOfPointsPerSecond { get; set; }
		private double MinYStepSize { get; set; }
		public double Offset { get; set; }
		private double MaxYStepSize { get; set; }

		public void AppendPoints(TimeSpan timeRange)
		{
			var points = CreatePoints(timeRange, true);
			_curve.DataSeries.Append(points);
		}

		public void AppendSine(TimeSpan timeRange, double freqFactor)
		{
			var points = CreateSine(timeRange, freqFactor);
			_curve.DataSeries.Append(points);
		}

		public void AppendBinary(TimeSpan timeRange)
		{
			var points = CreateBinary(timeRange);
			_curve.DataSeries.Append(points);
		}

		private IEnumerable<TimeDoubleDataEntry> CreatePoints(TimeSpan timeRange, bool append)
		{
			var points = new List<TimeDoubleDataEntry>();
			var point = append ? GetLastPointOrDefault() : GetFirstPointOrDefault();
			if (double.IsNaN(point.Y))
				point = new TimeDoubleDataEntry(point.X, Offset);
			var numOfSteps = (long)(NumberOfPointsPerSecond * timeRange.TotalSeconds);
			var stepSize = timeRange.TotalSeconds / numOfSteps;
			for (long i = 0; i < numOfSteps; i++)
			{
				// distance +- 5 from previous point
				var delta = _random.NextDouble() * _yStepSize;
				// pos or neg?
				if (_random.Next(0, 2) == 0)
					delta = -delta;

				var y = point.Y + delta;
				if (y > MaxValue)
					y = MaxValue;
				else if (y < MinValue)
					y = MinValue;

				point = append
					? new TimeDoubleDataEntry(point.X.AddSeconds(stepSize), y)
					: new TimeDoubleDataEntry(point.X.AddSeconds(-stepSize), y);
				points.Add(point);
			}
			return points;
		}

		private IEnumerable<TimeDoubleDataEntry> CreateSine(TimeSpan timeRange, double freqFactor)
		{
			var points = new List<TimeDoubleDataEntry>();
			var point = GetLastPointOrDefault();
			var numOfSteps = (long)(NumberOfPointsPerSecond * timeRange.TotalSeconds);
			var stepSize = timeRange.TotalSeconds / numOfSteps;
			for (long i = 0; i < numOfSteps; i++)
			{
				var y = Math.Sin((point.X.Ticks - StartTime.Ticks) / (1000000000d * freqFactor));

				point = new TimeDoubleDataEntry(point.X.AddSeconds(stepSize), y + Offset);
				points.Add(point);
			}
			return points;
		}

		private IEnumerable<TimeDoubleDataEntry> CreateBinary(TimeSpan timeRange)
		{
			var points = new List<TimeDoubleDataEntry>();
			var point = GetLastPointOrDefault();
			var numOfSteps = (long)(NumberOfPointsPerSecond * timeRange.TotalSeconds);
			var stepSize = timeRange.TotalSeconds / numOfSteps;
			double lastValue = 0;
			for (long i = 0; i < numOfSteps; i++)
			{
				var y = lastValue == 0 ? 1 : 0;
				lastValue = y;

				point = new TimeDoubleDataEntry(point.X.AddSeconds(stepSize), y);
				points.Add(point);
			}
			return points;
		}

		public void AppendGap(TimeSpan timeRange)
		{
			var latestTime = GetLastPointOrDefault().X;
			_curve.DataSeries.Append(
				new[]
					{
						new TimeDoubleDataEntry(latestTime.AddMilliseconds(1), _curve.DataSeries.UndefinedValue),
						new TimeDoubleDataEntry(latestTime.Add(timeRange).AddMilliseconds(-1), _curve.DataSeries.UndefinedValue),
					});
		}

		private TimeDoubleDataEntry GetLastPointOrDefault()
		{
			var lastPoint = _curve.DataSeries.LogicalEntries.LastOrDefault();
			return lastPoint ?? new TimeDoubleDataEntry(StartTime, Offset);
		}

		private TimeDoubleDataEntry GetFirstPointOrDefault()
		{
			var firstPoint = _curve.DataSeries.LogicalEntries.FirstOrDefault();
			return firstPoint ?? new TimeDoubleDataEntry(StartTime, Offset);
		}
	}
}