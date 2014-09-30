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
using System.Windows;
using TechNewLogic.GraphIT.Helper;

namespace TechNewLogic.GraphIT.Hv.Vertical
{
	/// <summary>
	/// Represents a base axis with a double value as dimension parameter.
	/// </summary>
	public abstract class DoubleAxisBase : AxisBase<double>
	{
		internal DoubleAxisBase(
			DoubleAxisParameter doubleAxisParameter,
			IDrawingAreaInfo drawingAreaInfo)
			: base(doubleAxisParameter)
		{
			Uom = doubleAxisParameter.Uom;
			AxisFormat = doubleAxisParameter.AxisFormat;
			_drawingAreaInfo = drawingAreaInfo;
		}

		private readonly IDrawingAreaInfo _drawingAreaInfo;

		/// <summary>
		/// The unit of measure for that <see cref="DoubleAxis"/>.
		/// </summary>
		public string Uom { get; private set; }

		internal AxisFormat AxisFormat { get; private set; }

		/// <summary>
		/// Moves the <see cref="AxisBase{t}.ActualLowerBound"/> and the <see cref="AxisBase{t}.ActualUpperBound"/> equally by the given absolute <paramref name="diff"/>.
		/// </summary>
		/// <param name="diff">The absolute difference that indicates move offset.</param>
		public void MoveBounds(double diff)
		{
			var lowerBound = ActualLowerBound + diff;
			var upperBound = ActualUpperBound + diff;
			SetBounds(lowerBound, upperBound);
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
			var range = ActualLowerBound - ActualUpperBound;
			var diff = range * relativeDiff;
			MoveBounds(diff);
		}

		internal GroupingAbilities GroupingMatches(DoubleAxisBase target, AxisMatchingMode matchingMode)
		{
			if (target == this)
				return new GroupingAbilities { NotAllowed = true };
			if (target.AxisFormat != AxisFormat)
				return new GroupingAbilities { NotAllowed = true };
			switch (matchingMode)
			{
				case AxisMatchingMode.None:
					return new GroupingAbilities { NotAllowed = true };
				case AxisMatchingMode.UomOnly:
					return target.Uom != Uom 
						? new GroupingAbilities { Relative = true } 
						: new GroupingAbilities { Relative = true, Absolute = true };
				case AxisMatchingMode.UomAndBounds:
					return target.LowerBound == LowerBound && target.UpperBound == UpperBound && target.Uom == Uom 
						? new GroupingAbilities {Relative = true, Absolute = true} 
						: new GroupingAbilities {Relative = true};
				default:
					throw new ArgumentOutOfRangeException("matchingMode");
			}
		}

		/// <summary>
		/// Zooms the <see cref="AxisBase{t}.ActualLowerBound"/> and the <see cref="AxisBase{t}.ActualUpperBound"/> 
		/// by the given <paramref name="factor"/> and the relative center point <paramref name="relativeCenter"/>.
		/// </summary>
		/// <param name="factor">The zoom factor. A value between 0 and 1 zooms in. A value greater than 1 zooms out.</param>
		/// <param name="relativeCenter">The relative center point of the zoom operation. A value of 0 is mapped to the 
		/// <see cref="AxisBase{t}.ActualLowerBound"/>, a value of 1 is mapped to the <see cref="AxisBase{t}.ActualUpperBound"/>.</param>
		public void Zoom(double factor, double relativeCenter)
		{
			// TODO: Werte validieren
			double newLowerBound;
			double newUpperBound;
			MathHelper.Zoom(
				ActualLowerBound,
				ActualUpperBound,
				factor,
				out newLowerBound,
				out newUpperBound,
				relativeCenter);
			SetBounds(newLowerBound, newUpperBound);
		}

		/// <summary>
		/// Centers  the <see cref="AxisBase{t}.ActualLowerBound"/> and the <see cref="AxisBase{t}.ActualUpperBound"/> equaly by around the given <paramref name="value"/>.
		/// </summary>
		/// <param name="value">The absolute value that will become the center of this <see cref="DoubleAxisBase"/>.</param>
		public void Center(double value)
		{
			var diff = ActualUpperBound - ActualLowerBound;
			SetBounds(value - diff / 2, value + diff / 2);
		}

		// OPT: Diese beiden Methoden evtl. in einen eigenen ScreenMapper auslagern

		/// <summary>
		/// Maps a given logical axes value to the absolute screen point of the drawing area.
		/// </summary>
		/// <param name="value">The logical value that should be mapped.</param>
		/// <returns>The screen point.</returns>
		/// <seealso cref="MapScreenToLogical"/>
		public double MapLogicalToScreen(double value)
		{
			var actualLowerBound = ActualLowerBound;
			var actualUpperBound = ActualUpperBound;
			return MapLogicalToScreen(actualLowerBound, actualUpperBound, value, true);
		}

		internal double MapLogicalToScreen(
			double frozenLowerBound, double frozenUpperBound, double value, bool useScaledCoordinates)
		{
			var s = frozenLowerBound;
			var t = frozenUpperBound;

			var drawingArea = new Rect(
				0, 0,
				useScaledCoordinates ? _drawingAreaInfo.ScaledDrawingWidth : _drawingAreaInfo.DrawingWidth,
				useScaledCoordinates ? _drawingAreaInfo.ScaledDrawingHeight : _drawingAreaInfo.DrawingHeight);
			var a = drawingArea.Bottom;
			var b = drawingArea.Top;

			return MathHelper.MapPoint(a, b, s, t, value);
		}

		/// <summary>
		/// Maps a given screen point of the drawing area to a logical axes value.
		/// </summary>
		/// <param name="value">The screen point that should be mapped.</param>
		/// <returns>The logical value.</returns>
		public double MapScreenToLogical(double value)
		{
			var drawingArea = new Rect(
				0, 0, _drawingAreaInfo.ScaledDrawingWidth, _drawingAreaInfo.ScaledDrawingHeight);
			var s = drawingArea.Bottom;
			var t = drawingArea.Top;

			var a = ActualLowerBound;
			var b = ActualUpperBound;

			var res = MathHelper.MapPoint(a, b, s, t, value);
			return res;
		}
	}
}