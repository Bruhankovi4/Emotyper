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
using System.Windows.Media;
using System.Windows.Shapes;

using Autofac;

using TechNewLogic.GraphIT.Hv;
using TechNewLogic.GraphIT.Printing;
using IContainer = Autofac.IContainer;
using TechNewLogic.GraphIT.Helper;

namespace TechNewLogic.GraphIT
{
	/// <summary>
	/// Defines an area containing curves and scales.
	/// </summary>
	public sealed partial class CurveDisplay : INotifyPropertyChanged, ICurveRegistrar, ICurvePool, IDisposable
	{
		/// <summary>
		/// Initializes a new instance of <see cref="CurveDisplay"/>.
		/// </summary>
		public CurveDisplay()
		{
			InitializeComponent();

			_container = new ContainerFactory(this).Build();

			_printingRedrawRequest = _container.Resolve<IPrintingRedrawRequest>();
			TimeDoublePlottingSystem = _container.Resolve<TimeDoublePlottingSystem>();

			_curveDrawingSurfaceControl = (FrameworkElement)_container.Resolve<ICurveDrawingSurface>();
			SurfaceViewbox.Child = _curveDrawingSurfaceControl;
		}

		private readonly IContainer _container;

		private readonly IPrintingRedrawRequest _printingRedrawRequest;
		private readonly FrameworkElement _curveDrawingSurfaceControl;

		/// <summary>
		/// Gets the instance of <see cref="Hv.TimeDoublePlottingSystem"/> that is associated with this <see cref="CurveDisplay"/>.
		/// </summary>
		public TimeDoublePlottingSystem TimeDoublePlottingSystem { get; private set; }

		private readonly List<Curve> _curves = new List<Curve>();
		/// <summary>
		/// Gets an enumeration of all curves that are associated with this instance of <see cref="CurveDisplay"/>.
		/// </summary>
		public IEnumerable<Curve> Curves { get { return _curves; } }

		/// <summary>
		/// Occurs after a new <see cref="Curve"/> has been added.
		/// </summary>
		public event EventHandler<EventArgs<Curve>> CurveAdded;
		private void OnCurveAdded(Curve curve)
		{
			if (CurveAdded != null)
				CurveAdded(this, new EventArgs<Curve>(curve));
		}

		/// <summary>
		/// Occurs after a <see cref="Curve"/> has been removed.
		/// </summary>
		public event EventHandler<EventArgs<Curve>> CurveRemoved;
		private void OnCurveRemoved(Curve curve)
		{
			if (CurveRemoved != null)
				CurveRemoved(this, new EventArgs<Curve>(curve));
		}

		/// <summary>
		/// Occurs after a <see cref="Curve"/> has changed it's <see cref="Curve.IsSelected"/> state.
		/// </summary>
		public event EventHandler SelectedCurveChanged;
		private void OnSelectedCurveChanged()
		{
			if (SelectedCurveChanged != null)
				SelectedCurveChanged(this, new EventArgs());
		}

		/// <summary>
		/// Gets the selected curve from the <see cref="Curves"/> enumeration.
		/// </summary>
		/// <remarks>
		/// If no curve is selected, null is returned.
		/// </remarks>
		public Curve SelectedCurve
		{
			get { return _curves.FirstOrDefault(it => it.IsSelected); }
		}

		private void UpdateSelectedCurve(object sender, EventArgs e)
		{
			OnPropertyChanged("SelectedCurve");
			OnSelectedCurveChanged();
		}

		void ICurveRegistrar.AddCurve(Curve curve)
		{
			if (_curves.Contains(curve))
				throw new Exception("The curve is already an element of the curve display.");

			_curves.Add(curve);
			curve.IsSelectedChanged += UpdateSelectedCurve;
			OnCurveAdded(curve);
		}

		void ICurveRegistrar.RemoveCurve(Curve curve)
		{
			if (!_curves.Contains(curve))
				throw new Exception("The curve is not an element of the curve display.");

			curve.IsSelectedChanged -= UpdateSelectedCurve;
			_curves.Remove(curve);
			OnCurveRemoved(curve);
		}

		/// <summary>
		/// Occurs when a property value changes.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged(string p)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(p));
		}

		/// <summary>
		/// Opens thr print dialog and prints this instance of <see cref="CurveDisplay"/>.
		/// </summary>
		/// <param name="pageMargin">Specifies the print page margin in screen units.</param>
		/// <param name="additionalPrintContent">Specifies additional content that will be placed around this instance of <see cref="CurveDisplay"/>.</param>
		public void Print(int pageMargin, params PrintContent[] additionalPrintContent)
		{
			// IMP: Fehlerbehandlung (auch an anderen öffentlichen Stellen)
			// IMP: AdditionalContent darf nicht null sein
			var viewDrawingState = new ViewDrawingState(_printingRedrawRequest);
			var printer = new DirectPrinter(mainGrid, additionalPrintContent, viewDrawingState, pageMargin);
			DoPrint(printer);
		}

		/// <summary>
		/// Opens thr print preview dialog and prints this instance of <see cref="CurveDisplay"/>.
		/// </summary>
		/// <param name="additionalPrintContent">Specifies additional content that will be placed around this instance of <see cref="CurveDisplay"/>.</param>
		public void ShowPrintPreview(params PrintContent[] additionalPrintContent)
		{
			// IMP: Fehlerbehandlung (auch an anderen öffentlichen Stellen
			// IMP: AdditionalContent darf nicht null sein
			var viewDrawingState = new ViewDrawingState(_printingRedrawRequest);
			var printer = new PrintPreviewWindow(mainGrid, additionalPrintContent, viewDrawingState);
			DoPrint(printer);
		}

		#region Background / Foreground

		const string GeneralBackgroundKey = "GeneralBackground";
		const string GeneralForegroundKey = "GeneralForeground";

		/// <summary>
		/// Gets or sets a foreground brush that will be used for many visual elements of this <see cref="CurveDisplay"/>.
		/// </summary>
		public SolidColorBrush GeneralForeground
		{
			get
			{
				var generalForeground = mainGrid.TryFindResource(GeneralForegroundKey) as SolidColorBrush;
				if (generalForeground == null)
					throw new Exception("GeneralForeground Resources not found.");
				return generalForeground;
			}
			// IMP: Value darf nicht null sein
			set { mainGrid.Resources[GeneralForegroundKey] = value; }
		}

		/// <summary>
		/// Gets or sets a background brush that will be used for many visual elements of this <see cref="CurveDisplay"/>.
		/// </summary>
		public SolidColorBrush GeneralBackground
		{
			get
			{
				var generalBackground = mainGrid.TryFindResource(GeneralBackgroundKey) as SolidColorBrush;
				if (generalBackground == null)
					throw new Exception("GeneralBackground Resources not found.");
				return generalBackground;
			}
			// IMP: Value darf nicht null sein
			set { mainGrid.Resources[GeneralBackgroundKey] = value; }
		}

		#endregion

		private void DoPrint(IPrinter printer)
		{
			var generalBackground = GeneralBackground;
			var generalForeground = GeneralForeground;
			var isRulerManagementControlVisible = TimeDoublePlottingSystem.IsRulerManagementControlVisible;
			var isIntervalControlVisible = TimeDoublePlottingSystem.IsIntervalControlVisible;
			printer.StartPrinting += () =>
				{
					Content = new Image { Source = mainGrid.CaptureImage() };

					// Switch Foreground / Background and hide ruler management control and interval control
					GeneralBackground = new SolidColorBrush(Colors.White);
					GeneralForeground = new SolidColorBrush(Colors.Black);
					TimeDoublePlottingSystem.IsRulerManagementControlVisible = false;
					TimeDoublePlottingSystem.IsIntervalControlVisible = false;
				};
			printer.PrintingFinished += () =>
				{
					// Reset Foreground / Background, ruler management control and interval control
					GeneralBackground = generalBackground;
					GeneralForeground = generalForeground;
					TimeDoublePlottingSystem.IsRulerManagementControlVisible = isRulerManagementControlVisible;
					TimeDoublePlottingSystem.IsIntervalControlVisible = isIntervalControlVisible;

					Content = mainGrid;
				};
			printer.Print();
		}

		private void SurfaceViewbox_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			UpdateSurfaceSize();
		}

		private void SurfaceViewbox_Loaded(object sender, RoutedEventArgs e)
		{
			UpdateSurfaceSize();
		}

		private void UpdateSurfaceSize()
		{
			// OPT: Größe begrenzen
			var width = SurfaceViewbox.ActualWidth;
			var height = SurfaceViewbox.ActualHeight;

			// TODO: Parameter auslagern (auch in eigene Klasse machen)
			const int maxLength = 1200;
			const int minLength = 100;

			if (width < minLength || height < minLength)
			{
				_curveDrawingSurfaceControl.Width = minLength;
				_curveDrawingSurfaceControl.Height = minLength;
			}
			else if (width > maxLength || height > maxLength)
			{
				var ratio = width / height;
				_curveDrawingSurfaceControl.Width = maxLength;
				_curveDrawingSurfaceControl.Height = maxLength / ratio;
			}
			else
			{
				_curveDrawingSurfaceControl.Width = width;
				_curveDrawingSurfaceControl.Height = height;
			}
		}

		#region Implementation of IDisposable

		/// <summary>
		/// Releases all resources used by this <see cref="CurveDisplay"/>.
		/// </summary>
		public void Dispose()
		{
			_container.Dispose();
		}

		#endregion
	}
}
