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
using System.Reactive.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TechNewLogic.GraphIT.Helper;
using Microsoft.Expression.Interactivity.Core;

namespace TechNewLogic.GraphIT.Hv.Horizontal
{
	/// <summary>
	/// Interaction logic for TimeScale.xaml
	/// </summary>
	partial class TimeScale : IHScale, INotifyPropertyChanged, IDisposable
	{
		public TimeScale(
			TimeAxis timeAxis,
			IHDynamicRulerEventBroker dynamicRulerEventBroker,
			HStepHelper hStepHelper,
			IOnlineMode onlineMode)
		{
			_timeAxis = timeAxis;
			_timeAxis.BoundsChanged += (s, e) => UpdateSectionLabels();
			_timeAxis.BoundsChanged += (s, e) => UpdateDynamicRulerLabel();

			_hStepHelper = hStepHelper;
			_hStepHelper.StepsChanged += (s, e) =>
			{
				CreateSectionLabels();
				UpdateSectionLabels();
			};

			_onlineMode = onlineMode;
			_onlineMode.IsOnlineChanged += (s, e) => OnPropertyChanged("IsInOfflineMode");

			IntervalLeftCommand = new RelayCommand(IntervalLeft);
			IntervalRightCommand = new RelayCommand(IntervalRight);

			InitializeComponent();

			MouseEnter += (s, e) => UpdateVisualStates();
			MouseLeave += (s, e) => UpdateVisualStates();

			var uiMoveHelper = new UiMoveHelper(MoveScaleSurface, Cursors.ScrollWE, onlineMode);
			uiMoveHelper.Moved += uiMoveHelper_Moved;

			var scaleZoomHelper = new ScaleZoomHelper(MoveScaleSurface, true);
			scaleZoomHelper.Scrolled += scaleZoomHelper_Scrolled;

			dynamicRulerEventBroker.ShowRuler += ShowDynamicRulerLabel;
			dynamicRulerEventBroker.HideRuler += HideDynamicRulerLabel;

			_updateRulerObservable = Observable
				.FromEventPattern<DynamicRulerChangedEventArgs>(dynamicRulerEventBroker, "UpdateRuler")
				.ObserveOn(SynchronizationContext.Current)
				.Subscribe(evt => UpdateDynamicRuler(evt.EventArgs));

			CreateSectionLabels();
			UpdateSectionLabels();
		}

		private readonly TimeAxis _timeAxis;
		private readonly HStepHelper _hStepHelper;
		private readonly IOnlineMode _onlineMode;
		private readonly List<HSectionLabel> _sectionLabels = new List<HSectionLabel>();
		private readonly IDisposable _updateRulerObservable;

		private DateTime _currentDynamicRulerTime;

		private void CreateSectionLabels()
		{
			sectionGrid.Children.Clear();
			_sectionLabels.Clear();

			for (var i = 0; i <= _hStepHelper.Steps; i++)
			{
				var sectionLabel = new HSectionLabel();
				_sectionLabels.Add(sectionLabel);
				sectionGrid.Children.Add(sectionLabel);
			}
		}

		private void UpdateSectionLabels()
		{
			if (_sectionLabels.Count != _hStepHelper.Steps + 1)
				return;

			//var visualStepSize = ActualWidth / _hStepHelper.Steps;
			var visualStepSize = _lastSize.Width / _hStepHelper.Steps;
			var upperBound = _timeAxis.ActualUpperBound.Ticks;
			var lowerBound = _timeAxis.ActualLowerBound.Ticks;
			var logicalStepSize = (upperBound - lowerBound) / _hStepHelper.Steps;
			for (var i = 0; i <= _hStepHelper.Steps; i++)
			{
				var sectionLabel = _sectionLabels[i];
				var range = _timeAxis.ActualUpperBound - _timeAxis.ActualLowerBound;
				sectionLabel.Text = new DateTime(lowerBound + logicalStepSize * i).ToString(range);
				sectionLabel.Margin = new Thickness(visualStepSize * i - sectionLabel.Width / 2, 0, 0, 0);
			}
		}

		private void ShowDynamicRulerLabel()
		{
			DynamicRulerLabel.Visibility = Visibility.Visible;
		}

		private void HideDynamicRulerLabel()
		{
			DynamicRulerLabel.Visibility = Visibility.Collapsed;
		}

		private void UpdateDynamicRuler(DynamicRulerChangedEventArgs dynamicRulerChangedEventArgs)
		{
			// TODO: Vorsicht - Conversion Error
			//_currentDynamicRulerTime = ActualWidth == 0
			_currentDynamicRulerTime = _lastSize.Width == 0
				? DateTime.MinValue
				: new DateTime(
					(long)MathHelper.MapPoint(
						_timeAxis.ActualLowerBound.Ticks, _timeAxis.ActualUpperBound.Ticks,
						//0, ActualWidth,
						0, _lastSize.Width,
						dynamicRulerChangedEventArgs.X));

			UpdateDynamicRulerLabel();

			var top = dynamicRulerChangedEventArgs.InputReference == MoveScaleSurface ? -30 : -5;
			Canvas.SetTop(DynamicRulerLabel, top);
			Canvas.SetLeft(DynamicRulerLabel, dynamicRulerChangedEventArgs.X - (DynamicRulerLabel.ActualWidth / 2));
		}

		private void UpdateDynamicRulerLabel()
		{
			//var range = _timeAxis.ActualUpperBound - _timeAxis.ActualLowerBound;
			//DynamicRulerLabel.CurrentValue = _currentDynamicRulerTime.ToString(range);
			// Vorsicht: Nicht einfach ToString() machen, sondern die ExtensionMethod benutzen, wegen der LocalTime
			DynamicRulerLabel.CurrentValue = _currentDynamicRulerTime.ToString(TimeSpan.MaxValue);
		}

		#region Interval

		public bool IsInOfflineMode { get { return !_onlineMode.IsOnline; } }

		public ICommand IntervalLeftCommand { get; private set; }

		public void IntervalLeft()
		{
			var timespan = _timeAxis.ActualUpperBound - _timeAxis.ActualLowerBound;
			_timeAxis.MoveBounds(timespan.Negate());
		}

		public ICommand IntervalRightCommand { get; private set; }

		public void IntervalRight()
		{
			var timespan = _timeAxis.ActualUpperBound - _timeAxis.ActualLowerBound;
			_timeAxis.MoveBounds(timespan);
		}

		#endregion

		private Size _lastSize;

		protected override Size ArrangeOverride(Size arrangeBounds)
		{
			var innerSize = base.ArrangeOverride(arrangeBounds);
			var outerSize = this.GetOuterSize(innerSize);
			if (!_lastSize.Equals(outerSize))
			{
				// Wichtig, dass die _lastSize VOR UpdateSectionLabels
				_lastSize = outerSize;
				UpdateSectionLabels();
			}
			return innerSize;
		}

		//protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
		//{
		//    UpdateSectionLabels();
		//    base.OnRenderSizeChanged(sizeInfo);
		//}

		void uiMoveHelper_Moved(MoveInfo moveInfo)
		{
			if (_onlineMode.IsOnline)
			{
				_timeAxis.MoveBoundsRelative(moveInfo.RelativeDiff.X);
			}
			else
				_timeAxis.MoveBoundsRelative(moveInfo.RelativeDiff.X);
		}

		void scaleZoomHelper_Scrolled(ScrollInfo scrollInfo)
		{
			_timeAxis.Zoom(
				scrollInfo.Delta < 0 ? 2 : 0.5,
				_onlineMode.IsOnline ? 1 : scrollInfo.RelativePosition);
		}

		private void UpdateVisualStates()
		{
			if (IsMouseOver)
				ExtendedVisualStateManager.GoToElementState(this, "MouseOver", true);
			else
				ExtendedVisualStateManager.GoToElementState(this, "Normal", true);
		}

		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		private bool _isDisposed;

		public void Dispose()
		{
			if (_isDisposed)
				return;
			_isDisposed = true;

			_updateRulerObservable.Dispose();
		}
	}
}