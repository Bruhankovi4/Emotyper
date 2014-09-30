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
// 
// 

using System.Windows;
using System.Windows.Controls;

namespace TestHost
{
	public class DropdownMenu : ContentControl
	{
		static DropdownMenu()
		{
			DefaultStyleKeyProperty.OverrideMetadata(
				typeof(DropdownMenu),
				new FrameworkPropertyMetadata(typeof(DropdownMenu)));
		}

		#region MenuName

		public string MenuName
		{
			get { return (string)GetValue(MenuNameProperty); }
			set { SetValue(MenuNameProperty, value); }
		}

		// Using a DependencyProperty as the backing store for MenuName.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty MenuNameProperty =
			DependencyProperty.Register("MenuName", typeof(string), typeof(DropdownMenu), new UIPropertyMetadata(null));

		#endregion
	}
}
