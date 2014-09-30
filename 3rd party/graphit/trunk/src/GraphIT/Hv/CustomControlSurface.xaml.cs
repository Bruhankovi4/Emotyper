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
using System.Windows.Controls;

namespace TechNewLogic.GraphIT.Hv
{
	/// <summary>
	/// Interaction logic for CustomControlSurface.xaml
	/// </summary>
	partial class CustomControlSurface : ICustomControlSurface
	{
		public CustomControlSurface()
		{
			InitializeComponent();
		}

		private readonly List<CustomControlInfo> _customControlInfos
			= new List<CustomControlInfo>();

		public void AddControl(CustomControlInfo customControlInfo)
		{
			if (_customControlInfos.Any(it => it.Control == customControlInfo.Control))
				throw new Exception("Custom Control is already added.");

			UpdatePosition(customControlInfo);
			customControlInfo.RefreshRequest = () => UpdatePosition(customControlInfo);
			canvas.Children.Add(customControlInfo.Control);
			_customControlInfos.Add(customControlInfo);
		}

		public void RemoveControl(UIElement control)
		{
			var controlInfo = _customControlInfos.FirstOrDefault(it => it.Control == control);
			if (controlInfo == null)
				return;
			controlInfo.RefreshRequest = null;
			canvas.Children.Remove(control);
			_customControlInfos.Remove(controlInfo);
		}

		private static void UpdatePosition(CustomControlInfo customControlInfo)
		{
			Canvas.SetTop(customControlInfo.Control, customControlInfo.Top);
			Canvas.SetLeft(customControlInfo.Control, customControlInfo.Left);
		}

		private Size _lastSize;

		protected override Size ArrangeOverride(Size arrangeBounds)
		{
			var innerSize = base.ArrangeOverride(arrangeBounds);
			var outerSize = this.GetOuterSize(innerSize);
			if (!_lastSize.Equals(outerSize))
			{
				_lastSize = outerSize;
				_customControlInfos.ForEachElement(UpdatePosition);
			}
			return innerSize;
		}

		//protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
		//{
		//    base.OnRenderSizeChanged(sizeInfo);
		//    _customControlInfos.ForEachElement(UpdatePosition);
		//}
	}
}
