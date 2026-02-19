using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using HarmonyLib;
using LBoL.Core;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
using LBoL.Core.Stations;
using LBoL.Presentation;
using LBoL.Presentation.InputSystemExtend;
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
using LBoL.Core.Randoms;
using LBoL.Core.StatusEffects;

namespace lvalonmima.Patches.Exhibits
{
	[HarmonyPatch]
	public static class ExhibitWidget_Exquesting_Patch
	{
		[HarmonyPatch(typeof(SystemBoard), nameof(SystemBoard.CreateExhibitWidget))]
		[HarmonyPostfix]
		public static void HijackExhibitClick(Exhibit exhibit, ExhibitWidget __result)
		{
			if (exhibit is exquesting exquestingExhibit)
			{
				BepinexPlugin.log.LogInfo($"[EXQUESTING SAVE] CreateExhibitWidget using runtime state pending={exquestingExhibit.PendingQuestProgress.Count}, req={exquestingExhibit.QuestRequirements.Count}, rolled={exquestingExhibit.RolledQuestCards.Count}, soldOut={exquestingExhibit.SoldOutQuestSlots.Count}");

				BepinexPlugin.log.LogInfo("[EXQUESTING UI] Detected exquesting being added to UI. Hijacking its click event.");
				FieldInfo fieldInfo = AccessTools.Field(typeof(ExhibitWidget), "ExhibitClicked");
				fieldInfo?.SetValue(__result, null);
				__result.ExhibitClicked -= ExquestingUiLauncher.OnExhibitClicked;
				__result.ExhibitClicked += ExquestingUiLauncher.OnExhibitClicked;
			}
		}
	}

	public sealed class ExquestingPanelMarker : MonoBehaviour
	{
	}

	public sealed class ExquestingCancelInputHandler : MonoBehaviour, IInputActionHandler
	{
		private bool _registered;

		private void OnEnable()
		{
			if (_registered)
			{
				return;
			}

			UiManager.PushActionHandler(this);
			_registered = true;
		}

		private void OnDisable()
		{
			if (!_registered)
			{
				return;
			}

			UiManager.PopActionHandler(this);
			_registered = false;
		}

		void IInputActionHandler.OnCancel()
		{
			ShopPanel panel = GetComponent<ShopPanel>();
			if (panel == null || panel.GetComponent<ExquestingPanelMarker>() == null || !panel.IsVisible)
			{
				return;
			}

			if (!panel.LockedByInteractionMinimized)
			{
				panel.Hide();
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
				BepinexPlugin.log.LogDebug("[EXQUESTING UI] ShopPanel hide transition started; deferring custom layout restore until OnHided.");
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
			if (!ExquestingPanelController.IsExquestingPanelVisible())
			{
				return true;
			}

			BepinexPlugin.log.LogInfo("[EXQUESTING UI] Blocked SettingPanel open while Exquesting shop is active.");
			return false;
		}
	}

	public static class ExquestingPanelController
	{
		private static readonly Dictionary<GameObject, bool> _originalActiveStates = new Dictionary<GameObject, bool>();
		private static readonly Dictionary<GameObject, bool> _originalChildActiveStates = new Dictionary<GameObject, bool>();
		private static readonly Dictionary<RectTransform, Vector2> _originalPositions = new Dictionary<RectTransform, Vector2>();
		private static readonly Dictionary<Behaviour, bool> _originalComponentStates = new Dictionary<Behaviour, bool>();
		private static readonly Dictionary<RectTransform, Vector2> _originalAnchorMins = new Dictionary<RectTransform, Vector2>();
		private static readonly Dictionary<RectTransform, Vector2> _originalAnchorMaxs = new Dictionary<RectTransform, Vector2>();
		private static readonly Dictionary<RectTransform, Vector2> _originalPivots = new Dictionary<RectTransform, Vector2>();
		private static readonly Dictionary<RectTransform, Vector2> _originalSizeDeltas = new Dictionary<RectTransform, Vector2>();
		private static readonly HashSet<ShopStation> _exquestingStations = new HashSet<ShopStation>();
		private static readonly HashSet<ShopPanel> _interactionLockedPanels = new HashSet<ShopPanel>();
		private static bool _initialHadNextButton;
		private static bool _initialNextButtonActive;
		private static bool _initialStateCaptured;
		private static bool? _pendingNextButtonActive;
		private static bool _isApplied;
		private static Transform _originalParent;
		private static int _originalSiblingIndex;
		private static readonly HashSet<int> _hiddenSlots = new HashSet<int> { 0, 4, 8, 9 };

		public static void SetInitialState()
		{
			VnPanel vnPanel = UiManager.GetPanel<VnPanel>();
			_pendingNextButtonActive = null;
			if (vnPanel != null && vnPanel.nextButton != null)
			{
				_initialHadNextButton = true;
				_initialNextButtonActive = vnPanel.nextButton.gameObject.activeSelf;
				_initialStateCaptured = true;
				BepinexPlugin.log.LogInfo($"[EXQUESTING UI] Remembering VnPanel.nextButton state: {_initialNextButtonActive}");
			}
			else
			{
				_initialHadNextButton = false;
				_initialNextButtonActive = false;
				_initialStateCaptured = true;
				BepinexPlugin.log.LogInfo("[EXQUESTING UI] No VnPanel.nextButton present at open; will force-hide on close.");
			}
		}

		public static void OnShow(ShopPanel panel)
		{
			if (_isApplied)
			{
				return;
			}

			BepinexPlugin.log.LogInfo("[EXQUESTING UI] Applying custom empty shop layout.");

			panel._quotedSomething = true;

			_originalActiveStates.Clear();
			_originalChildActiveStates.Clear();
			_originalPositions.Clear();
			_originalComponentStates.Clear();
			_originalAnchorMins.Clear();
			_originalAnchorMaxs.Clear();
			_originalPivots.Clear();
			_originalSizeDeltas.Clear();
			_originalParent = panel.transform.parent;
			_originalSiblingIndex = panel.transform.GetSiblingIndex();

			FieldInfo topLayerField = AccessTools.Field(typeof(UiManager), "topLayer");
			Transform uiRoot = topLayerField?.GetValue(UiManager.Instance) as Transform ?? (UiManager.Instance?.transform);
			if (uiRoot != null)
			{
				panel.transform.SetParent(uiRoot, true);
				panel.transform.SetAsLastSibling();
			}

			VnPanel vnPanel = UiManager.GetPanel<VnPanel>();
			if (vnPanel != null && vnPanel.nextButton != null)
			{
				vnPanel.SetNextButton(false, null, null);
			}

			ExquestingCancelInputHandler cancelHandler = panel.GetComponent<ExquestingCancelInputHandler>() ?? panel.gameObject.AddComponent<ExquestingCancelInputHandler>();
			cancelHandler.enabled = true;

			StoreAndSetActive(panel.transform.Find("Root/Box/ShopBoard/Exhibits")?.gameObject, false);
			StoreAndSetActive(panel.transform.Find("Root/Box/ShopBoard/CardService")?.gameObject, false);
			StoreAndSetActive(panel.transform.Find("Root/Box/ShopBoard/SoldOut")?.gameObject, false);

			Transform returnButton = panel.transform.Find("Root/Box/ShopBoard/ReturnButton");
			if (returnButton != null)
			{
				StoreAndSetActive(returnButton.Find("Portrait")?.gameObject, false);
				RectTransform returnRect = returnButton.GetComponent<RectTransform>();
				if (returnRect != null)
				{
					StoreOriginalPosition(returnRect);
					returnRect.anchoredPosition = new Vector2(-300f, 590f);
				}
			}

			Transform normalCards = panel.transform.Find("Root/Box/ShopBoard/NormalCards");
			if (normalCards != null)
			{
				GridLayoutGroup grid = normalCards.GetComponent<GridLayoutGroup>();
				if (grid != null)
				{
					StoreAndSetComponentEnabled(grid, false);
				}
			}

			if (panel.shopCardList != null)
			{
				for (int i = 0; i < panel.shopCardList.Count; i++)
				{
					ShopCard shopCard = panel.shopCardList[i];
					if (shopCard == null)
					{
						continue;
					}

					RectTransform cardRect = shopCard.GetComponent<RectTransform>();
					if (cardRect != null)
					{
						StoreOriginalPosition(cardRect);
						StoreOriginalAnchorsAndPivot(cardRect);
						StoreOriginalSizeDelta(cardRect);
						cardRect.anchorMin = new Vector2(0f, 1f);
						cardRect.anchorMax = new Vector2(0f, 1f);
						cardRect.pivot = new Vector2(0f, 1f);
						cardRect.sizeDelta = new Vector2(430f, 650f);

						if (i == 1) cardRect.anchoredPosition = new Vector2(35f, -150f);
						if (i == 2) cardRect.anchoredPosition = new Vector2(765f, -150f);
						if (i == 3) cardRect.anchoredPosition = new Vector2(1495f, -150f);
						if (i == 5) cardRect.anchoredPosition = new Vector2(35f, -880f);
						if (i == 6) cardRect.anchoredPosition = new Vector2(765f, -880f);
						if (i == 7) cardRect.anchoredPosition = new Vector2(1495f, -880f);
					}

					if (_hiddenSlots.Contains(i))
					{
						StoreAndSetActive(_originalChildActiveStates, shopCard.transform.Find("Content")?.gameObject, false);
						StoreAndSetActive(_originalChildActiveStates, shopCard.transform.Find("Price")?.gameObject, false);
						StoreAndSetActive(_originalChildActiveStates, shopCard.transform.Find("SoldOut")?.gameObject, false);
					}

					StoreAndSetActive(_originalChildActiveStates, shopCard.transform.Find("Content/Price")?.gameObject, false);
					StoreAndSetActive(_originalChildActiveStates, shopCard.transform.Find("Content/GoldIcon")?.gameObject, false);
					if (shopCard.price != null)
					{
						StoreAndSetActive(shopCard.price.gameObject, false);
					}
				}
			}

			_isApplied = true;
		}

		public static void OnHide(ShopPanel panel)
		{
			if (!_isApplied)
			{
				return;
			}

			BepinexPlugin.log.LogInfo("[EXQUESTING UI] Restoring shop layout from custom empty mode.");
			foreach (KeyValuePair<GameObject, bool> kv in _originalActiveStates)
			{
				kv.Key?.SetActive(kv.Value);
			}
			foreach (KeyValuePair<GameObject, bool> kv in _originalChildActiveStates)
			{
				kv.Key?.SetActive(kv.Value);
			}
			foreach (KeyValuePair<RectTransform, Vector2> kv in _originalPositions)
			{
				if (kv.Key != null)
				{
					kv.Key.anchoredPosition = kv.Value;
				}
			}
			foreach (KeyValuePair<Behaviour, bool> kv in _originalComponentStates)
			{
				if (kv.Key != null)
				{
					kv.Key.enabled = kv.Value;
				}
			}
			foreach (KeyValuePair<RectTransform, Vector2> kv in _originalAnchorMins)
			{
				if (kv.Key != null)
				{
					kv.Key.anchorMin = kv.Value;
				}
			}
			foreach (KeyValuePair<RectTransform, Vector2> kv in _originalAnchorMaxs)
			{
				if (kv.Key != null)
				{
					kv.Key.anchorMax = kv.Value;
				}
			}
			foreach (KeyValuePair<RectTransform, Vector2> kv in _originalPivots)
			{
				if (kv.Key != null)
				{
					kv.Key.pivot = kv.Value;
				}
			}
			foreach (KeyValuePair<RectTransform, Vector2> kv in _originalSizeDeltas)
			{
				if (kv.Key != null)
				{
					kv.Key.sizeDelta = kv.Value;
				}
			}

			if (panel?.shopCardList != null)
			{
				for (int i = 0; i < panel.shopCardList.Count; i++)
				{
					ExquestingUiLauncher.ResetCardWidgetEdge(panel.shopCardList[i]);
				}
			}

			_pendingNextButtonActive = !_initialStateCaptured || !_initialHadNextButton
				? false
				: _initialNextButtonActive;

			if (panel?.ShopStation != null)
			{
				_exquestingStations.Remove(panel.ShopStation);
			}
			_interactionLockedPanels.Remove(panel);

			ExquestingCancelInputHandler cancelHandler = panel?.GetComponent<ExquestingCancelInputHandler>();
			if (cancelHandler != null)
			{
				Object.Destroy(cancelHandler);
			}

			if (_originalParent != null)
			{
				panel.transform.SetParent(_originalParent, true);
				if (_originalSiblingIndex >= 0)
				{
					panel.transform.SetSiblingIndex(_originalSiblingIndex);
				}
			}

			_originalActiveStates.Clear();
			_originalChildActiveStates.Clear();
			_originalPositions.Clear();
			_originalComponentStates.Clear();
			_originalAnchorMins.Clear();
			_originalAnchorMaxs.Clear();
			_originalPivots.Clear();
			_originalSizeDeltas.Clear();
			_isApplied = false;
			_originalParent = null;
			_originalSiblingIndex = 0;
		}

		public static void OnHidden(ShopPanel panel)
		{
			if (_isApplied)
			{
				OnHide(panel);
			}

			panel.Clear();
			panel.Close();
			panel.ShopStation = null;
			GameMaster.ShowPoseAnimation = true;

			VnPanel vnPanel = UiManager.GetPanel<VnPanel>();
			if (vnPanel != null)
			{
				vnPanel.SetNextButton(_pendingNextButtonActive ?? false, null, null);
			}

			ExquestingPanelMarker marker = panel.GetComponent<ExquestingPanelMarker>();
			if (marker != null)
			{
				Object.Destroy(marker);
			}

			_initialHadNextButton = false;
			_initialNextButtonActive = false;
			_initialStateCaptured = false;
			_pendingNextButtonActive = null;
		}

		private static void StoreAndSetActive(GameObject gameObject, bool active)
		{
			StoreAndSetActive(_originalActiveStates, gameObject, active);
		}

		private static void StoreAndSetActive(Dictionary<GameObject, bool> dict, GameObject gameObject, bool active)
		{
			if (gameObject == null)
			{
				return;
			}

			if (!dict.ContainsKey(gameObject))
			{
				dict.Add(gameObject, gameObject.activeSelf);
			}
			gameObject.SetActive(active);
		}

		private static void StoreOriginalPosition(RectTransform rect)
		{
			if (rect == null)
			{
				return;
			}

			if (!_originalPositions.ContainsKey(rect))
			{
				_originalPositions.Add(rect, rect.anchoredPosition);
			}
		}

		private static void StoreOriginalAnchorsAndPivot(RectTransform rect)
		{
			if (rect == null)
			{
				return;
			}

			if (!_originalAnchorMins.ContainsKey(rect))
			{
				_originalAnchorMins.Add(rect, rect.anchorMin);
			}
			if (!_originalAnchorMaxs.ContainsKey(rect))
			{
				_originalAnchorMaxs.Add(rect, rect.anchorMax);
			}
			if (!_originalPivots.ContainsKey(rect))
			{
				_originalPivots.Add(rect, rect.pivot);
			}
		}

		private static void StoreOriginalSizeDelta(RectTransform rect)
		{
			if (rect == null)
			{
				return;
			}

			if (!_originalSizeDeltas.ContainsKey(rect))
			{
				_originalSizeDeltas.Add(rect, rect.sizeDelta);
			}
		}

		private static void StoreAndSetComponentEnabled(Behaviour component, bool enabled)
		{
			if (component == null)
			{
				return;
			}

			if (!_originalComponentStates.ContainsKey(component))
			{
				_originalComponentStates.Add(component, component.enabled);
			}
			component.enabled = enabled;
		}

		internal static void ApplyPendingNextButtonState(VnPanel vnPanel)
		{
			if (_pendingNextButtonActive == null)
			{
				return;
			}
			if (vnPanel != null)
			{
				vnPanel.SetNextButton(_pendingNextButtonActive.Value, null, null);
				_pendingNextButtonActive = null;
			}
		}

		public static void RegisterExquestingStation(ShopStation station)
		{
			if (station != null)
			{
				_exquestingStations.Add(station);
			}
		}

		public static bool IsExquestingStation(ShopStation station)
		{
			return station != null && _exquestingStations.Contains(station);
		}

		public static bool IsExquestingPanelVisible()
		{
			ShopPanel panel;
			try
			{
				panel = UiManager.GetPanel<ShopPanel>();
			}
			catch (System.InvalidOperationException)
			{
				return false;
			}

			return panel != null && panel.IsVisible && panel.GetComponent<ExquestingPanelMarker>() != null;
		}

		public static bool TryLockInteraction(ShopPanel panel)
		{
			if (panel == null || _interactionLockedPanels.Contains(panel))
			{
				return false;
			}

			_interactionLockedPanels.Add(panel);
			return true;
		}

		public static void UnlockInteraction(ShopPanel panel)
		{
			if (panel != null)
			{
				_interactionLockedPanels.Remove(panel);
			}
		}

		public static void SetShopCardInteractionEnabled(ShopCard shopCard, bool enabled)
		{
			StoreAndSetComponentEnabled(shopCard, enabled);
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
			{
				return true;
			}

			if (eventData == null)
			{
				return true;
			}

			ShopPanel panel = __instance.ShopPanel;
			if (panel == null)
			{
				return false;
			}

			if (eventData.button == PointerEventData.InputButton.Right)
			{
				return false;
			}

			if (eventData.button != PointerEventData.InputButton.Left)
			{
				return false;
			}

			if (!ExquestingPanelController.TryLockInteraction(panel))
			{
				return false;
			}

			panel.StartCoroutine(ExquestingUiLauncher.CoHandleCustomCardClick(panel, __instance.Index));
			return false;
		}

		[HarmonyPatch(typeof(ShopStation), nameof(ShopStation.RefreshAfterBought))]
		[HarmonyPrefix]
		public static bool RefreshAfterBoughtPrefix(ShopStation __instance)
		{
			if (ExquestingPanelController.IsExquestingStation(__instance))
			{
				return false;
			}

			return true;
		}
	}

	public static class ExquestingUiLauncher
	{
		public static void RefreshCustomSoldOutSlots(ShopPanel panel)
		{
			if (panel?.ShopStation?.ShopCards == null || panel.shopCardList == null)
			{
				return;
			}

			exquesting exhibit = panel.GameRun?.Player?.GetExhibit<exquesting>();

			int max = Mathf.Min(panel.ShopStation.ShopCards.Count, panel.shopCardList.Count);
			for (int i = 0; i < max; i++)
			{
				ShopCard shopCard = panel.shopCardList[i];
				if (shopCard == null)
				{
					continue;
				}

				ShopItem<Card> item = panel.ShopStation.ShopCards[i];
				if (item != null && item.IsSoldOut)
				{
					RefreshCustomSlotVisual(panel, i);
				}
				else
				{
					bool accepted = exhibit != null && exhibit.IsQuestSlotAccepted(i);
					TryRefreshCardWidgetVisual(shopCard, item, accepted);
					ExquestingPanelController.SetShopCardInteractionEnabled(shopCard, true);
				}
			}
		}

		private static void RefreshCustomSlotVisual(ShopPanel panel, int slotIndex)
		{
			if (panel?.shopCardList == null || slotIndex < 0 || slotIndex >= panel.shopCardList.Count)
			{
				return;
			}

			ShopCard shopCard = panel.shopCardList[slotIndex];
			if (shopCard == null)
			{
				return;
			}

			ExquestingPanelController.SetShopCardInteractionEnabled(shopCard, false);

			StoreSetActive(shopCard.transform.Find("Content")?.gameObject, false);
			StoreSetActive(shopCard.transform.Find("Price")?.gameObject, false);
			StoreSetActive(shopCard.transform.Find("SoldOut")?.gameObject, true);
			StoreSetActive(shopCard.transform.Find("Content/Price")?.gameObject, false);
			StoreSetActive(shopCard.transform.Find("Content/GoldIcon")?.gameObject, false);
			if (shopCard.price != null)
			{
				StoreSetActive(shopCard.price.gameObject, false);
			}

			TrySetCardWidgetEdge(shopCard, CardWidget.EdgeStatus.None);
		}

		private static void RefreshAcceptedSlotVisual(ShopPanel panel, int slotIndex)
		{
			if (panel?.shopCardList == null || panel.ShopStation?.ShopCards == null || slotIndex < 0 || slotIndex >= panel.shopCardList.Count || slotIndex >= panel.ShopStation.ShopCards.Count)
			{
				return;
			}

			ShopCard shopCard = panel.shopCardList[slotIndex];
			ShopItem<Card> item = panel.ShopStation.ShopCards[slotIndex];
			if (shopCard == null || item == null)
			{
				return;
			}

			item.IsSoldOut = false;
			TryRefreshShopCardBinding(shopCard, item);
			TryRefreshCardWidgetVisual(shopCard, item, accepted: true);
			ExquestingPanelController.SetShopCardInteractionEnabled(shopCard, true);

			StoreSetActive(shopCard.transform.Find("Content")?.gameObject, true);
			StoreSetActive(shopCard.transform.Find("Price")?.gameObject, false);
			StoreSetActive(shopCard.transform.Find("SoldOut")?.gameObject, false);
			StoreSetActive(shopCard.transform.Find("Content/Price")?.gameObject, false);
			StoreSetActive(shopCard.transform.Find("Content/GoldIcon")?.gameObject, false);
			if (shopCard.price != null)
			{
				StoreSetActive(shopCard.price.gameObject, false);
			}

			RectTransform rect = shopCard.GetComponent<RectTransform>();
			if (rect != null)
			{
				LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
			}
		}

		private static void TryRefreshCardWidgetVisual(ShopCard shopCard, ShopItem<Card> item, bool accepted)
		{
			if (shopCard == null || item?.Content == null)
			{
				return;
			}

			CardWidget cardWidget = TryGetCardWidget(shopCard);
			if (cardWidget == null)
			{
				return;
			}

			cardWidget.Card = item.Content;
			cardWidget.RefreshStatus();
			cardWidget.SetProperties();

			CardWidget.EdgeStatus edgeStatus = accepted ? CardWidget.EdgeStatus.AffordKicker : CardWidget.EdgeStatus.None;
			TrySetCardWidgetEdge(shopCard, edgeStatus);
		}

		internal static void ResetCardWidgetEdge(ShopCard shopCard)
		{
			TrySetCardWidgetEdge(shopCard, CardWidget.EdgeStatus.None);
		}

		private static void TrySetCardWidgetEdge(ShopCard shopCard, CardWidget.EdgeStatus edge)
		{
			CardWidget cardWidget = TryGetCardWidget(shopCard);
			if (cardWidget != null)
			{
				cardWidget.SetCardEdge(edge);
			}
		}

		private static CardWidget TryGetCardWidget(ShopCard shopCard)
		{
			if (shopCard == null)
			{
				return null;
			}

			CardWidget cardWidget = shopCard.GetComponentInChildren<CardWidget>(true);
			if (cardWidget != null)
			{
				return cardWidget;
			}

			try
			{
				FieldInfo cardWidgetField = AccessTools.Field(shopCard.GetType(), "cardWidget");
				if (cardWidgetField != null)
				{
					return cardWidgetField.GetValue(shopCard) as CardWidget;
				}
			}
			catch (global::System.Exception)
			{
			}

			return null;
		}

		private static void TryRefreshShopCardBinding(ShopCard shopCard, ShopItem<Card> item)
		{
			if (shopCard == null || item == null)
			{
				return;
			}

			try
			{
				MethodInfo bindMethod = TryGetInstanceMethod(shopCard.GetType(), "SetShopItem", new[] { typeof(ShopItem<Card>) })
					?? TryGetInstanceMethod(shopCard.GetType(), "SetData", new[] { typeof(ShopItem<Card>) });
				if (bindMethod != null)
				{
					bindMethod.Invoke(shopCard, new object[] { item });
					return;
				}

				MethodInfo refreshMethod = TryGetInstanceMethod(shopCard.GetType(), "Refresh", System.Type.EmptyTypes)
					?? TryGetInstanceMethod(shopCard.GetType(), "OnPointerExit", new[] { typeof(PointerEventData) });
				if (refreshMethod != null)
				{
					if (refreshMethod.GetParameters().Length == 0)
					{
						refreshMethod.Invoke(shopCard, null);
					}
					else
					{
						refreshMethod.Invoke(shopCard, new object[] { null });
					}
				}
			}
			catch (global::System.Exception ex)
			{
				BepinexPlugin.log.LogDebug($"[EXQUESTING UI] Failed to invoke ShopCard refresh methods: {ex.Message}");
			}
		}

		private static MethodInfo TryGetInstanceMethod(System.Type type, string methodName, System.Type[] parameterTypes)
		{
			if (type == null || string.IsNullOrEmpty(methodName))
			{
				return null;
			}

			const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
			return type.GetMethod(methodName, flags, binder: null, types: parameterTypes ?? System.Type.EmptyTypes, modifiers: null);
		}

		private static void StoreSetActive(GameObject target, bool isActive)
		{
			if (target != null)
			{
				target.SetActive(isActive);
			}
		}

		public static IEnumerator CoHandleCustomCardClick(ShopPanel panel, int index)
		{
			if (panel == null)
			{
				yield break;
			}

			ShopPanel shopPanel = null;
			bool lockedShop = false;
			Coroutine keepSelectPanelOnTopCoroutine = null;

			try
			{
				if (!panel.IsVisible || panel.ShopStation == null)
				{
					yield break;
				}

				if (index < 0 || index >= panel.ShopStation.ShopCards.Count)
				{
					yield break;
				}

				ShopItem<Card> item = panel.ShopStation.ShopCards[index];
				if (item == null || item.IsSoldOut)
				{
					yield break;
				}

				Card card = item.Content;
				if (card == null)
				{
					yield break;
				}

				shopPanel = UiManager.GetPanel<ShopPanel>();
				if (shopPanel != null)
				{
					shopPanel.LockedByInteractionMinimized = true;
					lockedShop = true;
				}

				keepSelectPanelOnTopCoroutine = panel.StartCoroutine(CoKeepSelectPanelOnTop(panel));

				if (!GameMaster.Instance.CurrentGameRun.Player.HasExhibit<exquesting>())
				{
					yield break;
				}

				exquesting exhibit = GameMaster.Instance.CurrentGameRun.Player.GetExhibit<exquesting>();
				if (exhibit == null)
				{
					yield break;
				}

				Card rolledCard = exhibit.GetRolledQuestCard(index);
				if (rolledCard != null)
				{
					card = rolledCard;
				}

				bool alreadyAccepted = exhibit.IsQuestSlotAccepted(index);

				string cardId = card?.Id ?? item.Content?.Id ?? "";
				bool canAbandon = true;
				if (cardId == nameof(cardquest11))
					canAbandon = false;

				SelectCardInteraction interaction = new SelectCardInteraction(0, (!canAbandon && alreadyAccepted) ? 0 : 1, new[] { card })
				{
					Source = null,
					CanCancel = true,
					Description = alreadyAccepted ? GetLoc("AbandonQuest") : GetLoc("AcceptQuest")
				};

				yield return panel.GameRun.InteractionViewer.View(interaction);

				if (cardId == "")
				{
					BepinexPlugin.log.LogWarning("[EXQUESTING UI] Clicked card has no ID, cannot track quest progress.");
				}

				if (!interaction.IsCanceled && interaction.SelectedCards.Count > 0)
				{
					if (alreadyAccepted)
					{
						switch (interaction.SelectedCards[0].Id)
						{
							case nameof(cardquest4):
								Card toRmv = panel.GameRun.BaseDeck.First(c => c.Id == nameof(cardgenji));
								if (toRmv != null)
								{
									panel.GameRun.RemoveDeckCard(toRmv);
								}
								break;
							case nameof(cardquest10):
								List<Card> toRmv2 = new List<Card>();
								for (int i = 0; i < Library.CreateCard<cardquest10>().Value20; i++)
								{
									toRmv2.Add(panel.GameRun.BaseDeck.FirstOrDefault(c => c.Id == nameof(LBoL.EntityLib.Cards.Neutral.Black.Shadow) && !toRmv2.Contains(c)));
								}
								if (toRmv2.Count > 0)
									panel.GameRun.RemoveDeckCards(toRmv2.Where(c => c != null && c is LBoL.EntityLib.Cards.Neutral.Black.Shadow).ToArray());
								break;
							default:
								break;
						}
						exhibit.PendingQuestProgress.Remove(cardId);
						exhibit.ClearQuestRequirement(cardId);
						exhibit.ClearQuestCompleted(cardId);
						exhibit.MarkQuestSlotSoldOut(index);
						item.IsSoldOut = true;
						RefreshCustomSlotVisual(panel, index);
						BepinexPlugin.log.LogInfo($"[EXQUESTING SAVE] UI action=Abandon quest={cardId}");
					}
					else
					{
						switch (interaction.SelectedCards[0].Id)
						{
							case nameof(cardquest4):
								panel.GameRun.AddDeckCard(Library.CreateCard<cardgenji>(), true);
								break;
							case nameof(cardquest8):
								Card grazeCard = panel.GameRun.RollCard(panel.GameRun.CardRng, new CardWeightTable(RarityWeightTable.EnemyCard, OwnerWeightTable.Valid, CardTypeWeightTable.CanBeLoot), false, false, config => config.RelativeEffects.Contains(nameof(Graze)) || config.UpgradedRelativeEffects.Contains(nameof(Graze)));
								if (grazeCard != null)
									panel.GameRun.AddDeckCard(grazeCard, true);
								break;
							case nameof(cardquest10):
								cardquest10 quest10 = Library.CreateCard<cardquest10>();
								panel.GameRun.AddDeckCards(Library.CreateCards<LBoL.EntityLib.Cards.Neutral.Black.Shadow>(quest10.Value20), true);
								break;
							case nameof(cardquest11):
								panel.GameRun.GainMoney(Library.CreateCard<cardquest11>().Value440);
								break;
							case nameof(cardquest13):
								panel.GameRun.AddDeckCard(panel.GameRun.GetRandomCurseCard(panel.GameRun.CardRng), true);
								break;
							default:
								break;
						}
						exhibit.ClearQuestCompleted(cardId);
						if (!exhibit.PendingQuestProgress.ContainsKey(cardId))
						{
							exhibit.PendingQuestProgress[cardId] = 0;
						}

						string lockedRequirement = exhibit.EnsureRequirementLockedForQuest(cardId);
						if (!string.IsNullOrEmpty(lockedRequirement))
						{
							BepinexPlugin.log.LogInfo($"[EXQUESTING SAVE] UI action=LockRequirement quest={cardId} requirement={lockedRequirement}");
						}

						RefreshAcceptedSlotVisual(panel, index);
						BepinexPlugin.log.LogInfo($"[EXQUESTING SAVE] UI action=Accept quest={cardId}");
					}

					exhibit.CleanupStaleQuestRequirements();
					ShopModHandlers.PersistQuestProgress(GameMaster.Instance?.CurrentGameRun, exhibit.PendingQuestProgress, syncToLiteShop: false, saveToDisk: false, questRequirements: exhibit.QuestRequirements, completedQuestCards: exhibit.CompletedQuestCards, writeToRunFlags: false);
				}
				// panel.GameRun.AddDeckCards(interaction.SelectedCards, true, null);
				// item.IsSoldOut = true;
				// panel.SetShop();
			}
			finally
			{
				if (keepSelectPanelOnTopCoroutine != null && panel != null)
				{
					panel.StopCoroutine(keepSelectPanelOnTopCoroutine);
				}

				if (lockedShop && shopPanel != null)
				{
					shopPanel.LockedByInteractionMinimized = false;
				}

				ExquestingPanelController.UnlockInteraction(panel);
			}
		}

		private static IEnumerator CoKeepSelectPanelOnTop(ShopPanel shopPanel)
		{
			while (shopPanel != null && shopPanel.IsVisible)
			{
				SelectCardPanel selectCardPanel = UiManager.GetPanel<SelectCardPanel>();
				if (selectCardPanel != null && selectCardPanel.IsVisible && selectCardPanel.transform.parent != null)
				{
					selectCardPanel.transform.SetAsLastSibling();
				}

				yield return null;
			}
		}

		public static void OnExhibitClicked()
		{
			GameMaster instance = Singleton<GameMaster>.Instance;
			object battleObj = instance?.CurrentGameRun?.Battle;
			if (battleObj != null)
			{
				BepinexPlugin.log.LogInfo("[EXQUESTING UI] In battle, exquesting click ignored.");
				return;
			}

			if (IsInteractionActive())
			{
				BepinexPlugin.log.LogInfo("[EXQUESTING UI] Interaction active, exquesting click ignored.");
				return;
			}

			BepinexPlugin.log.LogInfo("[EXQUESTING UI] Exhibit clicked - opening empty shop UI.");
			ExquestingPanelController.SetInitialState();
			GameRunController run = instance?.CurrentGameRun;
			if (run == null)
			{
				BepinexPlugin.log.LogError("Cannot open exquesting panel: GameRun is not active.");
				return;
			}

			exquesting exhibit = run.Player?.GetExhibit<exquesting>();
			if (exhibit != null)
			{
				BepinexPlugin.log.LogInfo($"[EXQUESTING SAVE] OnExhibitClicked using runtime state pending={exhibit.PendingQuestProgress.Count}, req={exhibit.QuestRequirements.Count}, rolled={exhibit.RolledQuestCards.Count}, soldOut={exhibit.SoldOutQuestSlots.Count}");
			}
			else
			{
				BepinexPlugin.log.LogError("Cannot open exquesting panel: Exhibit instance not found.");
				return;
			}

			ShopPanel panel = UiManager.GetPanel<ShopPanel>();
			if (panel == null)
			{
				BepinexPlugin.log.LogError("ShopPanel is not loaded.");
				return;
			}

			if (panel.IsVisible && panel.GetComponent<ExquestingPanelMarker>() == null)
			{
				BepinexPlugin.log.LogInfo("[EXQUESTING UI] Native ShopPanel is open; exquesting click ignored.");
				return;
			}

			if (panel.IsVisible)
			{
				BepinexPlugin.log.LogInfo("[EXQUESTING UI] ShopPanel already visible, click ignored.");
				return;
			}

			exhibit.EnsureRolledQuestCards();
			List<ShopItem<Card>> shopCards = exhibit.BuildRolledShopCards(run);

			ShopStation station = new ShopStation
			{
				GameRun = run,
				ShopCards = shopCards,
				ShopExhibits = new List<ShopItem<Exhibit>>(),
				CanUseCardService = false
			};

			while (station.ShopExhibits.Count < 3)
			{
				station.ShopExhibits.Add(null);
			}

			ExquestingPanelController.RegisterExquestingStation(station);

			if (panel.GetComponent<ExquestingPanelMarker>() == null)
			{
				panel.gameObject.AddComponent<ExquestingPanelMarker>();
			}

			if (panel is UiPanel<ShopStation> v)
			{
				v.Show(station);
			}
			else
			{
				BepinexPlugin.log.LogError("ShopPanel is not loaded.");
			}
		}

		private static bool IsInteractionActive()
		{
			SelectCardPanel selectCardPanel = UiManager.GetPanel<SelectCardPanel>();
			if (selectCardPanel != null && selectCardPanel.IsVisible)
			{
				return true;
			}

			ShopPanel shopPanel = UiManager.GetPanel<ShopPanel>();
			if (shopPanel != null && shopPanel.IsVisible)
			{
				return true;
			}

			return false;
		}
		private static string GetLoc(string key)
		{
			var locale = LBoL.Core.Localization.CurrentLocale;
			if (LocalisationKeys.LocTable.TryGetValue((locale, key), out var value))
				return value;
			if (LocalisationKeys.LocTable.TryGetValue((Locale.En, key), out var fallback))
				return fallback;
			return "<" + key + ">";
		}
	}
}
