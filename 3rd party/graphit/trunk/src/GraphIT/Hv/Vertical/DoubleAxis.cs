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
using TechNewLogic.GraphIT.Helper;

namespace TechNewLogic.GraphIT.Hv.Vertical
{
	/// <summary>
	/// Represents an axis with a double value as dimension parameter. This <see cref="DoubleAxis"/> is used by the <see cref="TimeDoubleCurve"/>.
	/// </summary>
	public sealed class DoubleAxis : DoubleAxisBase
	{
		internal DoubleAxis(
			TimeDoubleCurve parent,
			IDrawingAreaInfo drawingAreaInfo,
			DoubleAxisParameter doubleAxisParameter)
			: base(doubleAxisParameter, drawingAreaInfo)
		{
			Curve = parent;
		}

		/// <summary>
		/// The associated <see cref="TimeDoubleCurve"/> for this <see cref="DoubleAxis"/>.
		/// </summary>
		public TimeDoubleCurve Curve { get; private set; }
	}
}
