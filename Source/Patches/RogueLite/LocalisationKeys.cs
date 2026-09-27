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

public static class LocalisationKeys
{
	public const string ShopPrefix = "Shop.";
	public const string InitPrefix = "init.";
	public const string DiscountPrefix = "discount.";
	public const string FeaturePrefix = "feature.";
	public const string BattlePrefix = "battle.";
	public const string AlterPrefix = "alter.";
	public const string DifficultyPrefix = "difficulty.";

	// Shared display order for the shop-item category prefixes above, used
	// wherever items are grouped by category (loadout tooltip, watermark text).
	public static readonly string[] CategoryOrder =
	[
		DifficultyPrefix,
		InitPrefix,
		DiscountPrefix,
		FeaturePrefix,
		BattlePrefix,
		AlterPrefix,
	];

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

	// Roguelite shop strings live in DirResources/RogueliteShop{Locale}.yaml
	// (discovered by lvalonmimaLocalization.RogueliteShopLocFiles, the same
	// way Sideloader's BatchLocalization discovers entity localization
	// files). Each locale's file is parsed once and cached as a flat
	// key -> string table; ClearCache() is called from
	// LocalisationPatches.Postfix so a locale switch or a live-edited yaml
	// is picked up on the next ReloadCommonAsync.
	private static readonly Dictionary<Locale, Dictionary<string, string>> LocTableCache = new();

	public static void ClearCache() => LocTableCache.Clear();

	public static Dictionary<string, string> GetTable(Locale locale)
	{
		if (LocTableCache.TryGetValue(locale, out var cached))
			return cached;

		var table = new Dictionary<string, string>();
		if (lvalonmimaLocalization.RogueliteShopLocFiles.fileNames.ContainsKey(locale))
		{
			var root = lvalonmimaLocalization.RogueliteShopLocFiles.Load(locale);
			if (root != null)
				FlattenYaml(root, table);
		}
		LocTableCache[locale] = table;
		return table;
	}

	private static void FlattenYaml(YamlMappingNode mapping, Dictionary<string, string> table, string prefix = null)
	{
		foreach (var (keyNode, valueNode) in mapping.Children)
		{
			if (keyNode is not YamlScalarNode { Value: { } key })
				continue;

			// "Text" is reserved: it collapses onto the parent's own dotted
			// key instead of nesting further. It's how a node that is
			// addressed directly (e.g. "Shop.init.fp") can still carry
			// children (.Desc, .Next) in the yaml tree -- without it, the
			// node would have to choose between being a scalar or a
			// mapping. This is distinct from "Name"/"Description", which
			// keep their usual entity-localization meaning and nest like
			// any other key (see the BluePoint.* entries below).
			if (key == "Text" && prefix != null && valueNode is YamlScalarNode { Value: { } textScalar })
			{
				table[prefix] = textScalar;
				continue;
			}

			var fullKey = prefix != null ? $"{prefix}.{key}" : key;
			switch (valueNode)
			{
				case YamlScalarNode { Value: { } scalar }:
					table[fullKey] = scalar;
					break;
				case YamlMappingNode nested:
					FlattenYaml(nested, table, fullKey);
					break;
			}
		}
	}

	// Replaces the ~7 near-identical "current locale, else English, else <missing>"
	// lookups scattered across ShopPatches/ShopModHandlers/LiteShopButton/
	// ExquestingPatch. missingFormat lets callers keep their own placeholder
	// text for an unresolved key (some returned the bare key, others "<key>").
	public static string Get(string key, string missingFormat = null)
	{
		var locale = LBoL.Core.Localization.CurrentLocale;
		if (TryGet(locale, key, out var value))
			return value;
		return missingFormat != null ? string.Format(missingFormat, key) : key;
	}

	public static bool TryGet(string key, out string value) => TryGet(LBoL.Core.Localization.CurrentLocale, key, out value);

	public static bool TryGet(Locale locale, string key, out string value)
	{
		if (GetTable(locale).TryGetValue(key, out value))
			return true;
		return GetTable(Locale.En).TryGetValue(key, out value);
	}

	public static string GetShopItemDescription(string itemId, bool useEffectPrefix = true)
	{
		var locale = LBoL.Core.Localization.CurrentLocale;
		var key = $"{ShopPrefix}{itemId}.Desc";

		if (!TryGet(locale, key, out var template))
			return "";

		var effectKey = $"{ShopPrefix}LiteShopButton.Effect";
		TryGet(locale, effectKey, out var desc);
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
			arg0 = item.CurrentTier == 0 ? 0 : initial + (delta * (item.CurrentTier - 1));

		return string.Format(
			template,
			"|c:" + arg0 + "|" // {0}
		);
	}

}
