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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TechNewLogic.GraphIT.Drawing;

namespace TechNewLogic.GraphIT
{
	/// <summary>
	/// Provides basic constant information for the drawing subsystem.
	/// </summary>
	public static class DrawingParameter
	{
		static DrawingParameter()
		{
#if BPP8
			Bpp = Bpp.Eight;
			TargetPixelFormat = PixelFormats.Indexed8;
#elif BPP1
			Bpp = Bpp.One;
			TargetPixelFormat = PixelFormats.Indexed1;
#else
#error You need to specify either BPP1 or BPP8 compiler constant.
#endif

			TargetBitmapPalette = SolidPalette.GetSolid(Colors.Black, Colors.White, Bpp.Value);
		}

		/// <summary>
		/// Specifies a value which indicates the pixel depth of the bitmaps drawn by the <see cref="Curve"/>s.
		/// </summary>
		public static readonly Bpp Bpp;

		/// <summary>
		/// Specifies a value which indicates the pixel format of the bitmaps drawn by the <see cref="Curve"/>s.
		/// </summary>
		public static readonly PixelFormat TargetPixelFormat;

		/// <summary>
		/// Specifies a value which indicates the color palette which is used to color the <see cref="Curve"/>s.
		/// </summary>
		public static readonly BitmapPalette TargetBitmapPalette;
	}
}
