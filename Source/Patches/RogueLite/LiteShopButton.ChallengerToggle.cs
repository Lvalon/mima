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
	private const float DefaultButtonScale = 0.75f;

	private const float DefaultButtonFontSize = 32f;

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
		catch (Exception e)
		{
			BepinexPlugin.log?.LogDebug(e);
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
			return false;

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
				return;

			if (_currentShop == null)
				return;

			// Toggle the state
			_currentShop.ChallengerModeEnabled = !_currentShop.ChallengerModeEnabled;

			// Update button text to reflect new state
			UpdateChallengerModeButtonText(buttonChild);
			if (_currentCategoryId == "LiteShopButton")
				ShowLiteShopCategory(panel, _currentCategoryId);
			UpdateMainMenuButtonLabel();

			// Save immediately to persist changes
			ShopSaveLoader.Save();

		}
		catch (Exception e)
		{
			BepinexPlugin.log?.LogDebug(e);
		}
	}
}
