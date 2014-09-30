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
using System.Collections.Generic;
using System.Windows;

namespace TechNewLogic.GraphIT.Hv.Vertical
{
	class DoubleScaleLabelCreationStrategy : IScaleLabelCreationStrategy
	{
		public DoubleScaleLabelCreationStrategy(
			DoubleScale scale,
			VStepHelper vStepHelper)
		{
			_scale = scale;

			_vStepHelper = vStepHelper;
			_vStepHelper.StepsChanged += (s, e) =>
				{
					CreateSectionLabels();
					UpdateSectionLabels();
				};

			CreateSectionLabels();
			UpdateSectionLabels();
		}

		private readonly DoubleScale _scale;
		private readonly VStepHelper _vStepHelper;
		private readonly List<VSectionLabel> _sectionLabels = new List<VSectionLabel>();

		private void CreateSectionLabels()
		{
			_scale.ClearLabels();
			_sectionLabels.Clear();

			for (var i = 0; i <= _vStepHelper.Steps; i++)
			{
				var sectionLabel = new VSectionLabel(100, false);
				_sectionLabels.Add(sectionLabel);
				_scale.AddLabel(sectionLabel);
			}
		}

		public void UpdateSectionLabels()
		{
			if (_sectionLabels.Count != _vStepHelper.Steps + 1)
				return;

			//var visualStepSize = ActualHeight / _vStepHelper.Steps;
			var visualStepSize = _scale.LastRenderSize.Height / _vStepHelper.Steps;
			var upperBound = _scale.AxesGroup.ProxyAxis.ActualUpperBound;
			var lowerBound = _scale.AxesGroup.ProxyAxis.ActualLowerBound;
			var logicalStepSize = (upperBound - lowerBound) / _vStepHelper.Steps;
			for (var i = 0; i <= _vStepHelper.Steps; i++)
			{
				var sectionLabel = _sectionLabels[i];
				//sectionLabel.Text = (upperBound - logicalStepSize * i).ToString("F");
				sectionLabel.Text = _scale.GetFormattedLabelValue(upperBound - logicalStepSize * i);
				sectionLabel.Margin = new Thickness(0, visualStepSize * i - sectionLabel.Height / 2, 0, 0);
			}
		}
	}
}