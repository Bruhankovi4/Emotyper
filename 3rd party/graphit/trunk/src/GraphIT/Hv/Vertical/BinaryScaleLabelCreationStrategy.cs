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
using TechNewLogic.GraphIT.Helper;

namespace TechNewLogic.GraphIT.Hv.Vertical
{
	class BinaryScaleLabelCreationStrategy : IScaleLabelCreationStrategy
	{
		public BinaryScaleLabelCreationStrategy(
			DoubleScale scale)
		{
			_scale = scale;
			_scale.ClipLabels = true;

			_lowerSectionLabel = CreateLabel();
			_higherSectionLabel = CreateLabel();

			UpdateSectionLabels();
		}

		private readonly DoubleScale _scale;
		private readonly VSectionLabel _lowerSectionLabel;
		private readonly VSectionLabel _higherSectionLabel;

		private VSectionLabel CreateLabel()
		{
			var label = new VSectionLabel(double.NaN, true);
			_scale.AddLabel(label);
			return label;
		}

		public void UpdateSectionLabels()
		{
			if (_scale.LastRenderSize.Height == 0)
				return;

			var h1 = SetLabelPosition(_lowerSectionLabel, 0);
			var h2 = SetLabelPosition(_higherSectionLabel, 1);
			if (h1 - h2 < 15)
			{
				_lowerSectionLabel.Text = string.Empty;
				_higherSectionLabel.Text = string.Empty;
			}
			else
			{
				_lowerSectionLabel.Text = "0";
				_higherSectionLabel.Text = "1";
			}
		}

		private double SetLabelPosition(VSectionLabel label, int labelTop)
		{
			var mappedTop = MapScalePoint(labelTop);
			var middleTop = mappedTop - label.ActualHeight / 2;
			label.Margin = new Thickness(0, middleTop, 0, 0);
			return middleTop;
		}

		private double MapScalePoint(int targetValue)
		{
			var lowerTop = MathHelper.MapPoint(
				_scale.LastRenderSize.Height,
				0,
				_scale.AxesGroup.ProxyAxis.ActualLowerBound,
				_scale.AxesGroup.ProxyAxis.ActualUpperBound,
				targetValue);
			return lowerTop;
		}
	}
}