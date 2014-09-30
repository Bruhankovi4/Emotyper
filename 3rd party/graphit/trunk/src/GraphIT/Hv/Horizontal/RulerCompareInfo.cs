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

namespace TechNewLogic.GraphIT.Hv.Horizontal
{
	// OPT: Evtl. auch in den Container tun?
	class RulerCompareInfo
	{
		public RulerCompareInfo()
		{
			RulerCompareMode = RulerCompareMode.Nothing;
		}

		public RulerCompareInfo(
			double compareValue,
			TimeSpan timeDiff,
			RelativeRulerPosition relativeRulerPosition,
			RulerCompareMode rulerCompareMode,
			HStaticRuler otherRuler)
		{
			switch (rulerCompareMode)
			{
				case RulerCompareMode.ToNextRuler:
					NextRuler = otherRuler;
					break;
				case RulerCompareMode.ToReferenceRuler:
					RefRuler = otherRuler;
					break;
				case RulerCompareMode.Nothing:
					throw new InvalidOperationException("RulerCompareMode cannot be nothing.");
				default:
					throw new ArgumentOutOfRangeException("rulerCompareMode");
			}

			CompareValue = compareValue;
			TimeDiff = timeDiff;
			RelativeRulerPosition = relativeRulerPosition;
			RulerCompareMode = rulerCompareMode;
		}

		public double CompareValue { get; private set; }
		public TimeSpan TimeDiff { get; private set; }
		public RelativeRulerPosition RelativeRulerPosition { get; private set; }
		public RulerCompareMode RulerCompareMode { get; private set; }

		public HStaticRuler NextRuler { get; private set; }
		public HStaticRuler RefRuler { get; private set; }
	}
}