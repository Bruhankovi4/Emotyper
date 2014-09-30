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
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace TechNewLogic.GraphIT.Hv.Legend
{
	class IndexedObservableCollection<T> : INotifyCollectionChanged
		where T : class
	{
		/// <param name="keySelector">Null, wenn nicht vorhanden</param>
		public IndexedObservableCollection(
			Func<IEnumerable<T>, string, T> keySelector,
			Func<string, T> createValue)
		{
			_keySelector = keySelector;
			_createValue = createValue;
			_innerCollection.CollectionChanged += (s, e) =>
				{
					if (CollectionChanged != null)
						CollectionChanged(this, e);
				};
		}

		private readonly Func<IEnumerable<T>, string, T> _keySelector;
		private readonly Func<string, T> _createValue;

		public T this[string key]
		{
			get
			{
				T value;
				if (!TryGetValue(key, out value))
				{
					value = _createValue(key);
					Add(value);
				}

				return value;
			}
		}

		private readonly ObservableCollection<T> _innerCollection
			= new ObservableCollection<T>();

		#region Implementation of INotifyCollectionChanged

		public event NotifyCollectionChangedEventHandler CollectionChanged;

		#endregion

		private bool TryGetValue(string key, out T value)
		{
			value = _keySelector(_innerCollection, key);
			return value != null;
		}

		private void Add(T value)
		{
			if (_innerCollection.Contains(value))
				throw new InvalidOperationException("The indexed collection already contains the item.");
			_innerCollection.Add(value);
		}
	}
}
