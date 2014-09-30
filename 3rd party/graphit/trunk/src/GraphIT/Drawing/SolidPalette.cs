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
using System.Linq;
using System.Reflection;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TechNewLogic.GraphIT.Drawing
{
	static class SolidPalette
	{
		public static BitmapPalette GetSolid(Color firstColor, Color color, int colorCount)
		{
			return new BitmapPalette(
				new[] { firstColor }
				.Concat(Enumerable.Repeat(color, colorCount))
				.ToList());
		}
	}
}
