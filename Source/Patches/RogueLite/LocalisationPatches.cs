using HarmonyLib;
using LBoL.Core;

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
