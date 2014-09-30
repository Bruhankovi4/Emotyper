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
using System.Reflection;
using System.Text;

namespace TechNewLogic.GraphIT.Helper
{
	class SynchronizedCollection<T> : ICollection<T>
	{
		private readonly List<T> _innerList = new List<T>();

		public void Add(T item)
		{
			lock (_innerList)
				_innerList.Add(item);
		}

		public void Clear()
		{
			lock (_innerList)
				_innerList.Clear();
		}

		public bool Contains(T item)
		{
			lock (_innerList)
				return _innerList.Contains(item);
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			lock (_innerList)
				_innerList.CopyTo(array, arrayIndex);
		}

		public bool Remove(T item)
		{
			lock (_innerList)
				return _innerList.Remove(item);
		}

		public int Count
		{
			get
			{
				lock (_innerList)
					return _innerList.Count;
			}
		}

		public bool IsReadOnly
		{
			get { return false; }
		}

		public IEnumerator<T> GetEnumerator()
		{
			lock (_innerList)
			{
				var list = _innerList.ToList();
				return list.GetEnumerator();
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
