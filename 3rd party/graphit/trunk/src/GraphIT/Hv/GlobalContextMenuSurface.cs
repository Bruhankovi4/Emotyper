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
using System.Windows.Controls;
using System.Windows.Input;

namespace TechNewLogic.GraphIT.Hv
{
	class GlobalContextMenuSurface : ContentControl
	{
		public GlobalContextMenuSurface(
			ContextMenuControl contextMenuControl,
			InputReference inputReference)
		{
			_contextMenuControl = contextMenuControl;
			Content = contextMenuControl;
			// TODO: abmelden
			inputReference.InputElement.MouseRightButtonDown += InputElement_MouseRightButtonDown;
		}

		private readonly ContextMenuControl _contextMenuControl;

		void InputElement_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{
			_contextMenuControl.Show();
		}
	}
}
