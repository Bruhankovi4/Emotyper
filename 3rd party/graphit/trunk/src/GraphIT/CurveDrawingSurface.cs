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
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using TechNewLogic.GraphIT.Helper;
using TechNewLogic.GraphIT.MultiLanguage;

namespace TechNewLogic.GraphIT
{
	class CurveDrawingSurface : FrameworkElement, ICurveDrawingSurface, IRedrawRequest, IPrintingRedrawRequest, IDrawingAreaInfo, IDisposable
	{
		public CurveDrawingSurface(
			ICurvePool curvePool,
			InputReference inputReference)
		{
			RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.Linear);

			// Wichtig für "Pan" und "Zoom"
			ClipToBounds = true;

			_curvePool = curvePool;
			_curvePool.CurveAdded += OnCurveAdded;
			_curvePool.CurveRemoved += OnCurveRemoved;

			inputReference.InputElement.MouseMove += (s, e) => HandleMouseMove(e);
			inputReference.InputElement.MouseLeave += (s, e) => HandleMouseLeave(e);
			inputReference.InputElement.MouseLeftButtonUp += (s, e) => HandleMouseLeftButtonUp(e);
			inputReference.InputElement.MouseLeftButtonDown += (s, e) => HandleMouseLeftButtonDown(e);
			inputReference.InputElement.MouseRightButtonUp += (s, e) => HandleMouseRightButtonUp(e);
			inputReference.InputElement.MouseRightButtonDown += (s, e) => HandleMouseRightButtonDown(e);

			_redrawSubject = new Subject<object>();
			_redrawSubject
				//.Skip(1)
				.Throttle(TimeSpan.FromMilliseconds(50))
				.Subscribe(it => _redrawEvent.Set());

			_redrawThread = new Thread(RedrawThreadProc);
			_redrawThread.IsBackground = true;
			_redrawThread.SetApartmentState(ApartmentState.STA);
			_redrawThread.Start();
		}

		private readonly ICurvePool _curvePool;
		private readonly SynchronizedCollection<CurveBitmap> _curveBitmaps = new SynchronizedCollection<CurveBitmap>();
		private readonly Subject<object> _redrawSubject;

		private readonly AutoResetEvent _redrawEvent = new AutoResetEvent(false);
		private readonly Thread _redrawThread;
		private readonly List<AutoResetEvent> _redrawFinishedEvents = new List<AutoResetEvent>();
		private readonly List<AutoResetEvent> _refreshViewFinishedEvents = new List<AutoResetEvent>();

		private Size _lastControlSize = new Size(1, 1);
		private Size _currentDrawingSize = new Size(1, 1);
		private bool _suspendDraw;

		private bool _isDisposed;

		private void OnCurveAdded(object sender, EventArgs<Curve> curve)
		{
			var curveBitmap = new CurveBitmap(curve.Arg);
			_curveBitmaps.Add(curveBitmap);
			curve.Arg.IsSelectedChanged += CurveIsSelectedChanged;
			Redraw();
		}

		private void OnCurveRemoved(object sender, EventArgs<Curve> curve)
		{
			curve.Arg.IsSelectedChanged -= CurveIsSelectedChanged;
			RemoveCurveBitmap(curve.Arg);
		}

		public void RaiseRedrawRequest()
		{
			RefreshView();
			Redraw();
		}

		void IPrintingRedrawRequest.RaiseRedrawRequest(Action<bool> callback, TimeSpan timeout)
		{
			AutoResetEvent evt;
			lock (_redrawFinishedEvents)
			{
				evt = new AutoResetEvent(false);
				_redrawFinishedEvents.Add(evt);
			}
			Dispatcher.BeginInvoke(RaiseRedrawRequest);
			var res = evt.WaitOne(timeout);

			callback(res);
		}

		private void RedrawThreadProc()
		{
			while (!_isDisposed)
			{
				try
				{
					_redrawEvent.WaitOne();
					if (_isDisposed || Dispatcher.HasShutdownStarted)
						return;

					IEnumerable<AutoResetEvent> currentRedrawFinishedEvents;
					lock (_redrawFinishedEvents)
					{
						currentRedrawFinishedEvents = _redrawFinishedEvents.ToArray();
						_redrawFinishedEvents.Clear();
					}

					var sizeChanged = !_currentDrawingSize.Equals(_lastControlSize);
					_currentDrawingSize = _lastControlSize;

					// Don't draw the curves if the size has changed
					_suspendDraw = sizeChanged;
					if (sizeChanged && _curveBitmaps.Count > 5)
						Dispatcher.InvokeAndThrow(RefreshView);

					var currentDrawingSize = _lastControlSize;
					var width = (int)currentDrawingSize.Width;
					var height = (int)currentDrawingSize.Height;

					var curveBitmaps = _curveBitmaps
						.Where(it => it.Curve.NeedsRedraw || sizeChanged)
						.ToList();
					Dispatcher.InvokeAndThrow(() => 
						curveBitmaps.ForEachElement(it => it.Curve.Freeze()));

					// TODO: Switch für NET40
					//curveBitmaps.AsParallel().ForEachElement(it =>
					curveBitmaps.ForEachElement(it =>
						{
							byte[] bitmap, selection;
							it.Curve.Draw(
								width,
								height,
								out bitmap,
								out selection);

							it.BeginFreeze(width, height, it.Curve.GetStroke(), bitmap, selection);
							it.Curve.NeedsRedraw = false;
						});
					Dispatcher.InvokeAndThrow(() => curveBitmaps.ForEachElement(it => it.EndFreeze()));

					// HACK: nach dieser Zeit "sollte" der Front-Buffer der WriteableBitmaps upgedatet sein. Leider kann man das nicht sicher sagen.
					// Siehe dazu z.B: http://social.msdn.microsoft.com/Forums/en-US/wpf/thread/165e38ca-6314-4cec-8cd9-1edec3699674/#ed67f718-af66-4eaa-acdf-ec797c3c57c3
					// oder http://social.msdn.microsoft.com/Forums/en-US/wpf/thread/9bb416c6-5e7c-4809-92a3-cb6226d8d8bd/#1bfd6dd9-827e-4ca1-9800-72daf89993cb
					Thread.Sleep(150);

					Dispatcher.InvokeAndThrow(() =>
						{
							curveBitmaps.ForEachElement(it =>
								{
									it.Curve.Unfreeze();
									it.Flip();
								});
							_suspendDraw = false;

							// Die Events (vom Printing) werden ans RefreshView weitergegeben. Erst, wenn das fertig ist, darf das Printing weitermachen
							lock (_refreshViewFinishedEvents)
								_refreshViewFinishedEvents.AddRange(currentRedrawFinishedEvents);

							RefreshView();
						});


					// TODO: Irgrnfwo hier in der Methode kommt oft eine Exception (Collection Has Changed) 
					// Ist das jetzt behoben, weil nun SynchronizedCollection für _curveBitmaps verwendet wird?

					Dispatcher.BeginInvoke(OnDrawingSizeChanged);
				}
				catch (ObjectDisposedException)
				{
					// Kann passieren, dass Dispose aufgerufen wurde,
				}
				catch
				{
					// Es kann passieren, dass das während des laufenden Threads passiert
					if (!Dispatcher.HasShutdownStarted)
						throw;
				}
			}
		}

		private void Redraw()
		{
			_redrawSubject.OnNext(null);
		}

		private void RefreshView()
		{
			InvalidateVisual();
		}

		void CurveIsSelectedChanged(object sender, EventArgs e)
		{
			RefreshView();
		}

		#region Mouse Event Bubbling

		private Curve _currentMouseMoveCurve;

		private void HandleMouseMove(MouseEventArgs e)
		{
			var curve = HitTestCurves(MousePosition);
			if (_currentMouseMoveCurve == null && curve == null)
				return;
			if (_currentMouseMoveCurve == null)
			{
				_currentMouseMoveCurve = curve;
				curve.OnMouseEnter(e);
				return;
			}
			if (curve == null)
			{
				_currentMouseMoveCurve.OnMouseLeave(e);
				_currentMouseMoveCurve = curve;
				return;
			}
			if (_currentMouseMoveCurve != curve)
			{
				_currentMouseMoveCurve.OnMouseLeave(e);
				_currentMouseMoveCurve = curve;
				_currentMouseMoveCurve.OnMouseEnter(e);
				return;
			}
			_currentMouseMoveCurve.OnMouseMove(e);
		}

		private void HandleMouseLeave(MouseEventArgs e)
		{
			if (_currentMouseMoveCurve != null)
				_currentMouseMoveCurve.OnMouseLeave(e);
		}

		private void HandleMouseLeftButtonUp(MouseButtonEventArgs e)
		{
			if (_currentMouseMoveCurve != null)
				_currentMouseMoveCurve.OnMouseLeftButtonUp(e);
		}

		private void HandleMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			if (_currentMouseMoveCurve != null)
				_currentMouseMoveCurve.OnMouseLeftButtonDown(e);
		}

		private void HandleMouseRightButtonUp(MouseButtonEventArgs e)
		{
			if (_currentMouseMoveCurve != null)
				_currentMouseMoveCurve.OnMouseRightButtonUp(e);
		}

		private void HandleMouseRightButtonDown(MouseButtonEventArgs e)
		{
			if (_currentMouseMoveCurve != null)
				_currentMouseMoveCurve.OnMouseRightButtonDown(e);
		}

		#endregion

		private Curve HitTestCurves(Point position)
		{
			Curve activeCurve = null;
			// Die Positionen werden wieder in die Absolut-Koordinaten umgerechnet,
			// denn der RenderOffset bezieht sich auch auf das Absolut-System
			var posX = MathHelper.MapPoint(0, DrawingWidth, 0, ScaledDrawingWidth, position.X);
			var posY = MathHelper.MapPoint(0, DrawingHeight, 0, ScaledDrawingHeight, position.Y);
			foreach (var it in _curveBitmaps.Select(it => it.Curve).Reverse())
			{
				var renderOffset = it.GetRenderOffset();
				var x = (int)Math.Round(
					MathHelper.MapPoint(0, DrawingWidth, renderOffset.Left, renderOffset.Right, posX));
				var y = (int)Math.Round(
					MathHelper.MapPoint(0, DrawingHeight, renderOffset.Top, renderOffset.Bottom, posY));
				if (!it.HitTest(x, y))
					continue;
				activeCurve = it;
				break;
			}
			return activeCurve;
		}

		#region Render / Visual Overrides

		protected override int VisualChildrenCount { get { return 0; } }

		protected override Size MeasureOverride(Size availableSize)
		{
			return new Size(
				double.IsInfinity(availableSize.Width)
					? 5000
					: (availableSize.Width == 0 ? 1 : availableSize.Width),
				double.IsInfinity(availableSize.Height)
					? 5000
					: (availableSize.Height == 0 ? 1 : availableSize.Height));
			//var measureOverride = base.MeasureOverride(availableSize);
			//return measureOverride;
		}

		protected override Size ArrangeOverride(Size finalSize)
		{
			var innerSize = base.ArrangeOverride(finalSize);
			var outerSize = this.GetOuterSize(innerSize);

			// TODO: Mit DrawingArea machen?
			//var currentSize = new Size(ActualWidth, ActualHeight);
			if (!outerSize.Equals(_lastControlSize))
			{
				_lastControlSize = new Size(
					outerSize.Width == 0 ? 1 : outerSize.Width,
					outerSize.Height == 0 ? 1 : outerSize.Height);
				RaiseRedrawRequest();
				OnDrawingSizeChanged();
			}
			return innerSize;
		}

		//protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
		//{
		//    // TODO: Mit DrawingArea machen?
		//    var currentSize = new Size(ActualWidth, ActualHeight);
		//    if (currentSize.Equals(_lastControlSize))
		//        return;
		//    _lastControlSize = currentSize;

		//    RaiseRedrawRequest();
		//    OnDrawingSizeChanged();
		//}

		private void OnDrawingSizeChanged()
		{
			if (DrawingSizeChanged != null)
				DrawingSizeChanged(this, new EventArgs());
		}

		private void RemoveCurveBitmap(Curve curve)
		{
			var curveBitmap = _curveBitmaps.First(it => it.Curve == curve);
			_curveBitmaps.Remove(curveBitmap);
		}

		protected override void OnRender(DrawingContext drawingContext)
		{
			if (_suspendDraw)
			{
				RenderText(MlResources.PleaseWait, drawingContext);
				return;
			}

			_curveBitmaps
				.Where(it => it.Curve.IsVisible)
				.ForEachElement(it =>
				{
					var renderOffset = it.Curve.GetRenderOffset();
					var offScreenOffset = new Rect(50000, 50000, renderOffset.Width, renderOffset.Height);
					drawingContext.DrawImage(it.FrozenBitmap1, it.CurrentImageSelection == ImageSelection.Primary ? renderOffset : offScreenOffset);
					drawingContext.DrawImage(it.FrozenBitmap2, it.CurrentImageSelection == ImageSelection.Secondary ? renderOffset : offScreenOffset);

					if (_curvePool.SelectedCurve == it.Curve)
					{
						drawingContext.PushOpacity(0.4);
						drawingContext.DrawImage(it.FrozenSelection1, it.CurrentImageSelection == ImageSelection.Primary ? renderOffset : offScreenOffset);
						drawingContext.DrawImage(it.FrozenSelection2, it.CurrentImageSelection == ImageSelection.Secondary ? renderOffset : offScreenOffset);
						drawingContext.Pop();
					}
				});

			// Inform the Print Preview
			IEnumerable<AutoResetEvent> currentRefreshViewFinishedEvents;
			lock (_refreshViewFinishedEvents)
			{
				currentRefreshViewFinishedEvents = _refreshViewFinishedEvents.ToArray();
				_refreshViewFinishedEvents.Clear();
			}
			currentRefreshViewFinishedEvents.ForEachElement(it => it.Set());
		}

		private void RenderText(string textMessage, DrawingContext drawingContext)
		{
			RenderText(textMessage, 0.4, drawingContext);
		}

		private void RenderText(string textMessage, double opacity, DrawingContext drawingContext)
		{
			var text = new FormattedText(
				textMessage,
				CultureInfo.InvariantCulture,
				FlowDirection.LeftToRight,
				new Typeface("Verdana"),
				36,
				Brushes.WhiteSmoke);
			drawingContext.PushOpacity(opacity);
			drawingContext.DrawText(
				text,
				new Point(
					_lastControlSize.Width / 2 - text.Width / 2,
					_lastControlSize.Height / 2 - text.Height / 2));
			drawingContext.Pop();
		}

		#endregion

		public event EventHandler DrawingSizeChanged;

		public double DrawingWidth { get { return _currentDrawingSize.Width; } }

		public double DrawingHeight { get { return _currentDrawingSize.Height; } }

		public double ScaledDrawingWidth
		{
			// HACK: Ist das gut so?
			get { return Parent is Viewbox ? ((Viewbox)Parent).ActualWidth : DrawingWidth; }
		}

		public double ScaledDrawingHeight
		{
			// HACK: Ist das gut so?
			get { return Parent is Viewbox ? ((Viewbox)Parent).ActualHeight : DrawingHeight; }
		}

		public Point MousePosition
		{
			get
			{
				var position = Mouse.GetPosition(this);
				return new Point(
					MathHelper.MapPoint(0, ScaledDrawingWidth, 0, DrawingWidth, position.X),
					MathHelper.MapPoint(0, ScaledDrawingHeight, 0, DrawingHeight, position.Y));
			}
		}

		public void Dispose()
		{
			_isDisposed = true;
			_redrawEvent.Set();
		}
	}
}