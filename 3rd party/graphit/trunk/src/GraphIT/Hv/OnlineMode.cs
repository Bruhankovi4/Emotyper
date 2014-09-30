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
using System.Windows.Threading;
using TechNewLogic.GraphIT.Hv.Horizontal;

namespace TechNewLogic.GraphIT.Hv
{
	class OnlineMode : IOnlineMode
	{
		public OnlineMode(TimeAxis timeAxis)
		{
			_timeAxis = timeAxis;

			_onlineTimer = new DispatcherTimer
			{
				Interval = TimeSpan.FromSeconds(1),
				IsEnabled = false
			};
			_onlineTimer.Tick += (s, e) => UpdateOnlineTime();
		}

		private readonly TimeAxis _timeAxis;
		private readonly DispatcherTimer _onlineTimer;

		private TimeSpan _referenceOffset;

		public void EnableOnlineMode(DateTime utcReferenceTime)
		{
			_referenceOffset = DateTime.UtcNow - utcReferenceTime;
			var axisTimeRange = _timeAxis.ActualUpperBound - _timeAxis.ActualLowerBound;
			_timeAxis.SetBounds(utcReferenceTime - axisTimeRange, utcReferenceTime);
			_onlineTimer.Start();
			IsOnline = true;
		}

		public void DisableOnlineMode()
		{
			_onlineTimer.Stop();
			IsOnline = false;
		}

		private void UpdateOnlineTime()
		{
			var referenceTime = DateTime.UtcNow - _referenceOffset;
			var axisTimeRange = _timeAxis.ActualUpperBound - _timeAxis.ActualLowerBound;
			_timeAxis.SetBounds(referenceTime - axisTimeRange, referenceTime);
		}

		public event EventHandler IsOnlineChanged;

		private bool _isOnline;
		public bool IsOnline
		{
			get { return _isOnline; }
			private set
			{
				_isOnline = value;
				if (IsOnlineChanged != null)
					IsOnlineChanged(this, new EventArgs());
			}
		}
	}
}
