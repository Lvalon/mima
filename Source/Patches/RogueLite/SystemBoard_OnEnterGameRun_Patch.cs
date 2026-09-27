using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Cysharp.Threading.Tasks;
using HarmonyLib;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.SaveData;
using LBoL.Presentation;
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.ExtraWidgets;
using LBoL.Presentation.UI.Panels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YamlDotNet.Serialization;
using static LBoL.Presentation.GameMaster;
using lvalonmima.Exhibits;

namespace lvalonmima.Source.Patches;

[HarmonyPatch(typeof(BattleController), nameof(BattleController.PlayerTurnFlow))]
public static class BattleController_PlayerTurnFlow_Patch
{
	// Vanilla PlayerTurnFlow returns IEnumerator<object>, not the non-generic
	// IEnumerator this previously declared. Both are reference types so Harmony's
	// IL-level result substitution tolerated the mismatch, but it was fragile —
	// this matches the original signature precisely.
	static bool Prefix(BattleController __instance, ref IEnumerator<object> __result)
	{
		try
		{
			if (MiniTracker.Instance?.CustomGrSaveData?.GetShopForCurrentProfile()?.ChallengerModeEnabled == true
			&& MiniTracker.Instance?.CustomGrSaveData?.GetShopForCurrentProfile()?.GetItem("difficulty.reverse")?.CurrentTier == 0
			&& __instance.Player?.TurnCounter == 0
			&& __instance.RoundCounter == 1)
			{
				__result = Enumerable.Empty<object>().GetEnumerator();
				return false;
			}
		}
		catch (Exception e)
		{
			BepinexPlugin.log?.LogDebug(e);
		}
		return true;
	}
}

[HarmonyPatch(typeof(SystemBoard), nameof(SystemBoard.OnEnterGameRun))]
public static class SystemBoard_OnEnterGameRun_Patch
{
	private const string WatermarkName = "challengerModeWatermark";
	private const string WatermarkAnchorName = "challengerModeWatermarkAnchor";
	private const string WatermarkHeaderLinkId = "challengerModeWatermarkHeader";
	private const float WatermarkSpacing = 50f;
	private static bool _watermarkDisabled;

	public static void Postfix(SystemBoard __instance)
	{
		try
		{
			_watermarkDisabled = false;
			UpdateChallengerModeWatermark(__instance);
		}
		catch (Exception e)
		{
			BepinexPlugin.log?.LogDebug(e);
		}
	}

	public static void DisableWatermarkAll()
	{
		_watermarkDisabled = true;
		try
		{
			foreach (var board in UnityEngine.Object.FindObjectsByType<SystemBoard>(FindObjectsSortMode.None))
			{
				if (board != null)
					SetWatermarkActive(board, false, null);
			}
		}
		catch (Exception e)
		{
			BepinexPlugin.log?.LogDebug(e);
		}
	}

	private static void UpdateChallengerModeWatermark(SystemBoard board)
	{
		if (_watermarkDisabled)
		{
			SetWatermarkActive(board, false, null);
			return;
		}

		var shop = MiniTracker.Instance?.CustomGrSaveData?.GetShopForCurrentProfile();
		if (shop == null || !shop.ChallengerModeEnabled)
		{
			SetWatermarkActive(board, false, null);
			return;
		}
		string text = BuildChallengerModeWatermarkText(shop);
		var itemEffects = BuildChallengerModeWatermarkItemEffects(shop);
		SetWatermarkActive(board, true, text, itemEffects);
	}

	private static string BuildChallengerModeWatermarkText(LiteShop shop)
	{
		string header = GetLocalizedText($"{LocalisationKeys.ShopPrefix}{LocalisationKeys.BattlePrefix}ChallengerModeWatermark.HoverHint");
		// Link the header too, so hovering it shows the same Roguelite Shop description as its menu button.
		header = $"<link=\"{WatermarkHeaderLinkId}\"><b>|e:{header}|</b></link>";

		var sb = new StringBuilder(header);

		foreach (var prefix in LocalisationKeys.CategoryOrder)
		{
			var items = shop.Items.Values
				.Where(item => item.CurrentTier > 0 && item.Id.StartsWith(prefix, StringComparison.Ordinal))
				.ToList();
			if (items.Count == 0)
				continue;

			string categoryKey = $"{LocalisationKeys.ShopPrefix}{prefix[..^1]}";
			string categoryName = GetLocalizedText(categoryKey);
			string categoryLinkId = prefix[..^1];
			// Make category header a link so the watermark tooltip system can show category descriptions
			sb.Append('\n').Append($"<link=\"{categoryLinkId}\"><b>{categoryName}</b></link>");

			foreach (var item in items)
			{
				string nameKey = $"{LocalisationKeys.ShopPrefix}{item.Id}";
				string name = GetLocalizedText(nameKey);
				if (name == nameKey)
					name = item.Id;
				string coloredName = LocalisationKeys.ColorizeTierName(name, item.CurrentTier, item.MaxTier);
				string linkedName = $"<link=\"{item.Id}\">{coloredName}</link>";
				string line = $"<b>{linkedName}</b>";
				if (item.MaxTier > 1)
					line = $"{line} {item.CurrentTier}";

				sb.Append('\n').Append("  ").Append(line);
			}
		}

		return StringDecorator.Decorate(sb.ToString());
	}

	private static Dictionary<string, WatermarkItemTooltipData> BuildChallengerModeWatermarkItemEffects(LiteShop shop)
	{
		var itemEffects = new Dictionary<string, WatermarkItemTooltipData>(StringComparer.Ordinal);

		string headerTitle = GetLocalizedText($"{LocalisationKeys.ShopPrefix}{LocalisationKeys.BattlePrefix}ChallengerModeWatermark.HoverHint");
		string headerEffect = GetLocalizedText($"{LocalisationKeys.ShopPrefix}LiteShopButton.Desc");
		if (!string.IsNullOrWhiteSpace(headerEffect))
			itemEffects[WatermarkHeaderLinkId] = new WatermarkItemTooltipData(StringDecorator.Decorate(headerTitle), StringDecorator.Decorate(headerEffect));

		foreach (var prefix in LocalisationKeys.CategoryOrder)
		{
			var items = shop.Items.Values
					.Where(item => item.CurrentTier > 0 && item.Id.StartsWith(prefix, StringComparison.Ordinal))
					.ToList();
			if (items.Count == 0)
				continue;

			// Add a tooltip entry for the category itself (keyed by the prefix without trailing dot)
			string categoryId = prefix[..^1];
			string categoryKey = $"{LocalisationKeys.ShopPrefix}{categoryId}";
			string categoryName = GetLocalizedText(categoryKey);
			string categoryEffect = LocalisationKeys.GetShopItemDescription(categoryId, false);
			if (!string.IsNullOrWhiteSpace(categoryEffect))
			{
				string decoratedTitle = StringDecorator.Decorate(categoryName);
				string decoratedEffect = StringDecorator.Decorate(categoryEffect);
				itemEffects[categoryId] = new WatermarkItemTooltipData(decoratedTitle, decoratedEffect);
			}

			foreach (var item in items)
			{
				string nameKey = $"{LocalisationKeys.ShopPrefix}{item.Id}";
				string name = GetLocalizedText(nameKey);
				if (name == nameKey)
					name = item.Id;
				string effect = LocalisationKeys.GetShopItemDescription(item.Id, false);
				if (string.IsNullOrWhiteSpace(effect))
					continue;

				string decoratedTitle = StringDecorator.Decorate(name);
				string decoratedEffect = StringDecorator.Decorate(effect);
				itemEffects[item.Id] = new WatermarkItemTooltipData(decoratedTitle, decoratedEffect);
			}
		}

		return itemEffects;
	}

	private static void SetWatermarkActive(SystemBoard board, bool isActive, string text, Dictionary<string, WatermarkItemTooltipData> itemEffects = null)
	{
		if (board?.gameVersion == null)
			return;

		// SystemBoard (and so board.gameVersion's own parent) sits on PanelLayer.Base, which
		// is drawn *under* the map (Top) and the mod's own Challenger Mode shop screen
		// (Normal, a ComplexRulesPanel). Read the vertical-stacking info from that parent
		// (other version/hint lines live there), but actually host the watermark at the bottom
		// of the TooltipsLayer so it renders above those panels yet below tooltips. It still
		// can't block anything (CanvasGroup + raycastTarget=false), so this is purely visual.
		Transform spacingParent = board.gameVersion.gameObject.transform.parent;
		if (spacingParent == null)
			return;

		Transform hostParent = GetWatermarkHostParent(board);
		if (hostParent == null)
			hostParent = spacingParent;

		GameObject watermark = null;
		foreach (Transform item in hostParent)
		{
			if (item.name == WatermarkName)
			{
				watermark = item.gameObject;
				break;
			}
		}

		float? baseY = null;
		int activeCount = 0;
		foreach (Transform item in spacingParent)
		{
			var child = item;
			if (child.gameObject.activeSelf && child.name != "Hint" && child.name != WatermarkName)
			{
				activeCount++;
				if (!baseY.HasValue)
					baseY = child.localPosition.y;
			}
		}

		if (!isActive)
		{
			watermark?.SetActive(false);
			return;
		}

		if (watermark == null)
		{
			watermark = UnityEngine.Object.Instantiate(board.gameVersion.gameObject, hostParent);
			watermark.name = WatermarkName;
		}

		var tmp = watermark.GetComponent<TextMeshProUGUI>();
		if (tmp != null)
		{
			tmp.text = text ?? string.Empty;
			tmp.alignment = TextAlignmentOptions.TopRight;
			tmp.textWrappingMode = TextWrappingModes.NoWrap;
			tmp.ForceMeshUpdate();
			var rectTransform = tmp.rectTransform;
			rectTransform.pivot = new Vector2(1f, 1f);
			rectTransform.anchorMin = new Vector2(1f, 1f);
			rectTransform.anchorMax = new Vector2(1f, 1f);
			rectTransform.sizeDelta = new Vector2(tmp.preferredWidth, tmp.preferredHeight);
			ApplyWatermarkVisibility(tmp);

			RectTransform tooltipAnchor = GetOrCreateWatermarkTooltipAnchor(tmp);
			var tooltip = tooltipAnchor.GetComponent<SimpleTooltipSource>();
			if (tooltip == null)
				tooltip = SimpleTooltipSource.CreateDirect(tooltipAnchor.gameObject, string.Empty).WithPosition(TooltipDirection.Bottom, TooltipAlignment.Max);
			var hoverController = watermark.TryGetComponent<WatermarkHoverController>(out var existingController) ? existingController : watermark.AddComponent<WatermarkHoverController>();
			hoverController.SetData(tmp, tooltipAnchor, tooltip, itemEffects ?? new Dictionary<string, WatermarkItemTooltipData>(StringComparer.Ordinal));

			// The stacking position is expressed in spacingParent's local space (under the
			// version/hint lines); the controller maps it to world space every frame, since the
			// watermark itself now lives under a different parent.
			Vector3 versionLocal = board.gameVersion.transform.localPosition;
			var followPoint = new Vector3(
				versionLocal.x,
				baseY.HasValue ? baseY.Value - (activeCount * WatermarkSpacing) : versionLocal.y,
				0f
			);
			hoverController.SetFollowTarget(spacingParent, followPoint);
		}

		watermark.transform.SetAsFirstSibling();
		watermark.SetActive(true);
	}

	// Host the watermark as the *first* child of the TooltipsLayer panel. Tooltips already
	// render above the map and the mod's Challenger Mode shop screen, and TooltipsLayer
	// instantiates every tooltip widget as a later child of its own transform, so the
	// watermark ends up above those panels but below every tooltip (ours included).
	private static Transform GetWatermarkHostParent(SystemBoard board)
	{
		try
		{
			if (UiManager.IsInitialized)
			{
				var tooltipsLayer = UiManager.GetPanel<TooltipsLayer>();
				if (tooltipsLayer != null && tooltipsLayer.gameObject.activeInHierarchy)
					return tooltipsLayer.transform;
			}
		}
		catch (Exception e)
		{
			BepinexPlugin.log?.LogDebug(e);
		}

		Canvas canvas = board.gameVersion.GetComponentInParent<Canvas>();
		return canvas != null ? canvas.transform : null;
	}

	private static RectTransform GetOrCreateWatermarkTooltipAnchor(TextMeshProUGUI text)
	{
		Transform parent = text.transform;
		var existing = parent.Find(WatermarkAnchorName) as RectTransform;
		RectTransform anchorRect = existing;
		if (anchorRect == null)
		{
			var anchorObject = new GameObject(WatermarkAnchorName, typeof(RectTransform));
			anchorRect = anchorObject.GetComponent<RectTransform>();
			anchorRect.SetParent(parent, false);
		}

		anchorRect.anchorMin = new Vector2(1f, 1f);
		anchorRect.anchorMax = new Vector2(1f, 1f);
		anchorRect.pivot = new Vector2(0.5f, 0.5f);
		anchorRect.localScale = Vector3.one;
		anchorRect.SetAsLastSibling();
		return anchorRect;
	}

	private static void ApplyWatermarkVisibility(TextMeshProUGUI text)
	{
		text.color = Color.white;
		var outline = text.GetComponent<Outline>();
		if (outline != null)
			UnityEngine.Object.Destroy(outline);

		ApplyWatermarkOutline(text);
		text.raycastTarget = false;

		// Belt-and-suspenders: even if something re-enables raycastTarget on the text or
		// adds a raycastable child later, the CanvasGroup keeps the whole watermark from
		// ever intercepting clicks or hover meant for units, status icons, or intentions.
		var canvasGroup = text.TryGetComponent<CanvasGroup>(out var existingGroup) ? existingGroup : text.gameObject.AddComponent<CanvasGroup>();
		canvasGroup.blocksRaycasts = false;
		canvasGroup.interactable = false;
	}

	// Small black stroke so the text stays readable over light backgrounds. The TMP
	// Distance Field shader only renders an outline when OUTLINE_ON is enabled on the
	// material (the Inspector toggles it automatically; runtime property writes don't),
	// and the mesh needs extra padding or the stroke gets clipped at glyph edges.
	private static void ApplyWatermarkOutline(TextMeshProUGUI text)
	{
		const float OutlineWidth = 0.2f;

		Material material = text.fontMaterial; // per-text instance, never the shared font material
		if (material != null)
		{
			material.EnableKeyword(ShaderUtilities.Keyword_Outline);
			if (material.HasProperty(ShaderUtilities.ID_OutlineColor))
				material.SetColor(ShaderUtilities.ID_OutlineColor, Color.black);
			if (material.HasProperty(ShaderUtilities.ID_OutlineWidth))
				material.SetFloat(ShaderUtilities.ID_OutlineWidth, OutlineWidth);
		}

		text.outlineColor = Color.black;
		text.outlineWidth = OutlineWidth;
		text.UpdateMeshPadding();
		text.SetMaterialDirty();
	}

	private static string GetLocalizedText(string key) => LocalisationKeys.Get(key);
}
