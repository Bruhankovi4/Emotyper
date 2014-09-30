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

namespace TechNewLogic.GraphIT.Hv.Horizontal
{
	partial class HDynamicRuler : IDisposable
	{
		public HDynamicRuler(
			IHDynamicRulerEventBroker dynamicRulerEventBroker)
		{
			LineOpacity = 0.5;

			InitializeComponent();

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
			transform.X = dynamicRulerChangedEventArgs.X - Width / 2;
			crossTransform.Y = dynamicRulerChangedEventArgs.Y - cross.Height / 2;

			// OPT: gehört diese Entscheidung hier rein? Eigentlich sollte es hier frei von anderen Typen sein
			cross.Visibility = dynamicRulerChangedEventArgs.InputReference.IsOrHasParentOfType(typeof(IHScale))
				? Visibility.Collapsed
				: Visibility.Visible;
		}

		private bool _isDisposed;

		public void Dispose()
		{
			if (_isDisposed)
				return;
			_isDisposed = true;

			_updateRulerObservable.Dispose();
		}
	}
}
