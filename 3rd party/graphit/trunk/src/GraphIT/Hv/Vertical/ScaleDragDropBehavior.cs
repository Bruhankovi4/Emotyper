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
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using TechNewLogic.GraphIT.MultiLanguage;

namespace TechNewLogic.GraphIT.Hv.Vertical
{
	class ScaleDragDropBehavior
	{
		public ScaleDragDropBehavior(
			DoubleScale doubleScale,
			IDragDropManager dragDropManager,
			IScaleGroupManager scaleGroupManager,
			IDoubleScalePool doubleScalePool,
			Func<DragScaleAdorner> getDragScaleAdorner)
		{
			_doubleScale = doubleScale;
			_dragDropManager = dragDropManager;
			_scaleGroupManager = scaleGroupManager;
			_doubleScalePool = doubleScalePool;
			_getDragScaleAdorner = getDragScaleAdorner;
			_dragDropManager.Dragging += StartDrag;
			_dragDropManager.Releasing += EndDrag;

			_doubleScale.DragScaleSurface.PreviewMouseLeftButtonDown += (s1, e1) => IsCaptured = true;
			_doubleScale.DragScaleSurface.PreviewMouseLeftButtonUp += (s1, e1) => IsCaptured = false;
			_doubleScale.DragScaleSurface.PreviewMouseMove += MouseMove;
		}

		private readonly DoubleScale _doubleScale;
		private readonly IDragDropManager _dragDropManager;
		private readonly IScaleGroupManager _scaleGroupManager;
		private readonly IDoubleScalePool _doubleScalePool;

		private readonly Func<DragScaleAdorner> _getDragScaleAdorner;
		private DragScaleAdorner _dragScaleAdorner;

		private Point _lastMousePosition;

		private bool _isCaptured;
		private bool IsCaptured
		{
			get { return _isCaptured; }
			set
			{
				if (value)
					OnCaptured();
				else
					OnDecaptured();

				_isCaptured = value;
			}
		}

		private void OnCaptured()
		{
			_lastMousePosition = Mouse.GetPosition(_doubleScale.DragScaleSurface);
			Mouse.Capture(_doubleScale.DragScaleSurface);

			_doubleScale.MainElement.Opacity = 0.2;
			_dragScaleAdorner = _getDragScaleAdorner();
			var relativeMousePos = Mouse.GetPosition(_doubleScale);

			_dragScaleAdorner.Position = new Point(
				relativeMousePos.X + 10,
				relativeMousePos.Y + 10);

			AdornerLayer.GetAdornerLayer(_doubleScale).Add(_dragScaleAdorner);
			_dragDropManager.Drag(_doubleScale);
		}

		private void OnDecaptured()
		{
			if (!IsCaptured)
				return;
			Mouse.Capture(null);

			_doubleScale.MainElement.Opacity = 1;

			// Remove from adorner layer
			var adornerLayer = AdornerLayer.GetAdornerLayer(_doubleScale);
			if (adornerLayer == null)
				return;
			var adorners = adornerLayer.GetAdorners(_doubleScale);
			if (adorners == null)
				return;
			var dragScaleAdorner = adorners.OfType<DragScaleAdorner>().FirstOrDefault();
			if (dragScaleAdorner == null)
				return;
			adornerLayer.Remove(dragScaleAdorner);

			_dragDropManager.ReleaseDrag();
		}

		private void TryGroupAxes()
		{
			var targetScale = _doubleScalePool.Scales.FirstOrDefault(it => it.DropAbility != GroupingResult.None);
			if (targetScale != null)
				_scaleGroupManager.GroupScales(_dragDropManager.DraggedScale, targetScale);
		}

		private void MouseMove(object sender, MouseEventArgs e)
		{
			if (!IsCaptured)
				return;

			var currentMousePosition = e.GetPosition(_doubleScale.DragScaleSurface);
			var delta = new Point(
				_lastMousePosition.X - currentMousePosition.X,
				_lastMousePosition.Y - currentMousePosition.Y);

			_dragScaleAdorner.Position = new Point(
				_dragScaleAdorner.Position.X - delta.X,
				_dragScaleAdorner.Position.Y - delta.Y);

			var dropAbility = _doubleScalePool.Scales
				.Select(it => it.DropAbility)
				.FirstOrDefault(it => it != GroupingResult.None);
			if (dropAbility == GroupingResult.None)
				_dragDropManager.UserMessage = string.Empty;
			else if (dropAbility == GroupingResult.Absolute)
				_dragDropManager.UserMessage = MlResources.GroupAbsolute;
			else
				_dragDropManager.UserMessage = MlResources.GroupRelative;

			_lastMousePosition = currentMousePosition;
		}

		private void StartDrag()
		{
			_doubleScale.DropTarget.Visibility = Visibility.Visible;
		}

		private void EndDrag()
		{
			_doubleScale.DropTarget.Visibility = Visibility.Collapsed;
			TryGroupAxes();
		}
	}
}
