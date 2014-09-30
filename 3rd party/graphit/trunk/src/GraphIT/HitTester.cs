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

namespace TechNewLogic.GraphIT
{
	/// <summary>
	/// Ermittelt durch einen Vorher-Nachher-Vergleich zweier Pixel-Arrays, an welchen
	/// Stellen sich ein Element (z.B. eine Kurve) befindet und an welchen nicht.
	/// </summary>
	class HitTester
	{
		public HitTester()
		{
			_isEmpty = true;
		}

		public HitTester(byte[] buffer, int stride)
		{
			_buffer = buffer;
			_stride = stride;
		}

		private readonly bool _isEmpty;

		private readonly byte[] _buffer;
		private readonly int _stride;

		public bool HitTest(int x, int y)
		{
			if (_isEmpty)
				return false;

			var index = DrawingParameter.Bpp == Bpp.One
				? (_stride * y) + (x / 8)
				: (_stride * y) + x;
			if (_buffer.Length <= index || index < 0)
				return false;
			return _buffer[index] != 0;
		}
	}
}
