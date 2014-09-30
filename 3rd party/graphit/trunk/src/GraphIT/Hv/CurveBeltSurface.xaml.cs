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
using TechNewLogic.GraphIT.Helper;

namespace TechNewLogic.GraphIT.Hv
{
	/// <summary>
	/// Interaction logic for CurveBeltSurface.xaml
	/// </summary>
	partial class CurveBeltSurface
	{
		public CurveBeltSurface(
			ICurvePool curvePool,
			TimeDoubleCurveFactory curveFactory)
		{
			InitializeComponent();

			_curvePool = curvePool;
			_curveFactory = curveFactory;
			_curvePool.CurveAdded += RegisterCurve;
			_curvePool.CurveRemoved += DeregisterCurve;
		}

		private readonly ICurvePool _curvePool;
		private readonly TimeDoubleCurveFactory _curveFactory;

		private readonly Dictionary<TimeDoubleCurve, BeltRegistration> _curves
			= new Dictionary<TimeDoubleCurve, BeltRegistration>();

		private void RegisterCurve(object sender, EventArgs<Curve> curve)
		{
			var tdc = (curve.Arg as TimeDoubleCurve);
			if (tdc == null)
				return;

			var boundaryRegistration = _curveFactory.CreateBoundaryRegistration(tdc);
			container.Children.Add(boundaryRegistration.BeltControl);
			_curves.Add(tdc, boundaryRegistration);
		}

		private void DeregisterCurve(object sender, EventArgs<Curve> curve)
		{
			var tdc = (curve.Arg as TimeDoubleCurve);
			if (tdc == null)
				return;

			var boundaryRegistration = AssertCurveExixts(tdc);
			container.Children.Remove(boundaryRegistration.BeltControl);
			_curves.Remove(tdc);
		}

		private BeltRegistration AssertCurveExixts(TimeDoubleCurve curve)
		{
			BeltRegistration beltRegistration;
			if (!_curves.TryGetValue(curve, out beltRegistration))
				throw new Exception("The curve is not registered at the boundaries surfeca.");
			return beltRegistration;
		}
	}
}
