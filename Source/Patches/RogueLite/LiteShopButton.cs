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

[HarmonyPatch]
public partial class LiteShopButton
{
	private static ComplexRulesPanel _shopPanel;

	private static readonly HashSet<ComplexRulesPanel> LiteShopPanels = [];

	private static LiteShop _currentShop;

	private static GameObject _buttonTemplate;

	private static GameObject _mainMenuButton;

	private static LocalizedText _mainMenuLocalizedText;

	private static TextMeshProUGUI _mainMenuTmpText;

	private static Locale? _bootLocale = null;

	private static readonly Dictionary<ComplexRulesPanel, float> _originalFontSizes = [];

	// Load button template from settings panel
	[HarmonyPatch(typeof(SettingPanel), nameof(SettingPanel.Awake)), HarmonyPostfix]
	private static void LoadButtonTemplate(SettingPanel __instance)
	{
		if (_buttonTemplate != null) return;

		try
		{
			var preferenceTab = __instance.tabs.FirstOrDefault(t => t.name == "Preference");
			if (preferenceTab == null)
				return;

			var rightPanel = preferenceTab.transform.Find("RightPanel");
			if (rightPanel == null)
				return;

			var resetHint = rightPanel.Find("ResetHint");
			if (resetHint != null)
			{
				_buttonTemplate = resetHint.gameObject;
				// Capture boot locale here because SettingPanel.Awake runs on startup
				if (!_bootLocale.HasValue)
				{
					_bootLocale = LBoL.Core.Localization.CurrentLocale;
				}
			}
		}
		catch (Exception e)
		{
			BepinexPlugin.log?.LogDebug(e);
		}
	}

	// add button on main menu that opens the shop panel
	[HarmonyPatch(typeof(MainMenuPanel), nameof(MainMenuPanel.Awake)), HarmonyPostfix, HarmonyPriority(Priority.High)]
	private static void AddMainMenuButton(MainMenuPanel __instance)
	{
		try
		{
			var menu = __instance.transform;
			// try to find a sensible container for buttons
			Transform buttonsParent = menu.Find("Root/Buttons") ?? menu.Find("Buttons") ?? menu.Find("Root") ?? menu;

			// try to find an existing button to clone
			var templateBtn = buttonsParent.GetComponentsInChildren<Button>(true).FirstOrDefault()?.gameObject;
			GameObject newBtnGO = null;
			if (templateBtn != null)
			{
				newBtnGO = Object.Instantiate(templateBtn, templateBtn.transform.parent);
				newBtnGO.name = "LiteShopButton";
				_mainMenuButton = newBtnGO;

				// sanitize listeners
				var btn = newBtnGO.GetComponent<Button>();
				if (btn != null)
				{
					btn.onClick = new Button.ButtonClickedEvent();
					btn.onClick.AddListener(() =>
					{
						if (IsShiftHeld())
						{
							ToggleChallengerModeFromMenu();
							return;
						}
						OpenShopUI();
					});
					btn.interactable = true;
				}

				_mainMenuLocalizedText = newBtnGO.GetComponentInChildren<LocalizedText>();
				_mainMenuTmpText = newBtnGO.GetComponentInChildren<TextMeshProUGUI>();
				UpdateMainMenuButtonLabel();

				// place the button 3 menu slots above New Game (leaves room for cyan's companion)
				PlaceAboveNewGameButton(__instance, newBtnGO.transform, 3);
			}
			else
			{
				// fallback: create a simple empty button GameObject
				newBtnGO = new GameObject("LiteShopButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Button));
				newBtnGO.transform.SetParent(buttonsParent, false);
				_mainMenuButton = newBtnGO;
				_mainMenuLocalizedText = null;
				_mainMenuTmpText = null;
			}

			// ensure label refresh once profile is ready
			if (newBtnGO.GetComponent<ShopLabelUpdater>() == null)
				newBtnGO.AddComponent<ShopLabelUpdater>();
		}
		catch (Exception e)
		{
			BepinexPlugin.log?.LogDebug(e);
		}
	}

	// Positions `target` `slots` button-spacings above the New Game button, where one
	// spacing is the vertical distance between New Game and the next button below it.
	private static void PlaceAboveNewGameButton(MainMenuPanel panel, Transform target, int slots)
	{
		var newGame = panel.newGameButton != null ? panel.newGameButton.transform : null;
		var group = newGame != null ? newGame.parent : null;
		if (group == null)
			return;

		// nearest sibling button to New Game in either direction (Restore Game shares its slot, so skip same-y siblings);
		// New Game isn't guaranteed to have a neighbor below it (e.g. when it's the bottom-most button)
		Transform nearest = null;
		float bestGap = float.MaxValue;
		for (int i = 0; i < group.childCount; i++)
		{
			var sibling = group.GetChild(i);
			if (sibling == newGame || sibling == target || sibling.GetComponent<Button>() == null)
				continue;
			float gap = Mathf.Abs(newGame.localPosition.y - sibling.localPosition.y);
			if (gap > 0.5f && gap < bestGap)
			{
				bestGap = gap;
				nearest = sibling;
			}
		}
		if (nearest == null)
			return;

		// work in world space so this holds even if the clone lives under a different parent;
		// force the step to point upward regardless of whether the nearest neighbor is above or below
		Vector3 step = newGame.position - nearest.position;
		if (step.y < 0f)
			step = -step;
		Vector3 targetWorld = newGame.position + step * slots;
		var position = target.position;
		position.y = targetWorld.y;
		target.position = position;
	}

	private static LiteShop GetMenuShop()
	{
		var customData = MiniTracker.Instance?.CustomGrSaveData ?? MiniTracker.LoadedFromDiskCustomGrSaveData;
		if (customData == null)
			return null;

		var profile = GameMaster.Instance?.CurrentProfile;
		if (profile == null)
			return null;

		var key = LiteProfileSaveData.ProfileKey(profile);
		if (!customData.Saves.TryGetValue(key, out var shop))
			return null;

		return shop;
	}

	public static void RefreshMainMenuButtonLabel()
	{
		if (_mainMenuButton == null)
			return;

		UpdateMainMenuButtonLabel();
	}

	private static void UpdateMainMenuButtonLabel()
	{
		if (_mainMenuButton == null)
			return;

		var menuShop = GetMenuShop();
		string key;
		if (menuShop == null)
		{
			key = $"{LocalisationKeys.ShopPrefix}LiteShopButton";
		}
		else
		{
			key = menuShop.ChallengerModeEnabled
				? $"{LocalisationKeys.ShopPrefix}LiteShopButton.Active"
				: $"{LocalisationKeys.ShopPrefix}LiteShopButton.Inactive";
		}

		if (_mainMenuLocalizedText != null)
		{
			_mainMenuLocalizedText.key = key;
			_mainMenuLocalizedText.OnLocaleChanged();
			return;
		}

		if (_mainMenuTmpText != null)
			_mainMenuTmpText.text = GetShopLoc(key);
	}

	public static void OpenShopUI()
	{
		if (_shopPanel != null)
		{
			_shopPanel.gameObject.SetActive(true);
			_shopPanel.Show();
			return;
		}

		var original = UiManager.GetPanel<ComplexRulesPanel>();
		var clone = Object.Instantiate(original, original.transform.parent);

		_shopPanel = clone;
		LiteShopPanels.Add(clone);

		// Store original font size for this panel
		if (clone.descriptionText != null && !_originalFontSizes.ContainsKey(clone))
			_originalFontSizes[clone] = clone.descriptionText.fontSize;

		clone.name = "LiteShop";
		clone.gameObject.SetActive(true);

		// cleanup on close
		clone.bgButton.onClick.RemoveAllListeners();
		clone.bgButton.onClick.AddListener(() =>
		{
			clone.Hide();
			clone.gameObject.SetActive(false);
		});

		ShopSaveLoader.Save();

		clone.Show(); // triggers OnShowing, postfix rebuilds UI
	}

	private static bool IsShiftHeld()
	{
		var keyboard = Keyboard.current;
		return keyboard != null && (keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed);
	}

	private static void ToggleChallengerModeFromMenu()
	{
		var menuShop = GetMenuShop();
		if (menuShop == null)
			return;

		if (HasActiveGameRun())
			return;

		menuShop.ChallengerModeEnabled = !menuShop.ChallengerModeEnabled;
		UpdateMainMenuButtonLabel();
		ShopSaveLoader.Save();
	}
}
