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
using System.Text;

namespace TechNewLogic.GraphIT.Helper
{
	/// <summary>
	/// A generic base class for containing event data.
	/// </summary>
	/// <typeparam name="T">The type of event data.</typeparam>
	public class EventArgs<T> : EventArgs
	{
		internal EventArgs(T arg)
		{
			Arg = arg;
		}

		/// <summary>
		/// Gets the event data.
		/// </summary>
		public T Arg { get; private set; }
	}
}
