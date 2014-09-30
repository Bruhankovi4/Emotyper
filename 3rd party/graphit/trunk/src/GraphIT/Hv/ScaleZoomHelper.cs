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
using System.Text;
using System.Windows;
using System.Windows.Input;
using TechNewLogic.GraphIT.Helper;

namespace TechNewLogic.GraphIT.Hv
{
	class ScaleZoomHelper
	{
		public ScaleZoomHelper(
			FrameworkElement visual,
			bool isHorizontal)
		{
			_isHorizontal = isHorizontal;
			
			_visual = visual;
			// TODO: abhängen
			_visual.PreviewMouseWheel += MouseWheel;
		}

		private readonly FrameworkElement _visual;
		private readonly bool _isHorizontal;

		public event Action<ScrollInfo> Scrolled;
		private void OnScrolled(double delta, double relativeTop)
		{
			if (Scrolled != null)
				Scrolled(new ScrollInfo(delta, relativeTop));
		}

		private void MouseWheel(object sender, MouseWheelEventArgs e)
		{
			var pos = e.GetPosition(_visual);
			OnScrolled(
				e.Delta,
				_isHorizontal
					? pos.X / _visual.ActualWidth
					: 1 - pos.Y / _visual.ActualHeight);
		}
	}
}
