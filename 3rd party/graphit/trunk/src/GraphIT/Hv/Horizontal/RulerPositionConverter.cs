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
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TechNewLogic.GraphIT.Hv.Horizontal
{
	class RulerPositionConverter : IMultiValueConverter
	{
		public double Offset { get; set; }

		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			var positionToReference = (HorizontalAlignment)values[0];
			switch (positionToReference)
			{
				case HorizontalAlignment.Left:
					return Offset;
				case HorizontalAlignment.Right:
					var actualWidth = (double)values[1];
					var maxWidth = (double)values[2];
					var retVal = actualWidth > maxWidth ? maxWidth : actualWidth;
					return -retVal + Offset;
				default:
					return 0;
			}
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
