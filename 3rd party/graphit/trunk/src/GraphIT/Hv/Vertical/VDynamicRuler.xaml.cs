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
using System.Reactive.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TechNewLogic.GraphIT.Hv.Vertical
{
	// OPT: Kann man hier etwas mit HDynamicRuler vereinheitlichen?

	partial class VDynamicRuler : IDisposable
	{
		/// <summary>
		/// Lineal ein Lineal mit "Kreuz" (für das dynamische Lineal).
		/// </summary>
		public VDynamicRuler(IVDynamicRulerEventBroker dynamicRulerEventBroker)
		{
			LineOpacity = 0.5;

			InitializeComponent();

			cross.Visibility = Visibility.Visible;

			dynamicRulerEventBroker.ShowRuler += ShowDynamicRuler;
			dynamicRulerEventBroker.HideRuler += HideDynamicRuler;

			_updateRulerObservable = Observable
				.FromEventPattern<DynamicRulerChangedEventArgs>(dynamicRulerEventBroker, "UpdateRuler")
				.ObserveOn(SynchronizationContext.Current)
				.Subscribe(evt => UpdateDynamicRuler(evt.EventArgs));

			HideDynamicRuler();
		}

		private readonly IDisposable _updateRulerObservable;

		public double LineOpacity { get; private set; }

		private void HideDynamicRuler()
		{
			Visibility = Visibility.Collapsed;
		}

		private void ShowDynamicRuler()
		{
			Visibility = Visibility.Visible;
		}

		private void UpdateDynamicRuler(DynamicRulerChangedEventArgs dynamicRulerChangedEventArgs)
		{
			transform.Y = dynamicRulerChangedEventArgs.Y - Height / 2;
			crossTransform.X = dynamicRulerChangedEventArgs.X - cross.Width / 2;

			// OPT: gehört diese Entscheidung hier rein? Eigentlich sollte es hier frei von anderen Typen sein
			cross.Visibility = dynamicRulerChangedEventArgs.InputReference is IVScale
				? Visibility.Collapsed
				: Visibility.Visible;
		}

		public void Dispose()
		{
			_updateRulerObservable.Dispose();
		}
	}
}
