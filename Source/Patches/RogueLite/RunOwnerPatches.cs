using HarmonyLib;
using LBoL.Core;
using LBoL.Presentation;

namespace lvalonmima.Source.Patches;

// Keep the run's owning profile (a run flag, see LiteProfileSaveData.RunOwnerFlagPrefix)
// and the remembered active-run key in step with run start, Continue, leave and
// profile changes, whether or not Challenger mode is on. Continue, Restart station
// and abandon-from-menu rebuild the run in GameRunController.Restore, whose
// postfix (GameRunController_Restore_Patch in ShopSaveLoader.cs) calls
// OnRunRestored; profile changes are in ProfileChangeTrace.cs.

// Every new run is built here, with the profile it is started on.
[HarmonyPatch(typeof(GameRunController), nameof(GameRunController.Create))]
class GameRunController_Create_RunOwnerPatch
{
	static void Postfix(GameRunStartupParameters parameters, GameRunController __result)
	{
		MiniTracker.Instance.CustomGrSaveData?.OnRunStarted(__result, parameters?.UserProfile);
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
