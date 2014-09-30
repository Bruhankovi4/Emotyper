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

namespace TechNewLogic.GraphIT.Printing
{
	class ViewDrawingState : IViewDrawingState
	{
		public ViewDrawingState(IPrintingRedrawRequest redrawRequest)
		{
			_redrawRequest = redrawRequest;
		}

		private readonly IPrintingRedrawRequest _redrawRequest;

		public void EnsureViewUpToDate(Action successCallback, Action timeoutCallback, TimeSpan timeout)
		{
			_redrawRequest.RaiseRedrawRequest(
				b =>
				{
					if (b)
						successCallback();
					else
						timeoutCallback();
				},
				timeout);
		}
	}
}