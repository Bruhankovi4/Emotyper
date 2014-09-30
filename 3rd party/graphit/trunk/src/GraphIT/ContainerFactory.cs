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
using System.ComponentModel;
using System.Reflection;
using Autofac;
using Autofac.Core.Activators.Reflection;
using TechNewLogic.GraphIT.Helper;
using TechNewLogic.GraphIT.Hv;
using TechNewLogic.GraphIT.Hv.Horizontal;
using TechNewLogic.GraphIT.Hv.Vertical;
using IContainer = Autofac.IContainer;

namespace TechNewLogic.GraphIT
{
	class ContainerFactory
	{
		public ContainerFactory(CurveDisplay curveDisplay)
		{
			_curveDisplay = curveDisplay;
			_builder = new ContainerBuilder();
			RegisterComponents();
		}

		private readonly CurveDisplay _curveDisplay;
		private readonly ContainerBuilder _builder;

		private IContainer _container;

		public IContainer Build()
		{
			if (_container != null)
				throw new Exception("The container has already been built.");

			_container = _builder.Build();
			return _container;
		}

		private void RegisterComponents()
		{
			_builder
				.RegisterInstance(_curveDisplay)
				.As<CurveDisplay>()
				.As<ICurvePool>()
				.As<ICurveRegistrar>();

			_builder.RegisterInstance(
				new InputReference(_curveDisplay.InputReference));

			if (DesignerProperties.GetIsInDesignMode(_curveDisplay))
			{
				_builder
					.RegisterType<DesignerCurveDrawingSurface>()
					.As<IRedrawRequest>()
					.As<IPrintingRedrawRequest>()
					.As<IDrawingAreaInfo>()
					.As<ICurveDrawingSurface>()
					.SingleInstance();
			}
			else
			{
				_builder
					.RegisterType<CurveDrawingSurface>()
					.As<IRedrawRequest>()
					.As<IPrintingRedrawRequest>()
					.As<IDrawingAreaInfo>()
					.As<ICurveDrawingSurface>()
					.SingleInstance();
			}

			_builder
				.RegisterType<CurveControlSurface>()
				.As<ICurveControlSurface>()
				.SingleInstance();

			_builder
				.RegisterType<CustomControlManager>()
				.InstancePerLifetimeScope();
			_builder
				.RegisterType<CustomControlSurface>()
				.As<CustomControlSurface>()
				.As<ICustomControlSurface>()
				.SingleInstance();

			_builder
				.RegisterType<CurveContextMenuSurface>()
				.As<CurveContextMenuSurface>()
				.As<ICurveContextMenuSurface>()
				.SingleInstance();

			_builder
				.RegisterType<TimeAxis>()
				.FindConstructorsWith(new BindingFlagsConstructorFinder(BindingFlags.NonPublic))
				.SingleInstance();

			_builder
				.RegisterType<DynamicRulerManager>()
				.As<IHDynamicRulerEventBroker>()
				.SingleInstance();
			_builder
				.RegisterType<DynamicRulerManager>()
				.As<IVDynamicRulerEventBroker>()
				.SingleInstance();

			_builder
				.RegisterType<HStaticRuler>()
				.FindConstructorsWith(new BindingFlagsConstructorFinder(BindingFlags.NonPublic))
				.InstancePerDependency();
			_builder
				.RegisterType<HStaticRulerControl>()
				.InstancePerDependency();

			_builder
				.RegisterType<TimeDoublePlottingSystem>()
				.FindConstructorsWith(new BindingFlagsConstructorFinder(BindingFlags.NonPublic))
				.As<TimeDoublePlottingSystem>()
				.As<IScaleGroupManager>()
				.As<IDoubleScalePool>()
				.As<IStaticRulerManager>()
				.As<IDoubleAxesGroupPool>()
				.OnActivated(args => args.Instance.Initialize())
				.SingleInstance();

			_builder
				.RegisterType<DoubleAxesGroupFactory>();

			_builder
				.RegisterType<DragDropManager>()
				.As<IDragDropManager>()
				.SingleInstance();

			_builder
				.RegisterType<TimeDoubleCurveFactory>()
				.SingleInstance();

			_builder
				.RegisterType<OnlineMode>()
				.As<OnlineMode>()
				.As<IOnlineMode>()
				.SingleInstance();

			_builder
				.RegisterType<PanZoomManager>()
				.SingleInstance();
			_builder
				.RegisterType<KeyboardHelper>()
				.SingleInstance();

			_builder
				.RegisterType<TimeScale>()
				.SingleInstance();

			_builder
				.RegisterType<CurveBeltSurface>()
				.SingleInstance();

			_builder
				.RegisterType<HDynamicRuler>()
				.SingleInstance();
			_builder
				.RegisterType<HGridSurface>()
				.SingleInstance();

			_builder
				.RegisterType<VDynamicRuler>()
				.SingleInstance();
			_builder
				.RegisterType<VGridSurface>()
				.SingleInstance();
			
			_builder
				.RegisterType<VStepHelper>()
				.SingleInstance();
			_builder
				.RegisterType<HStepHelper>()
				.SingleInstance();

			_builder
				.RegisterType<HStaticRulerSurface>()
				.As<HStaticRulerSurface>()
				.As<IHStaticRulerSurface>()
				.SingleInstance();
			_builder
				.RegisterType<SectionZoomManager>()
				.SingleInstance();

			_builder
				.RegisterType<RulerManagementSurface>();
			_builder
				.RegisterType<RulerManagementControl>();
			_builder
				.RegisterType<RulerViewModel>()
				.InstancePerDependency();

			_builder
				.RegisterType<GlobalContextMenuSurface>()
				.SingleInstance();
			_builder
				.RegisterType<ContextMenuControl>()
				.As<ContextMenuControl>()
				.As<IContextMenuRegistrar>()
				.FindConstructorsWith(new BindingFlagsConstructorFinder(BindingFlags.NonPublic))
				.SingleInstance();
		}
	}
}
