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

namespace TechNewLogic.GraphIT.Hv.Legend
{
	class GridViewHelper
	{
		#region HeaderID Property

		public static string GetHeaderID(DependencyObject obj)
		{
			return (string)obj.GetValue(HeaderIDProperty);
		}

		public static void SetHeaderID(DependencyObject obj, string value)
		{
			obj.SetValue(HeaderIDProperty, value);
		}

		// Using a DependencyProperty as the backing store for HeaderID.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty HeaderIDProperty =
			DependencyProperty.RegisterAttached("HeaderID", typeof(string), typeof(GridViewHelper), new UIPropertyMetadata(string.Empty));

		#endregion

		#region CanColumnResize Property

		public static bool GetCanColumnResize(DependencyObject obj)
		{
			return (bool)obj.GetValue(CanColumnResizeProperty);
		}

		public static void SetCanColumnResize(DependencyObject obj, bool value)
		{
			obj.SetValue(CanColumnResizeProperty, value);
		}

		// Using a DependencyProperty as the backing store for CanColumnResize.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty CanColumnResizeProperty =
			DependencyProperty.RegisterAttached("CanColumnResize", typeof(bool), typeof(GridViewHelper), new UIPropertyMetadata(true));

		#endregion
	}
}
