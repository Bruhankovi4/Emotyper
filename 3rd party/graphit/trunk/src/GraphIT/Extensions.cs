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
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Xps;
using System.Windows.Xps.Packaging;
using TechNewLogic.GraphIT;
using TechNewLogic.GraphIT.Hv;
using TechNewLogic.GraphIT.MultiLanguage;
using TechNewLogic.GraphIT.Printing;

namespace System
{
	static class Extensions
	{
		public static string ToString(this DateTime me, TimeSpan range)
		{
			me = me.ToLocalTime();
			if (range.TotalSeconds <= 10)
				return me.ToString("ss.ffff");
			if (range.TotalHours <= 10)
				return me.ToString("HH:mm:ss");
			if (range.TotalDays <= 150)
				return me.ToString("dd.MM, HH:mm");
			if (range == TimeSpan.MaxValue)
				return me.ToString();
			return me.ToString("dd. MMM yyyy");
		}

		public static bool IsValidDateTime(this long ticks)
		{
			return ticks > DateTime.MinValue.Ticks && ticks < DateTime.MaxValue.Ticks;
		}

		//public static string GetFormattedValue(this double me, FormatDefinitions formatDefinition)
		//{
		//    var val = (double)me;
		//    if (double.IsNaN(val))
		//        // OPT: Evtl. konfigurierbar machen
		//        return MlResources.NoData;
		//    switch (formatDefinition)
		//    {
		//        case FormatDefinitions.FloatingComma:
		//            {
		//                if (val > -0.1 && val < 0.1)
		//                    return val.ToString("0.###");
		//                if (val > -10 && val < 10)
		//                    return val.ToString("0.##");
		//                if (val > -100 && val < 100)
		//                    return val.ToString("0.#");
		//                return val.ToString("0");
		//            }
		//        case FormatDefinitions.Fix1BehindComma:
		//            return val.ToString("0.#");
		//        case FormatDefinitions.Fix2BehindComma:
		//            return val.ToString("0.##");
		//        case FormatDefinitions.Fix3BehindComma:
		//            return val.ToString("0.###");
		//        default:
		//            return val.ToString("0.#");
		//    }
		//}

		public static string GetFormattedValue(this TimeSpan me)
		{
			// Geht nicht mehr wegen Abwärtskompatibilität zu .Net 3.5
			//if (me.TotalMilliseconds < 1000)
			//    return me.ToString("fff") + "ms";
			//if (me.TotalSeconds < 10)
			//    return me.ToString("s\\.ff") + "s";
			//if (me.TotalSeconds < 60)
			//    return me.ToString("ss\\.f") + "s";
			//if (me.TotalMinutes < 10)
			//    return me.ToString("m\\.ss") + "min";
			//if (me.TotalMinutes < 60)
			//    return me.ToString("mm\\.s") + "min";
			//if (me.TotalHours < 10)
			//    return me.ToString("h\\.mm") + "h";
			//if (me.TotalHours < 24)
			//    return me.ToString("hh\\.m") + "h";
			//if (me.TotalDays < 10)
			//    return me.ToString("d\\.hh") + "d";
			//if (me.TotalDays < 100)
			//    return me.ToString("dd\\.h") + "d";
			//if (me.TotalDays < 1000)
			//    return me.ToString("ddd") + "d";
			//if (me.TotalDays < 10000)
			//    return me.ToString("dddd") + "d";
			//if (me.TotalDays < 100000)
			//    return me.ToString("ddddd") + "d";
			//if (me.TotalDays < 1000000)
			//    return me.ToString("dddddd") + "d";			
			if (me.TotalSeconds < 60)
				return me.Seconds + "." + me.Milliseconds + "s";
			if (me.TotalMinutes < 60)
				return me.Minutes + "min " + me.Seconds + "s";
			if (me.TotalHours < 24)
				return me.Hours + "h " + me.Minutes + "min";
			if (me.TotalDays < 10)
				return me.Days + "d " + me.Hours + "h";
			return me.TotalDays + "d";
		}

		public static T[] Copy<T>(this T[] source)
		{
			var copy = new T[source.Length];
			Array.Copy(source, copy, source.Length);
			return copy;
		}

		public static TimeSpan Abs(this TimeSpan me)
		{
			return me.Ticks < 0 ? me.Negate() : me;
		}
	}
}

namespace System.Collections.Generic
{
	static class Extensions
	{
		//public static int IndexOf<T>(this IEnumerable<T> me, T element)
		//    where T : class
		//{
		//    return me.Contains(element)
		//        ? me.TakeWhile(it => it != element).Count()
		//        : -1;
		//}

		public static IEnumerable<T> ForEachElement<T>(this IEnumerable<T> me, Action<T> action)
		{
			foreach (var it in me)
				action(it);
			return me;
		}

		public static T MinOrFallback<T>(this IEnumerable<T> me, T fallbackValue)
		{
			return me.Any() ? me.Min() : fallbackValue;
		}

		public static T MaxOrFallback<T>(this IEnumerable<T> me, T fallbackValue)
		{
			return me.Any() ? me.Max() : fallbackValue;
		}

		public static double AverageOrFallback(this IEnumerable<double> me, double fallbackValue)
		{
			return me.Any() ? me.Average() : fallbackValue;
		}

		//public static double SumOrFallback(this IEnumerable<double> me, double fallbackValue)
		//{
		//    return me.Any() ? me.Sum() : fallbackValue;
		//}
	}
}

namespace System.Windows
{
	static class Extensions
	{
		public static bool IsOrHasParentOfType(this object visualHit, Type type)
		{
			if (!(visualHit is DependencyObject))
				return false;
			if (type.IsAssignableFrom(visualHit.GetType()))
				return true;

			var parent = VisualTreeHelper.GetParent((DependencyObject)visualHit);
			return parent != null && IsOrHasParentOfType(parent, type);
		}

		public static DependencyObject FindVisualChildByName(this DependencyObject me, string name)
		{
			if (me.GetValue(FrameworkElement.NameProperty) as string == name)
				return me;

			var children = LogicalTreeHelper.GetChildren(me).OfType<FrameworkElement>();
			foreach (var it in children)
			{
				var res = it.FindVisualChildByName(name);
				if (res != null)
					return res;
			}
			return null;
			//for (var i = 0; i < VisualTreeHelper.GetChildrenCount(me); i++)
			//{
			//    var child = VisualTreeHelper.GetChild(me, i);
			//    var controlName = child.GetValue(FrameworkElement.NameProperty) as string;
			//    if (controlName == name)
			//        return child as T;

			//    var result = FindVisualChildByName<T>(child, name);
			//    if (result != null)
			//        return result;
			//}
			//return null;
		}

		public static ImageSource CaptureImage(this FrameworkElement me)
		{
			var width = me.ActualWidth > 0 ? (int)me.ActualWidth : 1;
			var height = me.ActualHeight > 0 ? (int)me.ActualHeight : 1;
			var bmp = new RenderTargetBitmap(
				width, height,
				96, 96,
				PixelFormats.Pbgra32);
			bmp.Render(me);
			return bmp;
		}

		public static Size GetOuterSize(this FrameworkElement me, Size innerSize)
		{
			return new Size(
				innerSize.Width + me.Margin.Left + me.Margin.Right,
				innerSize.Height + me.Margin.Top + me.Margin.Bottom);
		}
	}
}

namespace System.Windows.Controls.Primitives
{
	static class Extensions
	{
		public static void UpdatePosition(this Popup me)
		{
			// HACK
			var offset = me.HorizontalOffset;
			me.HorizontalOffset = offset + 1;
			me.HorizontalOffset = offset;
		}
	}
}

namespace System.Windows.Media
{
	static class Extensions
	{
		public static FixedDocumentSequence ToXps(this Visual me)
		{
			var memoryStream = new MemoryStream();
			var package = Package.Open(memoryStream, FileMode.Create, FileAccess.ReadWrite);
			var uri = new Uri(string.Format("pack://{0}.xps", Guid.NewGuid()));
			PackageStore.AddPackage(uri, package);
			using (var xpsDocument = new XpsDocument(package, CompressionOption.NotCompressed, uri.AbsoluteUri))
			{
				var xpsdw = XpsDocument.CreateXpsDocumentWriter(xpsDocument);
				var vToXpsD = (VisualsToXpsDocument)xpsdw.CreateVisualsCollator();
				vToXpsD.Write(me);
				vToXpsD.EndBatchWrite();
				return xpsDocument.GetFixedDocumentSequence();

				// IMP: Muss vom Benutzer der Komponente gemacht werden
				// NICHT RemovePackage machen, da ansonsten das PrintPreview nicht mehr geht.
				//PackageStore.RemovePackage(uri);
			}
		}
	}
}

namespace System.Windows.Threading
{
	static class Extensions
	{
		public static void InvokeAndThrow(this Dispatcher me, Action del)
		{
			Exception ex = null;
			me.Invoke(() =>
				{
					try { del(); }
					catch (Exception e) { ex = e; }
				});
			if (ex != null)
				throw ex;
		}
	}
}

namespace System.Windows.Media.Imaging
{
	static class Extensions
	{
		public static int[] ToInt(this WriteableBitmap me)
		{
			var newBuffer = new int[me.PixelWidth * me.PixelHeight];
			me.CopyPixels(newBuffer, me.BackBufferStride, 0);
			return newBuffer;
		}

		public static byte[] ToByte(this WriteableBitmap me)
		{
			var stride = (me.PixelWidth * me.Format.BitsPerPixel + 7) / 8;
			var newBuffer = new byte[me.PixelHeight * stride];
			me.CopyPixels(newBuffer, stride, 0);
			return newBuffer;
		}
	}
}

namespace TechNewLogic.GraphIT
{
	static class Extensions
	{
		public static IEnumerable<T> PrepareAppend<T>(this EntriesCollection<T> me, IEnumerable<T> newEntries, int maxNumOfEntries)
		{
			var diff = (me.Count + newEntries.Count()) - maxNumOfEntries;
			if (diff < 0)
				return newEntries;

			var entriesToRemove = me.Take(diff);
			me.Remove(entriesToRemove);

			diff = (me.Count + newEntries.Count()) - maxNumOfEntries;
			return diff < 0
				? newEntries
				: newEntries.Skip(diff).ToArray();
		}

		public static IEnumerable<T> PreparePrepend<T>(this EntriesCollection<T> me, IEnumerable<T> newEntries, int maxNumOfEntries)
		{
			var diff = (me.Count + newEntries.Count()) - maxNumOfEntries;
			if (diff < 0)
				return newEntries;

			var entriesToRemove = me.Reverse().Take(diff).Reverse();
			me.Remove(entriesToRemove);

			diff = (me.Count + newEntries.Count()) - maxNumOfEntries;
			return diff < 0
				? newEntries
				: newEntries.Reverse().Skip(diff).Reverse().ToArray();
		}
	}
}