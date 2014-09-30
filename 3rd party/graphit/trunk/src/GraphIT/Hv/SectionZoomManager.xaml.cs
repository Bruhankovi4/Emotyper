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
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TechNewLogic.GraphIT.Hv.Horizontal;

namespace TechNewLogic.GraphIT.Hv
{
	/// <summary>
	/// Interaction logic for SectionZoomManager.xaml
	/// </summary>
	partial class SectionZoomManager : INotifyPropertyChanged
	{
		public SectionZoomManager(
			InputReference inputReference,
			TimeAxis timeAxis,
			IDoubleAxesGroupPool axesGroupPool,
			IDrawingAreaInfo drawingAreaInfo,
			IOnlineMode onlineMode)
		{
			_timeAxis = timeAxis;
			_axesGroupPool = axesGroupPool;
			_drawingAreaInfo = drawingAreaInfo;

			_onlineMode = onlineMode;
			// TODO: abhängen
			_onlineMode.IsOnlineChanged += (s, e) =>
				{
					if (_onlineMode.IsOnline)
						IsActive = false;
				};

			InitializeComponent();

			// Preview-Event ist wichtig, damit alle anderen Elemente nicht informiert werden
			inputReference.InputElement.PreviewMouseLeftButtonDown += InputElement_PreviewMouseLeftButtonDown;
			MouseMove += this_MouseMove;
			MouseLeftButtonUp += this_MouseLeftButtonUp;
		}

		private readonly TimeAxis _timeAxis;
		private readonly IDoubleAxesGroupPool _axesGroupPool;
		private readonly IDrawingAreaInfo _drawingAreaInfo;
		private readonly IOnlineMode _onlineMode;

		private Point _startingPoint;

		private Rect _sectionPosition;
		public Rect SectionPosition
		{
			get { return _sectionPosition; }
			private set
			{
				_sectionPosition = value;
				OnPropertyChanged("SectionPosition");
			}
		}

		private bool _isActive;
		public bool IsActive
		{
			get { return _isActive; }
			private set
			{
				if (_isActive == value)
					return;

				_isActive = value;
				if (value)
				{
					_startingPoint = _drawingAreaInfo.MousePosition;
					SectionPosition = new Rect(_startingPoint.X, _startingPoint.Y, 0, 0);
					CaptureMouse();
				}
				else
				{
					ReleaseMouseCapture();
					ZoomAxes();
				}
				OnPropertyChanged("IsActive");
			}
		}

		void InputElement_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (_onlineMode.IsOnline)
				return;
			if (IsActive
				|| e.LeftButton != MouseButtonState.Pressed
				|| !(
					Keyboard.IsKeyDown(Key.LeftCtrl) 
					|| Keyboard.IsKeyDown(Key.RightCtrl)))
			{
				return;
			}
			IsActive = true;
			e.Handled = true;
		}

		void this_MouseMove(object sender, MouseEventArgs e)
		{
			if (_onlineMode.IsOnline)
				return;
			if (!IsActive)
				return;
			SectionPosition = new Rect(_startingPoint, _drawingAreaInfo.MousePosition);
			// Hier muss ausnahmsweise gehandelt werden, weil dieses Element einen exklusiven Capture hat.
		}

		void this_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (_onlineMode.IsOnline)
				return;
			if (!IsActive)
				return;
			IsActive = false;
			e.Handled = true;
		}

		private void ZoomAxes()
		{
			var relLeft = SectionPosition.Left / _drawingAreaInfo.ScaledDrawingWidth;
			var relRight = SectionPosition.Right / _drawingAreaInfo.ScaledDrawingWidth;
			var relTop = SectionPosition.Top / _drawingAreaInfo.ScaledDrawingHeight;
			var relBottom = SectionPosition.Bottom / _drawingAreaInfo.ScaledDrawingHeight;

			var timeWidth = _timeAxis.ActualUpperBound.Ticks - _timeAxis.ActualLowerBound.Ticks;
			_timeAxis.SetBounds(
				new DateTime((long)(_timeAxis.ActualLowerBound.Ticks + timeWidth * relLeft)),
				new DateTime((long)(_timeAxis.ActualUpperBound.Ticks - timeWidth * (1 - relRight))));

			_axesGroupPool
				.AxesGroups
				.Where(it => !it.IgnoreCanvasMovementOrZoom)
				.Select(it => it.ProxyAxis)
				.ForEachElement(it =>
					{
						// 90 Grad nach rechts drehen und dann so wie bei der TimeAxis...
						var axisWidth = it.ActualUpperBound - it.ActualLowerBound;
						it.SetBounds(
							it.ActualLowerBound + axisWidth * (1 - relBottom),
							it.ActualUpperBound - axisWidth * relTop);
					});
		}

		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
