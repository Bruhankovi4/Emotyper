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
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace TechNewLogic.GraphIT.Hv.Legend
{
	class UniformWrapPanel : Panel
	{
		private double _elementWidth;
		private double _elementHeight;
		private int _maxElementPerRow;

		public int MaxRows { get; set; }

		protected override Size MeasureOverride(Size availableSize)
		{
			if (Children.Count == 0)
				return new Size();

			var children = Children.OfType<UIElement>().ToArray();

			// Damit die Children eine DesiredSize haben, muss erstmal Measure gemacht werden
			children.ForEachElement(it => it.Measure(availableSize));

			// Was ist das breiteste / höchste Element?
			var widestWidth = children.Max(it => it.DesiredSize.Width);
			_elementHeight = children.Max(it => it.DesiredSize.Height);

			// Wieviele Elemente bekommt man in eine Zeile?
			_maxElementPerRow = (int)(availableSize.Width / widestWidth);
			if (_maxElementPerRow == 0)
				_maxElementPerRow = 1;

			_elementWidth = availableSize.Width / _maxElementPerRow;

			var resultingHeight = ((children.Count() - 1) / _maxElementPerRow + 1) * _elementHeight;

			var desiredSize = new Size(availableSize.Width, resultingHeight);
			return desiredSize;
		}

		protected override Size ArrangeOverride(Size finalSize)
		{
			if (Children.Count == 0)
				return finalSize;

			var children = Children.OfType<UIElement>().ToArray();

			var actualTop = 0d;
			var currentNumOfChildrenInRow = 0;
			foreach (var child in children)
			{
				child.Arrange(new Rect(
					_elementWidth * currentNumOfChildrenInRow,
					actualTop,
					_elementWidth,
					_elementHeight));
				currentNumOfChildrenInRow++;

				if (currentNumOfChildrenInRow != _maxElementPerRow)
					continue;
				actualTop += _elementHeight;
				currentNumOfChildrenInRow = 0;
			}
			var size = new Size(
				_elementWidth * _maxElementPerRow,
				actualTop + _elementHeight);
			size.Width = Math.Max(size.Width, finalSize.Width);
			size.Height = Math.Max(size.Height, finalSize.Height);
			return size;
		}
	}
}
