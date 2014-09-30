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
using System.Windows;
using System.Windows.Input;

namespace TechNewLogic.GraphIT.Hv
{
	class DynamicRulerManager : IHDynamicRulerEventBroker, IVDynamicRulerEventBroker
	{
		public DynamicRulerManager(IDrawingAreaInfo drawingAreaInfo)
		{
			_drawingAreaInfo = drawingAreaInfo;
		}

		private readonly IDrawingAreaInfo _drawingAreaInfo;

		public event Action ShowRuler;
		private void OnShowRuler()
		{
			if (ShowRuler != null)
				ShowRuler();
		}

		public event Action HideRuler;
		private void OnHideRuler()
		{
			if (HideRuler != null)
				HideRuler();
		}

		public event EventHandler<DynamicRulerChangedEventArgs> UpdateRuler;
		private void OnUpdateRuler(DynamicRulerChangedEventArgs dynamicRulerChangedEventArgs)
		{
			if (UpdateRuler != null)
				UpdateRuler(this, dynamicRulerChangedEventArgs);
		}

		/// <summary>
		/// Registriert eine Quelle, welche bei Maus-Bewegungen das jeweilige Lineal anzeigt.
		/// </summary>
		/// <param name="inputReference"></param>
		public void AddSource(IInputElement inputReference)
		{
			inputReference.MouseEnter += inputReference_MouseEnter;
			inputReference.MouseLeave += inputReference_MouseLeave;
			inputReference.MouseMove += inputReference_MouseMove;
		}

		public void RemoveSource(IInputElement inputReference)
		{
			inputReference.MouseEnter -= inputReference_MouseEnter;
			inputReference.MouseLeave -= inputReference_MouseLeave;
			inputReference.MouseMove -= inputReference_MouseMove;
		}

		void inputReference_MouseEnter(object sender, MouseEventArgs e)
		{
			OnShowRuler();
		}

		private void inputReference_MouseLeave(object sender, MouseEventArgs e)
		{
			OnHideRuler();
		}

		private void inputReference_MouseMove(object sender, MouseEventArgs e)
		{
			var inputReference = (IInputElement)sender;
			//var position = e.GetPosition(inputReference);
			var position = _drawingAreaInfo.MousePosition;
			OnUpdateRuler(
				new DynamicRulerChangedEventArgs(
					position.X,
					position.Y,
					inputReference));
		}
	}
}
