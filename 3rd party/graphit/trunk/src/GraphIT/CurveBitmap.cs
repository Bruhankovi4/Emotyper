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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TechNewLogic.GraphIT.Drawing;

namespace TechNewLogic.GraphIT
{
	enum ImageSelection
	{
		Primary,
		Secondary
	}

	class CurveBitmap
	{
		public CurveBitmap(Curve curve)
		{
			Curve = curve;
			CurrentImageSelection = ImageSelection.Primary;
			_nextImageSelection = ImageSelection.Secondary;
		}

		private int _width;
		private int _height;
		private Color _stroke;
		private byte[] _bitmap;
		private byte[] _selection;

		private ImageSelection _nextImageSelection;

		public Curve Curve { get; private set; }

		public ImageSelection CurrentImageSelection { get; private set; }

		private WriteableBitmap _frozenBitmap1;
		public ImageSource FrozenBitmap1 { get { return _frozenBitmap1; } }

		private WriteableBitmap _frozenBitmap2;
		public ImageSource FrozenBitmap2 { get { return _frozenBitmap2; } }

		private WriteableBitmap _frozenSelection1;
		public ImageSource FrozenSelection1 { get { return _frozenSelection1; } }

		private WriteableBitmap _frozenSelection2;
		public ImageSource FrozenSelection2 { get { return _frozenSelection2; } }

		public void BeginFreeze(int width, int height, Color stroke, byte[] bitmap, byte[] selection)
		{
			_stroke = stroke;
			_width = width;
			_height = height;
			_bitmap = bitmap;
			_selection = selection;
		}

		public void EndFreeze()
		{
			if (_nextImageSelection == ImageSelection.Primary)
			{
				_frozenBitmap1 = EndFreezeBitmap(_frozenBitmap1, _bitmap);
				_frozenSelection1 = EndFreezeBitmap(_frozenSelection1, _selection);
			}
			else
			{
				_frozenBitmap2 = EndFreezeBitmap(_frozenBitmap2, _bitmap);
				_frozenSelection2 = EndFreezeBitmap(_frozenSelection2, _selection);
			}
		}

		public void Flip()
		{
			CurrentImageSelection = _nextImageSelection;
			_nextImageSelection = _nextImageSelection == ImageSelection.Primary
				? ImageSelection.Secondary
				: ImageSelection.Primary;
		}

		private WriteableBitmap EndFreezeBitmap(WriteableBitmap bitmap, byte[] sourceBuffer)
		{
			if (bitmap == null 
				|| bitmap.PixelWidth != _width 
				|| bitmap.PixelHeight != _height 
				|| !bitmap.Palette.Colors.Contains(_stroke))
			{
				bitmap = new WriteableBitmap(
					_width, _height, 96, 96,
					DrawingParameter.TargetPixelFormat,
					SolidPalette.GetSolid(
						Colors.Transparent,
						_stroke,
						DrawingParameter.Bpp.Value));
			}
			try
			{
				bitmap.Lock();
				// IMP: Curve (nur eine) Invisible: ArgumentException (sourceBuffer == null)
				bitmap.WritePixels(new Int32Rect(0, 0, _width, _height), sourceBuffer, bitmap.BackBufferStride, 0);
				bitmap.AddDirtyRect(new Int32Rect(0, 0, _width, _height));
			}
			finally
			{
				bitmap.Unlock();
			}

			return bitmap;
		}
	}
}