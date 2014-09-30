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
using System.ComponentModel;
using System.Linq;
using System.Text;

using TechNewLogic.GraphIT.Helper;

namespace TechNewLogic.GraphIT
{
	/// <summary>
	/// Provides textual information for a <see cref="Curve"/>.
	/// </summary>
	public sealed class CurveDescription : NotifyPropertyChanged
	{
		internal CurveDescription() { }

		private string _descriptionText1;
		/// <summary>
		/// 1st description text.
		/// </summary>
		public string DescriptionText1
		{
			get { return _descriptionText1; }
			set
			{
				_descriptionText1 = value;
				OnPropertyChanged("DescriptionText1");
			}
		}

		private string _descriptionText2;
		/// <summary>
		/// 2nd description text.
		/// </summary>
		public string DescriptionText2
		{
			get { return _descriptionText2; }
			set
			{
				_descriptionText2 = value;
				OnPropertyChanged("DescriptionText2");
			}
		}

		private string _descriptionText3;
		/// <summary>
		/// 3rd description text.
		/// </summary>
		public string DescriptionText3
		{
			get { return _descriptionText3; }
			set
			{
				_descriptionText3 = value;
				OnPropertyChanged("DescriptionText3");
			}
		}

		private string _descriptionText4;
		/// <summary>
		/// 4th description text.
		/// </summary>
		public string DescriptionText4
		{
			get { return _descriptionText4; }
			set
			{
				_descriptionText4 = value;
				OnPropertyChanged("DescriptionText4");
			}
		}

		private string _descriptionText5;
		/// <summary>
		/// 5th description text.
		/// </summary>
		public string DescriptionText5
		{
			get { return _descriptionText5; }
			set
			{
				_descriptionText5 = value;
				OnPropertyChanged("DescriptionText5");
			}
		}
	}
}
