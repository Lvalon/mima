using HarmonyLib;
using LBoL.Core.SaveData;
using LBoL.Presentation;

namespace lvalonmima.Source.Patches;

// Profile changes: each patch forgets the remembered active-run key (the run's
// owner flag still names its profile, see RunOwnerPatches.cs) and traces how the
// change moves the shop's profile key, and whether the old and new keys have a
// shop in the save.
internal static class ProfileChangeTrace
{
	internal static string KeyAtIndex(int index)
	{
		var gm = GameMaster.Instance;
		if (gm != null && gm._currentSaveIndex == index)
			return LiteProfileSaveData.CurrentProfileKey();
		return GameMaster.TryLoadProfileSaveData(index, out var profile) ? LiteProfileSaveData.ProfileKey(profile) : null;
	}

	internal static void Log(string action, string oldKey, string newKey)
	{
		var saves = MiniTracker.Instance.CustomGrSaveData?.Saves;
		bool oldExists = oldKey != null && saves != null && saves.ContainsKey(oldKey);
		bool newExists = newKey != null && saves != null && saves.ContainsKey(newKey);
		ShopSaveLoader.Trace($"{action}: old key {oldKey ?? "<none>"} (in save: {oldExists}) -> new key {newKey ?? "<none>"} (in save: {newExists})");
	}
}

[HarmonyPatch(typeof(GameMaster), nameof(GameMaster.SetProfileName))]
class GameMaster_SetProfileName_TracePatch
{
	static void Prefix(int index, out string __state)
	{
		__state = ShopSaveLoader.TraceEnabled ? ProfileChangeTrace.KeyAtIndex(index) : null;
	}

	static void Postfix(int index, ProfileSaveData __result, string __state)
	{
		if (!ShopSaveLoader.TraceEnabled)
			return;

		// Renaming the current profile returns null; the renamed profile is CurrentProfile.
		var gm = GameMaster.Instance;
		var newKey = gm != null && gm._currentSaveIndex == index
			? LiteProfileSaveData.CurrentProfileKey()
			: LiteProfileSaveData.ProfileKey(__result);
		ProfileChangeTrace.Log($"SetProfileName(slot {index})", __state, newKey);
	}
}

[HarmonyPatch(typeof(GameMaster), nameof(GameMaster.SelectProfile))]
class GameMaster_SelectProfile_Patch
{
	static void Prefix(out string __state)
	{
		__state = ShopSaveLoader.TraceEnabled ? LiteProfileSaveData.CurrentProfileKey() : null;
	}

	static void Postfix(int? index, string __state)
	{
		MiniTracker.Instance.CustomGrSaveData?.ClearActiveRunProfileKey("SelectProfile");
		if (ShopSaveLoader.TraceEnabled)
			ProfileChangeTrace.Log($"SelectProfile(slot {index?.ToString() ?? "none"})", __state, LiteProfileSaveData.CurrentProfileKey());
	}
}

[HarmonyPatch(typeof(GameMaster), nameof(GameMaster.CreateAndSelectProfile))]
class GameMaster_CreateAndSelectProfile_Patch
{
	// Old key is the profile this slot held before it was overwritten (usually none).
	static void Prefix(int index, out string __state)
	{
		__state = ShopSaveLoader.TraceEnabled ? ProfileChangeTrace.KeyAtIndex(index) : null;
	}

	static void Postfix(int index, ProfileSaveData __result, string __state)
	{
		MiniTracker.Instance.CustomGrSaveData?.ClearActiveRunProfileKey("CreateAndSelectProfile");
		if (ShopSaveLoader.TraceEnabled)
			ProfileChangeTrace.Log($"CreateAndSelectProfile(slot {index})", __state, LiteProfileSaveData.ProfileKey(__result));
	}
}

[HarmonyPatch(typeof(GameMaster), nameof(GameMaster.DeleteProfile))]
class GameMaster_DeleteProfile_Patch
{
	static void Prefix(int index, out string __state)
	{
		__state = ShopSaveLoader.TraceEnabled ? ProfileChangeTrace.KeyAtIndex(index) : null;
	}

	// New key is whatever profile is current afterwards (none if the current one was deleted).
	static void Postfix(int index, string __state)
	{
		MiniTracker.Instance.CustomGrSaveData?.ClearActiveRunProfileKey("DeleteProfile");
		if (ShopSaveLoader.TraceEnabled)
			ProfileChangeTrace.Log($"DeleteProfile(slot {index})", __state, LiteProfileSaveData.CurrentProfileKey());
	}
}
