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
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using Microsoft.Expression.Interactivity.Core;

namespace TechNewLogic.GraphIT.Hv
{
	/// <summary>
	/// Interaction logic for BeltControl.xaml
	/// </summary>
	partial class BeltControl : INotifyPropertyChanged
	{
		public BeltControl()
		{
			InitializeComponent();
		}

		private Brush _minColor = Brushes.Transparent;
		public Brush MinColor
		{
			get { return _minColor; }
			private set
			{
				if (_minColor.Equals(value))
					return;
				_minColor = value;
				OnPropertyChanged("MinColor");
			}
		}

		private Brush _maxColor = Brushes.Transparent;
		public Brush MaxColor
		{
			get { return _maxColor; }
			private set
			{
				if (_maxColor.Equals(value))
					return;
				_maxColor = value;
				OnPropertyChanged("MaxColor");
			}
		}

		private bool _areBoundsVisible;
		public bool AreBoundsVisible
		{
			get { return _areBoundsVisible; }
			set
			{
				_areBoundsVisible = value;
				OnPropertyChanged("AreBoundsVisible");
			}
		}

		public void SetBoundaries(
			double minMin, double min, double max, double maxMax)
		{
			if (double.IsInfinity(max) || double.IsInfinity(maxMax))
				return;
			minMin = Limit(minMin);
			min = Limit(min);
			max = Limit(max);
			maxMax = Limit(maxMax);

			MinMinBoundary.Height = minMin;
			MinBoundary.Height = min;
			// IMP: Bei ganz oft rein und rausscrollen kann es hier (max == Infinity) zu einem Absturz kommen
			// Ist das mit den Zeilen oben gelöst?
			MaxBoundary.Height = max;
			MaxMaxBoundary.Height = maxMax;
		}

		public void SetColors(Color minColor, Color maxColor)
		{
			MinColor = new LinearGradientBrush(
				new Color { A = 60, R = minColor.R, G = minColor.G, B = minColor.B }, 
				new Color { A = 20, R = minColor.R, G = minColor.G, B = minColor.B }, 
				new Point(0, 0), 
				new Point(0, 1));
			MaxColor = new LinearGradientBrush(
				new Color { A = 60, R = maxColor.R, G = maxColor.G, B = maxColor.B },
				new Color { A = 20, R = maxColor.R, G = maxColor.G, B = maxColor.B }, 
				new Point(0, 1), 
				new Point(0, 0));
		}

		private static double Limit(double value)
		{
			if (value < 0)
				value = 0;
			return value;
		}

		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));

			UpdateVisualState();
		}

		private void UpdateVisualState()
		{
			if (AreBoundsVisible)
				ExtendedVisualStateManager.GoToElementState(this, "Normal", true);
			else
				ExtendedVisualStateManager.GoToElementState(this, "Invisible", true);
		}
	}
}
