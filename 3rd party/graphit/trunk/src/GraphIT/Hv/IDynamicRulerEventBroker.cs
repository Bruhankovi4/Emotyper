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

namespace TechNewLogic.GraphIT.Hv
{
	/// <summary>
	/// Das Anzeigen von den DynamicRulers kann aufgrund mehrerer Ereignisse geschehen, nämlich: 
	/// Die Maus ist über dem Zeichenbereich oder über einer Skala.
	///	Je nachdem müssen beide (horizontal und vertikal) oder nur jeweils das h. oder v. Ruler angezeigt werden.
	///	Das Zeichnen selbst übernimmt die jeweilige Ruler-Klasse (HDynamicRuler oder VDynamicRuler).
	///	Die Kommunikation jedoch wird vom IDynamicRulerEventBroker übernommen.
	///	An dieser Klasse können sich die unterschiedlichsten Controls anmelden, welche als "Quelle" 
	/// zum Anziegen dienen (konkret: Die Skalen melden sich an und die GridSurfaces 
	/// (bzw. die InputReference als deren Stellvertreter) melden sich an). Wird die Maus über die Quelle bewegt, 
	/// werden Events ausgelöst, welche vom jeweiligen DynamicRuler abgefangen werden. 
	/// Das DynamicRuler zeichnet sich dann selbst auf seinem GridSurface.
	/// </summary>
	interface IDynamicRulerEventBroker
	{
		event Action ShowRuler;
		event Action HideRuler;
		event EventHandler<DynamicRulerChangedEventArgs> UpdateRuler;

		/// <summary>
		/// Registriert eine Quelle, welche bei Maus-Bewegungen das jeweilige Lineal anzeigt.
		/// </summary>
		/// <param name="inputReference"></param>
		void AddSource(IInputElement inputReference);

		void RemoveSource(IInputElement inputReference);
	}

	interface IHDynamicRulerEventBroker : IDynamicRulerEventBroker
	{
	}

	interface IVDynamicRulerEventBroker : IDynamicRulerEventBroker
	{
	}
}