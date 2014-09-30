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
using System.Threading;
using System.Windows;
using TechNewLogic.GraphIT.Drawing;
using TechNewLogic.GraphIT.Helper;
using TechNewLogic.GraphIT.Hv.Horizontal;
using TechNewLogic.GraphIT.Hv.Vertical;

namespace TechNewLogic.GraphIT.Hv
{
	/// <summary>
	/// Represents a time-ordered collection of <see cref="TimeDoubleDataEntry"/> entries.
	/// </summary>
	public sealed class TimeDoubleDataSeries : ILogicalToScreenMapper
	{
		internal TimeDoubleDataSeries(
			ValueFetcher valueFetcher,
			MaxEntriesCount maxEntriesCount,
			DoubleAxis doubleAxis,
			TimeAxis timeAxis)
		{
			_valueFetcher = valueFetcher;
			_doubleAxis = doubleAxis;
			_timeAxis = timeAxis;
			MaxNumOfEntries = maxEntriesCount.MaxNumOfEntries;
			UndefinedValue = double.NaN;

			_logicalEntries = new EntriesCollection<TimeDoubleDataEntry>();
			_logicalEntries.EntriesChanged += OnLogicalEntriesChanged;
		}

		private readonly ReaderWriterLockSlim _entriesLock = new ReaderWriterLockSlim();
		private readonly ValueFetcher _valueFetcher;
		private readonly DoubleAxis _doubleAxis;
		private readonly TimeAxis _timeAxis;

		/// <summary>
		/// The maximum number of entries for this series from which the collection will be truncated when appending or prepending data.
		/// </summary>
		public int MaxNumOfEntries { get; private set; }

		#region Logical Entries

		internal event Action LogicalEntriesChanged;
		private void OnLogicalEntriesChanged()
		{
			if (LogicalEntriesChanged != null)
				LogicalEntriesChanged();
		}

		private readonly EntriesCollection<TimeDoubleDataEntry> _logicalEntries;
		/// <summary>
		/// Gets an enumeration of the <see cref="TimeDoubleDataEntry"/> entries of the <see cref="TimeDoubleDataSeries"/>.
		/// </summary>
		public IEnumerable<TimeDoubleDataEntry> LogicalEntries { get { return ReadEntries(() => _logicalEntries.ToList()); } }

		/// <summary>
		/// Appends a <see cref="TimeDoubleDataEntry"/> child to the end of the <see cref="TimeDoubleDataSeries"/>.
		/// </summary>
		/// <remarks>
		/// The overall count of <see cref="TimeDoubleDataEntry"/> entries is trimmed to the value specified in the <see cref="MaxNumOfEntries"/> property.
		/// </remarks>
		/// <param name="entry">The <see cref="TimeDoubleDataEntry"/> to append.</param>
		/// <exception cref="Exception">The point in time of the <paramref name="entry"/> is earlier than the last entry of the <see cref="TimeDoubleDataSeries"/>.</exception>
		public void Append(TimeDoubleDataEntry entry)
		{
			Append(new[] { entry });
		}

		/// <summary>
		/// Appends a <see cref="TimeDoubleDataEntry"/> child to the end of the <see cref="TimeDoubleDataSeries"/>.
		/// </summary>
		/// <remarks>
		/// The <paramref name="entries"/> are ordered by time automatically.
		/// The overall count of entries is trimmed to the value specified in the <see cref="MaxNumOfEntries"/> property.
		/// </remarks>
		/// <param name="entries">The <see cref="TimeDoubleDataEntry"/>s to append.</param>
		/// <exception cref="Exception">The points in time of the <paramref name="entries"/> are earlier than the last entry of the <see cref="TimeDoubleDataSeries"/>.</exception>
		public void Append(IEnumerable<TimeDoubleDataEntry> entries)
		{
			entries = entries ?? Enumerable.Empty<TimeDoubleDataEntry>();
			entries = entries.OrderBy(it => it.X);
			var lastEntry = entries.FirstOrDefault();
			if (lastEntry == null)
				return;
			WriteEntries(() =>
			{
				AssertLastEntry(lastEntry);
				entries = _logicalEntries.PrepareAppend(entries, MaxNumOfEntries);
				_logicalEntries.Append(entries);
			});
		}

		private void AssertLastEntry(TimeDoubleDataEntry entry)
		{
			var lastEntry = _logicalEntries.LastOrDefault();
			if (lastEntry == null)
				return;
			// TODO: Diese Exception wird geschluckt! Darf das sein?
			if (lastEntry.X > entry.X)
				throw new Exception("The entry has to be a greater time than the existing ones.");
		}

		/// <summary>
		/// Prepends a <see cref="TimeDoubleDataEntry"/> child to the end of the <see cref="TimeDoubleDataSeries"/>.
		/// </summary>
		/// <remarks>
		/// The overall count of entries is trimmed to the value specified in the <see cref="MaxNumOfEntries"/> property.
		/// </remarks>
		/// <param name="entry">The <see cref="TimeDoubleDataEntry"/> to prepend.</param>
		/// <exception cref="Exception">The point in time of the <paramref name="entry"/> is later than the last entry of the <see cref="TimeDoubleDataSeries"/>.</exception>
		public void Prepend(TimeDoubleDataEntry entry)
		{
			Prepend(new[] { entry });
		}

		/// <summary>
		/// Prepends a <see cref="TimeDoubleDataEntry"/> child to the end of the <see cref="TimeDoubleDataSeries"/>.
		/// </summary>
		/// <remarks>
		/// The <paramref name="entries"/> are ordered by time automatically.
		/// The overall count of entries is trimmed to the value specified in the <see cref="MaxNumOfEntries"/> property.
		/// </remarks>
		/// <param name="entries">The <see cref="TimeDoubleDataEntry"/>s to prepend.</param>
		/// <exception cref="Exception">The points in time of the <paramref name="entries"/> are later than the last entry of the <see cref="TimeDoubleDataSeries"/>.</exception>
		public void Prepend(IEnumerable<TimeDoubleDataEntry> entries)
		{
			entries = entries ?? Enumerable.Empty<TimeDoubleDataEntry>();
			entries = entries.OrderBy(it => it.X);
			var firstEntry = entries.LastOrDefault();
			if (firstEntry == null)
				return;
			WriteEntries(() =>
			{
				AssertFirstEntry(firstEntry);
				entries = _logicalEntries.PreparePrepend(entries, MaxNumOfEntries);
				_logicalEntries.Prepend(entries);
			});
		}

		private void AssertFirstEntry(TimeDoubleDataEntry entry)
		{
			var firstEntry = _logicalEntries.FirstOrDefault();
			if (firstEntry == null)
				return;
			if (firstEntry.X < entry.X)
				throw new Exception("The entry has to be a smaller time than the existing ones.");
		}

		/// <summary>
		/// Removes all entries that are earlier than <paramref name="toTime"/>.
		/// </summary>
		/// <param name="toTime">The point in time to which entries are removed.</param>
		public void RemoveLeft(DateTime toTime)
		{
			WriteEntries(() => _logicalEntries.Remove(it => it.X < toTime));
		}

		/// <summary>
		/// Removes all entries that are later than <paramref name="fromTime"/>.
		/// </summary>
		/// <param name="fromTime">The point in time from which entries are removed.</param>
		public void RemoveRight(DateTime fromTime)
		{
			WriteEntries(() => _logicalEntries.Remove(it => it.X > fromTime));
		}

		/// <summary>
		/// Removes all entries from the <see cref=" TimeDoubleDataSeries"/>.
		/// </summary>
		public void Clear()
		{
			WriteEntries(() => _logicalEntries.Clear());
		}

		private T ReadEntries<T>(Func<T> del)
		{
			try
			{
				_entriesLock.EnterReadLock();
				return del();
			}
			finally { _entriesLock.ExitReadLock(); }
		}

		private void WriteEntries(Action del)
		{
			try
			{
				_entriesLock.EnterWriteLock();
				del();
			}
			finally { _entriesLock.ExitWriteLock(); }
		}

		#endregion

		/// <summary>
		/// Defines a value that can be used indicates a logical gap between the cohesive collection of entries.
		/// </summary>
		public double UndefinedValue { get; set; }

		internal IEnumerable<IList<Point>> MapLogicalToScreen()
		{
			AssertFrozen();

			var list = new List<IList<TimeDoubleDataEntry>>();

			List<TimeDoubleDataEntry> currentInnerList = null;
			foreach (var it in _frozenEntries)
			{
				if (it.Y.Equals(UndefinedValue) || currentInnerList == null)
				{
					currentInnerList = new List<TimeDoubleDataEntry>();
					list.Add(currentInnerList);
					if (it.Y.Equals(UndefinedValue))
						continue;
				}
				currentInnerList.Add(it);
				//currentInnerList.Add(
				//    new Point(
				//        _timeAxis.MapLogicalToScreen(
				//            _frozenTimeAxisBounds.E1, _frozenTimeAxisBounds.E2, it.X, false),
				//        _doubleAxis.MapLogicalToScreen(
				//            _frozenDoubleAxisBounds.E1, _frozenDoubleAxisBounds.E2, it.Y, false)));
			}

			var mappedList = list
				.Where(it => it.Count > 1)
				.Select(EntriesToPoints)
				.ToList();
			return mappedList;
		}

		private IList<Point> EntriesToPoints(IEnumerable<TimeDoubleDataEntry> entries)
		{
			return entries
				.Select(it =>
					new Point(
						_timeAxis.MapLogicalToScreen(
							_frozenTimeAxisBounds.E1, _frozenTimeAxisBounds.E2, it.X, false),
						MapY(it.Y)))
				.ToList();
		}

		private double MapY(double value)
		{
			return ((ILogicalToScreenMapper)this).MapY(value);
		}

		double ILogicalToScreenMapper.MapY(double value)
		{
			return _doubleAxis.MapLogicalToScreen(
				_frozenDoubleAxisBounds.E1, _frozenDoubleAxisBounds.E2, value, false);
		}

		/// <summary>
		/// Gets a value for the given time.
		/// </summary>
		/// <param name="dateTime">The point in time for which the value should be retrived.</param>
		/// <param name="mode">The <see cref="GetValueMode"/> which is used to fetch the value.</param>
		/// <returns>The value of the given point in time.</returns>
		public double GetValueAtTime(DateTime dateTime, GetValueMode mode)
		{
			var entries = ReadEntries(() => _logicalEntries.ToList());
			return _valueFetcher.GetValueAtTime(dateTime, entries, mode, UndefinedValue);
		}

		// OPT: GetShrinkedEntries
		//private IEnumerable<TimeDoubleDataEntry> GetShrinkedEntries()
		//{
		//    var entries = new List<TimeDoubleDataEntry>();

		//    TimeDoubleDataEntry previousEntry = null;
		//    foreach (var it in LogicalEntries)
		//    {
		//        if (previousEntry == null)
		//        {
		//            previousEntry = it;
		//            if (it.X < _timeAxis.LowerBound)
		//                continue;
		//            if(it.X <= _timeAxis.UpperBound)
		//        }
		//    }
		//}

		#region Freezing

		private bool _isFrozen;
		private IEnumerable<TimeDoubleDataEntry> _frozenEntries;
		private Pair<DateTime> _frozenTimeAxisBounds;
		private Pair<double> _frozenDoubleAxisBounds;

		internal void Freeze()
		{
			AssertNotFrozen();

			_frozenTimeAxisBounds =
				new Pair<DateTime>(_timeAxis.ActualLowerBound, _timeAxis.ActualUpperBound);
			_frozenDoubleAxisBounds =
				new Pair<double>(_doubleAxis.ActualLowerBound, _doubleAxis.ActualUpperBound);
			_frozenEntries = ReadEntries(() => _logicalEntries.ToList());

			_isFrozen = true;
		}

		internal void Unfreeze()
		{
			AssertFrozen();
			_isFrozen = false;
		}

		private void AssertFrozen()
		{
			if (!_isFrozen)
				throw new Exception("Cannot unfreeze an unfreezed dataseries.");
		}

		private void AssertNotFrozen()
		{
			if (_isFrozen)
				throw new Exception("Cannot freeze a freezed dataseries.");
		}

		#endregion
	}
}