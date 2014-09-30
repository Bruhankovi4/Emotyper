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
namespace TechNewLogic.GraphIT.Hv.Vertical
{
	class DoubleAxisParameter : AxisParameter<double>
	{
		public DoubleAxisParameter(string uom, double lowerBound, double upperBound, AxisFormat axisFormat)
			: base(lowerBound, upperBound)
		{
			// TODO: Bounds Validierung (hmm... die SetBounds Methode muss ja etwas ganz ähnliches machen...
			Uom = uom;
			AxisFormat = axisFormat;
		}

		public string Uom { get; private set; }
		public AxisFormat AxisFormat { get; private set; }
	}
}