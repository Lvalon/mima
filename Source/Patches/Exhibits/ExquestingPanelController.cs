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

public sealed class ExquestingPanelMarker : MonoBehaviour
{
}

public sealed class ExquestingCancelInputHandler : MonoBehaviour, IInputActionHandler
{
	private bool _registered;

	private void OnEnable()
	{
		if (_registered)
			return;

		UiManager.PushActionHandler(this);
		_registered = true;
	}

	private void OnDisable()
	{
		if (!_registered)
			return;

		UiManager.PopActionHandler(this);
		_registered = false;
	}

	void IInputActionHandler.OnCancel()
	{
		ShopPanel panel = GetComponent<ShopPanel>();
		if (panel == null || panel.GetComponent<ExquestingPanelMarker>() == null || !panel.IsVisible)
			return;

		if (!panel.LockedByInteractionMinimized)
			panel.Hide();
	}
}

public static class ExquestingPanelController
{
	private static readonly Dictionary<GameObject, bool> _originalActiveStates = [];
	private static readonly Dictionary<GameObject, bool> _originalChildActiveStates = [];
	private static readonly Dictionary<RectTransform, Vector2> _originalPositions = [];
	private static readonly Dictionary<Behaviour, bool> _originalComponentStates = [];
	private static readonly Dictionary<RectTransform, Vector2> _originalAnchorMins = [];
	private static readonly Dictionary<RectTransform, Vector2> _originalAnchorMaxs = [];
	private static readonly Dictionary<RectTransform, Vector2> _originalPivots = [];
	private static readonly Dictionary<RectTransform, Vector2> _originalSizeDeltas = [];
	private static readonly HashSet<ShopStation> _exquestingStations = [];
	private static readonly HashSet<ShopPanel> _interactionLockedPanels = [];
	private static bool _initialHadNextButton;
	private static bool _initialNextButtonActive;
	private static bool _initialStateCaptured;
	private static bool? _pendingNextButtonActive;
	private static bool _isApplied;
	private static Transform _originalParent;
	private static int _originalSiblingIndex;
	private static readonly HashSet<int> _hiddenSlots = [0, 4, 8, 9];

	public static void SetInitialState()
	{
		VnPanel vnPanel = UiManager.GetPanel<VnPanel>();
		_pendingNextButtonActive = null;
		if (vnPanel != null && vnPanel.nextButton != null)
		{
			_initialHadNextButton = true;
			_initialNextButtonActive = vnPanel.nextButton.gameObject.activeSelf;
			_initialStateCaptured = true;
		}
		else
		{
			_initialHadNextButton = false;
			_initialNextButtonActive = false;
			_initialStateCaptured = true;
		}
	}

	public static void OnShow(ShopPanel panel)
	{
		if (_isApplied)
			return;

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

		Transform uiRoot = UiManager.Instance?.topLayer ?? UiManager.Instance?.transform;
		if (uiRoot != null)
		{
			panel.transform.SetParent(uiRoot, true);
			panel.transform.SetAsLastSibling();
		}

		VnPanel vnPanel = UiManager.GetPanel<VnPanel>();
		if (vnPanel != null && vnPanel.nextButton != null)
			vnPanel.SetNextButton(false, null, null);

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
			return;

		foreach (var (key, value) in _originalActiveStates)
		{
			if (key != null)
				key.SetActive(value);
		}
		foreach (var (key, value) in _originalChildActiveStates)
		{
			if (key != null)
				key.SetActive(value);
		}
		foreach (var (key, value) in _originalPositions)
		{
			if (key != null)
				key.anchoredPosition = value;
		}
		foreach (var (key, value) in _originalComponentStates)
		{
			if (key != null)
				key.enabled = value;
		}
		foreach (var (key, value) in _originalAnchorMins)
		{
			if (key != null)
				key.anchorMin = value;
		}
		foreach (var (key, value) in _originalAnchorMaxs)
		{
			if (key != null)
				key.anchorMax = value;
		}
		foreach (var (key, value) in _originalPivots)
		{
			if (key != null)
				key.pivot = value;
		}
		foreach (var (key, value) in _originalSizeDeltas)
		{
			if (key != null)
				key.sizeDelta = value;
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
			_exquestingStations.Remove(panel.ShopStation);
		_interactionLockedPanels.Remove(panel);

		ExquestingCancelInputHandler cancelHandler = panel?.GetComponent<ExquestingCancelInputHandler>();
		if (cancelHandler != null)
			Object.Destroy(cancelHandler);

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
			OnHide(panel);

		panel.Clear();
		panel.Close();
		panel.ShopStation = null;
		GameMaster.ShowPoseAnimation = true;

		VnPanel vnPanel = UiManager.GetPanel<VnPanel>();
		if (vnPanel != null)
			vnPanel.SetNextButton(_pendingNextButtonActive ?? false, null, null);

		ExquestingPanelMarker marker = panel.GetComponent<ExquestingPanelMarker>();
		if (marker != null)
			Object.Destroy(marker);

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
			return;

		if (!dict.ContainsKey(gameObject))
			dict.Add(gameObject, gameObject.activeSelf);
		gameObject.SetActive(active);
	}

	private static void StoreOriginalPosition(RectTransform rect)
	{
		if (rect == null)
			return;

		if (!_originalPositions.ContainsKey(rect))
			_originalPositions.Add(rect, rect.anchoredPosition);
	}

	private static void StoreOriginalAnchorsAndPivot(RectTransform rect)
	{
		if (rect == null)
			return;

		if (!_originalAnchorMins.ContainsKey(rect))
			_originalAnchorMins.Add(rect, rect.anchorMin);
		if (!_originalAnchorMaxs.ContainsKey(rect))
			_originalAnchorMaxs.Add(rect, rect.anchorMax);
		if (!_originalPivots.ContainsKey(rect))
			_originalPivots.Add(rect, rect.pivot);
	}

	private static void StoreOriginalSizeDelta(RectTransform rect)
	{
		if (rect == null)
			return;

		if (!_originalSizeDeltas.ContainsKey(rect))
			_originalSizeDeltas.Add(rect, rect.sizeDelta);
	}

	private static void StoreAndSetComponentEnabled(Behaviour component, bool enabled)
	{
		if (component == null)
			return;

		if (!_originalComponentStates.ContainsKey(component))
			_originalComponentStates.Add(component, component.enabled);
		component.enabled = enabled;
	}

	internal static void ApplyPendingNextButtonState(VnPanel vnPanel)
	{
		if (_pendingNextButtonActive == null)
			return;
		if (vnPanel != null)
		{
			vnPanel.SetNextButton(_pendingNextButtonActive.Value, null, null);
			_pendingNextButtonActive = null;
		}
	}

	public static void RegisterExquestingStation(ShopStation station)
	{
		if (station != null)
			_exquestingStations.Add(station);
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
			return false;

		_interactionLockedPanels.Add(panel);
		return true;
	}

	public static void UnlockInteraction(ShopPanel panel)
	{
		if (panel != null)
			_interactionLockedPanels.Remove(panel);
	}

	public static void SetShopCardInteractionEnabled(ShopCard shopCard, bool enabled)
	{
		StoreAndSetComponentEnabled(shopCard, enabled);
	}
}
