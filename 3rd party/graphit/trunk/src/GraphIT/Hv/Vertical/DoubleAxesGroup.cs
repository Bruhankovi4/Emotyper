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
using System.Windows;

namespace TechNewLogic.GraphIT.Hv.Vertical
{
	/// <summary>
	/// Represents a group of <see cref="DoubleAxis"/> elements and provides unified access via a <see cref="ProxyAxis"/>.
	/// </summary>
	public sealed class DoubleAxesGroup
	{
		internal DoubleAxesGroup(
			ProxyDoubleAxis proxyAxis,
			Func<DoubleScale> createDoubleScale)
		{
			_createDoubleScale = createDoubleScale;
			IsRelative = proxyAxis.IsRelative;
			ProxyAxis = proxyAxis;
		}

		private readonly Func<DoubleScale> _createDoubleScale;

		internal void Initialize()
		{
			ProxyAxis.BoundsChanged += (s, e) => ProxyAxis_BoundsChanged();
			VisualScale = _createDoubleScale();
		}

		private bool _handleEvents = true;

		/// <summary>
		/// Gets a value that indicates whether the <see cref="DoubleAxesGroup"/> is in relative or absolute grouping mode.
		/// </summary>
		/// <seealso cref="GroupingResult"/>
		public bool IsRelative { get; private set; }

		private readonly List<DoubleAxis> _doubleAxes = new List<DoubleAxis>();
		/// <summary>
		/// Gets an enumeration of all <see cref="DoubleAxis"/> elements that are associated with this <see cref="DoubleAxesGroup"/>.
		/// </summary>
		public IEnumerable<DoubleAxis> DoubleAxes { get { return _doubleAxes.ToArray(); } }

		internal event Action AxesChanged;
		private void OnAxesChanged()
		{
			if (AxesChanged != null)
				AxesChanged();
		}

		/// <summary>
		/// Gets a proxy for all <see cref="DoubleAxis"/> elements that are associated with this <see cref="DoubleAxesGroup"/>.
		/// </summary>
		/// <remarks>
		/// The <see cref="ProxyAxis"/> can be used to apply state changes (like zoom, changes of bounds, etc.). 
		/// These changes are applied to all elements in the <see cref="DoubleAxes"/> enumeration.
		/// </remarks>
		public DoubleAxisBase ProxyAxis { get; private set; }

		///<summary>
		/// Gets or sets the UI Visibility of this <see cref="DoubleAxesGroup"/>.
		///</summary>
		public bool IsVisible
		{
			get { return VisualScale.Visibility == Visibility.Visible; }
			set { VisualScale.Visibility = value ? Visibility.Visible : Visibility.Collapsed; }
		}

		/// <summary>
		/// Gets or sets whether the axes of this group should be moved when the canvas is moved by the user or a section zoom is made or the canvas is centered.
		/// </summary>
		public bool IgnoreCanvasMovementOrZoom { get; set; }

		internal DoubleScale VisualScale { get; private set; }

		internal void AddAxis(DoubleAxis axis)
		{
			if (ProxyAxis.GroupingMatches(axis, AxisMatchingMode.UomOnly).NotAllowed && !IsRelative)
				throw new Exception("The scales cannot be grouped because they don't match.");

			SyncScale(axis);
			_doubleAxes.Add(axis);
			axis.BoundsChanged += (s, e) => axis_BoundsChanged(axis);
			OnAxesChanged();
		}

		internal void RemoveAxis(DoubleAxis axis)
		{
			if (!_doubleAxes.Contains(axis))
				throw new Exception("Axes group does not contain axis.");

			_doubleAxes.Remove(axis);
			// TODO: Event abhängen (siehe unten)
			//axis.BoundsChanged -= axis_BoundsChanged;
			OnAxesChanged();
		}

		internal string GetFormattedLabelValue(double value)
		{
			return _doubleAxes.Count > 0 
				? _doubleAxes[0].Curve.GetFormattedValue(value)
				: string.Empty;
		}

		void ProxyAxis_BoundsChanged()
		{
			if (!_handleEvents)
				return;

			_handleEvents = false;
			foreach (var it in _doubleAxes)
				SyncScale(it);
			_handleEvents = true;
		}

		void axis_BoundsChanged(DoubleAxisBase axis)
		{
			// HACK: Das hier kann wegfallen, wenn das Event abgehängt ist
			if (!_doubleAxes.OfType<DoubleAxisBase>().Contains(axis))
				return;

			if (!_handleEvents)
				return;
			if (IsRelative)
				ScaleRelative(axis, ProxyAxis);
			else
				ProxyAxis.SetBounds(axis.ActualLowerBound, axis.ActualUpperBound);
		}

		private void SyncScale(DoubleAxisBase destination)
		{
			if (IsRelative)
				ScaleRelative(ProxyAxis, destination);
			else
				destination.SetBounds(ProxyAxis.ActualLowerBound, ProxyAxis.ActualUpperBound);
		}

		private static void ScaleRelative(DoubleAxisBase source, DoubleAxisBase destination)
		{
			var normFactor = source.UpperBound - source.LowerBound;
			var ls0 = source.LowerBound / normFactor;
			var us0 = source.UpperBound / normFactor;
			var ls1 = source.ActualLowerBound / normFactor;
			var us1 = source.ActualUpperBound / normFactor;

			// Das hier sind jetzt relative Differenzen, da wir oben normiert haben
			var dls = ls1 - ls0; // Bedeutet: "x % von der Länge des Ursprungs-Vektors
			var dus = us1 - us0;

			// Destination:
			var destinationLength = destination.UpperBound - destination.LowerBound;
			var newLower = destination.LowerBound + dls * destinationLength;
			var newUpper = destination.UpperBound + dus * destinationLength;
			destination.SetBounds(newLower, newUpper);
		}
	}
}
