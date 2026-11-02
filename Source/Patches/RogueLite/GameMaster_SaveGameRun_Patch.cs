using System.Collections.Generic;
using HarmonyLib;
using LBoL.Core.SaveData;
using LBoL.Presentation;

namespace lvalonmima.Source.Patches;

// GameRunController.Save hands the live run's ExtraFlags set to the save data by
// reference, and GameMaster.SaveGameRun keeps that object as GameRunSaveData,
// which Restart station restores from. Flags changed after the save (quest
// rewards, accept/abandon) would then leak into the restart while the deck,
// HP and money rewind. The file has already been written at this point, so
// giving the kept snapshot its own copy makes it match what is on disk, and
// Restart station rewinds the same way quit-and-Continue does.
// Runs last so other mods' postfixes on SaveGameRun still see the original set.
[HarmonyPatch(typeof(GameMaster), nameof(GameMaster.SaveGameRun))]
[HarmonyPriority(Priority.Last)]
class GameMaster_SaveGameRun_Patch
{
	static void Postfix(GameMaster __instance, GameRunSaveData data)
	{
		// SaveGameRun returns early (keeping nothing) when no save slot is set.
		if (data == null || data.ExtraFlags == null || !ReferenceEquals(__instance.GameRunSaveData, data))
			return;

		data.ExtraFlags = new HashSet<string>(data.ExtraFlags, data.ExtraFlags.Comparer);

		if (ShopSaveLoader.Once("SaveGameRun flag copy"))
			ShopSaveLoader.Trace($"GameMaster.SaveGameRun: the kept game-run snapshot now holds its own copy of ExtraFlags ({data.ExtraFlags.Count} flags), so Restart station rewinds quest flags like Continue. (Logged once per session.)");
	}
}
