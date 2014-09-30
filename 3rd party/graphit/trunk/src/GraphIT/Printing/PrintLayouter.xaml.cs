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
using System.Collections.Generic;
using System.Windows.Controls;

namespace TechNewLogic.GraphIT.Printing
{
	/// <summary>
	/// Interaction logic for PrintLayouter.xaml
	/// </summary>
	partial class PrintLayouter
	{
		public PrintLayouter()
		{
			InitializeComponent();
		}

		private readonly List<ContentControl> _holders = new List<ContentControl>();

		internal void SetMainContent(object mainContent)
		{
			mainPrintContentHolder.Content = mainContent;
		}

		internal void AddAdditionalContent(PrintContent content)
		{
			var holder = new ContentControl { Content = content.Visual };
			holder.SetValue(DockPanel.DockProperty, content.Position);
			dockPanel.Children.Insert(0, holder);
			_holders.Add(holder);
		}

		internal void ReleaseContent()
		{
			mainPrintContentHolder.Content = null;
			_holders.ForEachElement(it => it.Content = null);
		}
	}
}
