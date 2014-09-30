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
using System.Windows;
using System.Windows.Input;

namespace TechNewLogic.GraphIT
{
	class DesignerCurveDrawingSurface : FrameworkElement, ICurveDrawingSurface, IRedrawRequest, IPrintingRedrawRequest, IDrawingAreaInfo
	{
		public void RaiseRedrawRequest()
		{
		}

		public void RaiseRedrawRequest(Action<bool> callback, TimeSpan timeout)
		{
		}

		public event EventHandler DrawingSizeChanged;

		public double DrawingWidth { get { return ActualWidth; } }
		public double DrawingHeight { get { return ActualHeight; } }

		public double ScaledDrawingWidth { get { return ActualWidth; } }
		public double ScaledDrawingHeight { get { return ActualHeight; } }

		public Point MousePosition { get { return Mouse.GetPosition(this); } }
	}
}
