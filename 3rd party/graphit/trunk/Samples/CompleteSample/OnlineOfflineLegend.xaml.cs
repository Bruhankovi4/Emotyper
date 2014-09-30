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
// 
// 

using System;
using System.Windows;
using TechNewLogic.GraphIT;
using TechNewLogic.GraphIT.Hv.Legend;

namespace CompleteSample
{
	/// <summary>
	/// Interaction logic for OnlineOfflineLegend.xaml
	/// </summary>
	public partial class OnlineOfflineLegend : IDisposable
	{
		public OnlineOfflineLegend(
			CurveDisplay curveDisplay,
			string description1Header = "Desc. 1",
			string description2Header = "Desc. 2",
			string description3Header = "Desc. 3",
			string description4Header = "Desc. 4",
			string description5Header = "Desc. 5")
		{
			_curveDisplay = curveDisplay;
			InitializeComponent();

			_tableLegend = new TableLegend(
				curveDisplay,
				description1Header,
				description2Header,
				description3Header,
				description4Header,
				description5Header);
			offlineLegendHolder.Content = _tableLegend;

			_tileLegend = new TileLegend(curveDisplay);
			onlineLegendHolder.Content = _tileLegend;

			_curveDisplay.TimeDoublePlottingSystem.IsOnlineChanged += (s, e) => UpdateOnlineState();
			UpdateOnlineState();
		}

		private readonly CurveDisplay _curveDisplay;

		private readonly TableLegend _tableLegend;
		private readonly TileLegend _tileLegend;

		private void UpdateOnlineState()
		{
			if (_curveDisplay.TimeDoublePlottingSystem.IsOnline)
			{
				onlineLegendHolder.Visibility = Visibility.Visible;
				offlineLegendHolder.Visibility = Visibility.Collapsed;
			}
			else
			{
				onlineLegendHolder.Visibility = Visibility.Collapsed;
				offlineLegendHolder.Visibility = Visibility.Visible;
			}
		}

		public void Dispose()
		{
			_tableLegend.Dispose();
			_tileLegend.Dispose();
		}
	}
}
