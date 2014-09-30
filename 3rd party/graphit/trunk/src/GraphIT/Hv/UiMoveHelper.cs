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
using System.Windows.Input;

namespace TechNewLogic.GraphIT.Hv
{
	/// <summary>
	/// Helper-Klasse, welche sich um das Verschieben von FrameworkElementen mit der Maus kümmert.
	/// </summary>
	class UiMoveHelper
	{
		public UiMoveHelper(
			FrameworkElement visual,
			Cursor cursor = null,
			IOnlineMode onlineMode = null)
		{
			_visual = visual;
			_cursor = cursor;

			if (onlineMode != null)
			{
				_onlineMode = onlineMode;
				// TODO: abhängen
				_onlineMode.IsOnlineChanged += (s, e) => UpdateOnlineMode();
				UpdateOnlineMode();
			}
			else
				IsActive = true;
		}

		private readonly FrameworkElement _visual;
		private readonly Cursor _cursor;
		private readonly IOnlineMode _onlineMode;

		/// <summary>
		/// Relative Diff
		/// </summary>
		public event Action<MoveInfo> Moved;
		private void OnMoved(Point relativeDiff)
		{
			if (Moved != null)
				Moved(new MoveInfo(relativeDiff));
		}

		private Point _lastMousePosition;

		public event Action IsCapturedChanged;

		private bool _isCaptured;
		public bool IsCaptured
		{
			get { return _isCaptured; }
			private set
			{
				if (_isCaptured == value)
					return;

				Mouse.Capture(value ? _visual : null);
				_isCaptured = value;

				if (IsCapturedChanged != null)
					IsCapturedChanged();
			}
		}

		private bool IsActive
		{
			set
			{
				if (value)
				{
					_visual.PreviewMouseLeftButtonDown += MouseLeftButtonDown;
					_visual.PreviewMouseLeftButtonUp += MouseLeftButtonUp;
					_visual.PreviewMouseMove += MouseMove;
					if (_cursor != null)
						_visual.Cursor = _cursor;
				}
				else
				{
					_visual.PreviewMouseLeftButtonDown -= MouseLeftButtonDown;
					_visual.PreviewMouseLeftButtonUp -= MouseLeftButtonUp;
					_visual.PreviewMouseMove -= MouseMove;
					if (_cursor != null)
						_visual.Cursor = null;
					IsCaptured = false;
				}
			}
		}

		private bool UpdateOnlineMode()
		{
			return IsActive = !_onlineMode.IsOnline;
		}

		private void MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
				return;
			_lastMousePosition = e.GetPosition(_visual);
			IsCaptured = true;
		}

		private void MouseMove(object sender, MouseEventArgs e)
		{
			if (!IsCaptured)
				return;

			var currentMousePosition = e.GetPosition(_visual);
			var delta = new Point(
				_lastMousePosition.X - currentMousePosition.X,
				_lastMousePosition.Y - currentMousePosition.Y);
			var absoluteScale = new Point(
				_visual.ActualWidth,
				_visual.ActualHeight);
			var relativeDelta = new Point(
				delta.X / absoluteScale.X,
				delta.Y / absoluteScale.Y);

			OnMoved(relativeDelta);

			_lastMousePosition = currentMousePosition;
		}

		private void MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			IsCaptured = false;
		}
	}
}