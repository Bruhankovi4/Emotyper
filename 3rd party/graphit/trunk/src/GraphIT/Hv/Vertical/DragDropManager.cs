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

namespace TechNewLogic.GraphIT.Hv.Vertical
{
	class DragDropManager : IDragDropManager
	{
		public event Action Dragging;
		public event Action Releasing;

		public void Drag(IVScale scale)
		{
			DraggedScale = scale;
		}

		public void ReleaseDrag()
		{
			DraggedScale = null;
		}

		public event Action UserMessageChanged;

		private string _userMessage;
		public string UserMessage
		{
			get { return _userMessage; }
			set
			{
				_userMessage = value;
				if (UserMessageChanged != null)
					UserMessageChanged();
			}
		}

		public bool IsDragging
		{
			get { return DraggedScale != null; }
		}

		private IVScale _draggedScale;
		public IVScale DraggedScale
		{
			get { return _draggedScale; }
			private set
			{
				if (value != null && Dragging != null)
				{
					_draggedScale = value;
					Dragging();
				}
				if (value == null && Releasing != null)
				{
					Releasing();
					_draggedScale = value;
				}
			}
		}
	}
}