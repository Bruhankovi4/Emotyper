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
using System.Reflection;
using System.Windows.Media;
using Autofac;
using Autofac.Core.Activators.Reflection;
using TechNewLogic.GraphIT.Drawing;
using TechNewLogic.GraphIT.Hv.Vertical;

namespace TechNewLogic.GraphIT.Hv
{
	class TimeDoubleCurveFactory : IDisposable
	{
		public TimeDoubleCurveFactory(ILifetimeScope container)
		{
			_outerContainer = container;
		}

		private readonly ILifetimeScope _outerContainer;
		private readonly Dictionary<TimeDoubleCurve, ILifetimeScope> _curveContainers
			= new Dictionary<TimeDoubleCurve, ILifetimeScope>();

		public TimeDoubleCurve CreateCurve(
			string uom,
			double lowerBound,
			double upperBound,
			Color color,
			RedrawTime redrawTime,
			CurveDrawingMode curveDrawingMode,
			IValueFormater valueFormater,
			IValueFetchStrategy valueFetchStrategy,
			AxisFormat axisFormat,
			int maxNumOfEntries)
		{
			var scope = _outerContainer.BeginLifetimeScope(builder =>
				RegisterComponents(
					builder, color, redrawTime, curveDrawingMode, 
					valueFormater, valueFetchStrategy, axisFormat, 
					uom, lowerBound, upperBound, maxNumOfEntries));
			var curve = scope.Resolve<TimeDoubleCurve>();
			_curveContainers.Add(curve, scope);
			return curve;
		}

		public void DestroyCurve(TimeDoubleCurve curve)
		{
			var container = _curveContainers[curve];
			_curveContainers.Remove(curve);
			container.Dispose();
		}

		public BeltRegistration CreateBoundaryRegistration(TimeDoubleCurve curve)
		{
			var scope = _curveContainers[curve];
			return scope.Resolve<BeltRegistration>();
		}

		private static void RegisterComponents(
			ContainerBuilder builder,
			Color color,
			RedrawTime redrawTime,
			CurveDrawingMode curveDrawingMode,
			IValueFormater valueFormater,
			IValueFetchStrategy valueFetchStrategy,
			AxisFormat axisFormat,
			string uom,
			double lowerBound,
			double upperBound,
			int maxNumOfEntries)
		{
			builder.RegisterInstance(new CurveColor(color));
			builder.RegisterInstance(redrawTime);
			builder
				.RegisterType<TimeDoubleCurve>()
				.FindConstructorsWith(new BindingFlagsConstructorFinder(BindingFlags.NonPublic))
				.OnActivated(args => args.Instance.Initialize())
				.InstancePerLifetimeScope();

			builder.RegisterInstance(new MaxEntriesCount(maxNumOfEntries));
			builder
				.RegisterType<TimeDoubleDataSeries>()
				.As<TimeDoubleDataSeries>()
				.As<ILogicalToScreenMapper>()
				.FindConstructorsWith(new BindingFlagsConstructorFinder(BindingFlags.NonPublic))
				.InstancePerLifetimeScope();

			builder
				.RegisterType<ValueFetcher>();
			builder
				.RegisterInstance(valueFormater)
				.As<IValueFormater>();
			builder
				.RegisterInstance(valueFetchStrategy)
				.As<IValueFetchStrategy>();

			builder.RegisterInstance(new DoubleAxisParameter(uom, lowerBound, upperBound, axisFormat));
			builder
				.RegisterType<DoubleAxis>()
				.FindConstructorsWith(new BindingFlagsConstructorFinder(BindingFlags.NonPublic))
				.InstancePerLifetimeScope();

			if (curveDrawingMode.UseSimpleLine)
			{
				builder
					.Register(c => new LineDrawingVisual(false))
					.As<WpfDrawingVisual>()
					.InstancePerDependency();
				builder
					.RegisterType<WpfExplicitSelectionDrawer>()
					//.RegisterType<WpfBlurSelectionDrawer>()
					//.RegisterType<TestDrawer>()
					.As<IDrawer>()
					.InstancePerLifetimeScope();
			}
			else
			{
				builder
					//.Register(c => new LineDrawingVisual(true))
					.Register(c =>
						{
							if (curveDrawingMode.UseFilledRectangleAuto)
								return RectangleWpfDrawingVisual.Auto();
							if (curveDrawingMode.UseFilledRectangleBaseline)
								return RectangleWpfDrawingVisual.Baseline(curveDrawingMode.BaselineValue);
							if (curveDrawingMode.UseFilledRectangleHighest)
								return RectangleWpfDrawingVisual.Highest();
							if (curveDrawingMode.UseFilledRectangleLowest)
								return RectangleWpfDrawingVisual.Lowest();
							throw new ArgumentOutOfRangeException("curveDrawingMode");
						})
					.As<WpfDrawingVisual>()
					.InstancePerDependency();
				builder
					.RegisterType<WpfExplicitSelectionDrawer>()
					//.RegisterType<WpfBlurSelectionDrawer>()
					//.RegisterType<TestDrawer>()
					.As<IDrawer>()
					.InstancePerLifetimeScope();
			}

			builder
				.RegisterType<ContextMenuControl>()
				.FindConstructorsWith(new BindingFlagsConstructorFinder(BindingFlags.NonPublic))
				.InstancePerLifetimeScope();

			builder
				.RegisterType<BeltRegistration>()
				.InstancePerLifetimeScope();
			builder
				.RegisterType<BeltControl>()
				.InstancePerDependency();
		}

		public void Dispose()
		{
			_curveContainers.Values.ForEachElement(it => it.Dispose());
		}
	}
}