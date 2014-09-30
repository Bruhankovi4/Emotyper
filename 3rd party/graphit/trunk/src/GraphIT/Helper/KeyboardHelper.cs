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
using System.Windows;
using System.Windows.Input;

namespace TechNewLogic.GraphIT.Helper
{
	class KeyboardHelper : IDisposable
	{
		public KeyboardHelper(InputReference inputReference)
		{
			if (inputReference.InputElement.IsLoaded)
				DoHookKeyboardDown();
			else
				inputReference.InputElement.Loaded += (s, e) => DoHookKeyboardDown();
		}

		private readonly List<Action<KeyEventArgs>> _keyDownHandler = new List<Action<KeyEventArgs>>();

		private IInputElement _rootVisual;

		public void HookKeyDown(Action<KeyEventArgs> keyDownHandler)
		{
			_keyDownHandler.Add(keyDownHandler);
		}

		public void UnhookKeyDown(Action<KeyEventArgs> keyDownHandler)
		{
			_keyDownHandler.Remove(keyDownHandler);
		}

		private void DoHookKeyboardDown()
		{
			if (Keyboard.PrimaryDevice.ActiveSource == null)
				return;
			_rootVisual = Keyboard.PrimaryDevice.ActiveSource.RootVisual as IInputElement;
			if (_rootVisual == null)
				return;
			_rootVisual.PreviewKeyDown += _rootVisual_PreviewKeyDown;
		}

		void _rootVisual_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			_keyDownHandler.ForEach(it => it(e));
		}

		#region Implementation of IDisposable

		public void Dispose()
		{
			if (_rootVisual == null)
				return;
			_rootVisual.PreviewKeyDown -= _rootVisual_PreviewKeyDown;
		}

		#endregion
	}
}
