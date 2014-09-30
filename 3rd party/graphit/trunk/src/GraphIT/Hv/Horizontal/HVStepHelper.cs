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

namespace TechNewLogic.GraphIT.Hv.Horizontal
{
	class HStepHelper
	{
		public HStepHelper(
			IDrawingAreaInfo drawingAreaInfo)
		{
			Steps = 2;
			_drawingAreaInfo = drawingAreaInfo;
			_drawingAreaInfo.DrawingSizeChanged += (s, e) => CalculateSteps();
		}

		private readonly IDrawingAreaInfo _drawingAreaInfo;

		public int Steps { get; private set; }

		private void CalculateSteps()
		{
			var lastSteps = Steps;
			if (_drawingAreaInfo.ScaledDrawingWidth > 700)
				Steps = 10;
			else if (_drawingAreaInfo.ScaledDrawingWidth > 350)
				Steps = 5;
			else if (_drawingAreaInfo.ScaledDrawingWidth > 150)
				Steps = 2;
			else
				Steps = 1;

			if (Steps != lastSteps)
				OnStepsChanged();
		}

		public event EventHandler StepsChanged;
		private void OnStepsChanged()
		{
			if (StepsChanged != null)
				StepsChanged(this, new EventArgs());
		}
	}
}
