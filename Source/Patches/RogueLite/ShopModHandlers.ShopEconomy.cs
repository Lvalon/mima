using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
using LBoL.Core.GapOptions;
using LBoL.Core.Randoms;
using LBoL.Core.Stations;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.Cards.Character.Cirno;
using LBoL.EntityLib.EnemyUnits.Character;
using LBoL.EntityLib.EnemyUnits.Normal;
using LBoL.EntityLib.EnemyUnits.Normal.Bats;
using LBoL.EntityLib.EnemyUnits.Normal.Drones;
using LBoL.EntityLib.EnemyUnits.Normal.Guihuos;
using LBoL.EntityLib.EnemyUnits.Normal.Maoyus;
using LBoL.EntityLib.EnemyUnits.Normal.Ravens;
using LBoL.EntityLib.EnemyUnits.Normal.Shenlings;
using LBoL.EntityLib.EnemyUnits.Normal.Yinyangyus;
using LBoL.EntityLib.EnemyUnits.Opponent;
using LBoL.EntityLib.Exhibits.Common;
using LBoL.EntityLib.StatusEffects.Enemy.Seija;
using LBoL.EntityLib.StatusEffects.Marisa;
using LBoL.Presentation;
using LBoL.Presentation.UI.Panels;
using LBoLEntitySideloader.CustomHandlers;
using lvalonmima.Cards;
using lvalonmima.Exhibits;
using lvalonmima.StatusEffects;

namespace lvalonmima.Source.Patches;

public partial class ShopModHandlers
{
	// Panels that started an UpgradeCard flow and are awaiting confirmation.
	internal static readonly HashSet<GapOptionsPanel> UpgradeStartedPanels = [];

	// stored on Stage.ExtraFlags: those are restored before Restore() calls FinishStation, GameRunController.ExtraFlags are not
	private const string FreeChoiceStageFlag = "lvalonmima.freechoice";

	private const string RemoveDiscountAppliedPrefix = "shop.remove.applied:";

	internal static int GetUpgradeDiscount(GameRunController gameRun)
	{
		var shop = MiniTracker.Instance?.CustomGrSaveData?.GetShopForCurrentProfile();
		if (shop == null || !shop.ChallengerModeEnabled)
			return 0;
		ShopItem item = shop.GetItem("discount.upgrade");
		if (item == null || item.CurrentTier <= 0)
			return 0;
		return 25 * item.CurrentTier;
	}

	internal static bool HasTeaSync()
	{
		var shop = MiniTracker.Instance?.CustomGrSaveData?.GetShopForCurrentProfile();
		if (shop == null || !shop.ChallengerModeEnabled)
			return false;
		ShopItem item = shop.GetItem("feature.teasync");
		return item != null && item.CurrentTier > 0;
	}

	internal static int GetGapplePerHeal()
	{
		var shop = MiniTracker.Instance?.CustomGrSaveData?.GetShopForCurrentProfile();
		if (shop == null || !shop.ChallengerModeEnabled)
			return -1;
		ShopItem item = shop.GetItem("feature.gapple");
		if (item == null || item.CurrentTier <= 0)
			return -1;
		return item.Initial + (item.Delta * (item.CurrentTier - 1));
	}

	internal static int GetSponsorGold()
	{
		var shop = MiniTracker.Instance?.CustomGrSaveData?.GetShopForCurrentProfile();
		if (shop == null || !shop.ChallengerModeEnabled)
			return -1;
		ShopItem item = shop.GetItem("feature.sponsor");
		if (item == null || item.CurrentTier <= 0)
			return -1;
		return item.Initial + item.Delta * item.CurrentTier;
	}

	internal static bool HasFreeChoice()
	{
		var shop = MiniTracker.Instance?.CustomGrSaveData?.GetShopForCurrentProfile();
		if (shop == null || !shop.ChallengerModeEnabled)
			return false;
		ShopItem item = shop.GetItem("alter.freechoice");
		return item != null && item.CurrentTier > 0;
	}

	internal static bool IsFreeChoiceRun(GameRunController gameRun)
	{
		return gameRun?.Stages?.Any(s => s.ExtraFlags.Contains(FreeChoiceStageFlag)) == true;
	}

	// sticky for the run: never removed, so shop changes can't flip it mid-run
	internal static void EnsureFreeChoiceFlag(GameRunController gameRun)
	{
		if (gameRun?.Stages == null || !HasFreeChoice())
			return;
		foreach (Stage stage in gameRun.Stages)
			stage.ExtraFlags.Add(FreeChoiceStageFlag);
	}

	internal static string FreeChoiceName()
	{
		string key = $"{LocalisationKeys.ShopPrefix}{LocalisationKeys.AlterPrefix}freechoice";
		if (LocalisationKeys.TryGet(key, out string name))
			return name;
		return key;
	}

	internal static int GetAppliedRemoveDiscount(GameRunController gameRun)
	{
		foreach (string payload in RunFlagPayloads(gameRun, RemoveDiscountAppliedPrefix))
		{
			if (int.TryParse(payload, out int parsed))
				return parsed;
		}
		return 0;
	}

	internal static void SetAppliedRemoveDiscount(GameRunController gameRun, int value)
	{
		if (gameRun?.ExtraFlags == null)
			return;
		string existing = gameRun.ExtraFlags.FirstOrDefault(flag =>
			flag.StartsWith(RemoveDiscountAppliedPrefix, StringComparison.Ordinal));
		if (!string.IsNullOrEmpty(existing))
			gameRun.ExtraFlags.Remove(existing);
		gameRun.ExtraFlags.Add(RemoveDiscountAppliedPrefix + value);
	}

}
[HarmonyPatch(typeof(GameRunController), nameof(GameRunController.UpgradeDeckCardPrice), MethodType.Getter)]
class GameRunController_UpgradeDeckCardPrice_Patch
{
	static void Postfix(GameRunController __instance, ref int __result)
	{
		int discount = ShopModHandlers.GetUpgradeDiscount(__instance);
		if (discount <= 0)
			return;
		int adjusted = __result - discount;
		__result = adjusted < 0 ? 0 : adjusted;
	}

}

[HarmonyPatch(typeof(GapOptionsPanel), nameof(GapOptionsPanel.UpgradeCard))]
class GapOptionsPanel_UpgradeCard_Flag_Patch
{
	static void Prefix(GapOptionsPanel __instance, GapOption option)
	{
		ShopModHandlers.UpgradeStartedPanels.Add(__instance);
	}

}

[HarmonyPatch(typeof(GapOptionsPanel), nameof(GapOptionsPanel.SelectedAndHide))]
class GapOptionsPanel_SelectedAndHide_TeaSync_Patch
{
	static void Postfix(GapOptionsPanel __instance)
	{
		if (!ShopModHandlers.UpgradeStartedPanels.Remove(__instance))
			return;

		if (ShopModHandlers.GetSponsorGold() > 0)
			GameMaster.Instance.CurrentGameRun.GainMoney(ShopModHandlers.GetSponsorGold(), true);

		if (!ShopModHandlers.HasTeaSync())
			return;
		GapStation gapStation = __instance._gapStation;
		if (gapStation == null)
			return;
		DrinkTea drinkTea = gapStation.GapOptions?.OfType<DrinkTea>().FirstOrDefault();
		if (drinkTea == null)
			return;
		gapStation.DrinkTea(drinkTea);
	}

}

// free choice: answer the vanilla true-ending checks from the run's stage flag instead of faking a TrueEndingProvider
[HarmonyPatch(typeof(GameRunController), nameof(GameRunController.CanEnterTrueEnding))]
class GameRunController_CanEnterTrueEnding_FreeChoice_Patch
{
	static void Postfix(GameRunController __instance, ref bool __result)
	{
		if (ShopModHandlers.IsFreeChoiceRun(__instance))
			__result = true;
	}

}

[HarmonyPatch(typeof(GameRunController), nameof(GameRunController.IsTrueEndingBlocked))]
class GameRunController_IsTrueEndingBlocked_FreeChoice_Patch
{
	static void Postfix(GameRunController __instance, ref bool __result)
	{
		if (ShopModHandlers.IsFreeChoiceRun(__instance))
			__result = false;
	}

}

[HarmonyPatch(typeof(LBoL.Core.Dialogs.DialogFunctions), nameof(LBoL.Core.Dialogs.DialogFunctions.HasTrueEndProvider))]
class DialogFunctions_HasTrueEndProvider_FreeChoice_Patch
{
	static void Postfix(LBoL.Core.Dialogs.DialogFunctions __instance, ref bool __result)
	{
		if (ShopModHandlers.IsFreeChoiceRun(__instance.GetGameRun()))
			__result = true;
	}

}

[HarmonyPatch(typeof(LBoL.Core.Dialogs.DialogFunctions), nameof(LBoL.Core.Dialogs.DialogFunctions.IsTrueEndBlocked))]
class DialogFunctions_IsTrueEndBlocked_FreeChoice_Patch
{
	static void Postfix(LBoL.Core.Dialogs.DialogFunctions __instance, ref bool __result)
	{
		if (ShopModHandlers.IsFreeChoiceRun(__instance.GetGameRun()))
			__result = false;
	}

}

[HarmonyPatch(typeof(LBoL.Core.Dialogs.DialogFunctions), nameof(LBoL.Core.Dialogs.DialogFunctions.TrueEndProviderName))]
class DialogFunctions_TrueEndProviderName_FreeChoice_Patch
{
	// vanilla does TrueEndingProviders.First(), which throws (and hangs the dialog) when the set is empty
	static bool Prefix(LBoL.Core.Dialogs.DialogFunctions __instance, ref string __result)
	{
		GameRunController gameRun = __instance.GetGameRun();
		if (!ShopModHandlers.IsFreeChoiceRun(gameRun) || gameRun.TrueEndingProviders.Count > 0)
			return true;
		__result = ShopModHandlers.FreeChoiceName();
		return false;
	}

}

// Checklist G16 ("discount.sc" is "Spell Card for Breakfast" -- reduce the
// Power cost of the Spell Card by {0}% -- see DirResources/RogueliteShop*.yaml).
// Was implemented by mutating gameRun.Player.Us.Config.PowerCost directly
// (see the removed code this replaces, in ShopModHandlers.StationEntered's
// "every stage" loop). That was wrong: UltimateSkill.Config comes from
// UltimateSkillConfig.FromId(id), which returns from a private static
// Dictionary populated once at game startup (confirmed via decompile of
// LBoL.ConfigData.dll) -- every UltimateSkill instance of the same skill,
// across every game run for the lifetime of the process, shares that one
// object. Mutating it was self-healing in the common case (StationEntered
// unconditionally reset it to a cached "true base" on every station entry,
// before checking Challenger Mode), but left a real window where the
// shared object stayed discounted from a just-finished Challenger run
// until the next run's first station entry corrected it -- e.g. a
// character-select or run-start screen reading the same skill's cost in
// that window would show the stale discounted value.
//
// Fixed by computing the discount at the read site instead, via a Postfix
// on UltimateSkill's PowerCost getter -- this is the one property every
// gameplay-relevant read goes through (affordability checks, the
// insufficient-power throw guard, and UseUsAction all read us.PowerCost,
// confirmed via decompile of LBoL.Core.dll). The shared Config object is
// never written to, so there is nothing left to leak across runs.
[HarmonyPatch(typeof(UltimateSkill), nameof(UltimateSkill.PowerCost), MethodType.Getter)]
class UltimateSkill_PowerCost_Discount_Patch
{
	static void Postfix(ref int __result)
	{
		var shop = MiniTracker.Instance?.CustomGrSaveData?.GetShopForCurrentProfile();
		if (shop == null || !shop.ChallengerModeEnabled)
			return;

		ShopItem item = shop.GetItem("discount.sc");
		if (item == null || item.CurrentTier <= 0)
			return;

		double discountMultiplier = Math.Max(0.0, 1.0 - (0.1 * item.CurrentTier));
		__result = toolbox.Round(__result * discountMultiplier);
	}
}
