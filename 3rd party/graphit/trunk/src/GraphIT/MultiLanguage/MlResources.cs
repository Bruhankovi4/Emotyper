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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading;
using System.Xml.Linq;

namespace TechNewLogic.GraphIT.MultiLanguage
{
	static class MlResources
	{
		static MlResources()
		{
			const string resourceKey = "TechNewLogic.GraphIT.MultiLanguage.MultiLanguageDB.xml";
			using (var s = typeof(MlResources).Assembly.GetManifestResourceStream(resourceKey))
			{
				if (s == null)
					throw new Exception("Could not find the multi language DB file.");

				using (var sr = new StreamReader(s))
					_document = XDocument.Load(sr);
			}
		}

		private static readonly XDocument _document;

		public static string CenterHere { get { return Get("CenterHere"); } }
		public static string FitCurvesToScreen { get { return Get("FitCurvesToScreen"); } }
		public static string ResetBounds { get { return Get("ResetBounds"); } }
		public static string From { get { return Get("From"); } }
		public static string GroupAbsolute { get { return Get("GroupAbsolute"); } }
		public static string GroupRelative { get { return Get("GroupRelative"); } }
		public static string NoData { get { return Get("NoData"); } }
		public static string NoRulersPresent { get { return Get("NoRulersPresent"); } }
		public static string PleaseWait { get { return Get("PleaseWait"); } }
		public static string Remove { get { return Get("Remove"); } }
		public static string Rulers { get { return Get("Rulers"); } }
		public static string To { get { return Get("To"); } }
		public static string ToggleReference { get { return Get("ToggleReference"); } }
		public static string UngroupAxes { get { return Get("UngroupAxes"); } }
		public static string Uom { get { return Get("Uom"); } }
		public static string Value { get { return Get("Value"); } }
		public static string Delete { get { return Get("Delete"); } }

		public static string SelectAggregates { get { return Get("SelectAggregates"); } }
		public static string RulerShortText { get { return Get("RulerShortText"); } }
		public static string DeltaY { get { return Get("DeltaY"); } }
		public static string DeltaT { get { return Get("DeltaT"); } }
		public static string Min { get { return Get("Min"); } }
		public static string Max { get { return Get("Max"); } }
		public static string Avg { get { return Get("Avg"); } }

		public static string DocumentIsBeingGenerated { get { return Get("DocumentIsBeingGenerated"); } }
		public static string PaperOrientationPortrait { get { return Get("PaperOrientationPortrait"); } }
		public static string PaperOrientationLandscape { get { return Get("PaperOrientationLandscape"); } }
		public static string CannotCreateDocument { get { return Get("CannotCreateDocument"); } }

		public static string PrintPreview { get { return Get("PrintPreview"); } }
		public static string Print { get { return Get("Print"); } }
		public static string ZoomIn { get { return Get("ZoomIn"); } }
		public static string ZoomOut { get { return Get("ZoomOut"); } }
		public static string ActualSize { get { return Get("ActualSize"); } }
		public static string FitToWidth { get { return Get("FitToWidth"); } }
		public static string WholePage { get { return Get("WholePage"); } }
		public static string TwoPages { get { return Get("TwoPages"); } }
		public static string PageBorder { get { return Get("PageBorder"); } }

		private static string Get(string key)
		{
			const string undefinedValue = "$UNDEFINED";
			if (_document.Root != null)
			{
				var element = _document.Root.Elements(key).FirstOrDefault();
				if (element == null)
					return undefinedValue;
				var currentCulture = Thread.CurrentThread.CurrentCulture;

				var matchingLanguageElement = element.Elements(currentCulture.Name).FirstOrDefault();
				if (matchingLanguageElement != null)
					return matchingLanguageElement.Value;
				var defaultLanguageElement = element.Elements("en-US").FirstOrDefault();
				if (defaultLanguageElement != null)
					return defaultLanguageElement.Value;
				return undefinedValue;
			}
			return undefinedValue;
		}
	}
}
