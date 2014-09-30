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
using System.Windows.Input;
using Microsoft.Expression.Interactivity.Core;

namespace TechNewLogic.GraphIT.Hv.Legend
{
	/// <summary>
	/// Interaction logic for TileControl.xaml
	/// </summary>
	sealed partial class TileControl
	{
		public TileControl()
		{
			InitializeComponent();
		}

		#region ViewModel

		public TileLegendItemViewModel ViewModel
		{
			get { return (TileLegendItemViewModel)GetValue(ViewModelProperty); }
			set { SetValue(ViewModelProperty, value); }
		}

		// Using a DependencyProperty as the backing store for ViewModel.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ViewModelProperty =
			DependencyProperty.Register(
				"ViewModel", typeof(TileLegendItemViewModel), typeof(TileControl),
				new UIPropertyMetadata((s, e) => (e.NewValue as TileLegendItemViewModel).PropertyChanged += (s1, e1) => (s as TileControl).UpdateVisualState()));

		#endregion

		private void UpdateVisualState()
		{
			if (ViewModel.IsSelected)
				ExtendedVisualStateManager.GoToElementState(this, "Selected", true);
			else if (!ViewModel.IsVisible)
				ExtendedVisualStateManager.GoToElementState(this, "Invisible", true);
			else
				ExtendedVisualStateManager.GoToElementState(this, "Normal", true);
		}

		private void ContentControl_MouseEnter(object sender, MouseEventArgs e)
		{
			ViewModel.SelectCurve();
		}

		private void ContentControl_MouseLeave(object sender, MouseEventArgs e)
		{
			ViewModel.UnselectCurve();
		}

		private void ContentControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			ViewModel.TogglePin();
		}
	}
}
