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
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Autofac;
using TechNewLogic.GraphIT.Helper;

namespace TechNewLogic.GraphIT.Hv.Legend
{
	/// <summary>
	/// Defines a table-style legend for a <see cref="CurveDisplay"/>.
	/// </summary>
	public sealed partial class TableLegend : INotifyPropertyChanged, IDisposable
	{
		/// <summary>
		/// Initializes a new instance of <see cref="TableLegend"/>.
		/// </summary>
		/// <param name="curveDisplay">The <see cref="CurveDisplay"/> instance which is associated with this <see cref="TableLegend"/>.</param>
		/// <param name="description1Header">The header text for the column that displays the <see cref="CurveDescription.DescriptionText1"/>.</param>
		/// <param name="description2Header">The header text for the column that displays the <see cref="CurveDescription.DescriptionText2"/>.</param>
		/// <param name="description3Header">The header text for the column that displays the <see cref="CurveDescription.DescriptionText3"/>.</param>
		/// <param name="description4Header">The header text for the column that displays the <see cref="CurveDescription.DescriptionText4"/>.</param>
		/// <param name="description5Header">The header text for the column that displays the <see cref="CurveDescription.DescriptionText5"/>.</param>
		public TableLegend(
			CurveDisplay curveDisplay,
			string description1Header,
			string description2Header,
			string description3Header,
			string description4Header,
			string description5Header)
		{
			InitializeComponent();

			var container = new TableLegendContainerFactory(
				curveDisplay,
				new GridViewTable(
					() => _viewModel.Curves,
					gridView)).Build();
			_viewModel = container.Resolve<TableLegendViewModel>();
			OnPropertyChanged("ViewModel");

			InitializeColumnHeaders(description1Header, description2Header, description3Header, description4Header, description5Header);

			Loaded += (s, e) => _viewModel.ResetRulerColumns();
		}

		private readonly TableLegendViewModel _viewModel;
		/// <summary>
		/// Gets a value representing the logical state of the <see cref="TableLegend"/>.
		/// </summary>
		public object ViewModel { get { return _viewModel; } }

		public IColumnConfiguration ColumnConfiguration { get { return _viewModel; } }

		private void InitializeColumnHeaders(
			string description1Header,
			string description2Header,
			string description3Header,
			string description4Header,
			string description5Header)
		{
			AssignHeaderText("d1", description1Header);
			AssignHeaderText("d2", description2Header);
			AssignHeaderText("d3", description3Header);
			AssignHeaderText("d4", description4Header);
			AssignHeaderText("d5", description5Header);
		}

		private void AssignHeaderText(string name, string description1Header)
		{
			var column = gridView.Columns
				.FirstOrDefault(it => GridViewHelper.GetHeaderID(it) == name);
			if (column != null)
				column.Header = description1Header;

			//var c1 = listView.FindName(name) as DataGridColumn;
			//if (c1 != null)
			//    c1.Header = description1Header;
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
		/// Releases all resources used by this <see cref="TableLegend"/>.
		/// </summary>
		public void Dispose()
		{
			_viewModel.Dispose();
		}

		private void ListView_DragDelta(object sender, DragDeltaEventArgs e)
		{
			var thumb = e.OriginalSource as Thumb;
			if (thumb == null)
				return;
			var header = thumb.TemplatedParent as GridViewColumnHeader;
			if (header == null)
				return;
			if (!GridViewHelper.GetCanColumnResize(header.Column)
				|| (e.HorizontalChange < 0 && header.Column.ActualWidth < 30)
				|| (e.HorizontalChange > 0 && header.Column.ActualWidth > 250))
			{
				e.Handled = true;
			}
		}
	}
}