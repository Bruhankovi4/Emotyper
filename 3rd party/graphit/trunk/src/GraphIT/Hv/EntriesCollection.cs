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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TechNewLogic.GraphIT.Hv
{
	class EntriesCollection<T> : IEnumerable<T>
	{
		public T this[int index] { get { return _innerList[index]; } }

		private readonly List<T> _innerList = new List<T>();

		public event Action EntriesChanged;
		private void OnEntriesChanged()
		{
			if (EntriesChanged != null)
				EntriesChanged();
		}

		public int Count { get { return _innerList.Count; } }

		public void Append(T item)
		{
			_innerList.Add(item);
			OnEntriesChanged();
		}

		public void Append(IEnumerable<T> items)
		{
			_innerList.AddRange(items);
			OnEntriesChanged();
		}

		public void Prepend(T item)
		{
			_innerList.Insert(0, item);
			OnEntriesChanged();
		}

		public void Prepend(IEnumerable<T> items)
		{
			_innerList.InsertRange(0, items);
			OnEntriesChanged();
		}

		public void Remove(T item)
		{
			_innerList.Remove(item);
			OnEntriesChanged();
		}

		public void Remove(IEnumerable<T> items)
		{
			items.ForEachElement(it => _innerList.Remove(it));
			OnEntriesChanged();
		}

		public void Remove(Predicate<T> predicate)
		{
			_innerList.RemoveAll(predicate);
			OnEntriesChanged();
		}

		public void Clear()
		{
			_innerList.Clear();
			OnEntriesChanged();
		}

		public IEnumerator<T> GetEnumerator()
		{
			var list = _innerList.ToList();
			return list.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}