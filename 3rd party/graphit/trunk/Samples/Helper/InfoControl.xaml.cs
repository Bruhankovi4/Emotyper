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
// 
// 

using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Helper
{
	/// <summary>
	/// Interaction logic for InfoControl.xaml
	/// </summary>
	public partial class InfoControl
	{
		public InfoControl(Window parent)
		{
			_parent = parent;
			Text = ReadInfoFile();
			InitializeComponent();
		}

		private readonly Window _parent;
		private readonly Grid _layoutGrid = new Grid();
		private UIElement _oldContent;

		public string Text { get; private set; }

		public void Show()
		{
			_oldContent = (UIElement)_parent.Content;
			_parent.Content = _layoutGrid;
			_layoutGrid.Children.Clear();
			_layoutGrid.Children.Add(_oldContent);
			_layoutGrid.Children.Add(this);
		}

		private void Hide()
		{
			_layoutGrid.Children.Clear();
			_parent.Content = _oldContent;
		}

		private string ReadInfoFile()
		{
			var assembly = _parent.GetType().Assembly;
			var infoTextResourceName = assembly
				.GetManifestResourceNames()
				.FirstOrDefault(it => it.EndsWith("InfoText.txt"));
			if (!string.IsNullOrEmpty(infoTextResourceName))
			{
				using (var sr = new StreamReader(assembly.GetManifestResourceStream(infoTextResourceName)))
					return sr.ReadToEnd();
			}
			return "No info text available";
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			Hide();
		}
	}
}
