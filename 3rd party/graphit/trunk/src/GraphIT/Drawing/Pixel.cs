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
namespace TechNewLogic.GraphIT.Drawing
{
	class Pixel
	{
		public static Pixel Empty
		{
			get { return new Pixel(double.NaN, double.NaN); }
		}

		public Pixel(double x, double y)
		{
			X = x;
			Y = y;
		}

		public double X { get; private set; }
		public double Y { get; private set; }

		public bool IsEmpty { get { return double.IsNaN(X) && double.IsNaN(Y); } }

		// override object.Equals
		public override bool Equals(object obj)
		{
			if (obj == null || GetType() != obj.GetType())
				return false;
			var p = (Pixel)obj;
			return X == p.X && Y == p.Y;
		}

		// override object.GetHashCode
		public override int GetHashCode()
		{
			return (X + Y.ToString()).GetHashCode();
		}

		public override string ToString()
		{
			return "X: " + X + " / Y: " + Y;
		}
	}
}