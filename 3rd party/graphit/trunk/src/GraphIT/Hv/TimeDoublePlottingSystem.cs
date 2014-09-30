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
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using TechNewLogic.GraphIT.Hv.Horizontal;
using TechNewLogic.GraphIT.Hv.Vertical;
using TechNewLogic.GraphIT.MultiLanguage;

namespace TechNewLogic.GraphIT.Hv
{
	// IMP: Alle öffentlichen API-Methoden müssen auf Eingabe validiert werden
	// IMP: Nochmal schauen, was public und was internal sein sollte

	/// <summary>
	/// Represents a two-dimensional time-based / double-valued coordinate and plotting system.
	/// It exposes an API for many drawing and configuration functionalities.
	/// </summary>
	public sealed class TimeDoublePlottingSystem
		: IScaleGroupManager, IDoubleScalePool, IStaticRulerManager, IDoubleAxesGroupPool
	{
		internal TimeDoublePlottingSystem(
			ICurveRegistrar curveRegistrar,
			ICurveControlSurface curveControlSurface,
			InputReference inputReference,
			TimeDoubleCurveFactory curveFactory,
			TimeAxis timeAxis,
			IHDynamicRulerEventBroker timeDynamicRulerManager,
			IVDynamicRulerEventBroker doubleDynamicRulerManager,
			CustomControlSurface customControlSurface,
			CustomControlManager customControlManager,
			CurveContextMenuSurface curveContextMenuSurface,
			OnlineMode onlineMode,
			HDynamicRuler hDynamicRuler,
			HGridSurface hGridSurface,
			VDynamicRuler vDynamicRuler,
			VGridSurface vGridSurface,
			TimeScale timeScale,
			CurveBeltSurface curveBeltSurface,
			ContextMenuControl contextMenuControl,
			GlobalContextMenuSurface globalContextMenuSurface,
			DoubleAxesGroupFactory doubleAxesGroupFactory,
			Func<PanZoomManager> getPanZoomManager,
			Func<HStaticRulerSurface> getHStaticRulerSurface,
			Func<SectionZoomManager> getSectionZoomManager,
			Func<RulerManagementSurface> getRulerManagementSurface,
			Func<DateTime, HStaticRuler> createStaticRuler)
		{
			_getPanZoomManager = getPanZoomManager;
			_getHStaticRulerSurface = getHStaticRulerSurface;
			_getSectionZoomManager = getSectionZoomManager;
			_getRulerManagementSurface = getRulerManagementSurface;
			_createStaticRuler = createStaticRuler;
			_inputReference = inputReference;
			_customControlManager = customControlManager;
			_curveFactory = curveFactory;
			_curveRegistrar = curveRegistrar;
			_curveControlSurface = curveControlSurface;

			_contextMenuControl = contextMenuControl;

			_onlineMode = onlineMode;
			_doubleAxesGroupFactory = doubleAxesGroupFactory;
			_onlineMode.IsOnlineChanged += (s, e) => OnIsOnlineChanged();

			timeDynamicRulerManager.AddSource(_inputReference.InputElement);
			timeDynamicRulerManager.AddSource(timeScale.MoveScaleSurface);

			_doubleDynamicRulerManager = doubleDynamicRulerManager;
			_doubleDynamicRulerManager.AddSource(_inputReference.InputElement);

			_vScalePanelLeft = new StackPanel { Orientation = Orientation.Horizontal };
			_vScalePanelRight = new StackPanel { Orientation = Orientation.Horizontal };

			_hScalePanel = new StackPanel();
			timeScale.Margin = new Thickness(0, 10, 0, 0);
			_hScalePanel.Children.Add(timeScale);

			TimeAxis = timeAxis;
			TimeAxis.BoundsChanged += (s, e) => _customControlManager.RefreshCustomControls();

			// Add all the content to the curve control surface
			_curveControlSurface.AddControl(_vScalePanelLeft, SurfacePlacement.Left);
			_curveControlSurface.AddControl(_vScalePanelRight, SurfacePlacement.Right);
			_curveControlSurface.AddControl(_hScalePanel, SurfacePlacement.Bottom);
			_curveControlSurface.AddControl(customControlSurface, SurfacePlacement.CenterFront);
			_curveControlSurface.AddControl(hDynamicRuler, SurfacePlacement.CenterFront);
			_curveControlSurface.AddControl(vDynamicRuler, SurfacePlacement.CenterFront);
			_curveControlSurface.AddControl(globalContextMenuSurface, SurfacePlacement.CenterFront);
			_curveControlSurface.AddControl(hGridSurface, SurfacePlacement.CenterBack);
			_curveControlSurface.AddControl(vGridSurface, SurfacePlacement.CenterBack);
			_curveControlSurface.AddControl(curveBeltSurface, SurfacePlacement.CenterBack);
			_curveControlSurface.AddControl(curveContextMenuSurface, SurfacePlacement.CenterTopmost);

			RegisterDefaultContextMenuEntries();
		}

		private readonly List<TimeDoubleCurve> _curves = new List<TimeDoubleCurve>();
		private readonly OnlineMode _onlineMode;
		private readonly DoubleAxesGroupFactory _doubleAxesGroupFactory;
		private readonly InputReference _inputReference;
		private readonly Panel _vScalePanelLeft;
		private readonly Panel _vScalePanelRight;
		private readonly Panel _hScalePanel;
		private readonly IVDynamicRulerEventBroker _doubleDynamicRulerManager;
		private readonly ICurveRegistrar _curveRegistrar;
		private readonly ICurveControlSurface _curveControlSurface;
		private readonly TimeDoubleCurveFactory _curveFactory;

		private readonly Func<PanZoomManager> _getPanZoomManager;
		private readonly Func<HStaticRulerSurface> _getHStaticRulerSurface;
		private readonly Func<SectionZoomManager> _getSectionZoomManager;
		private readonly Func<RulerManagementSurface> _getRulerManagementSurface;
		private readonly Func<DateTime, HStaticRuler> _createStaticRuler;

		private RulerManagementSurface _rulerManagementSurface;

		/// <summary>
		/// Gets the <see cref="TimeAxis"/> which is associates with this <see cref="TimeDoublePlottingSystem"/>.
		/// </summary>
		public TimeAxis TimeAxis { get; private set; }

		private readonly List<DoubleAxesGroup> _axesGroups = new List<DoubleAxesGroup>();
		/// <summary>
		/// Geta an enumeration of <see cref="DoubleAxesGroup"/> children.
		/// </summary>
		public IEnumerable<DoubleAxesGroup> AxesGroups { get { return _axesGroups; } }

		/// <summary>
		/// Gets an enumeration of all <see cref="IVScale"/> children of this <see cref="TimeDoublePlottingSystem"/>.
		/// </summary>
		public IEnumerable<IVScale> Scales
		{
			get { return _axesGroups.Select(it => it.VisualScale).ToArray(); }
		}

		IEnumerable<DoubleScale> IDoubleScalePool.Scales
		{
			get { return _axesGroups.Select(it => it.VisualScale).ToArray(); }
		}

		private readonly ContextMenuControl _contextMenuControl;
		/// <summary>
		/// Gets the <see cref="IContextMenuRegistrar"/> implementation which is associated with this <see cref="TimeDoublePlottingSystem"/>.
		/// </summary>
		public IContextMenuRegistrar ContextMenuRegistrar { get { return _contextMenuControl; } }

		internal void Initialize()
		{
			_getPanZoomManager();
			_curveControlSurface.AddControl(_getHStaticRulerSurface(), SurfacePlacement.CenterTopmost);
			_curveControlSurface.AddControl(_getSectionZoomManager(), SurfacePlacement.CenterTopmost);
			_rulerManagementSurface = _getRulerManagementSurface();
			_curveControlSurface.AddControl(_rulerManagementSurface, SurfacePlacement.CenterTopmost);
		}

		private void RegisterDefaultContextMenuEntries()
		{
			ContextMenuRegistrar.AddMenuEntry(
				MlResources.CenterHere,
				() =>
				{
					var screenPosition = _contextMenuControl.GetScreenPosition();
					if (!IsOnline)
						TimeAxis.Center(TimeAxis.MapScreenToLogical(screenPosition.X));
					AxesGroups
						.Where(it => !it.IgnoreCanvasMovementOrZoom)
						.ForEachElement(it =>
							it.ProxyAxis.Center(it.ProxyAxis.MapScreenToLogical(screenPosition.Y)));
				});
			ContextMenuRegistrar.AddMenuEntry(
				MlResources.FitCurvesToScreen,
				() =>
				{
					if (!IsOnline)
					{
						var entries = _curves.SelectMany(it => it.DataSeries.LogicalEntries).ToList();
						if (entries.Count == 0)
							return;
						var minTime = entries.Min(it2 => it2.X);
						var maxTime = entries.Max(it2 => it2.X);
						TimeAxis.SetBounds(minTime, maxTime);
					}
					AxesGroups
						.Where(it => !it.IgnoreCanvasMovementOrZoom)
						.ForEachElement(it =>
						{
							var entries = it.DoubleAxes
								.SelectMany(it2 => it2.Curve.DataSeries.LogicalEntries
									.Where(it3 => !it3.Y.Equals(it2.Curve.DataSeries.UndefinedValue))).ToList();
							if (entries.Count == 0)
								return;
							
							var minValue = entries.Min(it2 => it2.Y);
							var maxValue = entries.Max(it2 => it2.Y);
							it.ProxyAxis.SetBounds(minValue, maxValue);
						});
				});
			ContextMenuRegistrar.AddMenuEntry(
				MlResources.ResetBounds,
				() =>
				{
					if (!IsOnline)
						TimeAxis.SetBounds(TimeAxis.LowerBound, TimeAxis.UpperBound);
					AxesGroups
						.Where(it => !it.IgnoreCanvasMovementOrZoom)
						.ForEachElement(it =>
							it.ProxyAxis.SetBounds(it.ProxyAxis.LowerBound, it.ProxyAxis.UpperBound));
				});
		}

		#region Online Mode

		/// <summary>
		/// Occurs when the <see cref="IsOnline"/> property has changed.
		/// </summary>
		public event EventHandler IsOnlineChanged;
		private void OnIsOnlineChanged()
		{
			if (IsOnlineChanged != null)
				IsOnlineChanged(this, new EventArgs());
		}

		// TODO: Den Online-Mechanismus besser kommentieren.
		/// <summary>
		/// Gets a value that indicates if the current state is in online mode.
		/// </summary>
		public bool IsOnline
		{
			get { return _onlineMode.IsOnline; }
		}

		/// <summary>
		/// Enables the online mode.
		/// </summary>
		/// <remarks>
		/// In online mode, the system behaves differently:
		/// <list type="bullet">
		///	  <item>Panning and zooming is only possible vertically.</item>
		///	  <item>Every second, the system updates the upper bound of the <see cref="TimeAxis"/> (by adding 1 second).</item>
		/// </list>
		/// </remarks>
		/// <param name="referenceTime">When switching to online mode, the provided value is used as a starting time for the automatic update of the <see cref="TimeAxis"/>.</param>
		/// <seealso cref="DisableOnlineMode"/>
		public void EnableOnlineMode(DateTime referenceTime)
		{
			_onlineMode.EnableOnlineMode(referenceTime);
		}

		/// <summary>
		/// Disables the online mode.
		/// </summary>
		/// <seealso cref="EnableOnlineMode"/>
		public void DisableOnlineMode()
		{
			_onlineMode.DisableOnlineMode();
		}

		#endregion

		#region Static Ruler

		/// <summary>
		/// Occurs after the <see cref="StaticRulers"/> enumeration has changed.
		/// </summary>
		public event Action RulersChanged;

		/// <summary>
		/// Occurs after a ruler in the <see cref="StaticRulers"/> enumeration has changed it's <see cref="HStaticRuler.IsReference"/> state.
		/// </summary>
		public event Action ReferenceRulerChanged;

		void IStaticRulerManager.OnReferenceRulerChanged()
		{
			if (ReferenceRulerChanged != null)
				ReferenceRulerChanged();
		}

		private void OnRulersChanged()
		{
			if (RulersChanged != null)
				RulersChanged();
		}

		private readonly ObservableCollection<HStaticRuler> _staticRulers
			= new ObservableCollection<HStaticRuler>();
		/// <summary>
		/// Gets an enumeration of the current <see cref="HStaticRuler"/> elements.
		/// </summary>
		public IEnumerable<HStaticRuler> StaticRulers { get { return _staticRulers; } }

		/// <summary>
		/// Gets a value indicating whether the <see cref="StaticRulers"/> enumeration has a <see cref="HStaticRuler"/> which is in reference mode.
		/// </summary>
		public bool HasReferenceRuler
		{
			get { return StaticRulers.Any(it => it.IsReference); }
		}

		/// <summary>
		/// Adds a new <see cref="HStaticRuler"/> to the <see cref="StaticRulers"/> enumeration at the given <paramref name="position"/>.
		/// </summary>
		/// <param name="position">The point in time at which the ruler will be placed.</param>
		/// <returns>The added ruler.</returns>
		public HStaticRuler AddStaticRuler(DateTime position)
		{
			// TODO: Exception, falls schon vorhanden
			var ruler = _createStaticRuler(position);
			_staticRulers.Add(ruler);
			OnRulersChanged();
			return ruler;
		}

		/// <summary>
		/// Removs a <see cref="HStaticRuler"/> which was previousely added to the <see cref="StaticRulers"/> enumeration.
		/// </summary>
		/// <remarks>
		/// If the given <paramref name="ruler"/> is not part of the <see cref="StaticRulers"/> enumeration, no exception is thrown.
		/// </remarks>
		/// <param name="ruler">The <see cref="HStaticRuler"/> to remove.</param>
		public void RemoveStaticRuler(HStaticRuler ruler)
		{
			// TODO: Exception, falls nicht vorhanden
			_staticRulers.Remove(ruler);
			OnRulersChanged();
		}

		private bool _isRulerManagementControlVisible = true;
		/// <summary>
		/// Gets or sets a value that indicated whether the ruler management control on the drawing area is visible or not.
		/// </summary>
		public bool IsRulerManagementControlVisible
		{
			get { return _isRulerManagementControlVisible; }
			set
			{
				_isRulerManagementControlVisible = value;
				_rulerManagementSurface.Visibility = value
					? Visibility.Visible
					: Visibility.Hidden;
			}
		}

		private bool _isIntervalControlVisible = true;
		/// <summary>
		/// Gets or sets a value that indicated whether the ruler management control on the drawing area is visible or not.
		/// </summary>
		public bool IsIntervalControlVisible
		{
			get { return _isIntervalControlVisible; }
			set
			{
				_hScalePanel.Children.OfType<TimeScale>().First().IntervalCanvas.Visibility = value
					? Visibility.Visible
					: Visibility.Hidden;
			}
		}

		#endregion

		/// <summary>
		/// Adds a new curve to this <see cref="TimeDoublePlottingSystem"/>.
		/// </summary>
		/// <param name="uom">The unit of measure (UOM) of the curve.</param>
		/// <param name="lowerBound">The starting lower bound of the curve.</param>
		/// <param name="upperBound">The starting upper bound of the curve.</param>
		/// <param name="color">The starting color of the curve.</param>
		/// <param name="redrawTime">The <see cref="RedrawTime"/> interval of the curve.</param>
		/// <param name="autoGroupBehavior">Specifies the behavior how the axes for the curve should be grouped.</param>
		/// <param name="curveDrawingMode">The drawing mode of the curve.</param>
		/// <param name="axisFormat">TODO</param>
		/// <param name="maxNumOfEntries">
		/// The maximum number of entries for the associated <see cref="TimeDoubleDataSeries"/> of the curve at which the entries will be truncated.
		/// <seealso cref="TimeDoubleDataSeries.MaxNumOfEntries"/>.
		/// </param>
		/// <param name="valueFormater">TODO</param>
		/// <param name="valueFetchStrategy">TODO</param>
		/// <returns>The added <see cref="TimeDoubleCurve"/></returns>
		public TimeDoubleCurve AddCurve(
			string uom,
			double lowerBound,
			double upperBound,
			Color color,
			RedrawTime redrawTime,
			AxisMatchingMode autoGroupBehavior,
			CurveDrawingMode curveDrawingMode,
			IValueFormater valueFormater,
			IValueFetchStrategy valueFetchStrategy,
			AxisFormat axisFormat,
			int maxNumOfEntries)
		{
			var curve = _curveFactory.CreateCurve(
				uom,
				lowerBound,
				upperBound,
				color,
				redrawTime,
				curveDrawingMode,
				valueFormater,
				valueFetchStrategy,
				axisFormat,
				maxNumOfEntries);

			_curves.Add(curve);
			_curveRegistrar.AddCurve(curve);

			var doubleAxis = curve.DoubleAxis;
			var matchingAxesGroup = _axesGroups
				.FirstOrDefault(it => it.ProxyAxis.GroupingMatches(doubleAxis, autoGroupBehavior).Absolute);
			if (matchingAxesGroup == null)
				CreateGroupAndScale(doubleAxis);
			else
				matchingAxesGroup.AddAxis(doubleAxis);

			return curve;
		}

		/// <summary>
		/// Removs a <see cref="TimeDoubleCurve"/> which was previousely added to this <see cref="TimeDoublePlottingSystem"/>.
		/// </summary>
		/// <param name="curve">The <see cref="TimeDoubleCurve"/> to remove.</param>
		/// <exception cref="Exception">The <paramref name="curve"/> is not part of this <see cref="TimeDoublePlottingSystem"/>.</exception>
		public void RemoveCurve(TimeDoubleCurve curve)
		{
			if (!_curves.Contains(curve))
				throw new Exception("The curve is not an element of the plotting system.");

			RemoveAxis(curve.DoubleAxis);

			_curveRegistrar.RemoveCurve(curve);
			_curves.Remove(curve);

			_curveFactory.DestroyCurve(curve);
		}

		/// <summary>
		/// Sets the visibility of the scales which are placed at the left, the right or at the bottom of the drawing area.
		/// </summary>
		/// <param name="showLeft">Indicates whether the left scale section is visible or not.</param>
		/// <param name="showRight">Indicates whether the left scale section is visible or not.</param>
		/// <param name="showBottom">Indicates whether the bottom scale section is visible or not.</param>
		public void SetScalesVisibility(bool showLeft, bool showRight, bool showBottom)
		{
			_vScalePanelLeft.Visibility = showLeft ? Visibility.Visible : Visibility.Collapsed;
			_vScalePanelRight.Visibility = showRight ? Visibility.Visible : Visibility.Collapsed;
			_hScalePanel.Visibility = showBottom ? Visibility.Visible : Visibility.Collapsed;
		}

		#region Custom Control Placement

		private readonly CustomControlManager _customControlManager;

		/// <summary>
		/// Adds a <see cref="UIElement"/> to this <see cref="TimeDoublePlottingSystem"/> and displays it on the drawing area.
		/// </summary>
		/// <param name="time">The point in time which determins the horizontal placement of the <see cref="UIElement"/>.</param>
		/// <param name="screenYPosition">The vertical placement in absolute screen coordinates, starting at the top left corner of the drawing area.</param>
		/// <param name="control">The <see cref="UIElement"/> to display on the drawing area.</param>
		public void AddCustomControl(DateTime time, double screenYPosition, UIElement control)
		{
			_customControlManager.AddCustomControl(
				() => screenYPosition,
				() => TimeAxis.MapLogicalToScreen(time),
				control);
		}

		/// <summary>
		/// Removs a <see cref="UIElement"/> which was previousely added to this <see cref="TimeDoublePlottingSystem"/>.
		/// </summary>
		/// <remarks>
		/// If the given <paramref name="control"/> is not part of this <see cref="TimeDoublePlottingSystem"/>, no exception is thrown.
		/// </remarks>
		/// <param name="control">The <see cref="UIElement"/> to remove.</param>
		public void RemoveCustomControl(UIElement control)
		{
			_customControlManager.RemoveCustomControl(control);
		}

		#endregion

		#region Grouping

		private void CreateGroupAndScale(DoubleAxis doubleAxis)
		{
			var doubleAxesGroup = _doubleAxesGroupFactory.CreateAbsoluteGroup(
				new DoubleAxisParameter(
					doubleAxis.Uom,
					doubleAxis.ActualLowerBound,
					doubleAxis.ActualUpperBound,
					doubleAxis.AxisFormat));
			doubleAxesGroup.AddAxis(doubleAxis);
			DoCreateGroupAndScale(doubleAxesGroup);
		}

		private DoubleAxesGroup CreateGroupAndScale(AxisFormat axisFormat)
		{
			var doubleAxesGroup = _doubleAxesGroupFactory.CreateRelativeGroup(axisFormat);
			DoCreateGroupAndScale(doubleAxesGroup);
			return doubleAxesGroup;
		}

		private void DoCreateGroupAndScale(DoubleAxesGroup doubleAxesGroup)
		{
			_doubleDynamicRulerManager.AddSource(doubleAxesGroup.VisualScale);
			_axesGroups.Add(doubleAxesGroup);
			doubleAxesGroup.VisualScale.PositionChanged += VisualScale_PositionChanged;
			doubleAxesGroup.VisualScale.Position =
				_vScalePanelLeft.Children.Count <= _vScalePanelRight.Children.Count
					? ScalePosition.Left
					: ScalePosition.Right;
		}

		private void RemoveAxis(DoubleAxis doubleAxis)
		{
			var matchingAxesGroup = _axesGroups.First(it => it.DoubleAxes.Contains(doubleAxis));
			matchingAxesGroup.RemoveAxis(doubleAxis);
			var remainingAxesCount = matchingAxesGroup.DoubleAxes.Count();
			if (remainingAxesCount > 0)
			{
				if (remainingAxesCount == 1 && matchingAxesGroup.IsRelative)
				{
					RemoveAxesGroup(matchingAxesGroup);
					CreateGroupAndScale(doubleAxis);
				}
			}
			else
				RemoveAxesGroup(matchingAxesGroup);
		}

		private void RemoveAxesGroup(DoubleAxesGroup axesGroup)
		{
			_doubleDynamicRulerManager.RemoveSource(axesGroup.VisualScale);
			_axesGroups.Remove(axesGroup);
			axesGroup.DoubleAxes.ToList().ForEach(axesGroup.RemoveAxis);
			axesGroup.VisualScale.PositionChanged -= VisualScale_PositionChanged;
			RemoveScaleFromPanels(axesGroup.VisualScale);
		}

		private void RemoveScaleFromPanels(UIElement visualScale)
		{
			_vScalePanelLeft.Children.Remove(visualScale);
			_vScalePanelRight.Children.Remove(visualScale);
		}

		/// <summary>
		/// Calculates in which way two <see cref="IVScale"/> instances can be grouped together.
		/// </summary>
		/// <param name="scale1">First scale.</param>
		/// <param name="scale2">Second scale.</param>
		/// <returns>The <see cref="GroupingResult"/> of the calculation.</returns>
		public GroupingResult CheckGrouping(IVScale scale1, IVScale scale2)
		{
			var axesGroup1 = scale1.AxesGroup;
			var axesGroup2 = scale2.AxesGroup;
			//if (axesGroup1 == axesGroup2)
			//    return GroupingResult.None;
			var groupingAbilities = axesGroup1.ProxyAxis.GroupingMatches(
				axesGroup2.ProxyAxis, AxisMatchingMode.UomOnly);
			if (groupingAbilities.Absolute)
				return GroupingResult.Absolute;
			if (groupingAbilities.Relative)
				return GroupingResult.Relative;
			return GroupingResult.None;
		}

		/// <summary>
		/// Groups two <see cref="IVScale"/> instances.
		/// </summary>
		/// <param name="sourceScale">The source <see cref="IVScale"/> which will be added to the <paramref name="targetScale"/>.</param>
		/// <param name="targetScale">The target <see cref="IVScale"/> to which the <paramref name="sourceScale"/> will be added.</param>
		/// <exception cref="Exception">The <see cref="GroupingResult"/> of the given <see cref="IVScale"/> instances is <see cref="GroupingResult.None"/>.</exception>
		public void GroupScales(IVScale sourceScale, IVScale targetScale)
		{
			var groupingResult = CheckGrouping(sourceScale, targetScale);
			if (groupingResult == GroupingResult.None)
				throw new Exception("Cannot group scales.");

			var sourceAxesGroup = sourceScale.AxesGroup;
			var targetAxesGroup = targetScale.AxesGroup;
			var sourceAxes = sourceAxesGroup.DoubleAxes.ToArray();
			var targetAxes = targetAxesGroup.DoubleAxes.ToArray();

			// Remove the source visual scale
			RemoveAxesGroup(sourceAxesGroup);

			// Will the target be a relative scale?
			if (groupingResult == GroupingResult.Relative)
			{
				if (!targetAxesGroup.IsRelative)
				{
					RemoveAxesGroup(targetAxesGroup);
					var relativeGroup = CreateGroupAndScale(targetAxesGroup.ProxyAxis.AxisFormat);
					sourceAxes.ForEachElement(relativeGroup.AddAxis);
					targetAxes.ForEachElement(relativeGroup.AddAxis);
				}
				else
					sourceAxes.ForEachElement(targetAxesGroup.AddAxis);
			}
			else
				sourceAxes.ForEachElement(targetAxesGroup.AddAxis);
		}

		/// <summary>
		/// Ungroups all containing <see cref="DoubleScale"/> objects of the given <see cref="DoubleAxesGroup"/>.
		/// </summary>
		/// <param name="axesGroup">The <see cref="DoubleAxesGroup"/> which shall be ungrouped.</param>
		public void UngroupAxesGroup(DoubleAxesGroup axesGroup)
		{
			// Remove the axis from the group and add a new group
			var axes = axesGroup.DoubleAxes.ToArray();
			RemoveAxesGroup(axesGroup);
			axes.ForEachElement(CreateGroupAndScale);
		}

		/// <summary>
		/// Gets the corresponding (visual) <see cref="IVScale"/> of the given (logical) <paramref name="axis"/>.
		/// </summary>
		/// <param name="axis">The <see cref="DoubleAxis"/> for which the <see cref="IVScale"/> will be retrived.</param>
		/// <returns>The corresponding <see cref="IVScale"/>.</returns>
		public IVScale GetVScaleByAxis(DoubleAxis axis)
		{
			var doubleAxesGroup = _axesGroups
				.FirstOrDefault(it => it.DoubleAxes.Contains(axis));
			if (doubleAxesGroup == null)
				throw new Exception("The axis is not part of the system.");
			return doubleAxesGroup.VisualScale;
		}

		#endregion

		void VisualScale_PositionChanged(IVScale scale)
		{
			if (!(scale is UIElement))
				return;
			var frameworkElement = (FrameworkElement)scale;
			RemoveScaleFromPanels(frameworkElement);
			if (scale.Position == ScalePosition.Left)
			{
				frameworkElement.Margin = new Thickness(0, 0, 10, 0);
				_vScalePanelLeft.Children.Insert(0, frameworkElement);
			}
			else
			{
				frameworkElement.Margin = new Thickness(10, 0, 0, 0);
				_vScalePanelRight.Children.Add(frameworkElement);
			}
		}
	}
}