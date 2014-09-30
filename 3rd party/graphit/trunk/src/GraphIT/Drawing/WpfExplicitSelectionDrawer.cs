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
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TechNewLogic.GraphIT.Helper;

namespace TechNewLogic.GraphIT.Drawing
{
	class WpfExplicitSelectionDrawer : IDrawer
	{
		public WpfExplicitSelectionDrawer(
			Func<ILogicalToScreenMapper> logicalToScreenMapperFactory,
			Func<WpfDrawingVisual> drawingVisualFactory)
		{
			_logicalToScreenMapperFactory = logicalToScreenMapperFactory;
			_drawingVisualFactory = drawingVisualFactory;
		}

		private readonly Func<ILogicalToScreenMapper> _logicalToScreenMapperFactory;
		private readonly Func<WpfDrawingVisual> _drawingVisualFactory;
		
		private RenderTargetBitmap _bitmapRtb;
		private FormatConvertedBitmap _bitmapFcb;
		
		private RenderTargetBitmap _selectionRtb;
		private FormatConvertedBitmap _selectionFcb;

		public HitTester DrawLine(
			double thickness,
			IEnumerable<IList<Point>> points,
			int width,
			int height,
			out byte[] bitmap,
			out byte[] selection)
		{
			var stride = MathHelper.GetStride(width, DrawingParameter.Bpp.Value);

			bitmap = RenderCurve(ref _bitmapRtb, ref _bitmapFcb, width, points, thickness, 5, height, stride);
			selection = RenderCurve(ref _selectionRtb, ref _selectionFcb, width, points, 7.5, 5, height, stride);

			return new HitTester(selection.Copy(), stride);
		}

		private byte[] RenderCurve(
			ref RenderTargetBitmap rtb,
			ref FormatConvertedBitmap fcb,
			int width,
			IEnumerable<IList<Point>> collection,
			double thickness,
			int miterLimit,
			int height,
			int stride)
		{
			if (rtb == null
				|| rtb.PixelWidth != width
				|| rtb.PixelHeight != height)
			{
				rtb = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
				fcb = new FormatConvertedBitmap(rtb, DrawingParameter.TargetPixelFormat, DrawingParameter.TargetBitmapPalette, 1);
			}
			else
			{
				rtb.Clear();
			}

			collection
				.Select(it =>
					{
						var visual = _drawingVisualFactory();
						visual.Draw(it, thickness, miterLimit, _logicalToScreenMapperFactory());
						return visual;
					})
				.ForEachElement(rtb.Render);

			var curvePixels = new byte[stride * height];
			fcb.CopyPixels(curvePixels, stride, 0);
			return curvePixels;
		}
	}
}