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
using System.Windows.Documents;
using System.Windows.Media;

namespace TechNewLogic.GraphIT.Helper
{
	abstract class BaseContainerAdorner : Adorner
	{
		protected BaseContainerAdorner(UIElement adornedElement)
			: base(adornedElement)
		{
			VisualChildren = new VisualCollection(this);
		}

		protected VisualCollection VisualChildren { get; private set; }
		protected override int VisualChildrenCount { get { return VisualChildren.Count; } }
		protected override Visual GetVisualChild(int index) { return VisualChildren[index]; }
	}
}