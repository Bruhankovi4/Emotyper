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
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using TechNewLogic.GraphIT.Helper;

namespace TechNewLogic.GraphIT.Drawing
{
	class WpfBlurSelectionDrawer : IDrawer
	{
		public WpfBlurSelectionDrawer(
			Func<ILogicalToScreenMapper> logicalToScreenMapperFactory,
			Func<WpfDrawingVisual> drawingVisualFactory)
		{
			_logicalToScreenMapperFactory = logicalToScreenMapperFactory;
			_drawingVisualFactory = drawingVisualFactory;
		}

		private readonly Func<ILogicalToScreenMapper> _logicalToScreenMapperFactory;
		private readonly Func<WpfDrawingVisual> _drawingVisualFactory;
		private RenderTargetBitmap _renderTargetBitmap;
		private FormatConvertedBitmap _formatConvertedBitmap;
		private Image _blurImage;
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

			//bitmap = RenderCurve(width, points, thickness, 5, height, stride);
			//selection = RenderCurve(width, points, 7.5, 5, height, stride);
			RenderCurve(width, points, thickness, 5, height, stride, out bitmap, out selection);

			return new HitTester(selection.Copy(), stride);
		}

		private void RenderCurve(
			int width,
			IEnumerable<IList<Point>> collection,
			double thickness,
			int miterLimit,
			int height,
			int stride,
			out byte[] bitmap,
			out byte[] selection)
		{
			if (_renderTargetBitmap == null
				|| _renderTargetBitmap.PixelWidth != width
				|| _renderTargetBitmap.PixelHeight != height)
			{
				_renderTargetBitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
				_formatConvertedBitmap = new FormatConvertedBitmap(_renderTargetBitmap, DrawingParameter.TargetPixelFormat, DrawingParameter.TargetBitmapPalette, 1);
				
				_blurImage = new Image
					{
						Effect = new BlurEffect
							{
								Radius = 15,
								KernelType = KernelType.Gaussian
							},
						Source = _renderTargetBitmap
					};
				_blurImage.Measure(new Size(width, height));
				_blurImage.Arrange(new Rect(0, 0, width, height));
				_blurImage.InvalidateVisual();
				_selectionRtb = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
				_selectionFcb = new FormatConvertedBitmap(_selectionRtb, DrawingParameter.TargetPixelFormat, DrawingParameter.TargetBitmapPalette, 1);
			}
			else
			{
				_renderTargetBitmap.Clear();
				_selectionRtb.Clear();
			}

			collection
				.Select(it =>
					{
						try
						{
							var visual = _drawingVisualFactory();
							visual.Draw(it, thickness, miterLimit, _logicalToScreenMapperFactory());
							return visual;
						}
						// Kann vorkommen, wenn der Container schon disposed ist, aber noch gezeichnet werden soll
						catch (ObjectDisposedException)
						{
							return new DrawingVisual();
						}
					})
				.ForEachElement(_renderTargetBitmap.Render);

			bitmap = new byte[stride * height];
			_formatConvertedBitmap.CopyPixels(bitmap, stride, 0);

			//selection = bitmap;
			//return;


			//var writeableBitmap = new WriteableBitmap(width, height, 96, 96, DrawingParameter.TargetPixelFormat, DrawingParameter.TargetBitmapPalette);
			//writeableBitmap.Lock();
			//var rect = new Int32Rect(0, 0, width, height);
			//writeableBitmap.WritePixels(rect, bitmap, stride, 0);
			//writeableBitmap.AddDirtyRect(rect);
			//writeableBitmap.Unlock();
			// set the image to the original
			
			_selectionRtb.Render(_blurImage);
			selection = new byte[stride * height];
			_selectionFcb.CopyPixels(selection, stride, 0);

			// ABSOLUT notwendig, da es ansonsten MemoryLeaks gibt
			// siehe hier: http://stackoverflow.com/questions/192329/simple-wpf-sample-causes-uncontrolled-memory-growth
			_blurImage.UpdateLayout();

			//var png = new PngBitmapEncoder();
			//png.Frames.Add(BitmapFrame.Create(imageRtb));
			//using (var fs = new FileStream("c:\\temp\\test6.png", FileMode.Create))
			//    png.Save(fs);
		}
	}
}