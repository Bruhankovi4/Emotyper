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
using System.Windows;

namespace TechNewLogic.GraphIT
{
	/// <summary>
	/// Interaction logic for SimpleContextMenuAction.xaml
	/// </summary>
	partial class SimpleContextMenuAction
	{
		public SimpleContextMenuAction(
			string header,
			Action handler,
			Action hideMenu)
		{
			_handler = handler;
			_hideMenu = hideMenu;
			Header = header;
			InitializeComponent();
		}

		private readonly Action _handler;
		private readonly Action _hideMenu;

		public string Header { get; private set; }

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			_handler();
			_hideMenu();
		}
	}
}
