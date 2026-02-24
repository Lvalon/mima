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
using LBoL.Presentation.UI.Widgets;
using LBoL.Presentation.UI.ExtraWidgets;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

namespace lvalonmima.Source.Patches
{
	[HarmonyPatch]
	public class LiteShopButton
	{
		private static ComplexRulesPanel _shopPanel;
		private static readonly HashSet<ComplexRulesPanel> LiteShopPanels = new HashSet<ComplexRulesPanel>();
		private static string _currentCategoryId;

		private readonly struct LoadoutTooltipData
		{
			public readonly string Title;
			public readonly string Effect;

			public LoadoutTooltipData(string title, string effect)
			{
				Title = title ?? string.Empty;
				Effect = effect ?? string.Empty;
			}
		}
		private static LiteShop _currentShop;
		private static GameObject _buttonTemplate;
		private static GameObject _mainMenuButton;
		private static LocalizedText _mainMenuLocalizedText;
		private static TextMeshProUGUI _mainMenuTmpText;
		private static Locale? _bootLocale = null;
		private static readonly Dictionary<ComplexRulesPanel, float> _originalFontSizes = new Dictionary<ComplexRulesPanel, float>();
		private const float DefaultButtonScale = 0.75f;
		private const float DefaultButtonFontSize = 32f;
		[HarmonyPatch(typeof(ComplexRulesPanel), nameof(ComplexRulesPanel.OnShowing))]
		class Patch_ComplexRulesPanel_OnShowing
		{
			static void Postfix(ComplexRulesPanel __instance)
			{
				if (!LiteShopPanels.Contains(__instance))
					return;

				// font size mult
				// var text = __instance.descriptionText;
				// if (text != null)
				// {
				// 	text.fontSize *= 1.35f;
				// }

				// Set font before building tabs to ensure correct height calculations
				__instance.descriptionText.font = TMP_Settings.defaultFontAsset ?? __instance.descriptionText.font;
				__instance.descriptionText.fontWeight = FontWeight.Regular;
				__instance.descriptionText.fontStyle = FontStyles.Normal;

				// rebuild tabs AFTER vanilla clearing to support default tab
				PrepareLiteShopPanel(__instance);
				BuildLiteShopTabs(__instance);
			}
		}

		// suppress og content from loading
		[HarmonyPatch(typeof(ComplexRulesPanel), nameof(ComplexRulesPanel.LoadRule))]
		class Patch_ComplexRulesPanel_LoadRule
		{
			static bool Prefix(ComplexRulesPanel __instance)
			{
				if (LiteShopPanels.Contains(__instance))
					return false;

				return true;
			}
		}
		// suppress og content from loading
		[HarmonyPatch(typeof(ComplexRulesPanel), nameof(ComplexRulesPanel.SetDescription))]
		class Patch_ComplexRulesPanel_SetDescription
		{
			static bool Prefix(ComplexRulesPanel __instance)
			{
				if (LiteShopPanels.Contains(__instance))
					return false;

				return true;
			}
		}

		// Load button template from settings panel
		[HarmonyPatch(typeof(SettingPanel), nameof(SettingPanel.Awake)), HarmonyPostfix]
		private static void LoadButtonTemplate(SettingPanel __instance)
		{
			if (_buttonTemplate != null) return;

			try
			{
				var preferenceTab = __instance.tabs.FirstOrDefault(t => t.name == "Preference");
				if (preferenceTab == null)
				{
					return;
				}

				var rightPanel = preferenceTab.transform.Find("RightPanel");
				if (rightPanel == null)
				{
					return;
				}

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
				else
				{
				}
			}
			catch (Exception)
			{
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

					// move button higher in the menu for cyan's companion
					var rectTransform = newBtnGO.GetComponent<RectTransform>();
					if (rectTransform != null)
					{
						var anchoredPosition = rectTransform.anchoredPosition;
						anchoredPosition.y += 240f;
						rectTransform.anchoredPosition = anchoredPosition;
					}
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
			catch (Exception)
			{
			}
		}

		private static LiteShop GetMenuShop()
		{
			var customData = MiniTracker.Instance?.CustomGrSaveData ?? MiniTracker.LoadedFromDiskCustomGrSaveData;
			if (customData == null)
				return null;

			var profile = GameMaster.Instance?.CurrentProfile;
			if (profile == null)
				return null;

			var key = $"profile:{profile.CreationTimestamp}_{profile.Name}";
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
			{
				_originalFontSizes[clone] = clone.descriptionText.fontSize;
			}

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
			{
				return;
			}

			if (HasActiveGameRun())
			{
				return;
			}

			menuShop.ChallengerModeEnabled = !menuShop.ChallengerModeEnabled;
			UpdateMainMenuButtonLabel();
			ShopSaveLoader.Save();
		}
		private static void ShowLiteShopCategory(ComplexRulesPanel panel, string categoryId)
		{
			panel.entityContent.DestroyChildren();
			panel._entityList.Clear();

			var locale = LBoL.Core.Localization.CurrentLocale;
			_currentCategoryId = categoryId;
			_currentShop = MiniTracker.Instance?.CustomGrSaveData?.GetShopForCurrentProfile();

			// Special handling for the default "Roguelite Shop" tab - show description and Challenger Mode toggle
			if (categoryId == "LiteShopButton")
			{
				var baseDesc = GetShopLoc($"{LocalisationKeys.ShopPrefix}LiteShopButton.Desc");
				bool normalizeActive = _currentShop?.GetItem("difficulty.reverse")?.CurrentTier > 0;
				if (normalizeActive)
					baseDesc += GetShopLoc($"{LocalisationKeys.ShopPrefix}LiteShopButton.Desc.Normalize");
				bool isEnabled = _currentShop?.ChallengerModeEnabled ?? false;
				string statusKey = isEnabled
					? $"{LocalisationKeys.ShopPrefix}ChallengerModeToggle.On"
					: $"{LocalisationKeys.ShopPrefix}ChallengerModeToggle.Off";
				string statusLine = $"{GetShopLoc($"{LocalisationKeys.ShopPrefix}ChallengerModeToggleFront")}{GetShopLoc(statusKey)}{GetShopLoc($"{LocalisationKeys.ShopPrefix}ChallengerModeToggleBack")}";
				panel.descriptionText.text = StringDecorator.Decorate($"{baseDesc}\n{statusLine}");

				// Position toggle directly under the status line
				var descRect = panel.descriptionText.GetComponent<RectTransform>();
				float availableWidth = descRect.rect.width - panel.descriptionText.margin.x - panel.descriptionText.margin.z;
				float defaultDescHeight = panel.descriptionText.GetPreferredValues(availableWidth, float.PositiveInfinity).y;
				float defaultLeftMargin = panel.descriptionText.margin.x;
				const float togglePadding = 16f;
				float toggleY = defaultDescHeight + togglePadding;

				CreateChallengerModeToggle(panel, defaultLeftMargin, toggleY);

				panel.entityContent.sizeDelta = new Vector2(0f, toggleY + 100f);
				panel.rightContent.sizeDelta = new Vector2(0f, defaultDescHeight + toggleY + 100f);
				panel.SetContentSize();
				return;
			}

			if (categoryId == "Loadout")
			{
				string loadoutMoneyText = "";
				if (_currentShop != null)
				{
					var moneyFormat = GetShopLoc($"{LocalisationKeys.ShopPrefix}Money");
					loadoutMoneyText = $"<b>{string.Format(moneyFormat, _currentShop.MoneyOwned)}</b>";
				}
				panel.descriptionText.text = StringDecorator.Decorate(loadoutMoneyText);

				float loadoutLeftMargin = panel.descriptionText.margin.x;
				var loadoutDescRect = panel.descriptionText.GetComponent<RectTransform>();
				float loadoutAvailableWidth = loadoutDescRect.rect.width - panel.descriptionText.margin.x - panel.descriptionText.margin.z;
				float loadoutDescHeight = panel.descriptionText.GetPreferredValues(loadoutAvailableWidth, float.PositiveInfinity).y;
				const float loadoutTopPadding = -320f;
				float loadoutCurrentY = loadoutDescHeight + loadoutTopPadding;
				const float loadoutTextPadding = 10f;
				const float categoryIndent = 20f;

				var categoryOrder = new[]
				{
					LocalisationKeys.DifficultyPrefix,
					LocalisationKeys.InitPrefix,
					LocalisationKeys.DiscountPrefix,
					LocalisationKeys.FeaturePrefix,
					LocalisationKeys.BattlePrefix,
					LocalisationKeys.AlterPrefix,
				};

				bool anyCategoryItems = false;

				// Build tooltip mapping for categories and items
				var tooltipMap = new Dictionary<string, LoadoutTooltipData>(StringComparer.Ordinal);
				foreach (var prefix in categoryOrder)
				{
					var items = _currentShop?.Items.Values
						.Where(item => item.CurrentTier > 0 && item.Id.StartsWith(prefix, StringComparison.Ordinal))
						.ToList();
					if (items == null || items.Count == 0)
						continue;
					anyCategoryItems = true;

					string catId = prefix[..^1];
					string categoryKey = $"{LocalisationKeys.ShopPrefix}{catId}";
					string categoryName = GetShopLoc(categoryKey);
					// Add category tooltip data if description exists
					var categoryDesc = LocalisationKeys.GetShopItemDescription(catId, false);
					if (!string.IsNullOrWhiteSpace(categoryDesc))
						tooltipMap[catId] = new LoadoutTooltipData(StringDecorator.Decorate(categoryName), StringDecorator.Decorate(categoryDesc));

					// Render category header as a link so TMP link hover can be detected
					var headerRect = CreateItemText(panel, StringDecorator.Decorate($"<link=\"{catId}\">{categoryName}</link>"), loadoutCurrentY, loadoutLeftMargin);
					if (headerRect != null)
					{
						// Attach tooltip for the category header
						AttachLoadoutTooltip(headerRect.gameObject, tooltipMap);
						loadoutCurrentY -= headerRect.sizeDelta.y + loadoutTextPadding;
					}

					foreach (var item in items)
					{
						string nameKey = $"{LocalisationKeys.ShopPrefix}{item.Id}";
						string name = GetShopLoc(nameKey);
						if (name == nameKey)
							name = item.Id;
						string coloredName = LocalisationKeys.ColorizeTierName(name, item.CurrentTier, item.MaxTier);
						// Render item name as a link so TMP link hover can be detected
						string line = $"<link=\"{item.Id}\">{coloredName}</link>";
						if (item.MaxTier > 1)
							line = $"{line} {item.CurrentTier}";
						// Add item tooltip data if description exists
						var itemDesc = LocalisationKeys.GetShopItemDescription(item.Id, false);
						if (!string.IsNullOrWhiteSpace(itemDesc))
							tooltipMap[item.Id] = new LoadoutTooltipData(StringDecorator.Decorate(name), StringDecorator.Decorate(itemDesc));

						var itemRect = CreateItemText(panel, StringDecorator.Decorate(line), loadoutCurrentY, loadoutLeftMargin + categoryIndent);
						if (itemRect != null)
						{
							// Attach tooltip handler to this item text so hovering shows the tooltip
							AttachLoadoutTooltip(itemRect.gameObject, tooltipMap);
							loadoutCurrentY -= itemRect.sizeDelta.y + loadoutTextPadding;
						}
					}
				}

				// If no modifiers were purchased, show a friendly 'None' entry
				if (!anyCategoryItems)
				{
					string noneKey = $"{LocalisationKeys.ShopPrefix}Loadout.None";
					string noneText = GetShopLoc(noneKey);
					if (noneText == noneKey)
						noneText = GetShopLoc($"{LocalisationKeys.ShopPrefix}Loadout");
					var noneRect = CreateItemText(panel, StringDecorator.Decorate($"<b>|{noneText}|</b>"), loadoutCurrentY, loadoutLeftMargin);
					if (noneRect != null)
						loadoutCurrentY -= noneRect.sizeDelta.y + loadoutTextPadding;
				}

				var refundItem = _currentShop?.GetItem("refund");
				if (refundItem != null)
				{
					RenderShopItem(panel, "refund", refundItem, ref loadoutCurrentY, loadoutLeftMargin);
					// attach tooltip for refund entry if present in map
					// (refund is rendered via RenderShopItem which calls CreateItemText internally)
				}


				panel.entityContent.sizeDelta = new Vector2(0f, Mathf.Abs(loadoutCurrentY));
				panel.rightContent.sizeDelta = new Vector2(0f, loadoutDescHeight + Mathf.Abs(loadoutCurrentY));
				return;
			}

			// Display current money and category desc in the description text
			var moneyText = "";
			if (_currentShop != null)
			{
				var moneyFormat = GetShopLoc($"{LocalisationKeys.ShopPrefix}Money");
				moneyText = $"<b>{string.Format(moneyFormat, _currentShop.MoneyOwned)}</b>";
			}
			var categoryDescKey = GetShopLoc($"{LocalisationKeys.ShopPrefix}{categoryId}.Desc");
			panel.descriptionText.text = StringDecorator.Decorate(moneyText + "\n" + categoryDescKey);
			if (categoryId == "difficulty")
			{
				panel.descriptionText.text = StringDecorator.Decorate(categoryDescKey);
			}

			// Match descriptionText's left margin for alignment
			float leftMargin = panel.descriptionText.margin.x;
			var contentDescRect = panel.descriptionText.GetComponent<RectTransform>();
			float contentAvailableWidth = contentDescRect.rect.width - panel.descriptionText.margin.x - panel.descriptionText.margin.z;
			float contentDescHeight = panel.descriptionText.GetPreferredValues(contentAvailableWidth, float.PositiveInfinity).y;
			const float topPadding = -450f;
			float currentY = contentDescHeight + topPadding;

			// Special-case: the Refund tab is a single standalone item (Shop.refund)
			if (categoryId == "refund")
			{
				var refundItem = _currentShop?.GetItem("refund");
				if (refundItem != null)
				{
					RenderShopItem(panel, "refund", refundItem, ref currentY, leftMargin);
				}
				panel.entityContent.sizeDelta = new Vector2(0f, Mathf.Abs(currentY));
				panel.rightContent.sizeDelta = new Vector2(0f, contentDescHeight + Mathf.Abs(currentY));
				return;
			}
			// Create text and button elements for each item in entityContent
			foreach (var itemKey in GetCategoryItems(locale, categoryId))
			{
				var itemId = itemKey[LocalisationKeys.ShopPrefix.Length..];
				var item = _currentShop?.GetItem(itemId);
				if (item == null) continue;
				RenderShopItem(panel, itemId, item, ref currentY, leftMargin);
			}

			// Set entityContent height to fit all elements
			panel.entityContent.sizeDelta = new Vector2(0f, Mathf.Abs(currentY));

			// Update right content size
			panel.rightContent.sizeDelta = new Vector2(0f, contentDescHeight + Mathf.Abs(currentY));
		}

		private static RectTransform CreateItemText(ComplexRulesPanel panel, string text, float yPosition, float leftMargin = 0f)
		{
			try
			{
				// Create a container GameObject for the text
				var textContainer = new GameObject("ItemText");
				textContainer.transform.SetParent(panel.entityContent, false);
				textContainer.SetActive(true);

				// Add RectTransform
				var rectTransform = textContainer.AddComponent<RectTransform>();
				rectTransform.anchorMin = new Vector2(0, 1);
				rectTransform.anchorMax = new Vector2(0, 1);
				rectTransform.pivot = new Vector2(0, 1);

				// Add TextMeshProUGUI
				var textComponent = textContainer.AddComponent<TextMeshProUGUI>();
				textComponent.text = text;
				textComponent.font = TMP_Settings.defaultFontAsset ?? panel.descriptionText.font;
				textComponent.fontWeight = FontWeight.Regular;
				textComponent.fontStyle = FontStyles.Normal;
				textComponent.fontSize = panel.descriptionText.fontSize;
				textComponent.color = Color.white;
				textComponent.textWrappingMode = TextWrappingModes.Normal;
				textComponent.alignment = TextAlignmentOptions.TopLeft;
				textComponent.margin = new Vector4(0, 0, 0, 0);

				// Calculate available width (total width minus left margin and some right padding)
				float availableWidth = panel.entityContent.rect.width - leftMargin - 10f;
				var preferredHeight = textComponent.GetPreferredValues(availableWidth, float.MaxValue).y;
				rectTransform.sizeDelta = new Vector2(availableWidth, preferredHeight + 10f);
				// Position the text element with left margin
				rectTransform.anchoredPosition = new Vector2(leftMargin, yPosition);
				panel._entityList.Add(textContainer);
				return rectTransform;
			}
			catch (Exception)
			{
				return null;
			}
		}

		private static void AttachLoadoutTooltip(GameObject textContainer, Dictionary<string, LoadoutTooltipData> tooltipMap)
		{
			try
			{
				if (textContainer == null || tooltipMap == null || tooltipMap.Count == 0)
					return;

				var textComp = textContainer.GetComponent<TextMeshProUGUI>();
				if (textComp == null)
					return;

				// Raycast target
				var raycastObj = new GameObject("LoadoutRaycast", typeof(RectTransform));
				raycastObj.transform.SetParent(textContainer.transform, false);
				var raycastRect = raycastObj.GetComponent<RectTransform>();
				raycastRect.anchorMin = Vector2.zero;
				raycastRect.anchorMax = Vector2.one;
				raycastRect.pivot = new Vector2(0.5f, 0.5f);
				raycastRect.anchoredPosition = Vector2.zero;
				raycastRect.localScale = Vector3.one;
				raycastRect.sizeDelta = Vector2.zero;
				var image = raycastObj.AddComponent<Image>();
				image.color = new Color(1f, 1f, 1f, 0f);
				image.raycastTarget = true;

				// Tooltip anchor: attach under the root canvas so positioning matches watermark behavior
				var anchorObj = new GameObject("LoadoutTooltipAnchor", typeof(RectTransform));
				Canvas canvas = textComp.canvas ?? textContainer.GetComponentInParent<Canvas>();
				Transform anchorParent = canvas != null ? canvas.transform : textContainer.transform;
				anchorObj.transform.SetParent(anchorParent, false);
				var anchorRect = anchorObj.GetComponent<RectTransform>();
				anchorRect.anchorMin = new Vector2(1f, 1f);
				anchorRect.anchorMax = new Vector2(1f, 1f);
				anchorRect.pivot = new Vector2(0.5f, 0.5f);
				anchorRect.anchoredPosition = Vector2.zero;
				anchorRect.localScale = Vector3.one;
				anchorRect.sizeDelta = new Vector2(1f, 1f);

				var tooltip = SimpleTooltipSource.CreateDirect(anchorObj, string.Empty).WithPosition(TooltipDirection.Bottom, TooltipAlignment.Min);

				var handler = raycastObj.AddComponent<LoadoutItemTooltip>();
				handler.SetData(textComp, tooltip, anchorRect, tooltipMap);
			}
			catch (Exception)
			{
			}
		}

		private sealed class LoadoutItemTooltip : MonoBehaviour, IPointerMoveHandler, IPointerExitHandler
		{
			private TextMeshProUGUI _text;
			private SimpleTooltipSource _tooltip;
			private RectTransform _anchorRect;
			private Dictionary<string, LoadoutTooltipData> _map = new Dictionary<string, LoadoutTooltipData>(StringComparer.Ordinal);
			private string _currentId;
			private bool _tooltipVisible;
			private bool _suppressPointerEvents;

			public void SetData(TextMeshProUGUI text, SimpleTooltipSource tooltip, RectTransform anchorRect, Dictionary<string, LoadoutTooltipData> map)
			{
				_text = text;
				_tooltip = tooltip;
				_anchorRect = anchorRect;
				_map = map ?? new Dictionary<string, LoadoutTooltipData>(StringComparer.Ordinal);
				ClearTooltip();
			}

			public void OnPointerMove(PointerEventData eventData)
			{
				if (_text == null || _tooltip == null)
					return;
				if (EventSystem.current == null || _text.textInfo == null || _text.textInfo.linkCount == 0)
				{
					ClearTooltip();
					return;
				}

				int linkIndex = TMP_TextUtilities.FindIntersectingLink(_text, eventData.position, eventData.enterEventCamera);
				if (linkIndex == -1)
				{
					ClearTooltip();
					return;
				}

				string itemId = _text.textInfo.linkInfo[linkIndex].GetLinkID();
				if (string.IsNullOrEmpty(itemId) || itemId == _currentId)
					return;

				if (!_map.TryGetValue(itemId, out var data) || string.IsNullOrWhiteSpace(data.Effect))
				{
					ClearTooltip();
					return;
				}

				_currentId = itemId;
				UpdateAnchorToLink(linkIndex);
				_tooltip.SetDirect(data.Title, data.Effect);
				TriggerTooltipEnter(eventData);
			}

			public void OnPointerExit(PointerEventData eventData)
			{
				if (_suppressPointerEvents)
					return;

				ClearTooltip();
			}

			private void ClearTooltip()
			{
				_currentId = null;
				if (_tooltip == null)
					return;

				_tooltip.SetDirect(string.Empty, string.Empty);
				TriggerTooltipExit();
			}

			private void TriggerTooltipEnter(PointerEventData eventData)
			{
				if (_tooltipVisible || EventSystem.current == null)
					return;

				_suppressPointerEvents = true;
				ExecuteEvents.Execute<IPointerEnterHandler>(
					_tooltip.gameObject,
					eventData,
					ExecuteEvents.pointerEnterHandler
				);
				_suppressPointerEvents = false;
				_tooltipVisible = true;
			}

			private void TriggerTooltipExit()
			{
				if (!_tooltipVisible || EventSystem.current == null)
					return;

				_suppressPointerEvents = true;
				ExecuteEvents.Execute<IPointerExitHandler>(
					_tooltip.gameObject,
					new PointerEventData(EventSystem.current),
					ExecuteEvents.pointerExitHandler
				);
				_suppressPointerEvents = false;
				_tooltipVisible = false;
			}

			private void UpdateAnchorToLink(int linkIndex)
			{
				if (_anchorRect == null || _text == null || _text.textInfo == null)
					return;

				if (!TryGetLinkBounds(linkIndex, out var center, out var size))
					return;

				var paddedSize = size + new Vector2(6f, 4f);
				// Use world-space placement so the tooltip anchor sits directly under the hovered link
				_anchorRect.anchorMin = new Vector2(0.5f, 0.5f);
				_anchorRect.anchorMax = new Vector2(0.5f, 0.5f);
				// pivot set to left so tooltip aligns to the start of link
				_anchorRect.pivot = new Vector2(0f, 0.5f);
				_anchorRect.sizeDelta = paddedSize;
				// Transform the link-local center into world space and set the anchor's world position
				var leftX = center.x - (size.x * 0.5f);
				var worldLeft = _text.transform.TransformPoint(new Vector3(leftX, center.y, 0f));
				var canvas = _text.canvas;
				if (canvas != null)
				{
					var canvasRect = canvas.GetComponent<RectTransform>();
					_anchorRect.anchoredPosition = canvasRect.InverseTransformPoint(worldLeft);
				}
				else
				{
					_anchorRect.position = worldLeft;
				}
			}

			private bool TryGetLinkBounds(int linkIndex, out Vector2 center, out Vector2 size)
			{
				center = Vector2.zero;
				size = Vector2.zero;
				if (_text.textInfo.linkInfo == null || linkIndex < 0 || linkIndex >= _text.textInfo.linkCount)
					return false;

				TMP_LinkInfo linkInfo = _text.textInfo.linkInfo[linkIndex];
				int start = linkInfo.linkTextfirstCharacterIndex;
				int length = linkInfo.linkTextLength;
				var chars = _text.textInfo.characterInfo;
				if (start < 0 || start >= chars.Length || length <= 0)
					return false;

				float minX = float.PositiveInfinity;
				float minY = float.PositiveInfinity;
				float maxX = float.NegativeInfinity;
				float maxY = float.NegativeInfinity;
				bool hasVisible = false;
				int end = Math.Min(start + length, chars.Length);
				for (int i = start; i < end; i++)
				{
					var ch = chars[i];
					if (!ch.isVisible)
						continue;

					hasVisible = true;
					minX = Math.Min(minX, ch.bottomLeft.x);
					maxX = Math.Max(maxX, ch.topRight.x);
					minY = Math.Min(minY, ch.descender);
					maxY = Math.Max(maxY, ch.ascender);
				}

				if (!hasVisible)
					return false;

				size = new Vector2(maxX - minX, maxY - minY);
				center = new Vector2(minX + size.x * 0.5f, minY + size.y * 0.5f);
				return true;
			}

			private void OnDisable()
			{
				// Ensure the tooltip is hidden if this handler is disabled (e.g., panel closed with Escape)
				try
				{
					ClearTooltip();
				}
				catch (Exception)
				{
					// Swallow to avoid noise during teardown
				}
			}
		}

		private static void RenderShopItem(ComplexRulesPanel panel, string itemId, ShopItem item, ref float currentY, float leftMargin)
		{
			if (item == null)
				return;

			const float textPadding = 10f;
			const float buttonPadding = 8f;

			var name = GetShopLoc($"{LocalisationKeys.ShopPrefix}{itemId}");
			if (item.IsAlpha)
			{
				var suffix = GetShopLoc($"{LocalisationKeys.ShopPrefix}InDevelopmentSuffix");
				name += suffix;
			}
			name = LocalisationKeys.ColorizeTierName(name, item.CurrentTier, item.MaxTier);
			var desc = StringDecorator.Decorate(LocalisationKeys.GetShopItemDescription(itemId));

			var itemInfoSb = new StringBuilder();
			itemInfoSb.Append($"\n<b>{name}</b>\n");
			if (!string.IsNullOrEmpty(desc))
				itemInfoSb.Append($"{desc}\n");

			var tierFormat = GetShopLoc($"{LocalisationKeys.ShopPrefix}Tier");
			itemInfoSb.Append(string.Format(tierFormat, item.CurrentTier, item.MaxTier));

			if (!item.IsMaxTier)
			{
				var nextEffectKey = $"{LocalisationKeys.ShopPrefix}{itemId}.Next";
				if (TryGetShopLoc(nextEffectKey, out var nextEffectTemplate) && !string.IsNullOrEmpty(nextEffectTemplate))
				{
					int currentValue = item.Initial + (item.Delta * item.CurrentTier);
					int nextValue = item.Initial + (item.Delta * (item.CurrentTier + 1));
					if (item.Initial != 0)
					{
						nextValue = currentValue;
						currentValue = item.Initial + (item.Delta * (item.CurrentTier - 1));
					}
					itemInfoSb.Append("\n");
					itemInfoSb.Append(string.Format(nextEffectTemplate, $"|c:{currentValue}|", $"|c:{nextValue}|"));
				}

				var nextCost = item.GetNextTierCost();
				var canAfford = _currentShop.CanPurchase(itemId);
				var nextTierFormat = GetShopLoc($"{LocalisationKeys.ShopPrefix}NextTier");
				itemInfoSb.Append(string.Format(nextTierFormat, nextCost));
				if (!canAfford)
				{
					var notEnoughText = GetShopLoc($"{LocalisationKeys.ShopPrefix}NotEnoughBP");
					itemInfoSb.Append(notEnoughText);
				}
			}
			else
			{
				var maxTierText = GetShopLoc($"{LocalisationKeys.ShopPrefix}MaxTier");
				itemInfoSb.Append(maxTierText);
			}

			var textElement = CreateItemText(panel, StringDecorator.Decorate(itemInfoSb.ToString()), currentY, leftMargin);
			if (textElement != null)
				currentY -= textElement.sizeDelta.y + textPadding;

			if (!item.IsMaxTier)
			{
				CreatePurchaseButton(panel, itemId, currentY - 30f, leftMargin);
			}
			if (item.CurrentTier > 0)
			{
				CreateRefundButton(panel, itemId, currentY - 30f, leftMargin + 660f);
			}
			if (!item.IsMaxTier || item.CurrentTier > 0)
			{
				currentY -= 80f + buttonPadding;
			}
		}

		private static void CreatePurchaseButton(ComplexRulesPanel panel, string itemId, float yPosition, float leftMargin = 0f)
		{
			try
			{
				if (!TryCreateButtonBase(
					panel,
					$"PurchaseButton_{itemId}",
					new Vector2(300, 60),
					new Vector2(leftMargin, yPosition),
					DefaultButtonScale,
					out var buttonObj,
					out var buttonChild,
					out var button))
				{
					return;
				}

				if (button != null)
				{
					button.onClick = new Button.ButtonClickedEvent();
					button.onClick.AddListener(() => OnPurchaseClicked(panel, itemId));
					bool hasActiveRun = HasActiveGameRun();
					button.interactable = !hasActiveRun && (_currentShop?.CanPurchase(itemId) ?? false);
				}

				SetButtonText(buttonChild, $"{LocalisationKeys.ShopPrefix}BuyButton", DefaultButtonFontSize, true);
				SimpleTooltipSource.CreateWithGeneralKey(
					buttonObj,
					$"{LocalisationKeys.ShopPrefix}BuyButton",
					$"{LocalisationKeys.ShopPrefix}BuyButton.Desc"
				).WithPosition(TooltipDirection.Bottom, TooltipAlignment.Min);
				panel._entityList.Add(buttonObj);
			}
			catch (Exception)
			{
			}
		}

		private static void CreateRefundButton(ComplexRulesPanel panel, string itemId, float yPosition, float leftMargin = 0f)
		{
			try
			{
				if (!TryCreateButtonBase(
					panel,
					$"RefundButton_{itemId}",
					new Vector2(300, 60),
					new Vector2(leftMargin, yPosition),
					DefaultButtonScale,
					out var buttonObj,
					out var buttonChild,
					out var button))
				{
					return;
				}

				if (button != null)
				{
					button.onClick = new Button.ButtonClickedEvent();
					button.onClick.AddListener(() => OnRefundClicked(panel, itemId));
					button.interactable = !HasActiveGameRun();
				}

				SetButtonText(buttonChild, $"{LocalisationKeys.ShopPrefix}RefundButton", DefaultButtonFontSize, false);
				SimpleTooltipSource.CreateWithGeneralKey(
					buttonObj,
					$"{LocalisationKeys.ShopPrefix}RefundButton",
					$"{LocalisationKeys.ShopPrefix}RefundButton.Desc"
				).WithPosition(TooltipDirection.Bottom, TooltipAlignment.Min);
				panel._entityList.Add(buttonObj);
			}
			catch (Exception)
			{
			}
		}

		private static void OnPurchaseClicked(ComplexRulesPanel panel, string itemId)
		{
			try
			{
				if (_currentShop == null)
				{
					return;
				}

				void PurchaseItem()
				{
					bool success = _currentShop.Purchase(itemId);
					if (success)
					{
						AudioManager.PlayUi("Bought", false);
						// Refresh the UI to show updated state
						ShowLiteShopCategory(panel, _currentCategoryId);
						// Save immediately to persist changes
						ShopSaveLoader.Save();
					}
					else
					{
						// AudioManager.PlaySfx("SystemFx_Error");
					}
				}

				if (itemId == "refund")
				{
					CreateWidgets.ConfirmationPopup(
						$"{LocalisationKeys.ShopPrefix}refund.Confirm",
						PurchaseItem
					);
					return;
				}

				PurchaseItem();
			}
			catch (Exception)
			{
			}
		}

		private static void OnRefundClicked(ComplexRulesPanel panel, string itemId)
		{
			try
			{
				if (_currentShop == null)
				{
					return;
				}

				var item = _currentShop.GetItem(itemId);
				if (item != null && item.CurrentTier > 0)
				{
					_currentShop.Refund(item);
					// Refresh the UI to show updated state
					ShowLiteShopCategory(panel, _currentCategoryId);
					// Save immediately to persist changes
					ShopSaveLoader.Save();
				}
				else
				{
					// AudioManager.PlaySfx("SystemFx_Error");
				}
			}
			catch (Exception)
			{
			}
		}

		private static void CreateChallengerModeToggle(ComplexRulesPanel panel, float leftMargin, float yPosition)
		{
			try
			{
				if (!TryCreateButtonBase(
					panel,
					"ChallengerModeToggle",
					new Vector2(600, 80),
					new Vector2(leftMargin, yPosition),
					1f,
					out var buttonObj,
					out var buttonChild,
					out var button))
				{
					return;
				}

				if (button != null)
				{
					button.onClick = new Button.ButtonClickedEvent();
					button.onClick.AddListener(() => OnChallengerModeToggleClicked(panel, buttonObj, buttonChild));
					button.interactable = !HasActiveGameRun();
				}

				UpdateChallengerModeButtonText(buttonChild);

				SimpleTooltipSource.CreateWithGeneralKey(
					buttonObj,
					$"{LocalisationKeys.ShopPrefix}ChallengerModeToggle",
					$"{LocalisationKeys.ShopPrefix}ChallengerModeToggle.Desc"
				).WithPosition(TooltipDirection.Bottom, TooltipAlignment.Min);

				panel._entityList.Add(buttonObj);
			}
			catch (Exception)
			{
			}
		}

		private static void UpdateChallengerModeButtonText(Transform buttonChild)
		{
			var textPath = buttonChild.Find("Text (TMP)") ?? buttonChild.Find("Layout/Text (TMP)");
			if (textPath != null)
			{
				var textComponent = textPath.GetComponent<TextMeshProUGUI>();
				if (textComponent != null)
				{
					bool isEnabled = _currentShop?.ChallengerModeEnabled ?? false;
					string buttonKey = isEnabled
						? $"{LocalisationKeys.ShopPrefix}ChallengerModeToggle.ButtonOn"
						: $"{LocalisationKeys.ShopPrefix}ChallengerModeToggle.ButtonOff";
					textComponent.text = GetShopLoc(buttonKey);
					textComponent.fontSize = GetLocalizedFontSize(DefaultButtonFontSize, true);
				}
			}
		}

		private static bool TryCreateButtonBase(
			ComplexRulesPanel panel,
			string name,
			Vector2 size,
			Vector2 anchoredPosition,
			float scale,
			out GameObject buttonObj,
			out Transform buttonChild,
			out Button button)
		{
			buttonObj = null;
			buttonChild = null;
			button = null;

			if (_buttonTemplate == null)
			{
				return false;
			}

			buttonObj = Object.Instantiate(_buttonTemplate, panel.entityContent, false);
			buttonObj.name = name;
			buttonObj.SetActive(true);

			buttonChild = buttonObj.transform.Find("ResetHint") ?? buttonObj.transform;
			buttonObj.transform.localScale = Vector3.one * scale;

			var rectTransform = buttonObj.GetComponent<RectTransform>();
			rectTransform.anchorMin = new Vector2(0, 1);
			rectTransform.anchorMax = new Vector2(0, 1);
			rectTransform.pivot = new Vector2(0, 1);
			rectTransform.sizeDelta = size;
			rectTransform.anchoredPosition = anchoredPosition;

			button = buttonChild.GetComponent<Button>();
			return true;
		}

		private static void SetButtonText(Transform buttonChild, string key, float baseSize, bool alignCenter)
		{
			var textPath = buttonChild.Find("Text (TMP)") ?? buttonChild.Find("Layout/Text (TMP)");
			if (textPath == null)
				return;

			var textComponent = textPath.GetComponent<TextMeshProUGUI>();
			if (textComponent == null)
				return;

			textComponent.text = GetShopLoc(key);
			textComponent.fontSize = GetLocalizedFontSize(baseSize, true);
			if (alignCenter)
				textComponent.alignment = TextAlignmentOptions.Center;
		}

		private static float GetLocalizedFontSize(float baseSize, bool applyShrink)
		{
			if (!applyShrink)
				return baseSize;

			var locale = LBoL.Core.Localization.CurrentLocale;
			if (LocalizedText.FinalFallbackResizeTable.TryGetValue(locale, out var resizeFactor) && resizeFactor < 1f)
				return baseSize * resizeFactor;

			return baseSize;
		}

		private static void OnChallengerModeToggleClicked(ComplexRulesPanel panel, GameObject buttonObj, Transform buttonChild)
		{
			try
			{
				if (HasActiveGameRun())
				{
					return;
				}

				if (_currentShop == null)
				{
					return;
				}

				// Toggle the state
				_currentShop.ChallengerModeEnabled = !_currentShop.ChallengerModeEnabled;

				// Update button text to reflect new state
				UpdateChallengerModeButtonText(buttonChild);
				if (_currentCategoryId == "LiteShopButton")
				{
					ShowLiteShopCategory(panel, _currentCategoryId);
				}
				UpdateMainMenuButtonLabel();

				// Save immediately to persist changes
				ShopSaveLoader.Save();

			}
			catch (Exception)
			{
			}
		}

		private static bool HasActiveGameRun()
		{
			return GameMaster.Instance?.GameRunSaveData != null;
		}

		private static void BuildLiteShopTabs(ComplexRulesPanel panel)
		{
			panel.titleRoot.DestroyChildren();

			var locale = LBoL.Core.Localization.CurrentLocale;
			var prefix = LocalisationKeys.ShopPrefix;

			var categories = GetShopCategories(locale).ToList();

			var shopRoot = $"{prefix}LiteShopButton";
			var loadoutKey = $"{prefix}Loadout";
			if (categories.Remove(shopRoot))
				categories.Insert(0, shopRoot);
			if (categories.Remove(loadoutKey))
				categories.Insert(1, loadoutKey);

			string firstCategoryId = null;
			Button firstButton = null;

			for (int i = 0; i < categories.Count; i++)
			{
				var key = categories[i];

				var go = Object.Instantiate(panel.titleTemplate, panel.titleRoot);
				go.SetActive(true);

				go.GetComponentInChildren<TextMeshProUGUI>().text =
					GetShopLoc(key);

				var categoryId = key[prefix.Length..];
				var button = go.GetComponent<Button>();

				button.onClick.AddListener(() =>
				{
					ShowLiteShopCategory(panel, categoryId);
				});

				if (i == 0)
				{
					firstCategoryId = categoryId;
					firstButton = button;
					go.AddComponent<GamepadNavigationOrigin>();
				}
			}

			if (firstCategoryId != null)
			{
				ShowLiteShopCategory(panel, firstCategoryId);
			}
		}

		private static void PrepareLiteShopPanel(ComplexRulesPanel panel)
		{
			panel._stringTable = new Dictionary<string, ComplexRulesPanel.StringTableEntry>();

			panel._entityList.Clear();
			panel.entityContent.DestroyChildren();

			panel.titleRoot.DestroyChildren();
			panel.descriptionText.text = "";

			panel.titleTemplate.SetActive(false);

			// Apply size compensation once during initial setup
			ApplyDescriptionTextSizeCompensation(panel);

			panel.SetContentSize();
		}

		/// <summary>
		/// Compensates for LocalizedText automatic font size reduction on certain locales.
		/// LocalizedText reduces font size to 0.8f for English and other non-Asian languages,
		/// but keeps 1.0f for Chinese/Japanese. We compensate by applying inverse multiplier.
		/// Uses the boot locale (when game started) and original font size to prevent stacking.
		/// </summary>
		private static void ApplyDescriptionTextSizeCompensation(ComplexRulesPanel panel)
		{
			if (!_bootLocale.HasValue || panel.descriptionText == null)
				return;

			// Get or store the original font size
			if (!_originalFontSizes.TryGetValue(panel, out var originalSize))
			{
				originalSize = panel.descriptionText.fontSize;
				_originalFontSizes[panel] = originalSize;
			}

			// Get the resize factor that LocalizedText would apply for the boot locale
			float localizedTextResize = 1f;
			if (LocalizedText.FinalFallbackResizeTable.TryGetValue(_bootLocale.Value, out var resizeFactor))
			{
				localizedTextResize = resizeFactor;
			}

			// If LocalizedText shrinks the text (e.g., 0.8f for English),
			// we compensate by enlarging it back (1.0 / 0.8 = 1.25)
			// Always start from original size to prevent stacking
			if (localizedTextResize < 1f)
			{
				float compensationMultiplier = 1f / localizedTextResize;
				panel.descriptionText.fontSize = originalSize * compensationMultiplier;
			}
			else
			{
				// Restore to original size if no compensation needed
				panel.descriptionText.fontSize = originalSize;
			}
		}


		private static string GetShopLoc(string key)
		{
			var locale = LBoL.Core.Localization.CurrentLocale;

			if (LocalisationKeys.LocTable.TryGetValue((locale, key), out var value))
				return value;
			if (LocalisationKeys.LocTable.TryGetValue((Locale.En, key), out var fallback))
				return fallback;

			return key;
		}

		private static bool TryGetShopLoc(string key, out string value)
		{
			var locale = LBoL.Core.Localization.CurrentLocale;
			if (LocalisationKeys.LocTable.TryGetValue((locale, key), out value))
				return true;
			return LocalisationKeys.LocTable.TryGetValue((Locale.En, key), out value);
		}
		private static IEnumerable<string> GetShopCategories(Locale locale)
		{
			var prefix = LocalisationKeys.ShopPrefix;
			var uiTextKeys = new[]
			{
				"Money",
				"Tier",
				"NextTier",
				"NotEnoughBP",
				"MaxTier",
				"ChallengerModeToggle",
				"ChallengerModeToggleFront",
				"ChallengerModeToggleBack",
				"BuyButton",
				"RefundButton",
				"LiteShopButton.Active",
				"LiteShopButton.Inactive",
				"LiteShopButton.Effect",
				"LiteShopButton.Desc.Normalize",
				"InDevelopmentSuffix"
			};

			return LocalisationKeys.LocTable.Keys
				.Where(k => k.Item1 == locale)
				.Select(k => k.Item2)
				.Where(k =>
					k.StartsWith(prefix) &&
					k.IndexOf('.', prefix.Length) == -1 &&
					!uiTextKeys.Contains(k[prefix.Length..])
				)
				.Distinct();
		}

		private static IEnumerable<string> GetCategoryItems(Locale locale, string categoryId)
		{
			var prefix = $"{LocalisationKeys.ShopPrefix}{categoryId}.";
			return LocalisationKeys.LocTable.Keys
				.Where(k => k.Item1 == locale)
				.Select(k => k.Item2)
				.Where(k =>
					k.StartsWith(prefix) &&
					!k.EndsWith(".Desc") &&
					!k.EndsWith(".Next")
				)
				.Distinct();
		}
	}
	public static class LocalisationKeys
	{
		public const string ShopPrefix = "Shop.";
		public const string InitPrefix = "init.";
		public const string DiscountPrefix = "discount.";
		public const string FeaturePrefix = "feature.";
		public const string BattlePrefix = "battle.";
		public const string AlterPrefix = "alter.";
		public const string DifficultyPrefix = "difficulty.";

		private static float Clamp01(float value)
		{
			if (value < 0f)
				return 0f;
			if (value > 1f)
				return 1f;
			return value;
		}

		private static Color LerpColor(Color from, Color to, float t)
		{
			t = Clamp01(t);
			return new Color(
				from.r + (to.r - from.r) * t,
				from.g + (to.g - from.g) * t,
				from.b + (to.b - from.b) * t,
				1f
			);
		}

		private static string ColorToHex(Color color)
		{
			int r = Mathf.Clamp(Mathf.RoundToInt(color.r * 255f), 0, 255);
			int g = Mathf.Clamp(Mathf.RoundToInt(color.g * 255f), 0, 255);
			int b = Mathf.Clamp(Mathf.RoundToInt(color.b * 255f), 0, 255);
			return $"#{r:X2}{g:X2}{b:X2}";
		}

		public static string ColorizeTierName(string name, int currentTier, int maxTier)
		{
			if (string.IsNullOrEmpty(name))
				return name;
			if (maxTier <= 0)
				return name;
			if (currentTier <= 0)
				return name;

			float t = Clamp01(currentTier / (float)maxTier);
			Color color;
			if (maxTier == 1)
			{
				var white = new Color(1f, 1f, 1f);
				var red = new Color(1f, 0.36f, 0.36f);
				color = LerpColor(white, red, t);
			}
			else
			{
				var white = new Color(1f, 1f, 1f);
				var green = new Color(0.3f, 1f, 0.48f);
				var blue = new Color(0.3f, 0.76f, 1f);
				var purple = new Color(0.7f, 0.36f, 1f);
				var gold = new Color(1f, 0.82f, 0.36f);

				if (t <= 0.25f)
					color = LerpColor(white, green, t / 0.25f);
				else if (t <= 0.5f)
					color = LerpColor(green, blue, (t - 0.25f) / 0.25f);
				else if (t <= 0.75f)
					color = LerpColor(blue, purple, (t - 0.5f) / 0.25f);
				else
					color = LerpColor(purple, gold, (t - 0.75f) / 0.25f);
			}

			return $"<color={ColorToHex(color)}>{name}</color>";
		}

		public static readonly Dictionary<(Locale, string), string> LocTable = new Dictionary<(Locale, string), string>
		{
			/////////////
			// ENGLISH //
			/////////////
			[(Locale.En, $"{ShopPrefix}LiteShopButton")] = "Roguelite Shop",
			[(Locale.En, $"{ShopPrefix}LiteShopButton.Active")] = "Roguelite Shop (Active!)",
			[(Locale.En, $"{ShopPrefix}LiteShopButton.Inactive")] = "Roguelite Shop (Inactive)",
			[(Locale.En, $"{ShopPrefix}LiteShopButton.Desc")] = "<sprite=\"Point\" name=\"Point\"> gained at the end of the game under |Challenger Mode| are saved, they can be used in the |Roguelite Shop| to upgrade the Player as the |Challenger|.\n|Challenger Mode|: The Enemy Phase and the Player Phase in the combat round order are swapped.\nThe swap could be turned off by purchasing |Normalize| in |Difficulty|.",
			[(Locale.En, $"{ShopPrefix}LiteShopButton.Desc.Normalize")] = " |p:(Swapping currently cancelled by Difficulty's Normalize)|",
			[(Locale.En, $"{ShopPrefix}LiteShopButton.Effect")] = "<b>Effect</b>: ",
			[(Locale.En, $"{ShopPrefix}{DifficultyPrefix}"[..^1])] = "Difficulty Modifiers",
			[(Locale.En, $"{ShopPrefix}{DifficultyPrefix}Desc")] = "|Difficulty Modifiers| adds different modifiers to |Challenger Mode|, which would affect the <sprite=\"Point\" name=\"Point\"> saved or yield of other resources.",
			[(Locale.En, $"{ShopPrefix}{InitPrefix}"[..^1])] = "Starting Buffs",
			[(Locale.En, $"{ShopPrefix}{InitPrefix}Desc")] = "|Starting Buffs| provide permanent immediate stat advantage at the start, improving the overall game experience.",
			[(Locale.En, $"{ShopPrefix}{DiscountPrefix}"[..^1])] = "Discounts",
			[(Locale.En, $"{ShopPrefix}{DiscountPrefix}Desc")] = "|Discounts| reduce the cost of various shop items and actions, increasing the yield of each <sprite=\"Point\" name=\"Gold\">.",
			[(Locale.En, $"{ShopPrefix}{FeaturePrefix}"[..^1])] = "Feature Upgrades",
			[(Locale.En, $"{ShopPrefix}{FeaturePrefix}Desc")] = "|Feature Upgrades| enhance specific gameplay mechanics, providing extra benefits throughout the run.",
			[(Locale.En, $"{ShopPrefix}{BattlePrefix}"[..^1])] = "Battle Buffs",
			[(Locale.En, $"{ShopPrefix}{BattlePrefix}Desc")] = "|Battle Buffs| grant additional advantages during combat, improving survivability and resource management.",
			[(Locale.En, $"{ShopPrefix}{AlterPrefix}"[..^1])] = "Settings Altering",
			[(Locale.En, $"{ShopPrefix}{AlterPrefix}Desc")] = "|Settings Altering| allows modification of certain game defaults, twisting the rules to the |Challenger|'s liking.",
			[(Locale.En, $"{ShopPrefix}Loadout")] = "Current Modifiers",
			[(Locale.En, $"{ShopPrefix}Loadout.None")] = "None!",

			[(Locale.En, $"{ShopPrefix}{InitPrefix}fp")] = "Attack Boost",
			[(Locale.En, $"{ShopPrefix}{InitPrefix}fp.Desc")] = "At the start of combat, gain {0} |Firepower|.",
			[(Locale.En, $"{ShopPrefix}{InitPrefix}fp.Next")] = "<b>|c:Next|</b>: Gain {1} |Firepower|",
			[(Locale.En, $"{ShopPrefix}{InitPrefix}sp")] = "Defense Boost",
			[(Locale.En, $"{ShopPrefix}{InitPrefix}sp.Desc")] = "At the start of combat, gain {0} |Spirit|.",
			[(Locale.En, $"{ShopPrefix}{InitPrefix}sp.Next")] = "<b>|c:Next|</b>: Gain {1} |Spirit|",
			[(Locale.En, $"{ShopPrefix}{InitPrefix}hp")] = "Block in Red",
			[(Locale.En, $"{ShopPrefix}{InitPrefix}hp.Desc")] = "Begin the game with {0}% extra maximum life.",
			[(Locale.En, $"{ShopPrefix}{InitPrefix}hp.Next")] = "<b>|c:Next|</b>: {1}% extra maximum life",
			[(Locale.En, $"{ShopPrefix}{InitPrefix}gold")] = "Fundraiser",
			[(Locale.En, $"{ShopPrefix}{InitPrefix}gold.Desc")] = "Begin the game with {0} extra <sprite=\"Point\" name=\"Gold\">.",
			[(Locale.En, $"{ShopPrefix}{InitPrefix}gold.Next")] = "<b>|c:Next|</b>: {1} extra <sprite=\"Point\" name=\"Gold\">",
			[(Locale.En, $"{ShopPrefix}{InitPrefix}card")] = "The Chosen One",
			[(Locale.En, $"{ShopPrefix}{InitPrefix}card.Desc")] = "Begin the game by choosing {0} |Ability| card from the last game's Library to add to this game's Library.",
			[(Locale.En, $"{ShopPrefix}{InitPrefix}card.Next")] = "<b>|c:Next|</b>: Choose {1}",
			[(Locale.En, $"{ShopPrefix}{InitPrefix}exhibit")] = "Up an Elite",
			[(Locale.En, $"{ShopPrefix}{InitPrefix}exhibit.Desc")] = "Begin the game by gaining {0} random non-|Mythic| non-|Shining| exhibit that isn't obtained in the last game.",
			[(Locale.En, $"{ShopPrefix}{InitPrefix}exhibit.Next")] = "<b>|c:Next|</b>: Gain {1}",
			[(Locale.En, $"{ShopPrefix}{InitPrefix}solo")] = "Solo Questing",
			[(Locale.En, $"{ShopPrefix}{InitPrefix}solo.Desc")] = "Begin the game with the |Quest Board| exhibit. Clicking it outside of combat can open its Menu and accept |Quests|.",

			[(Locale.En, $"{ShopPrefix}{DiscountPrefix}sc")] = "Spell Cards for Breakfast",
			[(Locale.En, $"{ShopPrefix}{DiscountPrefix}sc.Desc")] = "Reduce the <sprite=\"Point\" name=\"Power\"> cost of the |Spell Card| by {0}%.",
			[(Locale.En, $"{ShopPrefix}{DiscountPrefix}sc.Next")] = "<b>|c:Next|</b>: Reduce by {1}%",
			[(Locale.En, $"{ShopPrefix}{DiscountPrefix}shop")] = "Regular Customer",
			[(Locale.En, $"{ShopPrefix}{DiscountPrefix}shop.Desc")] = "Takane's shop prices are {0}% cheaper.",
			[(Locale.En, $"{ShopPrefix}{DiscountPrefix}shop.Next")] = "<b>|c:Next|</b>: {1}% cheaper",
			[(Locale.En, $"{ShopPrefix}{DiscountPrefix}upgrade")] = "Competitor",
			[(Locale.En, $"{ShopPrefix}{DiscountPrefix}upgrade.Desc")] = "Takane's card upgrade serivce costs {0} less <sprite=\"Point\" name=\"Gold\">.",
			[(Locale.En, $"{ShopPrefix}{DiscountPrefix}upgrade.Next")] = "<b>|c:Next|</b>: {1} less <sprite=\"Point\" name=\"Gold\">",
			[(Locale.En, $"{ShopPrefix}{DiscountPrefix}remove")] = "Snowballing Advantage",
			[(Locale.En, $"{ShopPrefix}{DiscountPrefix}remove.Desc")] = "Takane's card removal serivce costs {0} less <sprite=\"Point\" name=\"Gold\">.",
			[(Locale.En, $"{ShopPrefix}{DiscountPrefix}remove.Next")] = "<b>|c:Next|</b>: Cost {1} less <sprite=\"Point\" name=\"Gold\">",

			[(Locale.En, $"{ShopPrefix}{FeaturePrefix}teasync")] = "Takeaway Tea",
			[(Locale.En, $"{ShopPrefix}{FeaturePrefix}teasync.Desc")] = "When upgrading cards at Gaps, also |Drink Tea|.",
			[(Locale.En, $"{ShopPrefix}{FeaturePrefix}gapple")] = "Golden Apple Tea",
			[(Locale.En, $"{ShopPrefix}{FeaturePrefix}gapple.Desc")] = "For every {0} life restored while drinking tea at Gaps, gain |c:1| maximum life.",
			[(Locale.En, $"{ShopPrefix}{FeaturePrefix}gapple.Next")] = "<b>|c:Next|</b>: Every {1} life restored",
			[(Locale.En, $"{ShopPrefix}{FeaturePrefix}sponsor")] = "Gap Sponsor",
			[(Locale.En, $"{ShopPrefix}{FeaturePrefix}sponsor.Desc")] = "Gain {0} <sprite=\"Point\" name=\"Gold\"> when upgrading cards at Gaps.",
			[(Locale.En, $"{ShopPrefix}{FeaturePrefix}sponsor.Next")] = "<b>|c:Next|</b>: Gain {1} <sprite=\"Point\" name=\"Gold\">",

			[(Locale.En, $"{ShopPrefix}{BattlePrefix}block")] = "Ever Vigilant",
			[(Locale.En, $"{ShopPrefix}{BattlePrefix}block.Desc")] = "At the start of combat, gain {0} |Block|.",
			[(Locale.En, $"{ShopPrefix}{BattlePrefix}block.Next")] = "<b>|c:Next|</b>: Gain {1} |Block|",
			[(Locale.En, $"{ShopPrefix}{BattlePrefix}graze")] = "Agile Reflexes",
			[(Locale.En, $"{ShopPrefix}{BattlePrefix}graze.Desc")] = "At the start of combat, gain {0} |Graze|.",
			[(Locale.En, $"{ShopPrefix}{BattlePrefix}graze.Next")] = "<b>|c:Next|</b>: Gain {1} |Graze|",
			[(Locale.En, $"{ShopPrefix}{BattlePrefix}heal")] = "Post-Combat Recovery",
			[(Locale.En, $"{ShopPrefix}{BattlePrefix}heal.Desc")] = "At the end of combat, gain {0} life.",
			[(Locale.En, $"{ShopPrefix}{BattlePrefix}heal.Next")] = "<b>|c:Next|</b>: Gain {1} life",
			[(Locale.En, $"{ShopPrefix}{BattlePrefix}seedraw")] = "Heart of the Cards",
			[(Locale.En, $"{ShopPrefix}{BattlePrefix}seedraw.Desc")] = "The draw pile can now be viewed in actual order.",
			[(Locale.En, $"{ShopPrefix}{BattlePrefix}rolldiscard")] = "Reroll",
			[(Locale.En, $"{ShopPrefix}{BattlePrefix}rolldiscard.Desc")] = "At the start of combat, the |Challenger| may send all cards in the draw pile and hand to the discard pile.",
			[(Locale.En, $"{ShopPrefix}{BattlePrefix}hacks")] = "End of turn",
			[(Locale.En, $"{ShopPrefix}{BattlePrefix}hacks.Desc")] = "At the end of the |Challenger|'s first turn, there is a {0}% chance to play |c:1| random |Ability| card on the battlefield.",
			[(Locale.En, $"{ShopPrefix}{BattlePrefix}hacks.Next")] = "<b>|c:Next|</b>: {1}% chance",
			[(Locale.En, $"{ShopPrefix}{BattlePrefix}ChallengerModeWatermark.HoverHint")] = "Challenger mode (Mouseover item for details)",

			[(Locale.En, $"{ShopPrefix}{AlterPrefix}freechoice")] = "I Knew It",
			[(Locale.En, $"{ShopPrefix}{AlterPrefix}freechoice.Desc")] = "The |Challenger| can successfully identify the true culprit without needing any items.",
			[(Locale.En, $"{ShopPrefix}{AlterPrefix}wings")] = "Gives You Wings",
			[(Locale.En, $"{ShopPrefix}{AlterPrefix}wings.Desc")] = "The |Challenger| may ignore paths when choosing the next level.",
			[(Locale.En, $"{ShopPrefix}{AlterPrefix}blankcard")] = "Unlimited Experience",
			[(Locale.En, $"{ShopPrefix}{AlterPrefix}blankcard.Desc")] = "Cards of all colors are added to the card pool.",

			[(Locale.En, $"{ShopPrefix}{DifficultyPrefix}reverse")] = "Normalize",
			[(Locale.En, $"{ShopPrefix}{DifficultyPrefix}reverse.Desc")] = "|Challenge Mode| will not swap the Enemy Phase and Player Phase anymore, but <sprite=\"Point\" name=\"Point\"> cannot be saved if the |Challenger| loses, and only |c:50|% of the <sprite=\"Point\" name=\"Point\"> will be saved.",
			[(Locale.En, $"{ShopPrefix}refund")] = "Refund All",
			[(Locale.En, $"{ShopPrefix}refund.Desc")] = "Refund all purchased upgrades in the Roguelite Shop.",
			[(Locale.En, $"MessageDialog.{ShopPrefix}refund.Confirm")] = "Confirm refunding all purchases?",

			// Custom end-game penalty localization
			[(Locale.En, "BluePoint.hurryact1level4.Name")] = "What's the hurry?",
			[(Locale.En, "BluePoint.hurryact1level4.Description")] = "Lose within Level 4 of Act 1.",
			[(Locale.En, "BluePoint.hurryact1.Name")] = "Why is Cirno the strongest?",
			[(Locale.En, "BluePoint.hurryact1.Description")] = "Lose after Level 4 and within Act 1.",
			[(Locale.En, "BluePoint.hurryact2.Name")] = "Half-phantom T-Shirted",
			[(Locale.En, "BluePoint.hurryact2.Description")] = "Lose in Act 2.",
			[(Locale.En, "BluePoint.hurryact3.Name")] = "So close, yet so far",
			[(Locale.En, "BluePoint.hurryact3.Description")] = "Lose in Act 3.",
			[(Locale.En, "BluePoint.hurryact3win.Name")] = "In another castle?",
			[(Locale.En, "BluePoint.hurryact3win.Description")] = "Resolve the incident in Act 3.",
			[(Locale.En, "BluePoint.hurryact4.Name")] = "We'll get 'em next time",
			[(Locale.En, "BluePoint.hurryact4.Description")] = "Lose in Act 4.",
			[(Locale.En, "BluePoint.hurryact4win.Name")] = "Triumph Resolve",
			[(Locale.En, "BluePoint.hurryact4win.Description")] = "Resolve the incident perfectly.",

			// UI Text
			[(Locale.En, $"{ShopPrefix}ChallengerModeHistory.Active")] = "Challenger Mode Active",
			[(Locale.En, $"{ShopPrefix}ChallengerModeHistory.Title")] = "Challenger Mode",
			[(Locale.En, $"{ShopPrefix}ChallengerModeToggle")] = "Challenger Mode",
			[(Locale.En, $"{ShopPrefix}ChallengerModeToggleFront")] = "|Challenger Mode| is currently... ",
			[(Locale.En, $"{ShopPrefix}ChallengerModeToggleBack")] = " Click the button below to toggle, or you could |shift-click| the main menu shop button to toggle as well!",
			[(Locale.En, $"{ShopPrefix}ChallengerModeToggle.Desc")] = "Enable or disable Challenger Mode. Cannot be changed during an active game run.",
			[(Locale.En, $"{ShopPrefix}ChallengerModeToggle.On")] = "|On!|",
			[(Locale.En, $"{ShopPrefix}ChallengerModeToggle.ButtonOn")] = "Toggle Off",
			[(Locale.En, $"{ShopPrefix}ChallengerModeToggle.ButtonOff")] = "Toggle On",
			[(Locale.En, $"{ShopPrefix}ChallengerModeToggle.Off")] = "Off.",
			[(Locale.En, $"{ShopPrefix}Money")] = "Money: {0} <sprite=\"Point\" name=\"Point\">",
			[(Locale.En, $"{ShopPrefix}Tier")] = "<b>|f:Tier|</b>: {0} / {1}",
			[(Locale.En, $"{ShopPrefix}NextTier")] = " - {0} <sprite=\"Point\" name=\"Point\">",
			[(Locale.En, $"{ShopPrefix}NotEnoughBP")] = " (Insufficient <sprite=\"Point\" name=\"Point\">)",
			[(Locale.En, $"{ShopPrefix}MaxTier")] = " - <b>|MAX|</b>",
			[(Locale.En, $"{ShopPrefix}InDevelopmentSuffix")] = " (In Development)",
			[(Locale.En, $"{ShopPrefix}BuyButton")] = "Purchase",
			[(Locale.En, $"{ShopPrefix}BuyButton.Desc")] = "Purchasing an upgrade will move it up a tier.\nCannot purchase when there is an ongoing run.",
			[(Locale.En, $"{ShopPrefix}RefundButton")] = "Refund",
			[(Locale.En, $"{ShopPrefix}RefundButton.Desc")] = "Refunding an upgrade will move it down a tier.\nCannot refund when there is an ongoing run.",

			[(Locale.En, "AcceptQuest")] = "Accept quest?",
			[(Locale.En, "AbandonQuest")] = "Abandon quest?",

			/////////////
			// 繁體中文 //
			/////////////

			[(Locale.ZhHant, $"{ShopPrefix}LiteShopButton")] = "Roguelite 商店",
			[(Locale.ZhHant, $"{ShopPrefix}LiteShopButton.Active")] = "Roguelite 商店 （啟用中）",
			[(Locale.ZhHant, $"{ShopPrefix}LiteShopButton.Inactive")] = "Roguelite 商店 （未啟用）",
			[(Locale.ZhHant, $"{ShopPrefix}LiteShopButton.Desc")] = "|挑戰者模式|下，遊戲結束時獲得的<sprite=\"Point\" name=\"Point\">將被儲存，能在 |Roguelite 商店| 中使用以強化作為|挑戰者|的玩家。\n|挑戰者模式|：每輪先讓敵人依次行動，再由玩家角色行動。",
			[(Locale.ZhHant, $"{ShopPrefix}LiteShopButton.Desc.Normalize")] = "|p:（行動順序逆轉已被難度調整的正常化取消）|",
			[(Locale.ZhHant, $"{ShopPrefix}LiteShopButton.Effect")] = "<b>效果</b>：",
			[(Locale.ZhHant, $"{ShopPrefix}{DifficultyPrefix}"[..^1])] = "難度調整",
			[(Locale.ZhHant, $"{ShopPrefix}{DifficultyPrefix}Desc")] = "|難度調整|能調整|挑戰者模式|的難度，但也會影響<sprite=\"Point\" name=\"Point\">的儲存或其他收益。",
			[(Locale.ZhHant, $"{ShopPrefix}{InitPrefix}"[..^1])] = "初始加成",
			[(Locale.ZhHant, $"{ShopPrefix}{InitPrefix}Desc")] = "|初始加成|能在遊戲開始時提供永久的即時數值優勢，改善遊戲的整體體驗。",
			[(Locale.ZhHant, $"{ShopPrefix}{DiscountPrefix}"[..^1])] = "折扣",
			[(Locale.ZhHant, $"{ShopPrefix}{DiscountPrefix}Desc")] = "|折扣|能減少商店中各種商品和服務的費用，提高<sprite=\"Point\" name=\"Point\">的使用效率。",
			[(Locale.ZhHant, $"{ShopPrefix}{FeaturePrefix}"[..^1])] = "功能升級",
			[(Locale.ZhHant, $"{ShopPrefix}{FeaturePrefix}Desc")] = "|功能升級|能增強特定的遊戲機制，在遊戲過程中提供額外的收益。",
			[(Locale.ZhHant, $"{ShopPrefix}{BattlePrefix}"[..^1])] = "戰鬥加成",
			[(Locale.ZhHant, $"{ShopPrefix}{BattlePrefix}Desc")] = "|戰鬥加成|能在戰鬥中提供額外的優勢，減少戰損和改善資源管理。",
			[(Locale.ZhHant, $"{ShopPrefix}{AlterPrefix}"[..^1])] = "設定更改",
			[(Locale.ZhHant, $"{ShopPrefix}{AlterPrefix}Desc")] = "|設定更改|能扭曲和更改部份遊戲設定，讓|挑戰者|從中得益。",
			[(Locale.ZhHant, $"{ShopPrefix}Loadout")] = "當前配置",
			[(Locale.ZhHant, $"{ShopPrefix}Loadout.None")] = "無！",

			[(Locale.ZhHant, $"{ShopPrefix}{InitPrefix}fp")] = "攻哈",
			[(Locale.ZhHant, $"{ShopPrefix}{InitPrefix}fp.Desc")] = "戰鬥開始時獲得 {0} 點|火力|。",
			[(Locale.ZhHant, $"{ShopPrefix}{InitPrefix}fp.Next")] = "<b>|c:下一級|</b>：{1} 點|火力|",
			[(Locale.ZhHant, $"{ShopPrefix}{InitPrefix}sp")] = "防殺",
			[(Locale.ZhHant, $"{ShopPrefix}{InitPrefix}sp.Desc")] = "戰鬥開始時獲得 {0} 點|靈力|。",
			[(Locale.ZhHant, $"{ShopPrefix}{InitPrefix}sp.Next")] = "<b>|c:下一級|</b>：{1} 點|靈力|",
			[(Locale.ZhHant, $"{ShopPrefix}{InitPrefix}hp")] = "紅色格檔條",
			[(Locale.ZhHant, $"{ShopPrefix}{InitPrefix}hp.Desc")] = "遊戲開始時，最大生命值提升 {0}% 。",
			[(Locale.ZhHant, $"{ShopPrefix}{InitPrefix}hp.Next")] = "<b>|c:下一級|</b>：提升 {1}%",
			[(Locale.ZhHant, $"{ShopPrefix}{InitPrefix}gold")] = "保護金幣",
			[(Locale.ZhHant, $"{ShopPrefix}{InitPrefix}gold.Desc")] = "遊戲開始時獲得 {0} 點<sprite=\"Point\" name=\"Gold\">。",
			[(Locale.ZhHant, $"{ShopPrefix}{InitPrefix}gold.Next")] = "<b>|c:下一級|</b>：{1} 點<sprite=\"Point\" name=\"Gold\">",
			[(Locale.ZhHant, $"{ShopPrefix}{InitPrefix}card")] = "盡孝",
			[(Locale.ZhHant, $"{ShopPrefix}{InitPrefix}card.Desc")] = "遊戲開始時可從上一局的牌庫中選擇 {0} 張能力牌加入這局的牌庫。",
			[(Locale.ZhHant, $"{ShopPrefix}{InitPrefix}card.Next")] = "<b>|c:下一級|</b>：{1} 張",
			[(Locale.ZhHant, $"{ShopPrefix}{InitPrefix}exhibit")] = "先賺一個精英",
			[(Locale.ZhHant, $"{ShopPrefix}{InitPrefix}exhibit.Desc")] = "遊戲開始時可獲得隨機 {0} 個上一局未擁有的非|光耀|非|祕寶|展品。",
			[(Locale.ZhHant, $"{ShopPrefix}{InitPrefix}exhibit.Next")] = "<b>|c:下一級|</b>：隨機 {1} 個",
			[(Locale.ZhHant, $"{ShopPrefix}{InitPrefix}solo")] = "我獨自做任務",
			[(Locale.ZhHant, $"{ShopPrefix}{InitPrefix}solo.Desc")] = "遊戲開始時獲得|任務欄|展品，可在戰鬥外點擊以打開面板接取|任務|。",

			[(Locale.ZhHant, $"{ShopPrefix}{DiscountPrefix}sc")] = "大招當平a",
			[(Locale.ZhHant, $"{ShopPrefix}{DiscountPrefix}sc.Desc")] = "使用|符卡|所需的<sprite=\"Point\" name=\"Power\">減少 {0}% 。",
			[(Locale.ZhHant, $"{ShopPrefix}{DiscountPrefix}sc.Next")] = "<b>|c:下一級|</b>：減少 {1}%",
			[(Locale.ZhHant, $"{ShopPrefix}{DiscountPrefix}shop")] = "常客",
			[(Locale.ZhHant, $"{ShopPrefix}{DiscountPrefix}shop.Desc")] = "所有山城商店的商品打折 {0}% 。",
			[(Locale.ZhHant, $"{ShopPrefix}{DiscountPrefix}shop.Next")] = "<b>|c:下一級|</b>：打折 {1}%",
			[(Locale.ZhHant, $"{ShopPrefix}{DiscountPrefix}upgrade")] = "競爭對手",
			[(Locale.ZhHant, $"{ShopPrefix}{DiscountPrefix}upgrade.Desc")] = "山城商店的升級卡牌服務費用減少 {0} 點<sprite=\"Point\" name=\"Gold\">。",
			[(Locale.ZhHant, $"{ShopPrefix}{DiscountPrefix}upgrade.Next")] = "<b>|c:下一級|</b>：減少 {1} 點<sprite=\"Point\" name=\"Gold\">",
			[(Locale.ZhHant, $"{ShopPrefix}{DiscountPrefix}remove")] = "擴大優勢",
			[(Locale.ZhHant, $"{ShopPrefix}{DiscountPrefix}remove.Desc")] = "山城商店的移除卡牌服務費用減少 {0} 點<sprite=\"Point\" name=\"Gold\">。",
			[(Locale.ZhHant, $"{ShopPrefix}{DiscountPrefix}remove.Next")] = "<b>|c:下一級|</b>：減少 {1} 點<sprite=\"Point\" name=\"Gold\">",

			[(Locale.ZhHant, $"{ShopPrefix}{FeaturePrefix}teasync")] = "手快全點",
			[(Locale.ZhHant, $"{ShopPrefix}{FeaturePrefix}teasync.Desc")] = "在隙間小屋內升級牌時也會|飲茶|。",
			[(Locale.ZhHant, $"{ShopPrefix}{FeaturePrefix}gapple")] = "金蘋果茶",
			[(Locale.ZhHant, $"{ShopPrefix}{FeaturePrefix}gapple.Desc")] = "在|飲茶|時每回復 {0} 點生命值，獲得 |c:1| 點最大生命值。",
			[(Locale.ZhHant, $"{ShopPrefix}{FeaturePrefix}gapple.Next")] = "<b>|c:下一級|</b>：{1} 點",
			[(Locale.ZhHant, $"{ShopPrefix}{FeaturePrefix}sponsor")] = "隙間贊助",
			[(Locale.ZhHant, $"{ShopPrefix}{FeaturePrefix}sponsor.Desc")] = "在隙間小屋內升級牌時，獲得 {0} 點<sprite=\"Point\" name=\"Gold\">。",
			[(Locale.ZhHant, $"{ShopPrefix}{FeaturePrefix}sponsor.Next")] = "<b>|c:下一級|</b>：{1} 點<sprite=\"Point\" name=\"Gold\">",

			[(Locale.ZhHant, $"{ShopPrefix}{BattlePrefix}block")] = "時刻警戒",
			[(Locale.ZhHant, $"{ShopPrefix}{BattlePrefix}block.Desc")] = "戰鬥開始時，獲得 {0} 點|格檔|。",
			[(Locale.ZhHant, $"{ShopPrefix}{BattlePrefix}block.Next")] = "<b>|c:下一級|</b>：{1} 點|格檔|",
			[(Locale.ZhHant, $"{ShopPrefix}{BattlePrefix}graze")] = "敏捷身手",
			[(Locale.ZhHant, $"{ShopPrefix}{BattlePrefix}graze.Desc")] = "戰鬥開始時，獲得 {0} 層閃避。",
			[(Locale.ZhHant, $"{ShopPrefix}{BattlePrefix}graze.Next")] = "<b>|c:下一級|</b>：{1} 層|閃避|",
			[(Locale.ZhHant, $"{ShopPrefix}{BattlePrefix}heal")] = "脫戰回復",
			[(Locale.ZhHant, $"{ShopPrefix}{BattlePrefix}heal.Desc")] = "戰鬥結束時，獲得 {0} 點生命值。",
			[(Locale.ZhHant, $"{ShopPrefix}{BattlePrefix}heal.Next")] = "<b>|c:下一級|</b>：{1} 點生命值",
			[(Locale.ZhHant, $"{ShopPrefix}{BattlePrefix}seedraw")] = "開眼了",
			[(Locale.ZhHant, $"{ShopPrefix}{BattlePrefix}seedraw.Desc")] = "可以查看抽牌堆的實際牌序。",
			[(Locale.ZhHant, $"{ShopPrefix}{BattlePrefix}rolldiscard")] = "roll了",
			[(Locale.ZhHant, $"{ShopPrefix}{BattlePrefix}rolldiscard.Desc")] = "戰鬥開始時可把抽牌堆的牌置入棄牌堆。",
			[(Locale.ZhHant, $"{ShopPrefix}{BattlePrefix}hacks")] = "回合結束時",
			[(Locale.ZhHant, $"{ShopPrefix}{BattlePrefix}hacks.Desc")] = "玩家的第一回合結束時，有 {0}% 機率可以打出戰場上的 |c:1| 張隨機能力牌。",
			[(Locale.ZhHant, $"{ShopPrefix}{BattlePrefix}hacks.Next")] = "<b>|c:下一級|</b>：{1}% 機率",
			[(Locale.ZhHant, $"{ShopPrefix}{BattlePrefix}ChallengerModeWatermark.HoverHint")] = "挑戰者模式（将滑鼠悬停在字條上查看效果）",

			[(Locale.ZhHant, $"{ShopPrefix}{AlterPrefix}freechoice")] = "我就知道",
			[(Locale.ZhHant, $"{ShopPrefix}{AlterPrefix}freechoice.Desc")] = "不需要搜索真兇用的道具也能成功搜索真兇。",
			[(Locale.ZhHant, $"{ShopPrefix}{AlterPrefix}wings")] = "送你一對翼",
			[(Locale.ZhHant, $"{ShopPrefix}{AlterPrefix}wings.Desc")] = "選擇下一關時可以無視路線。",
			[(Locale.ZhHant, $"{ShopPrefix}{AlterPrefix}blankcard")] = "無限體驗",
			[(Locale.ZhHant, $"{ShopPrefix}{AlterPrefix}blankcard.Desc")] = "獎勵和售賣的卡牌從所有顏色中隨機。",

			[(Locale.ZhHant, $"{ShopPrefix}{DifficultyPrefix}reverse")] = "正常化",
			[(Locale.ZhHant, $"{ShopPrefix}{DifficultyPrefix}reverse.Desc")] = "|挑戰者模式|不會再逆轉行動順序，不過遊戲勝利時只會儲存 |c:50|% 的<sprite=\"Point\" name=\"Point\">，且遊戲失敗時不會儲存<sprite=\"Point\" name=\"Point\">。",
			[(Locale.ZhHant, $"{ShopPrefix}refund")] = "全部退款",
			[(Locale.ZhHant, $"{ShopPrefix}refund.Desc")] = "退款所有在 |Roguelite 商店| 中購買過的升級。",
			[(Locale.ZhHant, $"MessageDialog.{ShopPrefix}refund.Confirm")] = "確定要退款所有的升級嗎？",

			[(Locale.ZhHant, "BluePoint.hurryact1level4.Name")] = "你沒急吧？",
			[(Locale.ZhHant, "BluePoint.hurryact1level4.Description")] = "在第一幕的第四關內失敗。",
			[(Locale.ZhHant, "BluePoint.hurryact1.Name")] = "琪露诺怎麼是最強的？",
			[(Locale.ZhHant, "BluePoint.hurryact1.Description")] = "在第一幕內但第四關外失敗。",
			[(Locale.ZhHant, "BluePoint.hurryact2.Name")] = "半靈 T 恤說是",
			[(Locale.ZhHant, "BluePoint.hurryact2.Description")] = "在第二幕裡失敗。",
			[(Locale.ZhHant, "BluePoint.hurryact3.Name")] = "這麼近，那麼遠",
			[(Locale.ZhHant, "BluePoint.hurryact3.Description")] = "在第三幕裡失敗。",
			[(Locale.ZhHant, "BluePoint.hurryact3win.Name")] = "在另一個城堡嗎？",
			[(Locale.ZhHant, "BluePoint.hurryact3win.Description")] = "在第三幕解決異變。",
			[(Locale.ZhHant, "BluePoint.hurryact4.Name")] = "屢敗屢戰",
			[(Locale.ZhHant, "BluePoint.hurryact4.Description")] = "在第四幕失敗。",
			[(Locale.ZhHant, "BluePoint.hurryact4win.Name")] = "凱旋而歸",
			[(Locale.ZhHant, "BluePoint.hurryact4win.Description")] = "在第四幕完美解決異變。",

			// UI Text
			[(Locale.ZhHant, $"{ShopPrefix}ChallengerModeHistory.Active")] = "挑戰者模式已啟用",
			[(Locale.ZhHant, $"{ShopPrefix}ChallengerModeHistory.Title")] = "挑戰者模式",
			[(Locale.ZhHant, $"{ShopPrefix}Money")] = "金錢：{0} <sprite=\"Point\" name=\"Point\">",
			[(Locale.ZhHant, $"{ShopPrefix}Tier")] = "<b>|f:等級|</b>：{0} / {1}",
			[(Locale.ZhHant, $"{ShopPrefix}NextTier")] = " ── {0} <sprite=\"Point\" name=\"Point\">",
			[(Locale.ZhHant, $"{ShopPrefix}NotEnoughBP")] = " (<sprite=\"Point\" name=\"Point\">不足)",
			[(Locale.ZhHant, $"{ShopPrefix}MaxTier")] = " ── <b>|最高等級|</b>",
			[(Locale.ZhHant, $"{ShopPrefix}InDevelopmentSuffix")] = "（開發中）",
			[(Locale.ZhHant, $"{ShopPrefix}BuyButton")] = "購買升級",
			[(Locale.ZhHant, $"{ShopPrefix}BuyButton.Desc")] = "購買後此商品將提升一級。\n遊戲進行中時不能購買。",
			[(Locale.ZhHant, $"{ShopPrefix}RefundButton")] = "全額退款",
			[(Locale.ZhHant, $"{ShopPrefix}RefundButton.Desc")] = "退款後此商品將降低一級。\n遊戲進行中時不能退款。",
			[(Locale.ZhHant, $"{ShopPrefix}ChallengerModeToggle")] = "挑戰者模式",
			[(Locale.ZhHant, $"{ShopPrefix}ChallengerModeToggleFront")] = "|挑戰者模式|目前⋯⋯",
			[(Locale.ZhHant, $"{ShopPrefix}ChallengerModeToggleBack")] = "點擊下方按鈕以更改模式，或|按住 Shift 鍵點擊|主頁面的商店按鈕也能更改模式！",
			[(Locale.ZhHant, $"{ShopPrefix}ChallengerModeToggle.Desc")] = "啟用或禁用挑戰者模式。在進行中的遊戲期間無法更改。",
			[(Locale.ZhHant, $"{ShopPrefix}ChallengerModeToggle.On")] = "|開啟中！|",
			[(Locale.ZhHant, $"{ShopPrefix}ChallengerModeToggle.ButtonOn")] = "禁用模式",
			[(Locale.ZhHant, $"{ShopPrefix}ChallengerModeToggle.Off")] = "關閉中。",
			[(Locale.ZhHant, $"{ShopPrefix}ChallengerModeToggle.ButtonOff")] = "啟用模式",

			[(Locale.ZhHant, "AcceptQuest")] = "接受任務？",
			[(Locale.ZhHant, "AbandonQuest")] = "放棄任務？",

			/////////////
			// 簡體中文 //
			/////////////

			[(Locale.ZhHans, $"{ShopPrefix}LiteShopButton")] = "Roguelite 商店",
			[(Locale.ZhHans, $"{ShopPrefix}LiteShopButton.Active")] = "Roguelite 商店 （启用中）",
			[(Locale.ZhHans, $"{ShopPrefix}LiteShopButton.Inactive")] = "Roguelite 商店 （未启用）",
			[(Locale.ZhHans, $"{ShopPrefix}LiteShopButton.Desc")] = "|挑战者模式|下，游戏结束时获得的<sprite=\"Point\" name=\"Point\">将被储存，能在 |Roguelite 商店| 中使用以强化作为|挑战者|的玩家。\n|挑战者模式|：每轮先让敌人依次行动，再由玩家角色行动。",
			[(Locale.ZhHans, $"{ShopPrefix}LiteShopButton.Desc.Normalize")] = "|p:（行动顺序逆转已被难度调整的正常化取消）|",
			[(Locale.ZhHans, $"{ShopPrefix}LiteShopButton.Effect")] = "<b>效果</b>：",
			[(Locale.ZhHans, $"{ShopPrefix}{DifficultyPrefix}"[..^1])] = "难度调整",
			[(Locale.ZhHans, $"{ShopPrefix}{DifficultyPrefix}Desc")] = "|难度调整|能调整|挑战者模式|的难度，但也会影响<sprite=\"Point\" name=\"Point\">的储存或其他收益。",
			[(Locale.ZhHans, $"{ShopPrefix}{InitPrefix}"[..^1])] = "初始加成",
			[(Locale.ZhHans, $"{ShopPrefix}{InitPrefix}Desc")] = "|初始加成|能在游戏开始时提供永久的即时数值优势，改善游戏的整体体验。",
			[(Locale.ZhHans, $"{ShopPrefix}{DiscountPrefix}"[..^1])] = "折扣",
			[(Locale.ZhHans, $"{ShopPrefix}{DiscountPrefix}Desc")] = "|折扣|能减少商店中各种商品和服务的费用，提高<sprite=\"Point\" name=\"Point\">的使用效率。",
			[(Locale.ZhHans, $"{ShopPrefix}{FeaturePrefix}"[..^1])] = "功能升级",
			[(Locale.ZhHans, $"{ShopPrefix}{FeaturePrefix}Desc")] = "|功能升级|能增强特定的游戏机制，在游戏过程中提供额外的收益。",
			[(Locale.ZhHans, $"{ShopPrefix}{BattlePrefix}"[..^1])] = "战斗加成",
			[(Locale.ZhHans, $"{ShopPrefix}{BattlePrefix}Desc")] = "|战斗加成|能在战斗中提供额外的优势，减少战损和改善资源管理。",
			[(Locale.ZhHans, $"{ShopPrefix}{AlterPrefix}"[..^1])] = "设定更改",
			[(Locale.ZhHans, $"{ShopPrefix}{AlterPrefix}Desc")] = "|设定更改|能扭曲和更改部分游戏设定，让|挑战者|从中得益。",
			[(Locale.ZhHans, $"{ShopPrefix}Loadout")] = "当前配置",
			[(Locale.ZhHans, $"{ShopPrefix}Loadout.None")] = "无！",

			[(Locale.ZhHans, $"{ShopPrefix}{InitPrefix}fp")] = "攻哈",
			[(Locale.ZhHans, $"{ShopPrefix}{InitPrefix}fp.Desc")] = "战斗开始时获得 {0} 点|火力|。",
			[(Locale.ZhHans, $"{ShopPrefix}{InitPrefix}fp.Next")] = "<b>|c:下一级|</b>：{1} 点|火力|",
			[(Locale.ZhHans, $"{ShopPrefix}{InitPrefix}sp")] = "防杀",
			[(Locale.ZhHans, $"{ShopPrefix}{InitPrefix}sp.Desc")] = "战斗开始时获得 {0} 点|灵力|。",
			[(Locale.ZhHans, $"{ShopPrefix}{InitPrefix}sp.Next")] = "<b>|c:下一级|</b>：{1} 点|灵力|",
			[(Locale.ZhHans, $"{ShopPrefix}{InitPrefix}hp")] = "红色格档条",
			[(Locale.ZhHans, $"{ShopPrefix}{InitPrefix}hp.Desc")] = "游戏开始时，最大生命值提升 {0}% 。",
			[(Locale.ZhHans, $"{ShopPrefix}{InitPrefix}hp.Next")] = "<b>|c:下一级|</b>：提升 {1}%",
			[(Locale.ZhHans, $"{ShopPrefix}{InitPrefix}gold")] = "保护金币",
			[(Locale.ZhHans, $"{ShopPrefix}{InitPrefix}gold.Desc")] = "游戏开始时获得 {0} 点<sprite=\"Point\" name=\"Gold\">。",
			[(Locale.ZhHans, $"{ShopPrefix}{InitPrefix}gold.Next")] = "<b>|c:下一级|</b>：{1} 点<sprite=\"Point\" name=\"Gold\">",
			[(Locale.ZhHans, $"{ShopPrefix}{InitPrefix}card")] = "尽孝",
			[(Locale.ZhHans, $"{ShopPrefix}{InitPrefix}card.Desc")] = "游戏开始时可从上一局的牌库中选择 {0} 张能力牌加入这局的牌库。",
			[(Locale.ZhHans, $"{ShopPrefix}{InitPrefix}card.Next")] = "<b>|c:下一级|</b>：{1} 张",
			[(Locale.ZhHans, $"{ShopPrefix}{InitPrefix}exhibit")] = "先赚一个精英",
			[(Locale.ZhHans, $"{ShopPrefix}{InitPrefix}exhibit.Desc")] = "游戏开始时可获得随机 {0} 个上一局未拥有的非|光耀|非|秘宝|展品。",
			[(Locale.ZhHans, $"{ShopPrefix}{InitPrefix}exhibit.Next")] = "<b>|c:下一级|</b>：随机 {1} 个",
			[(Locale.ZhHans, $"{ShopPrefix}{InitPrefix}solo")] = "我独自做任务",
			[(Locale.ZhHans, $"{ShopPrefix}{InitPrefix}solo.Desc")] = "游戏开始时获得|任务栏|展品，可在战斗外点击以打开面板接取|任务|。",

			[(Locale.ZhHans, $"{ShopPrefix}{DiscountPrefix}sc")] = "大招当平a",
			[(Locale.ZhHans, $"{ShopPrefix}{DiscountPrefix}sc.Desc")] = "使用|符卡|所需的<sprite=\"Point\" name=\"Power\">减少 {0}% 。",
			[(Locale.ZhHans, $"{ShopPrefix}{DiscountPrefix}sc.Next")] = "<b>|c:下一级|</b>：减少 {1}%",
			[(Locale.ZhHans, $"{ShopPrefix}{DiscountPrefix}shop")] = "常客",
			[(Locale.ZhHans, $"{ShopPrefix}{DiscountPrefix}shop.Desc")] = "所有山城商店的商品打折 {0}% 。",
			[(Locale.ZhHans, $"{ShopPrefix}{DiscountPrefix}shop.Next")] = "<b>|c:下一级|</b>：打折 {1}%",
			[(Locale.ZhHans, $"{ShopPrefix}{DiscountPrefix}upgrade")] = "竞争对手",
			[(Locale.ZhHans, $"{ShopPrefix}{DiscountPrefix}upgrade.Desc")] = "山城商店的升级卡牌服务费用减少 {0} 点<sprite=\"Point\" name=\"Gold\">。",
			[(Locale.ZhHans, $"{ShopPrefix}{DiscountPrefix}upgrade.Next")] = "<b>|c:下一级|</b>：减少 {1} 点<sprite=\"Point\" name=\"Gold\">",
			[(Locale.ZhHans, $"{ShopPrefix}{DiscountPrefix}remove")] = "扩大优势",
			[(Locale.ZhHans, $"{ShopPrefix}{DiscountPrefix}remove.Desc")] = "山城商店的移除卡牌服务费用减少 {0} 点<sprite=\"Point\" name=\"Gold\">。",
			[(Locale.ZhHans, $"{ShopPrefix}{DiscountPrefix}remove.Next")] = "<b>|c:下一级|</b>：减少 {1} 点<sprite=\"Point\" name=\"Gold\">",

			[(Locale.ZhHans, $"{ShopPrefix}{FeaturePrefix}teasync")] = "手快全点",
			[(Locale.ZhHans, $"{ShopPrefix}{FeaturePrefix}teasync.Desc")] = "在隙间小屋内升级牌时也会|饮茶|。",
			[(Locale.ZhHans, $"{ShopPrefix}{FeaturePrefix}gapple")] = "金苹果茶",
			[(Locale.ZhHans, $"{ShopPrefix}{FeaturePrefix}gapple.Desc")] = "在|饮茶|时每回复 {0} 点生命值，获得 |c:1| 点最大生命值。",
			[(Locale.ZhHans, $"{ShopPrefix}{FeaturePrefix}gapple.Next")] = "<b>|c:下一级|</b>：{1} 点",
			[(Locale.ZhHans, $"{ShopPrefix}{FeaturePrefix}sponsor")] = "隙间赞助",
			[(Locale.ZhHans, $"{ShopPrefix}{FeaturePrefix}sponsor.Desc")] = "在隙间小屋内升级牌时，获得 {0} 点<sprite=\"Point\" name=\"Gold\">。",
			[(Locale.ZhHans, $"{ShopPrefix}{FeaturePrefix}sponsor.Next")] = "<b>|c:下一级|</b>：{1} 点<sprite=\"Point\" name=\"Gold\">",

			[(Locale.ZhHans, $"{ShopPrefix}{BattlePrefix}block")] = "时刻警戒",
			[(Locale.ZhHans, $"{ShopPrefix}{BattlePrefix}block.Desc")] = "战斗开始时，获得 {0} 点|格档|。",
			[(Locale.ZhHans, $"{ShopPrefix}{BattlePrefix}block.Next")] = "<b>|c:下一级|</b>：{1} 点|格档|",
			[(Locale.ZhHans, $"{ShopPrefix}{BattlePrefix}graze")] = "敏捷身手",
			[(Locale.ZhHans, $"{ShopPrefix}{BattlePrefix}graze.Desc")] = "战斗开始时，获得 {0} 层闪避。",
			[(Locale.ZhHans, $"{ShopPrefix}{BattlePrefix}graze.Next")] = "<b>|c:下一级|</b>：{1} 层|闪避|",
			[(Locale.ZhHans, $"{ShopPrefix}{BattlePrefix}heal")] = "脱战回复",
			[(Locale.ZhHans, $"{ShopPrefix}{BattlePrefix}heal.Desc")] = "战斗结束时，获得 {0} 点生命值。",
			[(Locale.ZhHans, $"{ShopPrefix}{BattlePrefix}heal.Next")] = "<b>|c:下一级|</b>：{1} 点生命值",
			[(Locale.ZhHans, $"{ShopPrefix}{BattlePrefix}seedraw")] = "开眼了",
			[(Locale.ZhHans, $"{ShopPrefix}{BattlePrefix}seedraw.Desc")] = "可以查看抽牌堆的实际牌序。",
			[(Locale.ZhHans, $"{ShopPrefix}{BattlePrefix}rolldiscard")] = "roll了",
			[(Locale.ZhHans, $"{ShopPrefix}{BattlePrefix}rolldiscard.Desc")] = "战斗开始时可把抽牌堆的牌置入弃牌堆。",
			[(Locale.ZhHans, $"{ShopPrefix}{BattlePrefix}hacks")] = "回合结束时",
			[(Locale.ZhHans, $"{ShopPrefix}{BattlePrefix}hacks.Desc")] = "玩家的第一回合结束时，有 {0}% 概率可以打出战场上的 |c:1| 张随机能力牌。",
			[(Locale.ZhHans, $"{ShopPrefix}{BattlePrefix}hacks.Next")] = "<b>|c:下一级|</b>：{1}% 概率",
			[(Locale.ZhHans, $"{ShopPrefix}{BattlePrefix}ChallengerModeWatermark.HoverHint")] = "挑战者模式（将鼠标悬停在字条上查看效果）",

			[(Locale.ZhHans, $"{ShopPrefix}{AlterPrefix}freechoice")] = "我就知道",
			[(Locale.ZhHans, $"{ShopPrefix}{AlterPrefix}freechoice.Desc")] = "不需要搜索真凶用的道具也能成功搜索真凶。",
			[(Locale.ZhHans, $"{ShopPrefix}{AlterPrefix}wings")] = "送你一对翼",
			[(Locale.ZhHans, $"{ShopPrefix}{AlterPrefix}wings.Desc")] = "选择下一关时可以无视路线。",
			[(Locale.ZhHans, $"{ShopPrefix}{AlterPrefix}blankcard")] = "无限体验",
			[(Locale.ZhHans, $"{ShopPrefix}{AlterPrefix}blankcard.Desc")] = "奖励和售卖的卡牌从所有颜色中随机。",

			[(Locale.ZhHans, $"{ShopPrefix}{DifficultyPrefix}reverse")] = "正常化",
			[(Locale.ZhHans, $"{ShopPrefix}{DifficultyPrefix}reverse.Desc")] = "|挑战者模式|不会再逆转行动顺序，不过游戏胜利时只会储存 |c:50|% 的<sprite=\"Point\" name=\"Point\">，且游戏失败时不会储存<sprite=\"Point\" name=\"Point\">。",
			[(Locale.ZhHans, $"{ShopPrefix}refund")] = "全部退款",
			[(Locale.ZhHans, $"{ShopPrefix}refund.Desc")] = "退款所有在 |Roguelite 商店| 中购买过的升级。",
			[(Locale.ZhHans, $"MessageDialog.{ShopPrefix}refund.Confirm")] = "确定要退款所有的升级吗？",

			[(Locale.ZhHans, "BluePoint.hurryact1level4.Name")] = "你没急吧？",
			[(Locale.ZhHans, "BluePoint.hurryact1level4.Description")] = "在第一幕的第四关内失败。",
			[(Locale.ZhHans, "BluePoint.hurryact1.Name")] = "琪露诺怎么是最强的？",
			[(Locale.ZhHans, "BluePoint.hurryact1.Description")] = "在第一幕内但第四关外失败。",
			[(Locale.ZhHans, "BluePoint.hurryact2.Name")] = "半灵 T 恤说是",
			[(Locale.ZhHans, "BluePoint.hurryact2.Description")] = "在第二幕里失败。",
			[(Locale.ZhHans, "BluePoint.hurryact3.Name")] = "这么近，那么远",
			[(Locale.ZhHans, "BluePoint.hurryact3.Description")] = "在第三幕里失败。",
			[(Locale.ZhHans, "BluePoint.hurryact3win.Name")] = "在另一个城堡吗？",
			[(Locale.ZhHans, "BluePoint.hurryact3win.Description")] = "在第三幕解决异变。",
			[(Locale.ZhHans, "BluePoint.hurryact4.Name")] = "屡败屡战",
			[(Locale.ZhHans, "BluePoint.hurryact4.Description")] = "在第四幕失败。",
			[(Locale.ZhHans, "BluePoint.hurryact4win.Name")] = "凯旋而归",
			[(Locale.ZhHans, "BluePoint.hurryact4win.Description")] = "在第四幕完美解决异变。",

			// UI Text
			[(Locale.ZhHans, $"{ShopPrefix}ChallengerModeHistory.Active")] = "挑战者模式已启用",
			[(Locale.ZhHans, $"{ShopPrefix}ChallengerModeHistory.Title")] = "挑战者模式",
			[(Locale.ZhHans, $"{ShopPrefix}Money")] = "金钱：{0} <sprite=\"Point\" name=\"Point\">",
			[(Locale.ZhHans, $"{ShopPrefix}Tier")] = "<b>|f:等级|</b>：{0} / {1}",
			[(Locale.ZhHans, $"{ShopPrefix}NextTier")] = " ── {0} <sprite=\"Point\" name=\"Point\">",
			[(Locale.ZhHans, $"{ShopPrefix}NotEnoughBP")] = " (<sprite=\"Point\" name=\"Point\">不足)",
			[(Locale.ZhHans, $"{ShopPrefix}MaxTier")] = " ── <b>|最高等级|</b>",
			[(Locale.ZhHans, $"{ShopPrefix}InDevelopmentSuffix")] = "（开发中）",
			[(Locale.ZhHans, $"{ShopPrefix}BuyButton")] = "购买升级",
			[(Locale.ZhHans, $"{ShopPrefix}BuyButton.Desc")] = "购买后此商品将提升一级。\n游戏进行中时不能购买。",
			[(Locale.ZhHans, $"{ShopPrefix}RefundButton")] = "全额退款",
			[(Locale.ZhHans, $"{ShopPrefix}RefundButton.Desc")] = "退款后此商品将降低一级。\n游戏进行中时不能退款。",
			[(Locale.ZhHans, $"{ShopPrefix}ChallengerModeToggle")] = "挑战者模式",
			[(Locale.ZhHans, $"{ShopPrefix}ChallengerModeToggleFront")] = "|挑战者模式|目前⋯⋯",
			[(Locale.ZhHans, $"{ShopPrefix}ChallengerModeToggleBack")] = "点击下方按钮以更改模式，或|按住 Shift 键点击|主页面的商店按钮也能更改模式！",
			[(Locale.ZhHans, $"{ShopPrefix}ChallengerModeToggle.Desc")] = "启用或禁用挑战者模式。在进行中的游戏期间无法更改。",
			[(Locale.ZhHans, $"{ShopPrefix}ChallengerModeToggle.On")] = "|开启中！|",
			[(Locale.ZhHans, $"{ShopPrefix}ChallengerModeToggle.ButtonOn")] = "禁用模式",
			[(Locale.ZhHans, $"{ShopPrefix}ChallengerModeToggle.Off")] = "关闭中。",
			[(Locale.ZhHans, $"{ShopPrefix}ChallengerModeToggle.ButtonOff")] = "启用模式",

			[(Locale.ZhHans, "AcceptQuest")] = "接受任务？",
			[(Locale.ZhHans, "AbandonQuest")] = "放弃任务？",
		};
		public static string GetShopItemDescription(string itemId, bool useEffectPrefix = true)
		{
			var locale = LBoL.Core.Localization.CurrentLocale;
			var key = $"{ShopPrefix}{itemId}.Desc";


			if (!LocTable.TryGetValue((locale, key), out var template)
				&& !LocTable.TryGetValue((Locale.En, key), out template))
				return "";

			var effectKey = $"{ShopPrefix}LiteShopButton.Effect";
			if (!LocTable.TryGetValue((locale, effectKey), out var desc))
				LocTable.TryGetValue((Locale.En, effectKey), out desc);
			if (useEffectPrefix)
				template = (desc ?? "") + template;

			var shop = MiniTracker.Instance?.CustomGrSaveData?.GetShopForCurrentProfile();
			if (shop == null)
				return template; // fallback: no formatting

			var item = shop.GetItem(itemId);
			if (item == null)
				return template;

			int delta = item.Delta;
			int initial = item.Initial;
			int arg0 = initial + (delta * item.CurrentTier);
			if (initial != 0)
			{
				arg0 = item.CurrentTier == 0 ? 0 : initial + (delta * (item.CurrentTier - 1));
			}


			return string.Format(
				template,
				"|c:" + arg0 + "|" // {0}
			);
		}

	}

	[HarmonyPatch(typeof(LBoL.Core.Localization), nameof(LBoL.Core.Localization.ReloadCommonAsync))]
	public static class LocalisationPatches
	{
		public static void Postfix()
		{
			var currentLocale = LBoL.Core.Localization.CurrentLocale;
			var localizationTable = LBoL.Core.Localization.LocalizationTable;

			foreach (var (tuple2, value) in LocalisationKeys.LocTable.Where(kvp => kvp.Key.Item1 == currentLocale))
			{
				if (!localizationTable.ContainsKey(tuple2.Item2))
					localizationTable.Add(tuple2.Item2, value);
			}

			foreach (var (tuple2, value) in LocalisationKeys.LocTable.Where(kvp => kvp.Key.Item1 == Locale.En))
			{
				if (!localizationTable.ContainsKey(tuple2.Item2))
					localizationTable.Add(tuple2.Item2, value);
			}
		}
	}

	public class ShopLabelUpdater : MonoBehaviour
	{
		private float _timer;
		private const float Timeout = 3f;

		private void Update()
		{
			if (GameMaster.Instance?.CurrentProfile != null)
			{
				LiteShopButton.RefreshMainMenuButtonLabel();
				Destroy(this);
				return;
			}

			_timer += Time.deltaTime;
			if (_timer > Timeout)
			{
				Destroy(this);
			}
		}
	}

	[HarmonyPatch(typeof(MainMenuPanel), nameof(MainMenuPanel.RefreshProfile))]
	public static class MainMenuPanel_RefreshProfile_Patch
	{
		public static void Postfix()
		{
			LiteShopButton.RefreshMainMenuButtonLabel();
		}
	}
}