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
using Microsoft.Expression.Interactivity.Core;

namespace TechNewLogic.GraphIT.Hv.Horizontal
{
	/// <summary>
	/// Interaction logic for IntervalControl.xaml
	/// </summary>
	partial class IntervalControl
	{
		public IntervalControl()
		{
			InitializeComponent();
			MouseEnter += (s, e) => UpdateVisualStates();
			MouseLeave += (s, e) => UpdateVisualStates();
		}

		#region IsInOfflineMode

		public bool IsInOfflineMode
		{
			get { return (bool)GetValue(IsInOfflineModeProperty); }
			set { SetValue(IsInOfflineModeProperty, value); }
		}

		// Using a DependencyProperty as the backing store for IsInOfflineMode.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty IsInOfflineModeProperty =
			DependencyProperty.Register("IsInOfflineMode", typeof(bool), typeof(IntervalControl), new UIPropertyMetadata(false));

		#endregion

		#region IntervalLeftCommand

		public ICommand IntervalLeftCommand
		{
			get { return (ICommand)GetValue(IntervalLeftCommandProperty); }
			set { SetValue(IntervalLeftCommandProperty, value); }
		}

		// Using a DependencyProperty as the backing store for IntervalLeftCommand.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty IntervalLeftCommandProperty =
			DependencyProperty.Register("IntervalLeftCommand", typeof(ICommand), typeof(IntervalControl), new UIPropertyMetadata(null));

		#endregion

		#region IntervalRightCommand

		public ICommand IntervalRightCommand
		{
			get { return (ICommand)GetValue(IntervalRightCommandProperty); }
			set { SetValue(IntervalRightCommandProperty, value); }
		}

		// Using a DependencyProperty as the backing store for IntervalRightCommand.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty IntervalRightCommandProperty =
			DependencyProperty.Register("IntervalRightCommand", typeof(ICommand), typeof(IntervalControl), new UIPropertyMetadata(null));

		#endregion

		private void UpdateVisualStates()
		{
			if (IsMouseOver)
				ExtendedVisualStateManager.GoToElementState(this, "MouseOver", true);
			else
				ExtendedVisualStateManager.GoToElementState(this, "Normal", true);
		}
	}
}
