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
using System.IO;
using System.IO.Packaging;
using System.Threading;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Xps;
using System.Windows.Threading;
using System.Windows.Xps.Packaging;

using TechNewLogic.GraphIT.MultiLanguage;

namespace TechNewLogic.GraphIT.Printing
{
	/// <summary>
	/// Interaction logic for XpsDocumentGenerator.xaml
	/// </summary>
	partial class XpsDocumentGenerator : IDisposable
	{
		internal XpsDocumentGenerator(
			Visual mainPrintContent,
			IEnumerable<PrintContent> additionalPrintContent,
			IViewDrawingState viewDrawingState,
			int width,
			int height,
			int pageMargin)
		{
			_mainPrintContent = mainPrintContent;
			_additionalPrintContent = additionalPrintContent;
			_viewDrawingState = viewDrawingState;
			PageMargin = pageMargin;
			Width = width;
			Height = height;

			InitializeComponent();
		}

		private readonly Visual _mainPrintContent;
		private readonly IEnumerable<PrintContent> _additionalPrintContent;
		private readonly IViewDrawingState _viewDrawingState;

		internal FixedDocumentSequence PrintedDocument { get; private set; }
		public int PageMargin { get; private set; }

		internal event Action StartPrinting;
		private void OnStartPrinting()
		{
			if (StartPrinting != null)
				StartPrinting();
			printLayouter.SetMainContent(_mainPrintContent);
			_additionalPrintContent
				.ForEachElement(it => printLayouter.AddAdditionalContent(it));
		}

		internal event Action PrintingFinished;
		private void OnPrintingFinished()
		{
			printLayouter.ReleaseContent();
			Close();
			if (PrintingFinished != null)
				PrintingFinished();
		}

		internal void Print()
		{
			OnStartPrinting();
			if (!IsLoaded)
			{
				Loaded += (s, e) => DoStartPrint();
				ShowDialog();
			}
			else
				DoStartPrint();
		}

		// IMP: Error Handling -> Event muss auch kommen, wenn es einen Print Error gab
		private void DoStartPrint()
		{
			ThreadPool.QueueUserWorkItem(o =>
				_viewDrawingState.EnsureViewUpToDate(
					() => _mainPrintContent.Dispatcher.Invoke(() => DoPrint(this.ToXps())),
					() => _mainPrintContent.Dispatcher.Invoke(() =>
						{
							var document = TextDocumentGenerator.CreateDocument(
								(int)Width,
								(int)Height,
								MlResources.CannotCreateDocument);
							DoPrint(document);
						}),
					TimeSpan.FromSeconds(30)));
		}

		private void DoPrint(FixedDocumentSequence documemt)
		{
			PrintedDocument = documemt;
			OnPrintingFinished();
		}

		public void Dispose()
		{
			printLayouter.ReleaseContent();
		}
	}
}
