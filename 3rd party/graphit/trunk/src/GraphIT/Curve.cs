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
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

using TechNewLogic.GraphIT.Helper;

namespace TechNewLogic.GraphIT
{
	/// <summary>
	/// Provides base functionalities for drawing on the drawing area.
	/// </summary>
	public abstract class Curve : NotifyPropertyChanged
	{
		internal Curve(
			IRedrawRequest redrawRequest,
			ICurveContextMenuSurface curveContextMenuSurface,
			ICurvePool curvePool,
			ContextMenuControl contextMenuControl)
		{
			_curveContextMenuSurface = curveContextMenuSurface;
			_curvePool = curvePool;
			RedrawRequest = redrawRequest;

			Description = new CurveDescription();
			Description.PropertyChanged += (s, e) => OnPropertyChanged("Description." + e.PropertyName);

			_contextMenuControl = contextMenuControl;
			_curveContextMenuSurface.Register(_contextMenuControl);

			NeedsRedraw = true;
		}

		private readonly ICurveContextMenuSurface _curveContextMenuSurface;
		private readonly ICurvePool _curvePool;

		/// <summary>
		/// Gets an instance of <see cref="IRedrawRequest"/> that can be used to trigger a redraw in the drawing subsystem.
		/// </summary>
		protected IRedrawRequest RedrawRequest { get; private set; }

		/// <summary>
		/// Gets the <see cref="CurveDescription"/> that is associated with this <see cref="Curve"/>.
		/// </summary>
		public CurveDescription Description { get; private set; }

		private readonly ContextMenuControl _contextMenuControl;
		/// <summary>
		/// Gets the <see cref="IContextMenuRegistrar"/> implementation which is associated with this <see cref="Curve"/>.
		/// </summary>
		public IContextMenuRegistrar ContextMenuRegistrar { get { return _contextMenuControl; } }

		/// <summary>
		/// Gets or sets the visibility of this <see cref="Curve"/> on the drawing area.
		/// </summary>
		public abstract bool IsVisible { get; set; }

		/// <summary>
		/// Gets or sets a value that indicates whether the <see cref="Curve"/> needs a complete redraw.
		/// </summary>
		public bool NeedsRedraw { get; internal set; }

		/// <summary>
		/// Gets a dictionary in which any kind of metadata can be stored.
		/// </summary>
		public IDictionary<string, object> Metadata { get; private set; }

		#region Selection

		/// <summary>
		/// Occurs when the selection of the <see cref="Curve"/> has changed.
		/// </summary>
		public event EventHandler IsSelectedChanged;
		private void OnIsSelectedChanged()
		{
			if (IsSelectedChanged != null)
				IsSelectedChanged(this, new EventArgs());
		}

		private bool _isSelectionPinned;
		private bool _isSelected;
		/// <summary>
		/// Gets or sets a value that indicates whether the <see cref="Curve"/> is selected or not.
		/// </summary>
		public bool IsSelected
		{
			get { return _isSelected; }
			private set
			{
				if (_isSelected == value)
					return;
				_isSelected = value;
				OnPropertyChanged("IsSelected");
				OnIsSelectedChanged();
			}
		}

		internal void Select(bool pin)
		{
			// Eine andere Kurve ist selektiert: Diese Selektion aufheben
			if (_curvePool.SelectedCurve != null && _curvePool.SelectedCurve != this)
				_curvePool.SelectedCurve.IsSelected = false;

			// Wenn eine andere Kurve gepinnt ist und diese Kurve gepinnt werden soll, darf die andere Kurve nicht mehr gepinnt sein.
			var otherPinnedCurve = _curvePool.Curves.FirstOrDefault(it => it._isSelectionPinned && it != this);
			if (otherPinnedCurve != null && pin)
				otherPinnedCurve._isSelectionPinned = false;

			if (!_isSelectionPinned)
				_isSelectionPinned = pin;
			IsSelected = true;
		}

		internal void Unselect()
		{
			if (_isSelectionPinned)
				return;

			_isSelectionPinned = false;
			IsSelected = false;

			// war eine andere Kurve gepinnt, dann muss diese wieder selektiert werden
			var otherPinnedCurve = _curvePool.Curves.FirstOrDefault(it => it._isSelectionPinned && it != this);
			if (otherPinnedCurve != null)
				otherPinnedCurve.IsSelected = true;
		}

		private void Unpin()
		{
			_isSelectionPinned = false;
		}

		internal void TogglePin()
		{
			if (!_isSelectionPinned)
				Select(true);
			else
				Unpin();
		}

		#endregion

		#region Mouse Events

		private bool _mouseMovedSinceEnter;

		//public event MouseEventHandler MouseMove;
		internal void OnMouseMove(MouseEventArgs e)
		{
			_mouseMovedSinceEnter = true;
			//if (MouseMove != null)
			//    MouseMove(this, e);
		}

		//public event MouseEventHandler MouseEnter;
		internal void OnMouseEnter(MouseEventArgs e)
		{
			Select(false);
			//if (MouseEnter != null)
			//    MouseEnter(this, e);
		}

		//public event MouseEventHandler MouseLeave;
		internal void OnMouseLeave(MouseEventArgs e)
		{
			_mouseMovedSinceEnter = false;
			Unselect();
			//if (MouseLeave != null)
			//    MouseLeave(this, e);
		}

		//public event MouseEventHandler MouseLeftButtonDown;
		internal void OnMouseLeftButtonDown(MouseEventArgs e)
		{
			_mouseMovedSinceEnter = false;
			//if (MouseLeftButtonDown != null)
			//    MouseLeftButtonDown(this, e);
		}

		//public event MouseEventHandler MouseLeftButtonUp;
		internal void OnMouseLeftButtonUp(MouseEventArgs e)
		{
			if (!_mouseMovedSinceEnter)
				TogglePin();
			//if (MouseLeftButtonUp != null)
			//    MouseLeftButtonUp(this, e);
		}

		//public event MouseEventHandler MouseRightButtonDown;
		internal void OnMouseRightButtonDown(MouseEventArgs e)
		{
			//if (MouseRightButtonDown != null)
			//    MouseRightButtonDown(this, e);
			if (e.Handled)
				return;
			_contextMenuControl.Show();
			e.Handled = true;
		}

		//public event MouseEventHandler MouseRightButtonUp;
		internal void OnMouseRightButtonUp(MouseEventArgs e)
		{
			//if (MouseRightButtonUp != null)
			//    MouseRightButtonUp(this, e);
		}

		#endregion

		internal Rect GetRenderOffset() { return GetRenderOffsetOverride(); }
		/// <summary>
		/// Gets a value that is applied as a transformation by the rendering pipeline to the bitmap of this <see cref="Curve"/>.
		/// </summary>
		/// <returns>The rect that will be used as a 2D transformation.</returns>
		protected abstract Rect GetRenderOffsetOverride();

		internal Color GetStroke() { return GetStrokeOverride(); }
		/// <summary>
		/// Gets a value that is applied as coloring by the rendering pipeline to the bitmap of this <see cref="Curve"/>.
		/// </summary>
		/// <returns>The stroke color fot the <see cref="Curve"/>.</returns>
		protected abstract Color GetStrokeOverride();

		internal bool HitTest(int x, int y) { return HitTestOverride(x, y); }
		/// <summary>
		/// Gets a value that indicates whether the <see cref="Curve"/> is hit by the given <paramref name="x"/> and <paramref name="y"/> coordinates.
		/// </summary>
		/// <remarks>
		/// The values from the <see cref="GetRenderOffsetOverride"/> method are already applied to the
		/// given <paramref name="x"/> and <paramref name="y"/> coordinates.
		/// </remarks>
		/// <param name="x">The horizontal screen coordinate.</param>
		/// <param name="y">The vertical screen coordinate.</param>
		/// <returns>The value that indicates whether the <see cref="Curve"/> was hit.</returns>
		protected abstract bool HitTestOverride(int x, int y);

		internal void Draw(int width, int height, out byte[] bitmap, out byte[] selection)
		{
			DrawOverride(width, height, out bitmap, out selection);
		}
		/// <summary>
		/// Draws the curve and it's selection.
		/// </summary>
		/// <remarks>
		/// Bitmap formats and drawing information can be queries using the <see cref="DrawingParameter"/> class.
		/// </remarks>
		/// <param name="width">The width of the resulting bitmaps.</param>
		/// <param name="height">The height of the resulting bitmaps.</param>
		/// <param name="bitmap">The byte array of the drawn curve.</param>
		/// <param name="selectionBitmap">The byte array of the drawn selection.</param>
		protected abstract void DrawOverride(int width, int height, out byte[] bitmap, out byte[] selectionBitmap);

		internal void Freeze() { FreezeOverride(); }
		/// <summary>
		/// Can be used to freeze certain objects's properties at the beginning of a drawing cycle.
		/// </summary>
		/// <remarks>
		/// The <see cref="FreezeOverride"/> method is called before the <see cref="DrawOverride"/> method. 
		/// Use the <see cref="FreezeOverride"/> to save axes bounds, data series, etc for a drawing cycle.
		/// </remarks>
		/// <seealso cref="UnfreezeOverride"/>
		protected abstract void FreezeOverride();

		internal void Unfreeze() { UnfreezeOverride(); }
		/// <summary>
		/// Can be used to unfreeze certain objects's properties at the end of a drawing cycle.
		/// </summary>
		/// <remarks>
		/// The <see cref="FreezeOverride"/> method is called after the <see cref="DrawOverride"/> method. 
		/// Use the <see cref="UnfreezeOverride"/> to reset axes bounds or data series, update the render offset, etc.
		/// </remarks>
		/// <seealso cref="FreezeOverride"/>
		protected abstract void UnfreezeOverride();
	}
}