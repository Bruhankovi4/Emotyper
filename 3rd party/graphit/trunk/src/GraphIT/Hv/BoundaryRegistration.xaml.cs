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

namespace TechNewLogic.GraphIT.Hv
{
	class BeltRegistration : IDisposable
	{
		public BeltRegistration(
			TimeDoubleCurve curve,
			IDrawingAreaInfo drawingAreaInfo,
			BeltControl beltControl)
		{
			_curve = curve;
			_curve.BeltsChanged += (s, e) => UpdateBoundaries();
			_curve.DoubleAxis.BoundsChanged += (s, e) => UpdateBoundaries();
			_curve.IsSelectedChanged += (s, e) => UpdateBoundaries();

			_drawingAreaInfo = drawingAreaInfo;
			_drawingAreaInfo.DrawingSizeChanged += _drawingAreaInfo_DrawingSizeChanged;

			BeltControl = beltControl;
		}

		private readonly TimeDoubleCurve _curve;
		private readonly IDrawingAreaInfo _drawingAreaInfo;

		public BeltControl BeltControl { get; private set; }

		private void UpdateBoundaries()
		{
			var minMin = double.IsNaN(_curve.MinMinBelt)
				? 0
				: _drawingAreaInfo.DrawingHeight - _curve.DoubleAxis.MapLogicalToScreen(_curve.MinMinBelt);
			var min = double.IsNaN(_curve.MinBelt)
				? 0
				: _drawingAreaInfo.DrawingHeight - _curve.DoubleAxis.MapLogicalToScreen(_curve.MinBelt);
			var max = double.IsNaN(_curve.MaxBelt)
				? 0
				: _curve.DoubleAxis.MapLogicalToScreen(_curve.MaxBelt);
			var maxMax = double.IsNaN(_curve.MaxMaxBelt)
				? 0
				: _curve.DoubleAxis.MapLogicalToScreen(_curve.MaxMaxBelt);

			BeltControl.SetBoundaries(minMin, min, max, maxMax);
			BeltControl.SetColors(_curve.MinBeltColor, _curve.MaxBeltColor);
			BeltControl.AreBoundsVisible = _curve.IsSelected;
		}

		void _drawingAreaInfo_DrawingSizeChanged(object sender, EventArgs e)
		{
			UpdateBoundaries();
		}

		public void Dispose()
		{
			_drawingAreaInfo.DrawingSizeChanged -= _drawingAreaInfo_DrawingSizeChanged;
		}
	}
}