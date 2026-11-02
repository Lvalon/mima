using HarmonyLib;
using LBoL.Core;
using LBoL.Presentation;

namespace lvalonmima.Source.Patches;

// Keep the run's owning profile (a run flag, see LiteProfileSaveData.RunOwnerFlagPrefix)
// and the remembered active-run key in step with run start, Continue, leave and
// profile changes, whether or not Challenger mode is on.

// Every new run is built here, with the profile it is started on.
[HarmonyPatch(typeof(GameRunController), nameof(GameRunController.Create))]
class GameRunController_Create_RunOwnerPatch
{
	static void Postfix(GameRunStartupParameters parameters, GameRunController __result)
	{
		MiniTracker.Instance.CustomGrSaveData?.OnRunStarted(__result, parameters?.UserProfile);
	}
}

// Continue, Restart station and abandon-from-menu all rebuild the run here.
[HarmonyPatch(typeof(GameRunController), nameof(GameRunController.Restore))]
class GameRunController_Restore_RunOwnerPatch
{
	static void Postfix(GameRunController __result)
	{
		MiniTracker.Instance.CustomGrSaveData?.OnRunRestored(__result);
	}
}

// Save & Quit and the end of a finished run both leave through here.
[HarmonyPatch(typeof(GameMaster), nameof(GameMaster.LeaveGameRun))]
class GameMaster_LeaveGameRun_RunOwnerPatch
{
	static void Postfix()
	{
		MiniTracker.Instance.CustomGrSaveData?.ClearActiveRunProfileKey("run leave");
	}
}

[HarmonyPatch(typeof(GameMaster), nameof(GameMaster.SelectProfile))]
class GameMaster_SelectProfile_RunOwnerPatch
{
	static void Postfix()
	{
		MiniTracker.Instance.CustomGrSaveData?.ClearActiveRunProfileKey("SelectProfile");
	}
}

[HarmonyPatch(typeof(GameMaster), nameof(GameMaster.CreateAndSelectProfile))]
class GameMaster_CreateAndSelectProfile_RunOwnerPatch
{
	static void Postfix()
	{
		MiniTracker.Instance.CustomGrSaveData?.ClearActiveRunProfileKey("CreateAndSelectProfile");
	}
}

[HarmonyPatch(typeof(GameMaster), nameof(GameMaster.DeleteProfile))]
class GameMaster_DeleteProfile_RunOwnerPatch
{
	static void Postfix()
	{
		MiniTracker.Instance.CustomGrSaveData?.ClearActiveRunProfileKey("DeleteProfile");
	}
}
