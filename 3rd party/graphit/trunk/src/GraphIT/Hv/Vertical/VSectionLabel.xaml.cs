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
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace TechNewLogic.GraphIT.Hv.Vertical
{
	/// <summary>
	/// Interaction logic for VSectionLabel.xaml
	/// </summary>
	partial class VSectionLabel : INotifyPropertyChanged
	{
		public VSectionLabel(double height, bool clip)
		{
			_clip = clip;
			Height = height;
			InitializeComponent();
		}

		private readonly bool _clip;

		private string _text;
		public string Text
		{
			get { return _text; }
			internal set
			{
				_text = value;
				OnPropertyChanged("Text");
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		// Deshalb, weil ansonsten das unterste Label abgeschnitten würde
		protected override Geometry GetLayoutClip(Size layoutSlotSize)
		{
			return _clip ? base.GetLayoutClip(layoutSlotSize) : null;
		}
	}
}
