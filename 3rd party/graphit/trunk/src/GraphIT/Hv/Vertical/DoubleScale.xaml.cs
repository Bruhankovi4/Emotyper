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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TechNewLogic.GraphIT.Helper;
using Microsoft.Expression.Interactivity.Core;

namespace TechNewLogic.GraphIT.Hv.Vertical
{
	/// <summary>
	/// Interaction logic for DoubleScale.xaml
	/// </summary>
	partial class DoubleScale : IVScale, INotifyPropertyChanged
	{
		// TODO: Die ganzen Events abhängen
		public DoubleScale(
			DoubleAxesGroup axesGroup,
			IDragDropManager dragDropManager,
			IScaleGroupManager scaleGroupManager,
			IVDynamicRulerEventBroker dynamicRulerEventBroker,
			Func<ScaleDragDropBehavior> getScaleDragDropBehavior,
			Func<IScaleLabelCreationStrategy> getScaleLabelCreationStrategy)
		{
			_dragDropManager = dragDropManager;
			_scaleGroupManager = scaleGroupManager;
			_getScaleDragDropBehavior = getScaleDragDropBehavior;
			_getScaleLabelCreationStrategy = getScaleLabelCreationStrategy;

			EnableUserInput = true;

			AxesGroup = axesGroup;
			AxesGroup.ProxyAxis.BoundsChanged += (s, e) => UpdateSectionLabels();
			AxesGroup.ProxyAxis.BoundsChanged += (s, e) => UpdateDynamicRulerLabel();
			AxesGroup.AxesChanged += SynchronizeView;

			UngroupCommand = new RelayCommand(Ungroup);

			InitializeComponent();

			var uiMoveHelper = new UiMoveHelper(MoveScaleSurface, Cursors.ScrollNS);
			uiMoveHelper.Moved += uiMoveHelper_Moved;

			var scaleZoomHelper = new ScaleZoomHelper(MoveScaleSurface, false);
			scaleZoomHelper.Scrolled += scaleZoomHelper_Scrolled;

			dynamicRulerEventBroker.ShowRuler += ShowDynamicRulerLabel;
			dynamicRulerEventBroker.HideRuler += HideDynamicRulerLabel;
			_updateRulerObservable = Observable
				.FromEventPattern<DynamicRulerChangedEventArgs>(dynamicRulerEventBroker, "UpdateRuler")
				.ObserveOn(SynchronizationContext.Current)
				.Subscribe(evt => UpdateDynamicRuler(evt.EventArgs));

			axesGroupRoot.MouseEnter += (s, e) => UpdateVisualState();
			axesGroupRoot.MouseLeave += (s, e) => UpdateVisualState();
			groupingPopupMenu.MouseEnter += (s, e) => UpdateVisualState();
			groupingPopupMenu.MouseLeave += (s, e) => UpdateVisualState();
			PropertyChanged += (s, e) => UpdateVisualState();
		}

		private readonly IDragDropManager _dragDropManager;
		private readonly IScaleGroupManager _scaleGroupManager;
		private readonly Func<ScaleDragDropBehavior> _getScaleDragDropBehavior;
		private readonly Func<IScaleLabelCreationStrategy> _getScaleLabelCreationStrategy;
		private readonly IDisposable _updateRulerObservable;

		private IScaleLabelCreationStrategy _scaleLabelCreationStrategy;

		public DoubleAxesGroup AxesGroup { get; private set; }

		public bool EnableUserInput { get; set; }

		public GroupingResult DropAbility
		{
			get
			{
				var pos = Mouse.GetPosition(this);
				var hitTestResult = VisualTreeHelper.HitTest(this, pos);
				var isHit = hitTestResult != null && hitTestResult.VisualHit == DropTarget;
				return !isHit
					? GroupingResult.None
					: _scaleGroupManager.CheckGrouping(_dragDropManager.DraggedScale, this);
			}
		}

		internal event Action<IVScale> PositionChanged;

		internal Size LastRenderSize { get; private set; }

		private ScalePosition _position;
		public ScalePosition Position
		{
			get { return _position; }
			set
			{
				_position = value;
				OnPropertyChanged("Position");
				if (PositionChanged != null)
					PositionChanged(this);
			}
		}

		internal void Initialize()
		{
			// Instanziierung reicht aus
			_getScaleDragDropBehavior();
			LabelWidth = new GridLength(
				AxesGroup.ProxyAxis.AxisFormat == AxisFormat.Double ? 50 : 30,
				GridUnitType.Pixel);

			_scaleLabelCreationStrategy = _getScaleLabelCreationStrategy();
			SynchronizeView();
		}

		internal void AddLabel(VSectionLabel label)
		{
			sectionGrid.Children.Add(label);
		}

		internal void ClearLabels()
		{
			sectionGrid.Children.Clear();
		}

		#region ViewModel

		// OPT: Evtl. programmatisch via Api von außen zugänglich machen (siehe Ungroup Methode unten)
		public ICommand UngroupCommand { get; private set; }

		public string Description { get { return AxesGroup.ProxyAxis.Uom; } }
		public bool HasMultipleAxes { get { return AxesGroup.DoubleAxes.Count() > 1; } }
		public int NumberOfCurves { get { return AxesGroup.DoubleAxes.Count(); } }

		private readonly ObservableCollection<TimeDoubleCurve> _curves
			= new ObservableCollection<TimeDoubleCurve>();
		public IEnumerable<TimeDoubleCurve> Curves { get { return _curves; } }

		private bool _clipLabels;
		public bool ClipLabels
		{
			get { return _clipLabels; }
			set
			{
				_clipLabels = value;
				OnPropertyChanged("ClipLabels");
			}
		}

		private GridLength _labelWidth;
		public GridLength LabelWidth
		{
			get { return _labelWidth; }
			private set
			{
				_labelWidth = value;
				OnPropertyChanged("LabelWidth");
			}
		}

		private void Ungroup()
		{
			_scaleGroupManager.UngroupAxesGroup(AxesGroup);
		}

		#endregion

		private void SynchronizeView()
		{
			OnPropertyChanged("Description");
			OnPropertyChanged("HasMultipleAxes");
			OnPropertyChanged("NumberOfCurves");

			_curves.Clear();
			AxesGroup.DoubleAxes.ForEachElement(it => _curves.Add(it.Curve));
		}

		private void UpdateSectionLabels()
		{
			_scaleLabelCreationStrategy.UpdateSectionLabels();
		}

		#region Dynamic Ruler

		private double _currentDynamicRulerValue;

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
			_currentDynamicRulerValue = LastRenderSize.Width == 0
				? double.MinValue
				: MathHelper.MapPoint(
					AxesGroup.ProxyAxis.ActualUpperBound, AxesGroup.ProxyAxis.ActualLowerBound,
					0, LastRenderSize.Height,
					dynamicRulerChangedEventArgs.Y);

			UpdateDynamicRulerLabel();

			if (dynamicRulerChangedEventArgs.InputReference == this)
				HideDynamicRulerLabel();
			else
				ShowDynamicRulerLabel();

			//if (dynamicRulerChangedEventArgs.InputReference == this)
			//{
			//    Canvas.SetLeft(DynamicRulerLabel, LastRenderSize.Width - 13);
			//    Canvas.SetRight(DynamicRulerLabel, double.NaN);
			//}
			//else
			//{
			//    Canvas.SetLeft(DynamicRulerLabel, double.NaN);
			Canvas.SetRight(DynamicRulerLabel, 2);
			var width = ActualWidth - 12;
			DynamicRulerLabel.Width = width < 0 ? 0 : width;
			//}
			Canvas.SetTop(DynamicRulerLabel, dynamicRulerChangedEventArgs.Y - (DynamicRulerLabel.ActualHeight / 2));
		}

		private void UpdateDynamicRulerLabel()
		{
			DynamicRulerLabel.CurrentValue = GetFormattedLabelValue(_currentDynamicRulerValue);
		}

		public string GetFormattedLabelValue(double value)
		{
			return AxesGroup.GetFormattedLabelValue(value);
		}

		#endregion

		void uiMoveHelper_Moved(MoveInfo moveInfo)
		{
			if (!EnableUserInput)
				return;
			AxesGroup.ProxyAxis.MoveBoundsRelative(moveInfo.RelativeDiff.Y);
		}

		void scaleZoomHelper_Scrolled(ScrollInfo scrollInfo)
		{
			if (!EnableUserInput)
				return;
			AxesGroup.ProxyAxis.Zoom(scrollInfo.Delta < 0 ? 2 : 0.5, scrollInfo.RelativePosition);
		}

		private void UpdateVisualState()
		{
			if (HasMultipleAxes)
				ExtendedVisualStateManager.GoToElementState(axesGroupRoot, "MultipleAxes", true);
			else
				ExtendedVisualStateManager.GoToElementState(axesGroupRoot, "SingleAxes", true);

			if (groupIndicator.IsMouseOver || groupingPopupMenu.IsMouseOver)
				ExtendedVisualStateManager.GoToElementState(axesGroupRoot, "Open", true);
			else
				ExtendedVisualStateManager.GoToElementState(axesGroupRoot, "Closed", true);
		}

		protected override Size ArrangeOverride(Size arrangeBounds)
		{
			var innerSize = base.ArrangeOverride(arrangeBounds);
			var outerSize = this.GetOuterSize(innerSize);
			if (!LastRenderSize.Equals(outerSize))
			{
				LastRenderSize = outerSize;
				UpdateSectionLabels();
			}
			return innerSize;
		}

		//protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
		//{
		//    UpdateSectionLabels();
		//    base.OnRenderSizeChanged(sizeInfo);
		//}

		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
