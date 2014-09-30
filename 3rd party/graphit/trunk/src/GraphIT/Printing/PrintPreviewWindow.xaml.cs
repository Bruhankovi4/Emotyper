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
using System.Linq;
using System.Printing;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using TechNewLogic.GraphIT.Helper;
using TechNewLogic.GraphIT.MultiLanguage;

namespace TechNewLogic.GraphIT.Printing
{
	/// <summary>
	/// Interaction logic for PrintPreviewWindow.xaml
	/// </summary>
	partial class PrintPreviewWindow : IPrinter, INotifyPropertyChanged, IDisposable
	{
		internal PrintPreviewWindow(
			Visual mainPrintContent,
			IEnumerable<PrintContent> additionalPrintContent,
			IViewDrawingState viewDrawingState)
		{
			_mainPrintContent = mainPrintContent;
			_additionalPrintContent = additionalPrintContent;
			_viewDrawingState = viewDrawingState;

			PrintCommand = new RelayCommand(DoPrint);

			PaperFormats = new[]
				{
					new PaperFormatViewModel("DIN A3", PaperFormat.A3),
					new PaperFormatViewModel("DIN A4", PaperFormat.A4),
				};
			_selectedPaperFormat = PaperFormats.ElementAt(1);

			PaperOrientations = new[]
				{
					new PaperOrientationViewModel(MlResources.PaperOrientationPortrait, PaperOrientation.Portrait),
					new PaperOrientationViewModel(MlResources.PaperOrientationLandscape, PaperOrientation.Landscape),
				};
			_selectedPaperOrientation = PaperOrientations.ElementAt(1);

			InitializeComponent();
		}

		private readonly Visual _mainPrintContent;
		private readonly IEnumerable<PrintContent> _additionalPrintContent;
		private readonly IViewDrawingState _viewDrawingState;

		public ICommand PrintCommand { get; private set; }

		#region Format and Margin Selection

		public IEnumerable<PaperFormatViewModel> PaperFormats { get; private set; }

		private PaperFormatViewModel _selectedPaperFormat;
		public PaperFormatViewModel SelectedPaperFormat
		{
			get { return _selectedPaperFormat; }
			set
			{
				_selectedPaperFormat = value;
				OnPropertyChanged("SelectedPaperFormat");
				OnPageChanged();
			}
		}

		public IEnumerable<PaperOrientationViewModel> PaperOrientations { get; private set; }

		private PaperOrientationViewModel _selectedPaperOrientation;
		public PaperOrientationViewModel SelectedPaperOrientation
		{
			get { return _selectedPaperOrientation; }
			set
			{
				_selectedPaperOrientation = value;
				OnPropertyChanged("SelectedPaperOrientation");
				OnPageChanged();
			}
		}

		public IEnumerable<int> PageMargins { get { return new[] { 0, 10, 20, 30, 40, 50 }; } }

		private int _selectedPageMargin = 30;
		public int SelectedPageMargin
		{
			get { return _selectedPageMargin; }
			set
			{
				_selectedPageMargin = value;
				OnPropertyChanged("SelectedPageMargin");
				OnPageChanged();
			}
		}

		#endregion

		private IDocumentPaginatorSource _document;
		public IDocumentPaginatorSource Document
		{
			get { return _document; }
			private set
			{
				_document = value;
				OnPropertyChanged("Document");

				// HACK
				documentViewer.FitToMaxPagesAcross();
			}
		}

		public event Action StartPrinting;
		private void OnStartPrinting()
		{
			if (StartPrinting != null)
				StartPrinting();
		}

		public event Action PrintingFinished;
		private void OnPrintingFinished()
		{
			if (PrintingFinished != null)
				PrintingFinished();
		}

		private void OnPageChanged()
		{
			// Ticket "Demo": Wenn das nicht gemacht wird und beim PrintPreviewWindow sehr schnell die Page-Orientation
			// geändert wird, kommt es im PrimtLayouter.SetMainContent zu einer Exception, da der mainContent
			// noch nicht wieder abgehängt ist.
			if (_xpsDocumentGenerator != null)
				_xpsDocumentGenerator.Dispose();

			int width, height;
			SelectedPaperFormat.PaperFormat.GetWidthAndHeight(
				SelectedPaperOrientation.PaperOrientation, out width, out height);
			_xpsDocumentGenerator = new XpsDocumentGenerator(
				_mainPrintContent,
				_additionalPrintContent,
				_viewDrawingState,
				width,
				height,
				SelectedPageMargin);
			_xpsDocumentGenerator.PrintingFinished += () =>
				{
					Document = _xpsDocumentGenerator.PrintedDocument;
				};
			Document = TextDocumentGenerator.CreateDocument(width, height, MlResources.DocumentIsBeingGenerated);
			_xpsDocumentGenerator.Print();
		}

		public void Print()
		{
			ShowDialog();
		}

		private void DoPrint()
		{
			var printDialog = new PrintDialog();
			printDialog.PrintTicket.PageOrientation =
				SelectedPaperOrientation.PaperOrientation == PaperOrientation.Landscape
					? PageOrientation.Landscape
					: PageOrientation.Portrait;
			printDialog.PrintTicket.PageMediaSize =
				SelectedPaperFormat.PaperFormat == PaperFormat.A3
					? new PageMediaSize(PageMediaSizeName.ISOA3)
					: new PageMediaSize(PageMediaSizeName.ISOA4);
			if (printDialog.ShowDialog() != true)
				return;
			printDialog.PrintDocument(Document.DocumentPaginator, "Trend Curve");
		}

		private bool _firstActivation = true;
		private XpsDocumentGenerator _xpsDocumentGenerator;

		protected override void OnActivated(EventArgs e)
		{
			base.OnActivated(e);

			if (!_firstActivation)
				return;
	
			_firstActivation = false;
			OnStartPrinting();
			OnPageChanged();
		}

		protected override void OnClosed(EventArgs e)
		{
			base.OnClosed(e);
			OnPrintingFinished();
		}

		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		public void Dispose()
		{
			if (_xpsDocumentGenerator != null)
				_xpsDocumentGenerator.Dispose();
			_xpsDocumentGenerator = null;
		}
	}
}
