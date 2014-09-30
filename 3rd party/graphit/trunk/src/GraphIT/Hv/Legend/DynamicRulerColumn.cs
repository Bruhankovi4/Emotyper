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
using System.Windows.Media;
using TechNewLogic.GraphIT.Helper;

namespace TechNewLogic.GraphIT.Hv.Legend
{
	class DynamicRulerColumn : NotifyPropertyChanged
	{
		public DynamicRulerColumn(string name)
		{
			Name = name;
		}

		public string Name { get; private set; }

		private string _value;
		public string Value
		{
			get { return _value; }
			set
			{
				_value = value;
				OnPropertyChanged("Value");
			}
		}

		private Brush _background;
		public Brush Background
		{
			get { return _background; }
			set
			{
				_background = value;
				OnPropertyChanged("Background");
			}
		}
	}
}