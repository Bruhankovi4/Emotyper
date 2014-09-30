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
	/// Provides functionality to register menu elements to a context menu.
	/// </summary>
	public interface IContextMenuRegistrar
	{
		/// <summary>
		/// Adds a <see cref="UIElement"/> to the list of menu point to the context menu.
		/// </summary>
		/// <param name="entry">The <see cref="UIElement"/> to add.</param>
		void AddMenuEntry(UIElement entry);

		/// <summary>
		/// Adds a clickable element to the list of menu point to the context menu.
		/// </summary>
		/// <param name="header">The header text of the element.</param>
		/// <param name="handler">The action that will be invoked when the menu entry is clicked.</param>
		void AddMenuEntry(string header, Action handler);
	}
}