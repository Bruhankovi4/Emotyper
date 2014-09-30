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
using System.Text;

namespace TechNewLogic.GraphIT
{
	/// <summary>
	/// Provides information of the pixel depth for drawing the <see cref="Curve"/>s.
	/// </summary>
	public class Bpp
	{
		static Bpp()
		{
			One = new Bpp(1);
			Eight = new Bpp(8);
		}

		/// <summary>
		/// Defines a 1 bit per pixel value.
		/// </summary>
		public static readonly Bpp One;

		/// <summary>
		/// Defines a 8 bit per pixel value.
		/// </summary>
		public static readonly Bpp Eight;

		private Bpp(int value)
		{
			Value = value;
		}

		/// <summary>
		/// Gets the value in bit per pixel.
		/// </summary>
		public int Value { get; private set; }
	}
}
