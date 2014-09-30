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
using System.Diagnostics;
using System.Windows;
using TechNewLogic.GraphIT.Helper;

namespace TechNewLogic.GraphIT.Hv.Horizontal
{
	/// <summary>
	/// Represents an axis with a time dimension.
	/// </summary>
	public sealed class TimeAxis : AxisBase<DateTime>
	{
		internal TimeAxis(IDrawingAreaInfo drawingAreaInfo)
			: base(new AxisParameter<DateTime>(DateTime.UtcNow.AddHours(-1), DateTime.UtcNow.AddHours(1)))
		{
			_drawingAreaInfo = drawingAreaInfo;
		}

		private readonly IDrawingAreaInfo _drawingAreaInfo;
		private bool _boundsSet;

		/// <summary>
		/// Moves the <see cref="AxisBase{t}.ActualLowerBound"/> and the <see cref="AxisBase{t}.ActualUpperBound"/> equally by the given absolute <paramref name="diff"/>.
		/// </summary>
		/// <param name="diff">The absolute difference that indicates move offset.</param>
		public void MoveBounds(TimeSpan diff)
		{
			var newLowerBound = ActualLowerBound.Ticks + diff.Ticks;
			var newUpperBound = ActualUpperBound.Ticks + diff.Ticks;

			if (!newLowerBound.IsValidDateTime() || !newUpperBound.IsValidDateTime())
				return;

			SetBounds(new DateTime(newLowerBound), new DateTime(newUpperBound));
		}

		/// <summary>
		/// Moves the <see cref="AxisBase{t}.ActualLowerBound"/> and the <see cref="AxisBase{t}.ActualUpperBound"/> equally by the given relative <paramref name="relativeDiff"/>.
		/// </summary>
		/// <param name="relativeDiff">
		/// The relative difference that indicates move offset. The value must be between 0 and 1.
		/// A relative value of 1 (100%) is represented by the difference between 
		/// <see cref="AxisBase{t}.ActualLowerBound"/> and the <see cref="AxisBase{t}.ActualUpperBound"/>.
		/// </param>
		public void MoveBoundsRelative(double relativeDiff)
		{
			var timeRange = (ActualUpperBound - ActualLowerBound).Ticks;
			MoveBounds(TimeSpan.FromTicks((long)(timeRange * relativeDiff)));
		}

		// TODO: Move, zoom: GLeiche Methoden wie bei DoubleAxis

		/// <summary>
		/// Zooms the <see cref="AxisBase{t}.ActualLowerBound"/> and the <see cref="AxisBase{t}.ActualUpperBound"/> 
		/// by the given <paramref name="factor"/> and the relative center point <paramref name="relativeCenter"/>.
		/// </summary>
		/// <param name="factor">The zoom factor. A value between 0 and 1 zooms in. A value greater than 1 zooms out.</param>
		/// <param name="relativeCenter">The relative center point of the zoom operation. A value of 0 is mapped to the 
		/// <see cref="AxisBase{t}.ActualLowerBound"/>, a value of 1 is mapped to the <see cref="AxisBase{t}.ActualUpperBound"/>.</param>
		public void Zoom(double factor, double relativeCenter)
		{
			double newLowerBound;
			double newUpperBound;
			MathHelper.Zoom(
				ActualLowerBound.Ticks,
				ActualUpperBound.Ticks,
				factor,
				out newLowerBound,
				out newUpperBound,
				relativeCenter);

			var lowerBoundTicks = (long)newLowerBound;
			var upperBoundTicks = (long)newUpperBound;

			if (!lowerBoundTicks.IsValidDateTime() || !upperBoundTicks.IsValidDateTime())
				return;

			SetBounds(new DateTime(lowerBoundTicks), new DateTime(upperBoundTicks));
		}

		/// <summary>
		/// Centers  the <see cref="AxisBase{t}.ActualLowerBound"/> and the <see cref="AxisBase{t}.ActualUpperBound"/> equaly by around the given <paramref name="value"/>.
		/// </summary>
		/// <param name="value">The absolute value that will become the center of this <see cref="TimeAxis"/>.</param>
		public void Center(DateTime value)
		{
			var diff = ActualUpperBound - ActualLowerBound;
			var halfRange = TimeSpan.FromTicks(diff.Ticks / 2);
			SetBounds(value - halfRange, value + halfRange);
		}

		// OPT: Diese beiden Methoden evtl. in einen eigenen ScreenMapper auslagern

		/// <summary>
		/// Maps a given logical axes value to the absolute screen point of the drawing area.
		/// </summary>
		/// <param name="value">The logical value that should be mapped.</param>
		/// <returns>The screen point.</returns>
		/// <seealso cref="MapScreenToLogical"/>
		public double MapLogicalToScreen(DateTime value)
		{
			var actualLowerBound = ActualLowerBound;
			var actualUpperBound = ActualUpperBound;

			double res = MapLogicalToScreen(actualLowerBound, actualUpperBound, value, true);
			return res;
		}

		internal double MapLogicalToScreen(
			DateTime frozenLowerBound, DateTime frozenUpperBound, DateTime value, bool useScaledCoordinates)
		{
			var s = frozenLowerBound.Ticks;
			var t = frozenUpperBound.Ticks;

			var drawingArea = new Rect(
				0, 0,
				useScaledCoordinates ? _drawingAreaInfo.ScaledDrawingWidth : _drawingAreaInfo.DrawingWidth,
				useScaledCoordinates ? _drawingAreaInfo.ScaledDrawingHeight : _drawingAreaInfo.DrawingHeight);
			var a = drawingArea.Left;
			var b = drawingArea.Right;

			return MathHelper.MapPoint(a, b, s, t, value.Ticks);
		}

		/// <summary>
		/// Maps a given screen point of the drawing area to a logical axes value.
		/// </summary>
		/// <param name="value">The screen point that should be mapped.</param>
		/// <returns>The logical value.</returns>
		public DateTime MapScreenToLogical(double value)
		{
			var drawingArea = new Rect(
				0, 0, _drawingAreaInfo.ScaledDrawingWidth, _drawingAreaInfo.ScaledDrawingHeight);
			var s = drawingArea.Left;
			var t = drawingArea.Right;

			var a = ActualLowerBound.Ticks;
			var b = ActualUpperBound.Ticks;

			var res = MathHelper.MapPoint(a, b, s, t, value);
			// TODO: Gefährlich, kann Exception beim Konvertieren geben
			return double.IsNaN(res) ? DateTime.MinValue : new DateTime((long)res);
		}

		/// <summary>
		/// Sets the <see cref="AxisBase{T}.ActualLowerBound"/> and the <see cref="AxisBase{T}.ActualUpperBound"/> for this <see cref="AxisBase{T}"/>.
		/// </summary>
		/// <param name="lowerBound">The actual lower bound value.</param>
		/// <param name="upperBound">The actual upper bound value.</param>
		public override void SetBounds(DateTime lowerBound, DateTime upperBound)
		{
			// Da die Bounds nicht über den ctor kommen, werden sie beim 1. Mal gesetzt
			if (!_boundsSet)
			{
				LowerBound = lowerBound;
				UpperBound = upperBound;
				_boundsSet = true;
			}

			var diff = upperBound - lowerBound;
			if (diff < TimeSpan.FromMilliseconds(10))
				return;
			if (upperBound < lowerBound)
				return;
			base.SetBounds(lowerBound, upperBound);
		}
	}
}