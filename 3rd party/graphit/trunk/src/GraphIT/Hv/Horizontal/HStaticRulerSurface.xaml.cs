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
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace TechNewLogic.GraphIT.Hv.Horizontal
{
	/// <summary>
	/// Interaction logic for HStaticRulerSurface.xaml
	/// </summary>
	partial class HStaticRulerSurface : IHStaticRulerSurface
	{
		public HStaticRulerSurface(
			InputReference inputReference,
			IStaticRulerManager rulerManager,
			TimeAxis timeAxis,
			IDrawingAreaInfo drawingAreaInfo,
			Func<HStaticRuler, HStaticRulerControl> createRulerControl)
		{
			_inputReference = inputReference.InputElement;
			_inputReference.MouseLeftButtonDown += inputReference_MouseLeftButtonDown;

			_timeAxis = timeAxis;
			_drawingAreaInfo = drawingAreaInfo;
			_createRulerControl = createRulerControl;

			_rulerManager = rulerManager;
			_rulerManager.RulersChanged += UpdateRulers;

			InitializeComponent();
		}

		private readonly IInputElement _inputReference;
		private readonly IStaticRulerManager _rulerManager;
		private readonly TimeAxis _timeAxis;
		private readonly IDrawingAreaInfo _drawingAreaInfo;
		private readonly Func<HStaticRuler, HStaticRulerControl> _createRulerControl;

		public IEnumerable<HStaticRulerControl> RulerControls
		{
			get { return canvas.Children.OfType<HStaticRulerControl>(); }
		}

		private void UpdateRulers()
		{
			canvas.Children.OfType<HStaticRulerControl>()
				.ForEachElement(it => it.Finish());
			canvas.Children.Clear();
			_rulerManager.StaticRulers
				.Select(it => _createRulerControl(it))
				.ForEachElement(it =>
					{
						it.Height = ActualHeight;
						canvas.Children.Add(it);
						canvas.SetBinding(
							Panel.ZIndexProperty,
							new Binding("ZIndex") { Source = it });
					});
		}

		protected override Size ArrangeOverride(Size arrangeBounds)
		{
			var size = base.ArrangeOverride(arrangeBounds);
			canvas.Children.OfType<HStaticRulerControl>()
				.ForEachElement(it => it.Height = size.Height);
			return size;
		}

		void inputReference_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ClickCount != 2)
				return;

			var position = _drawingAreaInfo.MousePosition;
			_rulerManager.AddStaticRuler(
				_timeAxis.MapScreenToLogical(position.X));
		}
	}
}
