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
using System.Reflection;
using Autofac;
using Autofac.Core.Activators.Reflection;

namespace TechNewLogic.GraphIT.Hv.Vertical
{
	class DoubleAxesGroupFactory : IDisposable
	{
		public DoubleAxesGroupFactory(ILifetimeScope container)
		{
			_outerContainer = container;
		}

		private readonly ILifetimeScope _outerContainer;
		private readonly List<ILifetimeScope> _innerScopes = new List<ILifetimeScope>();
		
		private bool _isRelative;
		private DoubleAxisParameter _parameter;

		public DoubleAxesGroup CreateRelativeGroup(AxisFormat axisFormat)
		{
			_isRelative = true;
			_parameter = new DoubleAxisParameter("%", 0, 100, axisFormat);
			return DoCreateAbsoluteGroup();
		}

		public DoubleAxesGroup CreateAbsoluteGroup(DoubleAxisParameter parameter)
		{
			// relative?
			_isRelative = false;
			_parameter = parameter;
			return DoCreateAbsoluteGroup();
		}

		private DoubleAxesGroup DoCreateAbsoluteGroup()
		{
			var scope = _outerContainer.BeginLifetimeScope(RegisterComponents);
			_innerScopes.Add(scope);
			return scope.Resolve<DoubleAxesGroup>();
		}

		// TODO: DestroyGroup und dispose des scopes (wie bei TimeDoubleCurveFactory)

		private void RegisterComponents(ContainerBuilder builder)
		{
			builder
				.RegisterType<DoubleAxesGroup>()
				.FindConstructorsWith(new BindingFlagsConstructorFinder(BindingFlags.NonPublic))
				.OnActivated(args => args.Instance.Initialize())
				.InstancePerLifetimeScope();
			builder
				.Register(c => new ProxyDoubleAxis(
					c.Resolve<DoubleAxisParameter>(),
					c.Resolve<IDrawingAreaInfo>(),
					_isRelative))
				.InstancePerLifetimeScope();
			
			builder
				.RegisterType<DoubleScale>()
				.OnActivated(args => args.Instance.Initialize())
				.InstancePerLifetimeScope();
			if (_parameter.AxisFormat == AxisFormat.Double)
				builder
					.RegisterType<DoubleScaleLabelCreationStrategy>()
					.As<IScaleLabelCreationStrategy>()
					.InstancePerLifetimeScope();
			else
				builder
					.RegisterType<BinaryScaleLabelCreationStrategy>()
					.As<IScaleLabelCreationStrategy>()
					.InstancePerLifetimeScope();
	
			builder
				.RegisterType<ScaleDragDropBehavior>()
				.InstancePerLifetimeScope();
			builder
				.RegisterType<DragScaleControl>()
				.InstancePerDependency();
			builder
				.RegisterType<DragScaleAdorner>()
				.InstancePerDependency();
			builder.RegisterInstance(_parameter);
		}

		public void Dispose()
		{
			_innerScopes.ForEach(it => it.Dispose());
		}
	}
}