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
using System.ComponentModel;
using System.Windows;

namespace TechNewLogic.GraphIT.Hv.Vertical
{
	/// <summary>
	/// Interaction logic for DragScaleControl.xaml
	/// </summary>
	partial class DragScaleControl : INotifyPropertyChanged
	{
		public DragScaleControl(IDragDropManager dragDropManager)
		{
			_dragDropManager = dragDropManager;
			dragDropManager.UserMessageChanged += UpdateUserMessage;
			UpdateUserMessage();

			InitializeComponent();
		}

		private void UpdateUserMessage()
		{
			UserMessage = _dragDropManager.UserMessage;
		}

		private readonly IDragDropManager _dragDropManager;

		private string _userMessage;
		public string UserMessage
		{
			get { return _userMessage; }
			set
			{
				_userMessage = value;
				OnPropertyChanged("UserMessage");
				UserMessageVisibility = string.IsNullOrEmpty(value)
					? Visibility.Collapsed 
					: Visibility.Visible;
			}
		}

		private Visibility _userMessageVisibility;
		public Visibility UserMessageVisibility
		{
			get { return _userMessageVisibility; }
			private set
			{
				_userMessageVisibility = value;
				OnPropertyChanged("UserMessageVisibility");
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged(string p)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(p));
		}
	}
}
