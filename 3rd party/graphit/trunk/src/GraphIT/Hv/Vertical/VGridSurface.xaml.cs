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
using System.Windows;
using System.Windows.Media;

namespace TechNewLogic.GraphIT.Hv.Vertical
{
	/// <summary>
	/// Zeichnet das Raster sowie die Lineale.
	/// </summary>
	partial class VGridSurface
	{
		public VGridSurface(VStepHelper vStepHelper)
		{
			_vStepHelper = vStepHelper;
			_vStepHelper.StepsChanged += (s, e) => ResetGrid();

			InitializeComponent();
		}

		private readonly VStepHelper _vStepHelper;

		private void ResetGrid()
		{
			grid.Children.Clear();
			CreateStaticGrid();
		}

		private void CreateStaticGrid()
		{
			//var visualStepSize = ActualHeight / _vStepHelper.Steps;
			var visualStepSize = _lastSize.Height / _vStepHelper.Steps;
			for (var i = 0; i <= _vStepHelper.Steps; i++)
			{
				var ruler = new VGridLine();
				ruler.Margin = new Thickness(0, visualStepSize * i - ruler.Height / 2, 0, 0);
				grid.Children.Add(ruler);
			}
		}

		private Size _lastSize;

		protected override Size ArrangeOverride(Size arrangeBounds)
		{
			var innerSize = base.ArrangeOverride(arrangeBounds);
			var outerSize = this.GetOuterSize(innerSize);
			if (!_lastSize.Equals(outerSize))
			{
				_lastSize = outerSize;
				ResetGrid();
			}
			return innerSize;
		}

		//protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
		//{
		//    ResetGrid();
		//    base.OnRenderSizeChanged(sizeInfo);
		//}
	}
}
