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
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using TechNewLogic.GraphIT.Drawing;
using TechNewLogic.GraphIT.Helper;
using TechNewLogic.GraphIT.Hv.Horizontal;
using TechNewLogic.GraphIT.Hv.Vertical;

namespace TechNewLogic.GraphIT.Hv
{
	/// <summary>
	/// Represents a graphical curve of <see cref="DateTime"/> and <see cref="double"/> entries.
	/// </summary>
	public sealed class TimeDoubleCurve : Curve, IDisposable
	{
		internal TimeDoubleCurve(
			IHDynamicRulerEventBroker hDynamicRulerEventBroker,
			IDrawingAreaInfo drawingAreaInfo,
			IRedrawRequest redrawRequest,
			TimeAxis timeAxis,
			ICurveContextMenuSurface curveContextMenuSurface,
			IDrawer drawer,
			ICurvePool curvePool,
			CurveColor curveColor,
			RedrawTime redrawTime,
			CustomControlManager customControlManager,
			ContextMenuControl contextMenuControl,
			IValueFormater valueFormater,
			Func<DoubleAxis> doubleAxisFactory,
			Func<TimeDoubleDataSeries> timeDoubleDataSeriesFactory)
			: base(redrawRequest, curveContextMenuSurface, curvePool, contextMenuControl)
		{
			_doubleAxisFactory = doubleAxisFactory;
			_timeDoubleDataSeriesFactory = timeDoubleDataSeriesFactory;
			_drawingAreaInfo = drawingAreaInfo;
			_customControlManager = customControlManager;
			_valueFormater = valueFormater;
			_drawer = drawer;

			_stroke = curveColor.Stroke;
			_minBeltColor = curveColor.Stroke;
			_maxBeltColor = curveColor.Stroke;
			
			_timeAxis = timeAxis;
			_timeAxis.BoundsChanged += _timeAxis_BoundsChanged;

			_hDynamicRulerEventBroker = hDynamicRulerEventBroker;
			// Kein Throttle hier drin - das müssen die Verwender entscheiden!
			_hDynamicRulerEventBroker.UpdateRuler += _hDynamicRulerEventBroker_UpdateRuler;
			//_hDynamicRulerUpdateEvent = Observable
			//    .FromEventPattern<DynamicRulerChangedEventArgs>(_hDynamicRulerEventBroker, "UpdateRuler")
			//    .Sample(TimeSpan.FromMilliseconds(100))
			//    .ObserveOn(SynchronizationContext.Current)
			//    .Subscribe(evt => UpdateDynamicRuler(evt.EventArgs.X));

			_redrawSubject = new Subject<object>();
			_redrawObservable1 = _redrawSubject
				.Skip(1)
				.Sample(redrawTime.TimeSpan)
				.ObserveOn(SynchronizationContext.Current)
				.Subscribe(it =>
				{
					NeedsRedraw = true;
					RedrawRequest.RaiseRedrawRequest();
				});
			_redrawObservable2 = _redrawSubject
				.ObserveOn(SynchronizationContext.Current)
				.Subscribe(it => RedrawRequest.RaiseRedrawRequest());
		}

		private readonly IDrawer _drawer;
		private readonly IHDynamicRulerEventBroker _hDynamicRulerEventBroker;
		private readonly IDrawingAreaInfo _drawingAreaInfo;
		private readonly TimeAxis _timeAxis;
		private readonly Func<DoubleAxis> _doubleAxisFactory;
		private readonly Func<TimeDoubleDataSeries> _timeDoubleDataSeriesFactory;
		private readonly Subject<object> _redrawSubject;

		private readonly IDisposable _redrawObservable1;
		private readonly IDisposable _redrawObservable2;

		private HitTester _hitTester = new HitTester();
		private double _lastHDynamicRulerPosition = double.NaN;

		/// <summary>
		/// Gets the associated <see cref="DoubleAxis"/> instance for this <see cref="TimeDoubleCurve"/>.
		/// </summary>
		public DoubleAxis DoubleAxis { get; private set; }

		private Color _stroke;
		/// <summary>
		/// Gets or sets the stroke of the <see cref="TimeDoubleCurve"/>.
		/// </summary>
		public Color Stroke
		{
			get { return _stroke; }
			set
			{
				_stroke = value;
				OnPropertyChanged("Stroke");
				Redraw();
			}
		}

		private double _strokeThickness = 1;
		/// <summary>
		/// Gets or sets the stroke thickness of the <see cref="TimeDoubleCurve"/>.
		/// </summary>
		public double StrokeThickness
		{
			get { return _strokeThickness; }
			set
			{
				_strokeThickness = value;
				OnPropertyChanged("StrokeThickness");
				Redraw();
			}
		}

		private bool _isVisible = true;
		/// <summary>
		/// Gets or sets the visibility of this <see cref="TimeDoubleCurve"/> on the drawing area.
		/// </summary>
		public override bool IsVisible
		{
			get { return _isVisible; }
			set
			{
				_isVisible = value;
				OnPropertyChanged("IsVisible");
				Redraw();
			}
		}

		// OPT: Vielleicht doch lieber in ein ViewModel auslagern? Wird langsam etwas viel...
		private double _currentValue;
		/// <summary>
		/// Gets the value for the current selected point in time.
		/// </summary>
		/// <remarks>
		/// The current point in time is determined by the user. 
		/// When the user moves the mouse input over the drawing area, 
		/// this point in time is used to calculate the <see cref="CurrentValue"/>.
		/// </remarks>
		public double CurrentValue
		{
			get { return _currentValue; }
			private set
			{
				_currentValue = value;
				OnPropertyChanged("CurrentValue");
			}
		}

		/// <summary>
		/// Gets a string representation of a value, depending on the formatter of this <see cref="TimeDoubleCurve"/>.
		/// </summary>
		/// <param name="value">The value that should be formatted.</param>
		/// <returns>The formatted value.</returns>
		public string GetFormattedValue(double value)//, FormatDefinitions formatDefinitions)
		{
			return _valueFormater.GetFormattedValue(value);//, formatDefinitions);
		}

		private TimeDoubleDataSeries _dataSeries;
		/// <summary>
		/// Gets the <see cref="TimeDoubleDataSeries"/> that is associated with this <see cref="TimeDoubleCurve"/>.
		/// </summary>
		public TimeDoubleDataSeries DataSeries
		{
			get
			{
				if (_dataSeries == null)
					throw new Exception("Cannot access the data series because the curve has not bee initialized yet.");
				return _dataSeries;
			}
		}

		#region Min / Max

		internal event EventHandler BeltsChanged;
		private void OnBeltsChanged()
		{
			if (BeltsChanged != null)
				BeltsChanged(this, new EventArgs());
		}

		private double _minMinBelt = double.NaN;
		/// <summary>
		/// Gets or sets a value for the lowest minimum belt of this <see cref="TimeDoubleCurve"/>.
		/// </summary>
		/// <remarks>
		/// When no belt should be visualized. use the <see cref="double.NaN"/> value.
		/// </remarks>
		public double MinMinBelt
		{
			get { return _minMinBelt; }
			set
			{
				_minMinBelt = value;
				OnPropertyChanged("MinMinBelt");
				OnBeltsChanged();
			}
		}

		private double _minBelt = double.NaN;
		/// <summary>
		/// Gets or sets a value for the highest minimum belt of this <see cref="TimeDoubleCurve"/>.
		/// </summary>
		/// <remarks>
		/// When no belt should be visualized. use the <see cref="double.NaN"/> value.
		/// </remarks>
		public double MinBelt
		{
			get { return _minBelt; }
			set
			{
				_minBelt = value;
				OnPropertyChanged("MinBelt");
				OnBeltsChanged();
			}
		}

		private double _maxBelt = double.NaN;
		/// <summary>
		/// Gets or sets a value for the lowest maximum belt of this <see cref="TimeDoubleCurve"/>.
		/// </summary>
		/// <remarks>
		/// When no belt should be visualized. use the <see cref="double.NaN"/> value.
		/// </remarks>
		public double MaxBelt
		{
			get { return _maxBelt; }
			set
			{
				_maxBelt = value;
				OnPropertyChanged("MaxBelt");
				OnBeltsChanged();
			}
		}

		private double _maxMaxBelt = double.NaN;
		/// <summary>
		/// Gets or sets a value for the highest maximum belt of this <see cref="TimeDoubleCurve"/>.
		/// </summary>
		/// <remarks>
		/// When no belt should be visualized. use the <see cref="double.NaN"/> value.
		/// </remarks>
		public double MaxMaxBelt
		{
			get { return _maxMaxBelt; }
			set
			{
				_maxMaxBelt = value;
				OnPropertyChanged("MaxMaxBelt");
				OnBeltsChanged();
			}
		}

		private Color _minBeltColor;
		/// <summary>
		/// Gets or sets the color for the min and min/min belts.
		/// </summary>
		public Color MinBeltColor
		{
			get { return _minBeltColor; }
			set
			{
				_minBeltColor = value;
				OnPropertyChanged("MinBeltColor");
				OnBeltsChanged();
			}
		}

		private Color _maxBeltColor;
		/// <summary>
		/// Gets or sets the color for the max and max/max belts.
		/// </summary>
		public Color MaxBeltColor
		{
			get { return _maxBeltColor; }
			set
			{
				_maxBeltColor = value;
				OnPropertyChanged("MaxBeltColor");
				OnBeltsChanged();
			}
		}

		#endregion

		private void DataSeriesLogicalEntriesChanged()
		{
			Redraw();
		}

		private void Redraw()
		{
			_redrawSubject.OnNext(null);
		}

		internal void Initialize()
		{
			DoubleAxis = _doubleAxisFactory();
			DoubleAxis.BoundsChanged += DoubleAxis_BoundsChanged;

			// TODO: Event auch wieder abhängen in dem Fall, dass es Remove von Curves gibt
			// TODO: Ruler updaten, wenn sich Daten ändern (gilt generell an vielen Stellen)
			_dataSeries = _timeDoubleDataSeriesFactory();
			_dataSeries.LogicalEntriesChanged += DataSeriesLogicalEntriesChanged;

			_lastTimeAxisBounds = new Pair<DateTime>(_timeAxis.ActualLowerBound, _timeAxis.ActualUpperBound);
			_lastDoubleAxisBounds = new Pair<double>(DoubleAxis.ActualLowerBound, DoubleAxis.ActualUpperBound);
		}

		#region Freezing

		private bool _isFrozen;

		private Pair<DateTime> _frozenTimeAxisBounds;
		private Pair<double> _frozenDoubleAxisBounds;

		/// <summary>
		/// Can be used to freeze certain objects's properties at the beginning of a drawing cycle.
		/// </summary>
		/// <remarks>
		/// The <see cref="Curve.FreezeOverride"/> method is called before the <see cref="Curve.DrawOverride"/> method. 
		/// Use the <see cref="Curve.FreezeOverride"/> to save axes bounds, data series, etc for a drawing cycle.
		/// </remarks>
		/// <seealso cref="Curve.UnfreezeOverride"/>
		protected override void FreezeOverride()
		{
			AssertNotFrozen();

			_frozenTimeAxisBounds =
				new Pair<DateTime>(_timeAxis.ActualLowerBound, _timeAxis.ActualUpperBound);
			_frozenDoubleAxisBounds =
				new Pair<double>(DoubleAxis.ActualLowerBound, DoubleAxis.ActualUpperBound);
			DataSeries.Freeze();

			_isFrozen = true;
		}

		/// <summary>
		/// Can be used to unfreeze certain objects's properties at the end of a drawing cycle.
		/// </summary>
		/// <remarks>
		/// The <see cref="Curve.FreezeOverride"/> method is called after the <see cref="Curve.DrawOverride"/> method. 
		/// Use the <see cref="Curve.UnfreezeOverride"/> to reset axes bounds or data series, update the render offset, etc.
		/// </remarks>
		/// <seealso cref="Curve.FreezeOverride"/>
		protected override void UnfreezeOverride()
		{
			AssertFrozen();

			DataSeries.Unfreeze();
			_lastDoubleAxisBounds = _frozenDoubleAxisBounds;
			_lastTimeAxisBounds = _frozenTimeAxisBounds;
			UpdateRenderOffset();

			_isFrozen = false;
		}

		private void AssertFrozen()
		{
			if (!_isFrozen)
				throw new Exception("Cannot unfreeze an unfreezed curve.");
		}

		private void AssertNotFrozen()
		{
			if (_isFrozen)
				throw new Exception("Cannot freeze a freezed curve.");
		}

		#endregion

		#region RenderOffset

		private Pair<DateTime> _lastTimeAxisBounds;
		private Pair<double> _lastDoubleAxisBounds;

		private Rect _renderOffset;

		/// <summary>
		/// Gets a value that is applied as a transformation by the rendering pipeline to the bitmap of this <see cref="Curve"/>.
		/// </summary>
		/// <returns>The rect that will be used as a 2D transformation.</returns>
		protected override Rect GetRenderOffsetOverride()
		{
			return _renderOffset;
		}

		private void UpdateRenderOffset()
		{
			// Double-Achse
			var uy0 = _lastDoubleAxisBounds.E2;
			var ly0 = _lastDoubleAxisBounds.E1;
			var uy1 = DoubleAxis.ActualUpperBound;
			var ly1 = DoubleAxis.ActualLowerBound;
			var height = _drawingAreaInfo.DrawingHeight;
			var ty = Interpolate(0, height, ly1, uy1, uy0);
			var by = Interpolate(0, height, ly1, uy1, ly0);

			// Time-Achse
			var lx0 = _lastTimeAxisBounds.E2.Ticks;
			var ux0 = _lastTimeAxisBounds.E1.Ticks;
			var lx1 = _timeAxis.ActualUpperBound.Ticks;
			var ux1 = _timeAxis.ActualLowerBound.Ticks;
			var width = _drawingAreaInfo.DrawingWidth;
			var tx = Interpolate(0, width, lx1, ux1, ux0);
			var bx = Interpolate(0, width, lx1, ux1, lx0);

			_renderOffset = new Rect(
				new Point(tx, ty),
				new Point(bx, by));
		}

		// OPT: Ähnlich MathHelper?
		private static double Interpolate(double l0, double u0, double l1, double u1, double valueToMap)
		{
			return (u0 - l0) * (valueToMap - u1) / (l1 - u1);
		}

		//private static void OffsetForAxis(
		//    double p1, double p0, double q1, double q0, double drawingLength, bool invertDirection,
		//    out double r0, out double r1)
		//{
		//    var height = p1 - p0;
		//    var factorUpper = (q1 - p1) / height;
		//    var factorLower = (q0 - p0) / height;
		//    var invertFactor = invertDirection ? -1 : 1;
		//    r0 = 0 + factorUpper * drawingLength * invertFactor;
		//    r1 = drawingLength + factorLower * drawingLength * invertFactor;
		//}

		#endregion

		/// <summary>
		/// Gets a value that is applied as coloring by the rendering pipeline to the bitmap of this <see cref="Curve"/>.
		/// </summary>
		/// <returns>The stroke color fot the <see cref="Curve"/>.</returns>
		protected override Color GetStrokeOverride() { return Stroke; }

		/// <summary>
		/// Gets a value that indicates whether the <see cref="Curve"/> is hit by the given <paramref name="x"/> and <paramref name="y"/> coordinates.
		/// </summary>
		/// <remarks>
		/// The values from the <see cref="Curve.GetRenderOffsetOverride"/> method are already applied to the
		/// given <paramref name="x"/> and <paramref name="y"/> coordinates.
		/// </remarks>
		/// <param name="x">The horizontal screen coordinate.</param>
		/// <param name="y">The vertical screen coordinate.</param>
		/// <returns>The value that indicates whether the <see cref="Curve"/> was hit.</returns>
		protected override bool HitTestOverride(int x, int y)
		{
			return IsVisible && !NeedsRedraw ? _hitTester.HitTest(x, y) : false;
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
		protected override void DrawOverride(
			int width,
			int height,
			out byte[] bitmap,
			out byte[] selectionBitmap)
		{
			if (!_isFrozen)
				throw new Exception("Cannot draw an unfreezed curve.");

			_hitTester = _drawer.DrawLine(
				_strokeThickness,
				IsVisible ? DataSeries.MapLogicalToScreen() : Enumerable.Empty<IList<Point>>(),
				width,
				height,
				out bitmap,
				out selectionBitmap);
		}

		private void UpdateDynamicRuler(double screenX)
		{
			if (double.IsNaN(screenX))
				return;
			var dateTime = _timeAxis.MapScreenToLogical(screenX);
			CurrentValue = DataSeries.GetValueAtTime(dateTime, GetValueMode.MiddleValue);
			_lastHDynamicRulerPosition = screenX;
		}

		private void _hDynamicRulerEventBroker_UpdateRuler(object sender, DynamicRulerChangedEventArgs e)
		{
			UpdateDynamicRuler(e.X);
		}

		private void _timeAxis_BoundsChanged(object sender, EventArgs e)
		{
			UpdateRenderOffset();
			_customControlManager.RefreshCustomControls();
			UpdateDynamicRuler(_lastHDynamicRulerPosition);
			Redraw();
		}

		private void DoubleAxis_BoundsChanged(object sender, EventArgs e)
		{
			UpdateRenderOffset();
			_customControlManager.RefreshCustomControls();
			Redraw();
		}

		#region Custom Control Placement

		private readonly CustomControlManager _customControlManager;
		private readonly IValueFormater _valueFormater;

		/// <summary>
		/// Adds a <see cref="UIElement"/> to this <see cref="TimeDoubleCurve"/> and displays it on the drawing area.
		/// </summary>
		/// <remarks>
		/// The vertical placement of the <see cref="UIElement"/> is dynamically calculated by the corresponding value for the given <paramref name="time"/>.
		/// </remarks>
		/// <param name="time">The point in time which determins the horizontal placement of the <see cref="UIElement"/>.</param>
		/// <param name="control">The <see cref="UIElement"/> to display on the drawing area.</param>
		public void AddCustomControl(DateTime time, UIElement control)
		{
			var doubleValue = DataSeries.GetValueAtTime(time, GetValueMode.MiddleValue);
			_customControlManager.AddCustomControl(
				() => DoubleAxis.MapLogicalToScreen(doubleValue),
				() => _timeAxis.MapLogicalToScreen(time),
				control);
		}

		/// <summary>
		/// Adds a <see cref="UIElement"/> to this <see cref="TimeDoubleCurve"/> and displays it on the drawing area.
		/// </summary>
		/// <param name="time">The point in time which determins the horizontal placement of the <see cref="UIElement"/>.</param>
		/// <param name="yPosition">The vertical placement refered to the DoubleAxis of <see cref="TimeDoubleCurve"/>.</param>
		/// <param name="control">The <see cref="UIElement"/> to display on the drawing area.</param>
		public void AddCustomControl(DateTime time, double yPosition, UIElement control)
		{
			var mappedYValue = DoubleAxis.MapLogicalToScreen(yPosition);
			_customControlManager.AddCustomControl(
				() => DoubleAxis.MapLogicalToScreen(mappedYValue),
				() => _timeAxis.MapLogicalToScreen(time),
				control);
		}

		/// <summary>
		/// Removs a <see cref="UIElement"/> which was previousely added to this <see cref="TimeDoubleCurve"/>.
		/// </summary>
		/// <remarks>
		/// If the given <paramref name="control"/> is not part of this <see cref="TimeDoubleCurve"/>, no exception is thrown.
		/// </remarks>
		/// <param name="control">The <see cref="UIElement"/> to remove.</param>
		public void RemoveCustomControl(UIElement control)
		{
			_customControlManager.RemoveCustomControl(control);
		}

		#endregion

		/// <summary>
		/// Releases all resources used by this <see cref="TimeDoubleCurve"/>.
		/// </summary>
		public void Dispose()
		{
			_hDynamicRulerEventBroker.UpdateRuler -= _hDynamicRulerEventBroker_UpdateRuler;
			_dataSeries.LogicalEntriesChanged -= DataSeriesLogicalEntriesChanged;
			_customControlManager.Clear();
			_timeAxis.BoundsChanged -= _timeAxis_BoundsChanged;
			DoubleAxis.BoundsChanged -= DoubleAxis_BoundsChanged;
			_redrawObservable1.Dispose();
			_redrawObservable2.Dispose();
			_redrawSubject.Dispose();
		}
	}
}