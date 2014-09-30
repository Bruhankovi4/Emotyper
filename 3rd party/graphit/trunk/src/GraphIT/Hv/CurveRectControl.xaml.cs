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

namespace TechNewLogic.GraphIT.Hv
{
	/// <summary>
	/// Interaction logic for CurveRectControl.xaml
	/// </summary>
	partial class CurveRectControl
	{
		public CurveRectControl()
		{
			InitializeComponent();
		}

		#region Curve Property

		public TimeDoubleCurve Curve
		{
			get { return (TimeDoubleCurve)GetValue(CurveProperty); }
			set { SetValue(CurveProperty, value); }
		}

		// Using a DependencyProperty as the backing store for Curve.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty CurveProperty =
			DependencyProperty.Register(
				"Curve", typeof(TimeDoubleCurve), typeof(CurveRectControl),
				new UIPropertyMetadata(OnCurveChanged));

		private static void OnCurveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (!(e.NewValue is TimeDoubleCurve))
				return;
			var curve = (TimeDoubleCurve)e.NewValue;
			curve.PropertyChanged += (s1, e1) => ((CurveRectControl)d).UpdateVisualStates();
		}

		#endregion

		private void CurveRect_MouseEnter(object sender, MouseEventArgs e)
		{
			SetCurveSelection(true);
		}

		private void CurveRect_MouseLeave(object sender, MouseEventArgs e)
		{
			SetCurveSelection(false);
		}

		private void CurveRect_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (Curve == null)
				return;
			Curve.TogglePin();
		}

		private void SetCurveSelection(bool isSelected)
		{
			if (Curve == null)
				return;
			if (isSelected)
				Curve.Select(false);
			else
				Curve.Unselect();
		}

		private void UpdateVisualStates()
		{
			if (!Curve.IsVisible)
				ExtendedVisualStateManager.GoToElementState(LayoutRoot, "Invisible", true);
			else if (Curve.IsSelected)
				ExtendedVisualStateManager.GoToElementState(LayoutRoot, "Selected", true);
			else
				ExtendedVisualStateManager.GoToElementState(LayoutRoot, "Normal", true);
		}
	}
}
