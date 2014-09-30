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
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using TechNewLogic.GraphIT.Helper;
using Microsoft.Expression.Interactivity.Core;

namespace TechNewLogic.GraphIT.Hv.Horizontal
{
	/// <summary>
	/// Interaction logic for HStaticRulerControl.xaml
	/// </summary>
	partial class HStaticRulerControl : INotifyPropertyChanged
	{
		public HStaticRulerControl(
			HStaticRuler staticRuler,
			IStaticRulerManager staticRulerManager,
			IHStaticRulerSurface rulerSurface,
			TimeAxis timeAxis,
			ICurvePool curvePool,
			IOnlineMode onlineMode,
			IDrawingAreaInfo drawingAreaInfo)
		{
			_timeAxis = timeAxis;
			_timeAxis.BoundsChanged += (s, e) =>
				{
					UpdatePosition();
					UpdateDiffs();
				};

			_drawingAreaInfo = drawingAreaInfo;
			_drawingAreaInfo.DrawingSizeChanged += (s, e) =>
				{
					UpdatePosition();
					UpdateDiffs();
				};

			_staticRuler = staticRuler;
			_staticRuler.PositionUpdated += (s, e) => UpdatePosition();

			_staticRulerManager = staticRulerManager;
			_staticRulerManager.ReferenceRulerChanged += () =>
				{
					OnPropertyChanged("IsReference");
					UpdateDiffs();
				};

			_rulerSurface = rulerSurface;

			RemoveCommand = new RelayCommand(Remove);
			SetAsReferenceCommand = new RelayCommand(_staticRuler.ToggleReference);

			_onlineMode = onlineMode;

			_curvePool = curvePool;
			_curvePool.SelectedCurveChanged += (s, e) => UpdateDiffs();

			InitializeComponent();

			_moveHelper = new UiMoveHelper(backgroundGrid, Cursors.ScrollWE, _onlineMode);
			_moveHelper.Moved += moveHelper_Moved;
			_moveHelper.IsCapturedChanged += MenuStateChanged;

			MouseEnter += (s, e) =>
				{
					MenuStateChanged();
					UpdateZIndex();
				};
			MouseLeave += (s, e) =>
				{
					MenuStateChanged();
					UpdateZIndex();
				};
			menuPopup.MouseEnter += (s, e) => MenuStateChanged();
			menuPopup.MouseLeave += (s, e) => MenuStateChanged();

			UpdatePosition();
			UpdateDiffs();
		}

		private readonly TimeAxis _timeAxis;
		private readonly ICurvePool _curvePool;
		private readonly IOnlineMode _onlineMode;
		private readonly IDrawingAreaInfo _drawingAreaInfo;
		private readonly HStaticRuler _staticRuler;
		private readonly IStaticRulerManager _staticRulerManager;
		private readonly IHStaticRulerSurface _rulerSurface;

		private readonly UiMoveHelper _moveHelper;

		public ICommand RemoveCommand { get; private set; }
		public ICommand SetAsReferenceCommand { get; private set; }

		private double _toNextRulerWidth;
		public double ToNextRulerWidth
		{
			get { return _toNextRulerWidth; }
			private set
			{
				_toNextRulerWidth = value;
				OnPropertyChanged("ToNextRulerWidth");
			}
		}

		private double _refDiffMaxWidth;
		public double RefDiffMaxWidth
		{
			get { return _refDiffMaxWidth; }
			private set
			{
				_refDiffMaxWidth = value;
				OnPropertyChanged("RefDiffMaxWidth");
			}
		}

		private Visibility _rulerDiffVisibility;
		public Visibility RulerDiffVisibility
		{
			get { return _rulerDiffVisibility; }
			private set
			{
				_rulerDiffVisibility = value;
				OnPropertyChanged("RulerDiffVisibility");
			}
		}

		private Visibility _refDiffVisibility;
		public Visibility RefDiffVisibility
		{
			get { return _refDiffVisibility; }
			private set
			{
				_refDiffVisibility = value;
				OnPropertyChanged("RefDiffVisibility");
			}
		}

		private string _nextRulerDiffText;
		public string NextRulerDiffText
		{
			get { return _nextRulerDiffText; }
			private set
			{
				_nextRulerDiffText = value;
				OnPropertyChanged("NextRulerDiffText");
			}
		}

		private HorizontalAlignment _positionToReference;
		public HorizontalAlignment PositionToReference
		{
			get { return _positionToReference; }
			private set
			{
				_positionToReference = value;
				OnPropertyChanged("PositionToReference");
			}
		}

		private FlowDirection _diffPanelFlowDirection;
		public FlowDirection DiffPanelFlowDirection
		{
			get { return _diffPanelFlowDirection; }
			private set
			{
				_diffPanelFlowDirection = value;
				OnPropertyChanged("DiffPanelFlowDirection");
			}
		}

		private HorizontalAlignment _diffPanelAlignment;
		public HorizontalAlignment DiffPanelAlignment
		{
			get { return _diffPanelAlignment; }
			private set
			{
				_diffPanelAlignment = value;
				OnPropertyChanged("DiffPanelAlignment");
			}
		}

		private string _refRulerDiffText;
		public string RefRulerDiffText
		{
			get { return _refRulerDiffText; }
			private set
			{
				_refRulerDiffText = value;
				OnPropertyChanged("RefRulerDiffText");
			}
		}

		private string _timeDiffText;
		public string TimeDiffText { get { return _timeDiffText; } }
		private void SetTimeDiffText(TimeSpan timespan)
		{
			_timeDiffText = timespan.Abs().GetFormattedValue();
			OnPropertyChanged("TimeDiffText");
		}

		public bool IsReference { get { return _staticRuler.IsReference; } }

		public bool IsMenuOpen
		{
			get
			{
				return IsLoaded && (!_moveHelper.IsCaptured && (IsMouseOver || menuPopup.IsMouseOver));
			}
		}

		private int _zIndex;
		public int ZIndex
		{
			get { return _zIndex; }
			private set
			{
				_zIndex = value;
				OnPropertyChanged("ZIndex");
			}
		}

		private void Remove()
		{
			_staticRulerManager.RemoveStaticRuler(_staticRuler);
		}

		private void MenuStateChanged()
		{
			UpdatePosition();
			OnPropertyChanged("IsMenuOpen");
		}

		private void UpdatePosition()
		{
			var screenPoint = _timeAxis.MapLogicalToScreen(_staticRuler.Position);
			transform.X = screenPoint;
			menuPopup.UpdatePosition();
		}

		private void UpdateDiffs()
		{
			RulerDiffVisibility = Visibility.Collapsed;
			RefDiffVisibility = Visibility.Collapsed;

			var curve = _curvePool.SelectedCurve;
			if (curve == null || !curve.IsSelected || !(curve is TimeDoubleCurve))
				return;
			var tdCurve = (TimeDoubleCurve)curve;

			const int widthOffset = 3;

			var rulerDiffInfo = _staticRuler.GetDiff(tdCurve.DataSeries);
			switch (rulerDiffInfo.RulerCompareMode)
			{
				case RulerCompareMode.ToNextRuler:
					NextRulerDiffText =
						//rulerDiffInfo.CompareValue.GetFormattedValue(FormatDefinitions.FloatingComma)
						tdCurve.GetFormattedValue(rulerDiffInfo.CompareValue)//, FormatDefinitions.FloatingComma)
							+ GetDiffTextUom(tdCurve, rulerDiffInfo);
					SetTimeDiffText(rulerDiffInfo.TimeDiff);

					var thisX1 = _timeAxis.MapLogicalToScreen(_staticRuler.Position);
					var nextX1 = _timeAxis.MapLogicalToScreen(rulerDiffInfo.NextRuler.Position);
					ToNextRulerWidth = nextX1 - thisX1 - widthOffset;

					RulerDiffVisibility = Visibility.Visible;

					break;
				case RulerCompareMode.ToReferenceRuler:
					RefRulerDiffText = 
						//rulerDiffInfo.CompareValue.GetFormattedValue(FormatDefinitions.FloatingComma)
						tdCurve.GetFormattedValue(rulerDiffInfo.CompareValue)//, FormatDefinitions.FloatingComma)
							+ GetDiffTextUom(tdCurve, rulerDiffInfo);
					SetTimeDiffText(rulerDiffInfo.TimeDiff);

					switch (rulerDiffInfo.RelativeRulerPosition)
					{
						case RelativeRulerPosition.Left:
							SetPositionToReference(HorizontalAlignment.Left);
							break;
						case RelativeRulerPosition.Right:
							SetPositionToReference(HorizontalAlignment.Right);
							break;
						case RelativeRulerPosition.Center:
							SetPositionToReference(HorizontalAlignment.Center);
							break;
						default:
							throw new ArgumentOutOfRangeException();
					}

					var rulers = _staticRulerManager.StaticRulers;
					HStaticRuler nextRuler = null;
					switch (PositionToReference)
					{
						case HorizontalAlignment.Left:
							nextRuler = rulers.OrderBy(it => it.Position)
								.FirstOrDefault(it => it.Position > _staticRuler.Position);
							break;
						case HorizontalAlignment.Right:
							nextRuler = rulers.OrderByDescending(it => it.Position)
								.FirstOrDefault(it => it.Position < _staticRuler.Position);
							break;
					}
					if (nextRuler != null)
					{
						var thisX2 = _timeAxis.MapLogicalToScreen(_staticRuler.Position);
						var nextX2 = _timeAxis.MapLogicalToScreen(nextRuler.Position);
						var refDiffMaxWidth = Math.Abs(nextX2 - thisX2) - widthOffset;
						RefDiffMaxWidth = refDiffMaxWidth < 0 ? 0 : refDiffMaxWidth;
					}
					else
						RefDiffMaxWidth = 0;

					RefDiffVisibility = Visibility.Visible;

					break;
				case RulerCompareMode.Nothing:
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private static string GetDiffTextUom(
			TimeDoubleCurve tdCurve,
			RulerCompareInfo rulerDiffInfo)
		{
			return double.IsNaN(rulerDiffInfo.CompareValue)
				? string.Empty
				: tdCurve.DoubleAxis.Uom;
		}

		private void SetPositionToReference(HorizontalAlignment positionToReference)
		{
			PositionToReference = positionToReference;
			switch (PositionToReference)
			{
				case HorizontalAlignment.Left:
					DiffPanelFlowDirection = FlowDirection.LeftToRight;
					DiffPanelAlignment = HorizontalAlignment.Right;
					break;
				case HorizontalAlignment.Right:
					DiffPanelFlowDirection = FlowDirection.RightToLeft;
					DiffPanelAlignment = HorizontalAlignment.Left;
					break;
			}
		}

		private void UpdateZIndex()
		{
			ZIndex = IsMouseOver ? 100 : 0;
		}

		internal void Finish()
		{
			_staticRulerManager.ReferenceRulerChanged -= UpdateDiffs;
		}

		void moveHelper_Moved(MoveInfo moveInfo)
		{
			var mousePos = _drawingAreaInfo.MousePosition.X;
			var timePoint = _timeAxis.MapScreenToLogical(mousePos);
			_staticRuler.Position = timePoint;
			_rulerSurface.RulerControls.ForEachElement(it => it.UpdateDiffs());
		}

		private void UpdateVisualStates()
		{
			if (IsMouseOver)
				ExtendedVisualStateManager.GoToElementState(this, "MouseOver", true);
			else
				ExtendedVisualStateManager.GoToElementState(this, "Normal", true);

			if (IsReference)
				ExtendedVisualStateManager.GoToElementState(this, "Reference", true);
			else
				ExtendedVisualStateManager.GoToElementState(this, "NoReference", true);

			if (IsMenuOpen)
				ExtendedVisualStateManager.GoToElementState(this, "MenuOpen", true);
			else
				ExtendedVisualStateManager.GoToElementState(this, "MenuClosed", true);
		}

		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged(string p)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(p));
			UpdateVisualStates();
		}
	}
}
