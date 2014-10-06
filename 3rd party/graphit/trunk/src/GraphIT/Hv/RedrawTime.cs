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

namespace TechNewLogic.GraphIT.Hv
{
	/// <summary>
	/// Provides a time interval which is used to delay the redraw of a curve.
	/// </summary>
	/// <remarks>
	/// The <see cref="RedrawTime"/> is used by the <see cref="TimeDoubleCurve"/>. 
	/// When the curve needs to be redrawn (e.g. due to panning, zooming, changes in axis bounds, etc.),
	/// the redraw is not carried out immediately. Instead, the <see cref="RedrawTime"/> is used to delay
	/// the redraw process.
	/// By using the <see cref="RedrawTime"/>, it is possible to control the overall performance.
	/// </remarks>
	public sealed class RedrawTime
	{
		#region static constants

		static RedrawTime()
		{
            Ms100 = new RedrawTime(TimeSpan.FromMilliseconds(100));
			Ms500 = new RedrawTime(TimeSpan.FromMilliseconds(500));
			S1 = new RedrawTime(TimeSpan.FromSeconds(1));
			S2 = new RedrawTime(TimeSpan.FromSeconds(2));
			S3 = new RedrawTime(TimeSpan.FromSeconds(3));
			S4 = new RedrawTime(TimeSpan.FromSeconds(4));
			S5 = new RedrawTime(TimeSpan.FromSeconds(5));
			S6 = new RedrawTime(TimeSpan.FromSeconds(6));
			S7 = new RedrawTime(TimeSpan.FromSeconds(7));
			S8 = new RedrawTime(TimeSpan.FromSeconds(8));
			S9 = new RedrawTime(TimeSpan.FromSeconds(9));
			S10 = new RedrawTime(TimeSpan.FromSeconds(10));
		}

		/// <summary>
		/// Defines a redraw time of 500ms.
		/// </summary>
		public static RedrawTime Ms500 { get; private set; }
        public static RedrawTime Ms100 { get; private set; }
		/// <summary>
		/// Defines a redraw time of 1s.
		/// </summary>
		public static RedrawTime S1 { get; private set; }

		/// <summary>
		/// Defines a redraw time of 2s.
		/// </summary>
		public static RedrawTime S2 { get; private set; }

		/// <summary>
		/// Defines a redraw time of 3s.
		/// </summary>
		public static RedrawTime S3 { get; private set; }

		/// <summary>
		/// Defines a redraw time of 4s.
		/// </summary>
		public static RedrawTime S4 { get; private set; }

		/// <summary>
		/// Defines a redraw time of 5s.
		/// </summary>
		public static RedrawTime S5 { get; private set; }

		/// <summary>
		/// Defines a redraw time of 6s.
		/// </summary>
		public static RedrawTime S6 { get; private set; }

		/// <summary>
		/// Defines a redraw time of 7s.
		/// </summary>
		public static RedrawTime S7 { get; private set; }

		/// <summary>
		/// Defines a redraw time of 8s.
		/// </summary>
		public static RedrawTime S8 { get; private set; }

		/// <summary>
		/// Defines a redraw time of 9s.
		/// </summary>
		public static RedrawTime S9 { get; private set; }

		/// <summary>
		/// Defines a redraw time of 10s.
		/// </summary>
		public static RedrawTime S10 { get; private set; }

		#endregion

		private RedrawTime(TimeSpan timeSpan)
		{
			TimeSpan = timeSpan;
		}

		internal TimeSpan TimeSpan { get; private set; }
	}
}