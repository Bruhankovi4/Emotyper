using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Imaging;
using TechNewLogic.GraphIT.Helper;

namespace TechNewLogic.GraphIT.Drawing
{
	class NativeDrawer : IDrawer
	{
		public HitTester DrawLine(
			double thickness, 
			IEnumerable<IList<Point>> points, 
			int width, 
			int height, 
			out byte[] bitmap, 
			out byte[] selection)
		{
			var stride = MathHelper.GetStride(width, DrawingParameter.TargetPixelFormat.BitsPerPixel);
			bitmap = new byte[stride * height];
			selection = bitmap;

			foreach (var segmentPoints in points)
			{
				var count = segmentPoints.Count;
				var xs = new int[count];
				var ys = new int[count];

				for (var i = 0; i < count; i++)
				{
					var point = segmentPoints[i];
					xs[i] = (int)point.X;
					ys[i] = (int)point.Y;
				}

				DrawLine(count, 0, 0, xs, ys, width, height, stride, bitmap);
				break;
			}

			return new HitTester();
		}

		public static void DrawLine(int numberOfPoints, int offsetX, int offsetY, int[] xs, int[] ys, int width, int height, int stride, byte[] pixels)
		{
			DrawLinesBresenham(numberOfPoints, offsetX, offsetY, xs, ys, width, height, stride, pixels);
		}

		private static void DrawLinesBresenham(int numberOfPoints, int offsetX, int offsetY, int[] xs, int[] ys, int width, int height, int stride, byte[] pixels)
		{
			for (var elementIndex = 0; elementIndex < numberOfPoints - 1; elementIndex++)
			{
				var x1 = xs[elementIndex] + offsetX;
				var y1 = ys[elementIndex] + offsetY;
				var x2 = xs[elementIndex + 1] + offsetX;
				var y2 = ys[elementIndex + 1] + offsetY;

				// Distance start and end point
				var dx = x2 - x1;
				var dy = y2 - y1;

				// Determine sign for direction x
				var incx = 0;
				if (dx < 0)
				{
					dx = -dx;
					incx = -1;
				}
				else if (dx > 0)
					incx = 1;

				// Determine sign for direction y
				var incy = 0;
				if (dy < 0)
				{
					dy = -dy;
					incy = -1;
				}
				else if (dy > 0)
					incy = 1;

				// Which gradient is larger
				int pdx, pdy, odx, ody, es, el;
				if (dx > dy)
				{
					pdx = incx;
					pdy = 0;
					odx = incx;
					ody = incy;
					es = dy;
					el = dx;
				}
				else
				{
					pdx = 0;
					pdy = incy;
					odx = incx;
					ody = incy;
					es = dx;
					el = dy;
				}

				// Init start
				var x = x1;
				var y = y1;
				var error = el >> 1;
				if (y < height && y >= 0 && x < width && x >= 0)
				{
					var index = y * stride + x;
					pixels[index] = 1;
				}

				// Walk the line!
				for (var i = 0; i < el; i++)
				{
					// Update error term
					error -= es;

					// Decide which coord to use
					if (error < 0)
					{
						error += el;
						x += odx;
						y += ody;
					}
					else
					{
						x += pdx;
						y += pdy;
					}

					// Set pixel
					if (y < height && y >= 0 && x < width && x >= 0)
					{
						var index = y * stride + x;
						pixels[index] = 1;
					}
				}
			}
		}
	}
}