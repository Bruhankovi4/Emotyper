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
using System.Windows;

namespace TechNewLogic.GraphIT.Hv.Legend
{
	/// <summary>
	/// Defines a tile-style legend for a <see cref="CurveDisplay"/>.
	/// </summary>
	public sealed partial class TileLegend : IDisposable
	{
		/// <summary>
		/// Initializes a new instance of <see cref="TileLegend"/>.
		/// </summary>
		/// <param name="curveDisplay">The <see cref="CurveDisplay"/> instance which is associated with this <see cref="TileLegend"/>.</param>
		public TileLegend(CurveDisplay curveDisplay)
		{
			_viewModel = new TileLegendViewModel(curveDisplay, curveDisplay.TimeDoublePlottingSystem.TimeAxis);
			InitializeComponent();
		}

		private readonly TileLegendViewModel _viewModel;
		/// <summary>
		/// Gets a value representing the logical state of the <see cref="TileLegend"/>.
		/// </summary>
		public object ViewModel { get { return _viewModel; } }

		#region TileWidth

		/// <summary>
		/// Gets or sets the width for the tile items per curve.
		/// </summary>
		public double TileWidth
		{
			get { return (double)GetValue(TileWidthProperty); }
			set { SetValue(TileWidthProperty, value); }
		}

		// Using a DependencyProperty as the backing store for TileWidth.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty TileWidthProperty =
			DependencyProperty.Register("TileWidth", typeof(double), typeof(TileLegend), new UIPropertyMetadata(double.NaN));

		#endregion

		/// <summary>
		/// Releases all resources used by this <see cref="TileLegend"/>.
		/// </summary>
		public void Dispose()
		{
			_viewModel.Dispose();
		}
	}
}
