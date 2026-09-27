using System;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LBoL.Presentation.UI.Panels;
using LBoL.Presentation.I10N;
using LBoL.Core;
using System.Collections.Generic;
using LBoL.Presentation.UI;
using LBoL.Base.Extensions;
using System.Text;
using Object = UnityEngine.Object;
using LBoL.Presentation.InputSystemExtend;
using LBoL.Presentation;
using LBoL.Presentation.UI.ExtraWidgets;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using YamlDotNet.RepresentationModel;
using lvalonmima.Localization;

namespace lvalonmima.Source.Patches;

[HarmonyPatch(typeof(LBoL.Core.Localization), nameof(LBoL.Core.Localization.ReloadCommonAsync))]
public static class LocalisationPatches
{
	public static void Postfix()
	{
		// yaml files can be re-discovered/edited between reloads, so drop the cache first
		LocalisationKeys.ClearCache();

		var currentLocale = LBoL.Core.Localization.CurrentLocale;
		var localizationTable = LBoL.Core.Localization.LocalizationTable;

		foreach (var (key, value) in LocalisationKeys.GetTable(currentLocale))
		{
			if (!localizationTable.ContainsKey(key))
				localizationTable.Add(key, value);
		}

		foreach (var (key, value) in LocalisationKeys.GetTable(Locale.En))
		{
			if (!localizationTable.ContainsKey(key))
				localizationTable.Add(key, value);
		}
	}
}
