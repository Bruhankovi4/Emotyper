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
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Expression.Interactivity.Core;

namespace TechNewLogic.GraphIT.Hv.Horizontal
{
	/// <summary>
	/// Interaction logic for RulerManagementControl.xaml
	/// </summary>
	partial class RulerManagementControl
	{
		public RulerManagementControl(
			IStaticRulerManager staticRulerManager,
			Func<int, RulerViewModel> createViewModel)
		{
			_createViewModel = createViewModel;

			_staticRulerManager = staticRulerManager;
			_staticRulerManager.RulersChanged += StaticRulerManagerStaticRulersChanged;
			_staticRulerManager.ReferenceRulerChanged += RefreshAllRulers;

			InitializeComponent();

			MouseEnter += (s, e) => UpdateVisualStates();
			MouseLeave += (s, e) => UpdateVisualStates();

			UpdateRulers();
		}

		private readonly IStaticRulerManager _staticRulerManager;
		private readonly Func<int, RulerViewModel> _createViewModel;

		private readonly List<HStaticRuler> _rulers = new List<HStaticRuler>();

		private readonly ObservableCollection<RulerViewModel> _rulerViewModels = new ObservableCollection<RulerViewModel>();
		public IEnumerable<RulerViewModel> RulerViewModels { get { return _rulerViewModels; } }

		private void StaticRulerManagerStaticRulersChanged()
		{
			UpdateRulers();
			UpdateVisualStates();
		}

		private void UpdateRulers()
		{
			_rulerViewModels.ForEachElement(it => it.Dispose());
			_rulerViewModels.Clear();

			_rulers.ForEachElement(it => it.PositionUpdated -= it_PositionUpdated);

			_staticRulerManager
				.StaticRulers
				.ForEachElement(it =>
					{
						_rulerViewModels.Add(_createViewModel(_rulerViewModels.Count));
						_rulers.Add(it);
						it.PositionUpdated += it_PositionUpdated;
					});
		}

		void it_PositionUpdated(object sender, EventArgs e)
		{
			RefreshAllRulers();
		}

		private void RefreshAllRulers()
		{
			_rulerViewModels.ForEachElement(it => it.Refresh());
		}

		private void UpdateVisualStates()
		{
			if (IsMouseOver)
				ExtendedVisualStateManager.GoToElementState(this, "MouseOver", true);
			else
				ExtendedVisualStateManager.GoToElementState(this, "Normal", true);
			
			if (RulerViewModels.Any())
				ExtendedVisualStateManager.GoToElementState(this, "RulersPresent", true);
			else
				ExtendedVisualStateManager.GoToElementState(this, "Empty", true);
		}
	}
}
