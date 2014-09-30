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
using System.Windows.Media;

namespace TechNewLogic.GraphIT.Hv.Legend
{
	/// <summary>
	/// Interaction logic for TableCellControl.xaml
	/// </summary>
	public partial class TableCellControl
	{
		public TableCellControl()
		{
			InitializeComponent();
		}

		#region Text Property

		public string Text
		{
			get { return (string)GetValue(TextProperty); }
			set { SetValue(TextProperty, value); }
		}

		// Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty TextProperty =
			DependencyProperty.Register("Text", typeof(string), typeof(TableCellControl), new UIPropertyMetadata(string.Empty));

		#endregion

		#region CellBackground Property

		public Brush CellBackground
		{
			get { return (Brush)GetValue(CellBackgroundProperty); }
			set { SetValue(CellBackgroundProperty, value); }
		}

		// Using a DependencyProperty as the backing store for CellBackground.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty CellBackgroundProperty =
			DependencyProperty.Register("CellBackground", typeof(Brush), typeof(TableCellControl), new UIPropertyMetadata(Brushes.Transparent));

		#endregion

		#region ControlContent Property

		public object ControlContent
		{
			get { return (object)GetValue(ControlContentProperty); }
			set { SetValue(ControlContentProperty, value); }
		}

		// Using a DependencyProperty as the backing store for ControlContent.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ControlContentProperty =
			DependencyProperty.Register("ControlContent", typeof(object), typeof(TableCellControl), new UIPropertyMetadata(null));

		#endregion
	}
}
