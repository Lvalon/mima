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

public partial class LiteShopButton
{
	private static string _currentCategoryId;

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
			return !LiteShopPanels.Contains(__instance);
		}
	}

	// suppress og content from loading
	[HarmonyPatch(typeof(ComplexRulesPanel), nameof(ComplexRulesPanel.SetDescription))]
	class Patch_ComplexRulesPanel_SetDescription
	{
		static bool Prefix(ComplexRulesPanel __instance)
		{
			return !LiteShopPanels.Contains(__instance);
		}
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

			bool anyCategoryItems = false;

			// Build tooltip mapping for categories and items
			var tooltipMap = new Dictionary<string, LoadoutTooltipData>(StringComparer.Ordinal);
			foreach (var prefix in LocalisationKeys.CategoryOrder)
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
			panel.descriptionText.text = StringDecorator.Decorate(categoryDescKey);

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
			CreatePurchaseButton(panel, itemId, currentY - 30f, leftMargin);
		if (item.CurrentTier > 0)
			CreateRefundButton(panel, itemId, currentY - 30f, leftMargin + 660f);
		if (!item.IsMaxTier || item.CurrentTier > 0)
			currentY -= 80f + buttonPadding;
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
		catch (Exception e)
		{
			BepinexPlugin.log?.LogDebug(e);
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
		catch (Exception e)
		{
			BepinexPlugin.log?.LogDebug(e);
		}
	}

	private static void OnPurchaseClicked(ComplexRulesPanel panel, string itemId)
	{
		try
		{
			if (_currentShop == null)
				return;

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
		catch (Exception e)
		{
			BepinexPlugin.log?.LogDebug(e);
		}
	}

	private static void OnRefundClicked(ComplexRulesPanel panel, string itemId)
	{
		try
		{
			if (_currentShop == null)
				return;

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
		catch (Exception e)
		{
			BepinexPlugin.log?.LogDebug(e);
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
			ShowLiteShopCategory(panel, firstCategoryId);
	}

	private static void PrepareLiteShopPanel(ComplexRulesPanel panel)
	{
		panel._stringTable = [];

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
			localizedTextResize = resizeFactor;

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

	private static string GetShopLoc(string key) => LocalisationKeys.Get(key);

	private static bool TryGetShopLoc(string key, out string value) => LocalisationKeys.TryGet(key, out value);

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

		return LocalisationKeys.GetTable(locale).Keys
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
		return LocalisationKeys.GetTable(locale).Keys
			.Where(k =>
				k.StartsWith(prefix) &&
				!k.EndsWith(".Desc") &&
				!k.EndsWith(".Next")
			)
			.Distinct();
	}
}
