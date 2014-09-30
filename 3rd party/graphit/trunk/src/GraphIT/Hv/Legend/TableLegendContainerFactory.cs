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
using System.Windows.Controls;
using Autofac;

namespace TechNewLogic.GraphIT.Hv.Legend
{
	class TableLegendContainerFactory
	{
		public TableLegendContainerFactory(
			CurveDisplay curveDisplay,
			ITable table)
		{
			_curveDisplay = curveDisplay;
			_table = table;

			_builder = new ContainerBuilder();
			RegisterComponents();
		}

		private readonly CurveDisplay _curveDisplay;
		private readonly ITable _table;
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
				.As<ICurvePool>();
			_builder
				.RegisterInstance(_curveDisplay.TimeDoublePlottingSystem)
				.As<IStaticRulerManager>();
			_builder
				.RegisterInstance(_table)
				.As<ITable>();
			_builder
				.Register((c, p) => _table.CreateColumn())
				.As<IColumn>();

			_builder
				.RegisterType<TableLegendViewModel>()
				.As<TableLegendViewModel>()
				.As<IColumnConfiguration>()
				.SingleInstance();
			_builder
				.RegisterType<RefRulerColumnCollection>()
				.SingleInstance();
			_builder
				.RegisterType<RulerColumnCollection>()
				.InstancePerDependency();
			_builder
				.RegisterType<RefRulerColumnInfo>()
				.InstancePerDependency();
		}
	}
}