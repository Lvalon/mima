using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using HarmonyLib;
using LBoL.Core;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
using LBoL.Core.Stations;
using LBoL.Presentation;
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.ExtraWidgets;
using LBoL.Presentation.UI.Panels;
using LBoL.Presentation.UI.Widgets;
using lvalonmima.Cards;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using lvalonmima.Exhibits;
using lvalonmima.Source.Patches;
using System.Linq;
using LBoL.Base;

namespace lvalonmima.Patches.Exhibits;

[HarmonyPatch]
public static class ExhibitWidget_Exquesting_Patch
{
	[HarmonyPatch(typeof(SystemBoard), nameof(SystemBoard.CreateExhibitWidget))]
	[HarmonyPostfix]
	public static void HijackExhibitClick(Exhibit exhibit, ExhibitWidget __result)
	{
		if (exhibit is exquesting exquestingExhibit)
		{

			FieldInfo fieldInfo = AccessTools.Field(typeof(ExhibitWidget), "ExhibitClicked");
			fieldInfo?.SetValue(__result, null);
			__result.ExhibitClicked -= ExquestingUiLauncher.OnExhibitClicked;
			__result.ExhibitClicked += ExquestingUiLauncher.OnExhibitClicked;
		}
	}
}

[HarmonyPatch]
public static class ExquestingShopPanelPatches
{
	[HarmonyPatch(typeof(ShopPanel), nameof(ShopPanel.OnShowing))]
	[HarmonyPostfix]
	public static void OnShowing(ShopPanel __instance)
	{
		if (__instance.GetComponent<ExquestingPanelMarker>() != null)
		{
			ExquestingPanelController.OnShow(__instance);
			ExquestingUiLauncher.RefreshCustomSoldOutSlots(__instance);
		}
	}

	[HarmonyPatch(typeof(ShopPanel), nameof(ShopPanel.OnHiding))]
	[HarmonyPostfix]
	public static void OnHiding(ShopPanel __instance)
	{
		if (__instance.GetComponent<ExquestingPanelMarker>() != null)
		{
		}
	}

	[HarmonyPatch(typeof(ShopPanel), nameof(ShopPanel.OnHided))]
	[HarmonyPrefix]
	public static bool OnHided(ShopPanel __instance)
	{
		if (__instance.GetComponent<ExquestingPanelMarker>() != null)
		{
			if (__instance.LockedByInteractionMinimized)
			{
				return true;
			}

			ExquestingPanelController.OnHidden(__instance);
			return false;
		}

		return true;
	}

	[HarmonyPatch(typeof(SettingPanel), nameof(SettingPanel.OnShowing))]
	[HarmonyPrefix]
	public static bool BlockSettingPanelWhileExquestingOpen()
	{
		return !ExquestingPanelController.IsExquestingPanelVisible();
	}
}

[HarmonyPatch]
public static class ExquestingVnPanelPatches
{
	[HarmonyPatch(typeof(VnPanel), nameof(VnPanel.OnShowing))]
	[HarmonyPostfix]
	public static void OnShowing(VnPanel __instance)
	{
		ExquestingPanelController.ApplyPendingNextButtonState(__instance);
	}
}

[HarmonyPatch]
public static class ExquestingShopCardPatches
{
	[HarmonyPatch(typeof(ShopCard), nameof(ShopCard.OnPointerClick))]
	[HarmonyPrefix]
	public static bool OnPointerClickPrefix(ShopCard __instance, PointerEventData eventData)
	{
		if (__instance == null || __instance.GetComponentInParent<ExquestingPanelMarker>() == null)
			return true;

		if (eventData == null)
			return true;

		ShopPanel panel = __instance.ShopPanel;
		if (panel == null)
			return false;

		if (eventData.button == PointerEventData.InputButton.Right)
			return false;

		if (eventData.button != PointerEventData.InputButton.Left)
			return false;

		if (!ExquestingPanelController.TryLockInteraction(panel))
			return false;

		panel.StartCoroutine(ExquestingUiLauncher.CoHandleCustomCardClick(panel, __instance.Index));
		return false;
	}

	[HarmonyPatch(typeof(ShopStation), nameof(ShopStation.RefreshAfterBought))]
	[HarmonyPrefix]
	public static bool RefreshAfterBoughtPrefix(ShopStation __instance)
	{
		return !ExquestingPanelController.IsExquestingStation(__instance);
	}
}
