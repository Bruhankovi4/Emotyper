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
using System.Diagnostics;
using System.Windows;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;
using TechNewLogic.GraphIT.Helper;
using TechNewLogic.GraphIT.Hv.Horizontal;
using TechNewLogic.GraphIT.Hv.Vertical;

namespace TechNewLogic.GraphIT.Hv
{
	class PanZoomManager : IDisposable
	{
		public PanZoomManager(
			InputReference inputReference,
			TimeAxis timeAxis,
			IDoubleAxesGroupPool axesGroupPool,
			IDrawingAreaInfo drawingAreaInfo,
			IOnlineMode onlineMode,
			KeyboardHelper keyboardHelper)
		{
			_inputReference = inputReference;
			_timeAxis = timeAxis;
			_axesGroupPool = axesGroupPool;
			_drawingAreaInfo = drawingAreaInfo;
			_onlineMode = onlineMode;

			_moveHelper = new UiMoveHelper(_inputReference.InputElement);
			_moveHelper.Moved += moveHelper_Moved;

			_inputReference.InputElement.MouseWheel += InputElement_MouseWheel;
			
			_keyboardHelper = keyboardHelper;
			_keyboardHelper.HookKeyDown(KeyDown);
		}

		private readonly InputReference _inputReference;
		private readonly TimeAxis _timeAxis;
		private readonly IDoubleAxesGroupPool _axesGroupPool;
		private readonly IDrawingAreaInfo _drawingAreaInfo;
		private readonly IOnlineMode _onlineMode;
		private readonly UiMoveHelper _moveHelper;
		private readonly KeyboardHelper _keyboardHelper;

		#region Pan

		void moveHelper_Moved(MoveInfo moveInfo)
		{
			Pan(moveInfo.RelativeDiff);
		}

		private void KeyDown(KeyEventArgs e)
		{
			if (!_inputReference.InputElement.IsMouseOver)
				return;

			const double factor = 0.01;

			var key = e.Key;

			var relDiffX = 0d;
			if (key == Key.Right)
				relDiffX = -1;
			else if (key == Key.Left)
				relDiffX = 1;
			relDiffX = relDiffX * factor;

			var relDiffY = 0d;
			if (key == Key.Down)
				relDiffY = -1;
			else if (key == Key.Up)
				relDiffY = 1;
			relDiffY = relDiffY * factor;

			var moveInfo = new MoveInfo(new Point(relDiffX, relDiffY));
			Pan(moveInfo.RelativeDiff);
		}

		private void Pan(Point relativeDiff)
		{
			// Move the axes
			if (!_onlineMode.IsOnline)
				_timeAxis.MoveBoundsRelative(relativeDiff.X);
			GetRelevantAxesGroups().ForEachElement(it =>
				it.ProxyAxis.MoveBoundsRelative(relativeDiff.Y));
		}

		private IEnumerable<DoubleAxesGroup> GetRelevantAxesGroups()
		{
			return _axesGroupPool
				.AxesGroups
				.Where(it => !it.IgnoreCanvasMovementOrZoom);
		}

		#endregion

		#region Zoom

		/// <summary>
		/// Skalierungs-Fakrot für Zoom: [0...1]
		/// </summary>
		private const double IncreaseDecreaseFactor = 1.25;

		void InputElement_MouseWheel(object sender, MouseWheelEventArgs e)
		{
			var position = _drawingAreaInfo.MousePosition;
			var relativePosition = new Point(
				position.X / _drawingAreaInfo.ScaledDrawingWidth,
				position.Y / _drawingAreaInfo.ScaledDrawingHeight);

			if (e.Delta > 0)
				IncreaseZoom(relativePosition);
			else
				DecreaseZoom(relativePosition);
		}

		private void IncreaseZoom(Point relativePosition)
		{
			const double factor = 1 / IncreaseDecreaseFactor;
			_timeAxis.Zoom(
				factor,
				_onlineMode.IsOnline ? 1 : relativePosition.X);
			GetRelevantAxesGroups().ForEachElement(it =>
				it.ProxyAxis.Zoom(factor, -relativePosition.Y + 1));
		}

		private void DecreaseZoom(Point relativePosition)
		{
			_timeAxis.Zoom(
				IncreaseDecreaseFactor,
				_onlineMode.IsOnline ? 1 : relativePosition.X);
			GetRelevantAxesGroups().ForEachElement(it =>
				it.ProxyAxis.Zoom(IncreaseDecreaseFactor, -relativePosition.Y + 1));
		}

		#endregion

		#region Implementation of IDisposable

		public void Dispose()
		{
			_keyboardHelper.UnhookKeyDown(KeyDown);
		}

		#endregion
	}
}
