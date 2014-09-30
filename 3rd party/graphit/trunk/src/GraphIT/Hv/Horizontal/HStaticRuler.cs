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

namespace TechNewLogic.GraphIT.Hv.Horizontal
{
	/// <summary>
	/// Represents a horizontal ruler on the drawing area.
	/// </summary>
	public sealed class HStaticRuler
	{
		internal HStaticRuler(
			IStaticRulerManager staticRulerManager,
			DateTime position)
		{
			_staticRulerManager = staticRulerManager;
			Position = position;
		}

		private readonly IStaticRulerManager _staticRulerManager;

		/// <summary>
		/// Occurs when the position of the <see cref="HStaticRuler"/> has changed.
		/// </summary>
		public event EventHandler PositionUpdated;

		private DateTime _position;
		/// <summary>
		/// Gets or sets the position of the <see cref="HStaticRuler"/>.
		/// </summary>
		public DateTime Position
		{
			get { return _position; }
			set
			{
				_position = value;
				if (PositionUpdated != null)
					PositionUpdated(this, new EventArgs());
			}
		}

		private bool _isReference;
		/// <summary>
		/// Gets a value that indicates whether the <see cref="HStaticRuler"/> is the reference ruler.
		/// </summary>
		/// <remarks>
		/// When this value is true, all other <see cref="HStaticRuler"/> instances calculate their differences to this reference ruler.
		/// </remarks>
		public bool IsReference
		{
			get { return _isReference; }
			private set
			{
				_isReference = value;
				_staticRulerManager.OnReferenceRulerChanged();
			}
		}

		/// <summary>
		/// Toggles the state of the <see cref="IsReference"/> property.
		/// </summary>
		/// <remarks>
		/// The ruler system ensures that only one <see cref="HStaticRuler"/> at time can be the reference ruler.
		/// If another ruler has already set it's <see cref="IsReference"/> property set to true, it will automatically be disabled.
		/// </remarks>
		public void ToggleReference()
		{
			IsReference = !IsReference;
			_staticRulerManager.StaticRulers
				.Where(it => it != this)
				.ForEachElement(it => it.IsReference = false);
		}

		#region Comparison

		internal RulerCompareInfo GetDiff(TimeDoubleDataSeries dataSeries)
		{
			return GetCompareInfo(
				(otherRuler, position) =>
					dataSeries.GetValueAtTime(otherRuler.Position, GetValueMode.MiddleValue)
						- dataSeries.GetValueAtTime(Position, GetValueMode.MiddleValue));
		}

		internal RulerCompareInfo GetAggregate(
			TimeDoubleDataSeries dataSeries,
			Func<IEnumerable<TimeDoubleDataEntry>, double> aggregateFunction)
		{
			return GetCompareInfo(
				(otherRuler, position) =>
				{
					switch (position)
					{
						case RelativeRulerPosition.Left:
							return aggregateFunction(
								dataSeries.LogicalEntries
									.Where(it => it.X >= Position && it.X <= otherRuler.Position));
						case RelativeRulerPosition.Right:
							return aggregateFunction(
								dataSeries.LogicalEntries
									.Where(it => it.X <= Position && it.X >= otherRuler.Position));
						case RelativeRulerPosition.Center:
							return dataSeries.GetValueAtTime(
								Position, GetValueMode.MiddleValue);
						default:
							throw new ArgumentOutOfRangeException("position");
					}
				});
		}

		private RulerCompareInfo GetCompareInfo(
			Func<HStaticRuler, RelativeRulerPosition, double> getCompareValueMethod)
		{
			var refRuler = _staticRulerManager.StaticRulers
				.FirstOrDefault(it => it.IsReference);
			// To next ruler
			if (refRuler == null)
			{
				var nextRuler = _staticRulerManager.StaticRulers
					.OrderBy(it => it.Position)
					.FirstOrDefault(it => it.Position > Position);
				return nextRuler != null
					? CreateRulerCompareInfo(
						nextRuler,
						RulerCompareMode.ToNextRuler,
						getCompareValueMethod)
					: new RulerCompareInfo();
			}

			// To Reference
			return refRuler != this
					? CreateRulerCompareInfo(
						refRuler,
						RulerCompareMode.ToReferenceRuler,
						getCompareValueMethod)
				: new RulerCompareInfo();
		}

		private RulerCompareInfo CreateRulerCompareInfo(
			HStaticRuler otherRuler,
			RulerCompareMode rulerCompareMode,
			Func<HStaticRuler, RelativeRulerPosition, double> getCompareValueMethod)
		{
			RelativeRulerPosition pos;
			if (Position < otherRuler.Position)
				pos = RelativeRulerPosition.Left;
			else if (Position > otherRuler.Position)
				pos = RelativeRulerPosition.Right;
			else
				pos = RelativeRulerPosition.Center;

			return new RulerCompareInfo(
				getCompareValueMethod(otherRuler, pos),
				Position - otherRuler.Position,
				pos,
				rulerCompareMode,
				otherRuler);
		}

		#endregion
	}
}