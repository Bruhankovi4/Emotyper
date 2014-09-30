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
using System.Linq;
using System.Windows.Input;
using TechNewLogic.GraphIT.Helper;

namespace TechNewLogic.GraphIT.Hv.Horizontal
{
	sealed class RulerViewModel : NotifyPropertyChanged, IDisposable
	{
		public RulerViewModel(
			TimeAxis timeAxis,
			IStaticRulerManager rulerManager,
			int rulerIndex)
		{
			_timeAxis = timeAxis;
			_timeAxis.BoundsChanged += _timeAxis_BoundsChanged;
			_rulerIndex = rulerIndex;

			_rulerManager = rulerManager;
			_rulerManager.ReferenceRulerChanged += _rulerManager_ReferenceRulerChanged;

			DeleteCommand = new RelayCommand(Delete);
			GotoCommand = new RelayCommand(Goto);
		}

		private readonly TimeAxis _timeAxis;
		private readonly IStaticRulerManager _rulerManager;
		private readonly int _rulerIndex;

		public ICommand DeleteCommand { get; private set; }
		public ICommand GotoCommand { get; private set; }

		public string Name
		{
			get
			{
#if SORT_REF_RULER
				return IsReference
					? "REF"
					: _rulerManager.HasReferenceRuler
						? "#" + (_rulerIndex)
						: "#" + (_rulerIndex + 1);
#else
				return "#" + (_rulerIndex + 1);
#endif
			}
		}

		public string Position
		{
			get
			{
				return GetRuler().Position.ToString(
					_timeAxis.ActualUpperBound - _timeAxis.ActualLowerBound);
			}
		}

		public bool IsReference { get { return GetRuler().IsReference; } }

		private void Delete()
		{
			_rulerManager.RemoveStaticRuler(GetRuler());
		}

		private void Goto()
		{
			_timeAxis.Center(GetRuler().Position);
		}

		private HStaticRuler GetRuler()
		{
#if SORT_REF_RULER
			return _rulerManager
				.StaticRulers
				.OrderBy(it => !it.IsReference)
				.ThenBy(it => it.Position)
				.ElementAt(_rulerIndex);
#else
			return _rulerManager
				.StaticRulers
				.OrderBy(it => it.Position)
				.ElementAt(_rulerIndex);
#endif
		}

		void _timeAxis_BoundsChanged(object sender, EventArgs e)
		{
			Refresh();
		}

		void _rulerManager_ReferenceRulerChanged()
		{
			Refresh();
		}

		public void Refresh()
		{
			OnPropertyChanged("Position");
			OnPropertyChanged("IsReference");
			OnPropertyChanged("Name");
		}

		#region Implementation of IDisposable

		private bool _isDisposed;

		public void Dispose()
		{
			if (_isDisposed)
				return;
			_isDisposed = true;

			_timeAxis.BoundsChanged -= _timeAxis_BoundsChanged;
			_rulerManager.ReferenceRulerChanged -= _rulerManager_ReferenceRulerChanged;
		}

		#endregion
	}
}