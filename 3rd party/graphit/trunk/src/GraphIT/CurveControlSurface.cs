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
using System.Windows.Controls;

namespace TechNewLogic.GraphIT
{
	class CurveControlSurface : ICurveControlSurface
	{
		public CurveControlSurface(CurveDisplay curveDisplay)
		{
			_left = curveDisplay.LeftPlaceholder;
			_right = curveDisplay.RightPlaceholder;
			_top = curveDisplay.TopPlaceholder;
			_bottom = curveDisplay.BottomPlaceholder;
			_centerFront = curveDisplay.CenterFrontPlaceholder;
			_centerBack = curveDisplay.CenterBackPlaceholder;
			_centerTopmost = curveDisplay.CenterTopmostPlaceholder;
		}

		private readonly Panel _left;
		private readonly Panel _right;
		private readonly Panel _top;
		private readonly Panel _bottom;
		private readonly Panel _centerFront;
		private readonly Panel _centerBack;
		private readonly Panel _centerTopmost;

		public void AddControl(UIElement control, SurfacePlacement placement)
		{
			Panel panel;
			switch (placement)
			{
				case SurfacePlacement.Left:
					panel = _left;
					break;
				case SurfacePlacement.Right:
					panel = _right;
					break;
				case SurfacePlacement.Top:
					panel = _top;
					break;
				case SurfacePlacement.Bottom:
					panel = _bottom;
					break;
				case SurfacePlacement.CenterFront:
					panel = _centerFront;
					break;
				case SurfacePlacement.CenterBack:
					panel = _centerBack;
					break;
				case SurfacePlacement.CenterTopmost:
					panel = _centerTopmost;
					break;
				default:
					throw new ArgumentOutOfRangeException("placement");
			}

			panel.Children.Add(control);
		}
	}
}