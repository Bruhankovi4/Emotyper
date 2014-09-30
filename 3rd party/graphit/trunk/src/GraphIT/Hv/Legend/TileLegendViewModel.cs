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
using TechNewLogic.GraphIT.Helper;
using TechNewLogic.GraphIT.Hv.Horizontal;

namespace TechNewLogic.GraphIT.Hv.Legend
{
	sealed class TileLegendViewModel : IDisposable
	{
		public TileLegendViewModel(
			ICurvePool curvePool,
			TimeAxis timeAxis)
		{
			_timeAxis = timeAxis;

			_curvePool = curvePool;
			_curvePool.CurveAdded += curvePool_CurveAdded;
			_curvePool.CurveRemoved += curvePool_CurveRemoved;

			_curvePool.Curves
				.OfType<TimeDoubleCurve>()
				.ForEachElement(it => _curves.Add(new TileLegendItemViewModel(it, _timeAxis)));
		}

		private ICurvePool _curvePool;
		private readonly TimeAxis _timeAxis;

		private readonly ObservableCollection<TileLegendItemViewModel> _curves
			= new ObservableCollection<TileLegendItemViewModel>();
		public IEnumerable<TileLegendItemViewModel> Curves { get { return _curves; } }

		void curvePool_CurveAdded(object sender, EventArgs<Curve> curve)
		{
			var tdc = (curve.Arg as TimeDoubleCurve);
			if (tdc == null)
				return;
			_curves.Add(new TileLegendItemViewModel(tdc, _timeAxis));
		}

		void curvePool_CurveRemoved(object sender, EventArgs<Curve> curve)
		{
			var tdc = (curve.Arg as TimeDoubleCurve);
			if (tdc == null)
				return;

			var item = _curves.FirstOrDefault(it => it.Curve == tdc);
			if (item != null)
				_curves.Remove(item);
		}

		public void Dispose()
		{
			_curvePool.CurveAdded -= curvePool_CurveAdded;
			_curvePool.CurveRemoved -= curvePool_CurveRemoved;
			_curves.ForEachElement(it => it.Dispose());

			// Das muss gemacht werden, weil aus irgend einem WPF internen Grund nicht richtig abgebaut wird
			_curvePool = null;
		}
	}
}
