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

namespace TechNewLogic.GraphIT.Hv
{
	class CustomControlManager
	{
		public CustomControlManager(ICustomControlSurface customControlSurface)
		{
			_customControlSurface = customControlSurface;
		}

		private readonly ICustomControlSurface _customControlSurface;
		private readonly List<CustomControlInfo> _customControls = new List<CustomControlInfo>();

		public void AddCustomControl(Func<double> getTop, Func<double> getLeft, UIElement control)
		{
			if (_customControls.Any(it => it.Control == control))
				throw new Exception("The custom control is already placed on the surface.");

			var customControlInfo = new CustomControlInfo(control, getTop, getLeft);
			_customControlSurface.AddControl(customControlInfo);
			_customControls.Add(customControlInfo);
		}

		public void RemoveCustomControl(UIElement control)
		{
			_customControlSurface.RemoveControl(control);
			var customControlInfo = _customControls.FirstOrDefault(it => it.Control == control);
			if (customControlInfo != null)
				_customControls.Remove(customControlInfo);
		}

		public void Clear()
		{
			_customControls
				.Select(it => it.Control)
				.ToList()
				.ForEach(RemoveCustomControl);
		}

		public void RefreshCustomControls()
		{
			_customControls.ForEach(it => it.RaiseRefreshRequest());
		}
	}
}
