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
using System.IO.Packaging;
using System.Linq;
using System.Printing;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Xps.Packaging;

namespace TechNewLogic.GraphIT.Printing
{
	class DirectPrinter : IPrinter
	{
		public DirectPrinter(
			Visual mainPrintContent, 
			IEnumerable<PrintContent> additionalPrintContent, 
			IViewDrawingState viewDrawingState, 
			int pageMargin)
		{
			_mainPrintContent = mainPrintContent;
			_additionalPrintContent = additionalPrintContent;
			_viewDrawingState = viewDrawingState;
			_pageMargin = pageMargin;
		}

		private readonly Visual _mainPrintContent;
		private readonly IEnumerable<PrintContent> _additionalPrintContent;
		private readonly IViewDrawingState _viewDrawingState;
		private readonly int _pageMargin;

		private XpsDocumentGenerator _xpsDocumentGenerator;
		private PrintDialog _printDialog;

		public event Action StartPrinting;
		private void OnStartPrinting()
		{
			if (StartPrinting != null)
				StartPrinting();
		}

		public event Action PrintingFinished;
		private void OnPrintingFinished()
		{
			_printDialog.PrintDocument(_xpsDocumentGenerator.PrintedDocument.DocumentPaginator, "Trend Curve");
			if (PrintingFinished != null)
				PrintingFinished();
		}

		public void Print()
		{
			if (_xpsDocumentGenerator != null)
				throw new InvalidOperationException("Cannot print twice!");

			_printDialog = new PrintDialog();
			_printDialog.PrintTicket.PageOrientation = PageOrientation.Landscape;
			if (_printDialog.ShowDialog() != true)
				return;

			var width = (int)_printDialog.PrintableAreaWidth;
			var height = (int)_printDialog.PrintableAreaHeight;
			_xpsDocumentGenerator = new XpsDocumentGenerator(
				_mainPrintContent, _additionalPrintContent, _viewDrawingState, width, height, _pageMargin);
			_xpsDocumentGenerator.StartPrinting += OnStartPrinting;
			_xpsDocumentGenerator.PrintingFinished += OnPrintingFinished;
			_xpsDocumentGenerator.Print();
		}
	}
}
