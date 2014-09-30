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
using System.Reactive.Linq;
using System.Threading;
using TechNewLogic.GraphIT.Hv.Vertical;

namespace TechNewLogic.GraphIT.Hv
{
	using TechNewLogic.GraphIT.Hv.Horizontal;

	/// <summary>
	/// Provides base functionality for implementing axes.
	/// </summary>
	/// <remarks>
	/// An axis is used to represent and control the logical state of a one-dimensional unit system like <see cref="TimeAxis"/> or <see cref="DoubleAxis"/>.
	/// </remarks>
	/// <typeparam name="T">The entry type of this <see cref="AxisBase{T}"/>.</typeparam>
	public abstract class AxisBase<T> : IDisposable
	{
		internal AxisBase(AxisParameter<T> parameter)
		{
			LowerBound = parameter.LowerBound;
			UpperBound = parameter.UpperBound;
			ActualLowerBound = parameter.LowerBound;
			ActualUpperBound = parameter.UpperBound;

			var throttleDueTime = TimeSpan.FromMilliseconds(250);
			if (throttleDueTime != TimeSpan.Zero)
			{
				_boundsChangedThrottledObservable = Observable
					.FromEventPattern<EventArgs>(this, "BoundsChanged")
					.Sample(throttleDueTime)
					.ObserveOn(SynchronizationContext.Current)
					.Subscribe(e => OnBoundsChangedThrottled());
			}
		}

		private readonly IDisposable _boundsChangedThrottledObservable;

		/// <summary>
		/// Occurs when the bounds of this <see cref="AxisBase{T}"/> are changed.
		/// </summary>
		public event EventHandler BoundsChanged;
		private void OnBoundsChanged()
		{
			if (BoundsChanged != null)
				BoundsChanged(this, new EventArgs());
		}

		/// <summary>
		/// Occurs in a throttled manner when the bounds of this <see cref="AxisBase{T}"/> are changed.
		/// </summary>
		/// <remarks>
		/// The event is throttled, which means that many changes in a short time of the bounds lead to
		/// only a few events. Use this event instad of the <see cref="BoundsChanged"/>
		/// event for computational intense operations (e.g. UI) or for fetching data from an underlying data source.
		/// </remarks>
		public event EventHandler BoundsChangedThrottled;
		private void OnBoundsChangedThrottled()
		{
			if (BoundsChangedThrottled != null)
				BoundsChangedThrottled(this, new EventArgs());
		}

		/// <summary>
		/// The initial lower bound of this <see cref="AxisBase{T}"/>.
		/// </summary>
		public T LowerBound { get; protected set; }

		/// <summary>
		/// The initial upper bound of this <see cref="AxisBase{T}"/>.
		/// </summary>
		public T UpperBound { get; protected set; }

		/// <summary>
		/// The actual lower bound of this <see cref="AxisBase{T}"/>.
		/// </summary>
		public T ActualLowerBound { get; private set; }

		/// <summary>
		/// The actual upper bound of this <see cref="AxisBase{T}"/>.
		/// </summary>
		public T ActualUpperBound { get; private set; }

		/// <summary>
		/// Sets the <see cref="ActualLowerBound"/> and the <see cref="ActualUpperBound"/> for this <see cref="AxisBase{T}"/>.
		/// </summary>
		/// <param name="lowerBound">The actual lower bound value.</param>
		/// <param name="upperBound">The actual upper bound value.</param>
		public virtual void SetBounds(T lowerBound, T upperBound)
		{
			// TODO: Validierung? Kann das überhaupt gemacht werden, so generisch, oder soll das in den abgeleiteten Klassen entschieden werden?
			ActualLowerBound = lowerBound;
			ActualUpperBound = upperBound;
			OnBoundsChanged();
		}

		/// <summary>
		/// Resets the initial lower and upper bounds to the actual bound values
		/// </summary>
		public void ResetInitialBounds()
		{
			LowerBound = ActualLowerBound;
			UpperBound = ActualUpperBound;
		}

		/// <summary>
		/// Releases all resources used by this <see cref="AxisBase{T}"/>.
		/// </summary>
		public void Dispose()
		{
			_boundsChangedThrottledObservable.Dispose();
		}
	}
}