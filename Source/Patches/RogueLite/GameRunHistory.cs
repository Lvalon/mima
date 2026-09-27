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

[HarmonyPatch(typeof(GameMaster), nameof(EndGameStatistics)), HarmonyPriority(Priority.VeryLow)]
public static class GameMaster_EndGameStatistics_Patch
{
	private const string BluePointPrefix = "BluePoint.";

	static void Postfix(ref GameStatisticData __result, GameRunController gameRun, GameResultType resultType)
	{
		SystemBoard_OnEnterGameRun_Patch.DisableWatermarkAll();
		if (__result == null)
			return;
		var shop = MiniTracker.Instance?.CustomGrSaveData?.GetShopForCurrentProfile();
		if (shop == null || !shop.ChallengerModeEnabled)
			return;

		var ids = new HashSet<string>(StringComparer.Ordinal);
		foreach (var key in LocalisationKeys.GetTable(Locale.En).Keys)
		{
			if (!key.StartsWith(BluePointPrefix, StringComparison.Ordinal))
				continue;

			var remainder = key[BluePointPrefix.Length..];
			var root = remainder.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries)[0];
			var fullId = BluePointPrefix + root;
			ids.Add(fullId);
		}

		if (ids.Count == 0)
			return;

		__result.ScoreDatas ??= [];

		float diffMult = gameRun.Difficulty switch
		{
			GameDifficulty.Easy => 0.75f,
			GameDifficulty.Normal => 1f,
			GameDifficulty.Hard => 1.25f,
			GameDifficulty.Lunatic => 1.5f,
			_ => 0f,
		};

		float toAdd = 0;

		GameRunSaveData gameRunSaveData = Singleton<GameMaster>.Instance.GameRunSaveData;

		foreach (var fullId in ids)
		{
			if (__result.ScoreDatas.Exists(sd => sd.Id == fullId))
				continue;

			var idRoot = fullId[BluePointPrefix.Length..];
			int delta = idRoot switch
			{
				// known specific adjustments
				"hurryact1level4" => -10,
				"hurryact1" => -1,
				"hurryact2" => 100,
				"hurryact3" => 500,
				"hurryact3win" => 1000,
				"hurryact4" => 1500,
				"hurryact4win" => 2500,
				_ => 0
			};

			// stage check
			int indexSTAGE = -1;
			int levelSTATION = -1;
			if (shop?.BPProgress != null)
			{
				shop.BPProgress.TryGetValue("stage", out indexSTAGE);
				shop.BPProgress.TryGetValue("level", out levelSTATION);
			}

			bool skipReward = false;
			switch (idRoot)
			{
				case "hurryact1level4":
					if (indexSTAGE > 0 || levelSTATION > 4)
						skipReward = true;
					break;
				case "hurryact1":
					if (indexSTAGE > 0 || levelSTATION <= 4)
						skipReward = true;
					break;
				case "hurryact2":
					if (indexSTAGE != 1)
						skipReward = true;
					break;
				case "hurryact3":
					if (indexSTAGE != 2 || resultType != GameResultType.Failure)
						skipReward = true;
					break;
				case "hurryact3win":
					if (indexSTAGE != 2 || resultType != GameResultType.NormalEnd)
						skipReward = true;
					break;
				case "hurryact4":
					if (indexSTAGE != 3 || (resultType != GameResultType.TrueEndFail && resultType != GameResultType.Failure))
						skipReward = true;
					break;
				case "hurryact4win":
					if (indexSTAGE != 3 || resultType != GameResultType.TrueEnd)
						skipReward = true;
					break;
				default:
					break;
			}

			if (skipReward)
				continue;

			EnsureLocalizationKey(fullId + ".Name");
			EnsureLocalizationKey(fullId + ".Description");

			// panel showing, mult is handled elsewhere
			__result.ScoreDatas.Add(new ScoreData
			{
				Id = fullId,
				TotalBluePoint = delta
			});
			toAdd += delta;
		}
		__result.BluePoint += (int)(toAdd * diffMult);

		shop.BPProgress = [];
		MiniTracker.Instance.CustomGrSaveData.Save(__result.BluePoint);
		ShopSaveLoader.Save();  //save progress on the spot
	}

	private static void EnsureLocalizationKey(string key)
	{
		var locale = LBoL.Core.Localization.CurrentLocale;
		if (!TryAddLocalizationKey(locale, key))
			TryAddLocalizationKey(Locale.En, key);
	}

	private static bool TryAddLocalizationKey(Locale locale, string key)
	{
		if (!LocalisationKeys.GetTable(locale).TryGetValue(key, out var value))
			return false;

		var table = LBoL.Core.Localization.LocalizationTable;
		if (!table.ContainsKey(key))
			table.Add(key, value);

		return true;
	}
}

[HarmonyPatch(typeof(GameResultPanel), nameof(GameResultPanel.CustomLocalizationAsync))]
public static class GameResultPanel_CustomLocalizationAsync_Patch
{
	private const string BluePointPrefix = "BluePoint.";

	static void Postfix(GameResultPanel __instance, ref UniTask __result)
	{
		__result = __result.ContinueWith(() => AddCustomScoreEntries(__instance));
	}

	private static void AddCustomScoreEntries(GameResultPanel panel)
	{
		if (panel?._stringTable == null)
			return;

		var ids = new HashSet<string>(StringComparer.Ordinal);
		foreach (var key in LocalisationKeys.GetTable(Locale.En).Keys)
		{
			if (!key.StartsWith(BluePointPrefix, StringComparison.Ordinal))
				continue;

			var remainder = key[BluePointPrefix.Length..];
			var root = remainder.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries)[0];
			var fullId = BluePointPrefix + root;
			ids.Add(fullId);
		}

		foreach (var fullId in ids)
		{
			panel._stringTable[fullId] = new GameResultPanel.StringTableEntry
			{
				Name = GetLoc(fullId + ".Name"),
				Description = GetLoc(fullId + ".Description")
			};
		}

		// reset quest run-state snapshots
		var shop = MiniTracker.Instance?.CustomGrSaveData?.GetShopForCurrentProfile();
		if (shop != null)
		{
			shop.QuestProgress = new Dictionary<string, int>(StringComparer.Ordinal);
			shop.QuestRequirements = new Dictionary<string, string>(StringComparer.Ordinal);
			shop.QuestCompletedCards = new HashSet<string>(StringComparer.Ordinal);
			shop.QuestModifiers = new Dictionary<string, int>(StringComparer.Ordinal);
		}

		MiniTracker.Instance.CustomGrSaveData.Save(0, false);
		ShopSaveLoader.Save();
	}

	private static string GetLoc(string key) => LocalisationKeys.Get(key, "<{0}>");
}

[HarmonyPatch(typeof(GameMaster), nameof(GameMaster.AppendGameRunHistory))]
public static class GameMaster_AppendGameRunHistory_Patch
{
	static void Postfix(GameRunRecordSaveData record)
	{
		try
		{
			if (record == null || string.IsNullOrEmpty(record.SaveTimestamp))
				return;

			var shop = MiniTracker.Instance?.CustomGrSaveData?.GetShopForCurrentProfile();
			if (shop == null || !shop.ChallengerModeEnabled)
				return;

			shop.RunModifiersByTimestamp ??= [];
			var modifiers = shop.Items?.Values
					.Where(item => item != null && item.CurrentTier > 0)
					.Select(item => (item.Id, item.CurrentTier))
					.ToList() ?? [];

			// Always record that Challenger Mode was active for this run when the shop indicates so.
			// If no modifiers were purchased, store an empty list so the history UI can still
			// indicate "Challenger Mode Active" and show the "None" tooltip.
			modifiers.Sort((left, right) => string.CompareOrdinal(left.Id, right.Id));
			shop.RunModifiersByTimestamp[record.SaveTimestamp] = modifiers;

			shop.RunCompletedQuestsByTimestamp ??= new Dictionary<string, List<string>>(StringComparer.Ordinal);
			List<string> completedQuests = shop.CurrentRunCompletedQuests != null
				? [.. shop.CurrentRunCompletedQuests.Where(id => !string.IsNullOrEmpty(id))]
				: [];
			shop.RunCompletedQuestsByTimestamp[record.SaveTimestamp] = completedQuests;

			ShopModHandlers.ResetCurrentRunRewardedQuestCompletions(shop);
			MiniTracker.Instance?.CustomGrSaveData?.Save(0, false);
			ShopSaveLoader.Save();
		}
		catch (Exception e)
		{
			BepinexPlugin.log?.LogDebug(e);
		}
	}
}

[HarmonyPatch(typeof(HistoryPanel), nameof(HistoryPanel.Awake))]
public static class HistoryPanel_Awake_Patch
{
	static void Postfix(HistoryPanel __instance)
	{
		try
		{
			HistoryPanelChallengerHistory.EnsureChallengerHistoryUi(__instance);
		}
		catch (Exception e)
		{
			BepinexPlugin.log?.LogDebug(e);
		}
	}
}

[HarmonyPatch(typeof(HistoryPanel), nameof(HistoryPanel.SetRecord))]
public static class HistoryPanel_SetRecord_Patch
{
	static void Postfix(HistoryPanel __instance, GameRunRecordSaveData record)
	{
		try
		{
			HistoryPanelChallengerHistory.UpdateChallengerHistoryUi(__instance, record);
		}
		catch (Exception e)
		{
			BepinexPlugin.log?.LogDebug(e);
		}
	}
}

internal static class HistoryPanelChallengerHistory
{
	private sealed class ChallengerHistoryUi
	{
		public TextMeshProUGUI Label;
		public SimpleTooltipSource Tooltip;
		public TextMeshProUGUI QuestLabel;
		public SimpleTooltipSource QuestTooltip;
	}

	private const string ChallengerHistoryLabelName = "challengerModeHistoryLabel";
	private const string ChallengerQuestHistoryLabelName = "challengerModeQuestHistoryLabel";
	private static readonly Dictionary<HistoryPanel, ChallengerHistoryUi> ChallengerHistoryUiMap = [];

	public static void EnsureChallengerHistoryUi(HistoryPanel panel)
	{
		GetOrCreateUi(panel);
	}

	private static ChallengerHistoryUi GetOrCreateUi(HistoryPanel panel)
	{
		if (panel == null || panel.packImage == null)
			return null;
		if (ChallengerHistoryUiMap.TryGetValue(panel, out var ui) && ui?.Label != null && ui.QuestLabel != null)
			return ui;

		Transform parent = panel.packImage.transform.parent;
		if (parent == null)
			return null;

		TextMeshProUGUI label = null;
		var existing = parent.Find(ChallengerHistoryLabelName) as RectTransform;
		if (existing != null)
			label = existing.GetComponent<TextMeshProUGUI>();

		if (label == null)
		{
			var go = new GameObject(ChallengerHistoryLabelName, typeof(RectTransform), typeof(TextMeshProUGUI));
			go.transform.SetParent(parent, false);
			label = go.GetComponent<TextMeshProUGUI>();

			var packRect = panel.packImage.rectTransform;
			var labelRect = label.rectTransform;
			labelRect.anchorMin = packRect.anchorMin;
			labelRect.anchorMax = packRect.anchorMax;
			labelRect.pivot = packRect.pivot;
			labelRect.anchoredPosition = packRect.anchoredPosition + new Vector2(0f, packRect.sizeDelta.y + 12f);
			labelRect.sizeDelta = new Vector2(Mathf.Max(200f, packRect.sizeDelta.x * 2f), packRect.sizeDelta.y);

			if (panel.seedText != null)
			{
				label.font = panel.seedText.font;
				label.fontSize = panel.seedText.fontSize;
				label.color = panel.seedText.color;
			}

			label.alignment = TextAlignmentOptions.Center;
			label.textWrappingMode = TextWrappingModes.NoWrap;
			label.raycastTarget = true;
			label.text = string.Empty;
			label.gameObject.SetActive(false);
			label.transform.SetAsLastSibling();

		}

		TextMeshProUGUI questLabel = null;
		var questExisting = parent.Find(ChallengerQuestHistoryLabelName) as RectTransform;
		if (questExisting != null)
			questLabel = questExisting.GetComponent<TextMeshProUGUI>();

		if (questLabel == null)
		{
			var go = new GameObject(ChallengerQuestHistoryLabelName, typeof(RectTransform), typeof(TextMeshProUGUI));
			go.transform.SetParent(parent, false);
			questLabel = go.GetComponent<TextMeshProUGUI>();

			var packRect = panel.packImage.rectTransform;
			var labelRect = questLabel.rectTransform;
			labelRect.anchorMin = packRect.anchorMin;
			labelRect.anchorMax = packRect.anchorMax;
			labelRect.pivot = packRect.pivot;
			labelRect.anchoredPosition = packRect.anchoredPosition + new Vector2(0f, packRect.sizeDelta.y + 32f);
			labelRect.sizeDelta = new Vector2(Mathf.Max(200f, packRect.sizeDelta.x * 2f), packRect.sizeDelta.y);

			if (panel.seedText != null)
			{
				questLabel.font = panel.seedText.font;
				questLabel.fontSize = panel.seedText.fontSize;
				questLabel.color = panel.seedText.color;
			}

			questLabel.alignment = TextAlignmentOptions.Center;
			questLabel.textWrappingMode = TextWrappingModes.NoWrap;
			questLabel.raycastTarget = true;
			questLabel.text = string.Empty;
			questLabel.gameObject.SetActive(false);
			questLabel.transform.SetAsLastSibling();
		}

		var tooltip = label.GetComponent<SimpleTooltipSource>() ?? SimpleTooltipSource.CreateDirect(label.gameObject, string.Empty, string.Empty)
			.WithPosition(TooltipDirection.Top, TooltipAlignment.Center);
		var questTooltip = questLabel.GetComponent<SimpleTooltipSource>() ?? SimpleTooltipSource.CreateDirect(questLabel.gameObject, string.Empty, string.Empty)
			.WithPosition(TooltipDirection.Top, TooltipAlignment.Center);

		ui = new ChallengerHistoryUi
		{
			Label = label,
			Tooltip = tooltip,
			QuestLabel = questLabel,
			QuestTooltip = questTooltip
		};
		ChallengerHistoryUiMap[panel] = ui;

		return ui;
	}

	public static void UpdateChallengerHistoryUi(HistoryPanel panel, GameRunRecordSaveData record)
	{
		var ui = GetOrCreateUi(panel);
		if (ui?.Label == null || ui.QuestLabel == null)
			return;

		ui.Label.gameObject.SetActive(false);
		ui.Tooltip?.SetDirect(string.Empty, string.Empty);
		ui.QuestLabel.gameObject.SetActive(false);
		ui.QuestTooltip?.SetDirect(string.Empty, string.Empty);

		if (record == null || string.IsNullOrEmpty(record.SaveTimestamp))
			return;

		var shop = MiniTracker.Instance?.CustomGrSaveData?.GetShopForCurrentProfile();
		if (shop == null)
			return;

		List<(string, int)> modifiers = null;
		bool hasModifierData = shop.RunModifiersByTimestamp != null
			&& shop.RunModifiersByTimestamp.TryGetValue(record.SaveTimestamp, out modifiers);

		if (hasModifierData)
		{
			string labelText = GetShopLocalizedText($"{LocalisationKeys.ShopPrefix}ChallengerModeHistory.Active");
			labelText = StringDecorator.Decorate($"<b>{labelText}</b>");
			ui.Label.text = labelText;

			ui.Label.ForceMeshUpdate();
			var labelRect = ui.Label.rectTransform;
			float padW = 8f;
			float padH = 4f;
			float prefW = ui.Label.preferredWidth;
			float prefH = ui.Label.preferredHeight;
			labelRect.sizeDelta = new Vector2(Mathf.Max(32f, prefW + padW), Mathf.Max(16f, prefH + padH));
			if (panel?.packImage != null)
			{
				var packRect = panel.packImage.rectTransform;
				labelRect.anchoredPosition = packRect.anchoredPosition + new Vector2(0f, packRect.sizeDelta.y * 0.5f + labelRect.sizeDelta.y * 0.5f + 6f);
			}
			labelRect.SetAsLastSibling();
			ui.Label.gameObject.SetActive(true);

			string title = GetShopLocalizedText($"{LocalisationKeys.ShopPrefix}ChallengerModeHistory.Title");
			string body;
			if (modifiers == null || modifiers.Count == 0)
			{
				body = StringDecorator.Decorate("|r" + GetShopLocalizedText($"{LocalisationKeys.ShopPrefix}Loadout.None") + "|");
			}
			else
			{
				body = BuildChallengerHistoryTooltip(shop, modifiers);
			}
			ui.Tooltip?.SetDirect(title, body);
		}

		List<string> completedQuests = null;
		if (shop.RunCompletedQuestsByTimestamp != null
			&& shop.RunCompletedQuestsByTimestamp.TryGetValue(record.SaveTimestamp, out var history)
			&& history != null)
		{
			completedQuests = [.. history.Where(id => !string.IsNullOrEmpty(id))];
		}

		if (completedQuests == null || completedQuests.Count == 0)
			return;

		string questLabelText = GetShopLocalizedText($"{LocalisationKeys.ShopPrefix}QuestHistory.Active");
		questLabelText = StringDecorator.Decorate($"<b>{questLabelText}</b>");
		ui.QuestLabel.text = questLabelText;
		ui.QuestLabel.ForceMeshUpdate();

		var questLabelRect = ui.QuestLabel.rectTransform;
		float questPadW = 8f;
		float questPadH = 4f;
		float questPrefW = ui.QuestLabel.preferredWidth;
		float questPrefH = ui.QuestLabel.preferredHeight;
		questLabelRect.sizeDelta = new Vector2(Mathf.Max(32f, questPrefW + questPadW), Mathf.Max(16f, questPrefH + questPadH));

		if (panel?.packImage != null)
		{
			var packRect = panel.packImage.rectTransform;
			float baseY = packRect.anchoredPosition.y + packRect.sizeDelta.y * 0.5f + questLabelRect.sizeDelta.y * 0.5f + 6f;
			if (ui.Label.gameObject.activeSelf)
			{
				var labelRect = ui.Label.rectTransform;
				baseY = labelRect.anchoredPosition.y + labelRect.sizeDelta.y * 0.5f + questLabelRect.sizeDelta.y * 0.5f + 6f;
			}
			questLabelRect.anchoredPosition = new Vector2(packRect.anchoredPosition.x, baseY);
		}

		questLabelRect.SetAsLastSibling();
		ui.QuestLabel.gameObject.SetActive(true);

		string questTitle = GetShopLocalizedText($"{LocalisationKeys.ShopPrefix}QuestHistory.Title");
		string questBody = BuildQuestHistoryTooltip(completedQuests);
		ui.QuestTooltip?.SetDirect(questTitle, questBody);
	}

	private static string BuildQuestHistoryTooltip(List<string> completedQuests)
	{
		if (completedQuests == null || completedQuests.Count == 0)
			return string.Empty;

		var lines = new List<string>(completedQuests.Count);
		foreach (string questCardId in completedQuests)
		{
			if (string.IsNullOrEmpty(questCardId))
				continue;

			lines.Add(GetQuestName(questCardId));
		}

		return string.Join("\n", lines);
	}

	private static string GetQuestName(string questCardId)
	{
		if (string.IsNullOrEmpty(questCardId))
			return string.Empty;

		Card card = Library.TryCreateCard(questCardId, false);
		if (card == null)
			return questCardId;

		string localizedName = TypeFactory<Card>.LocalizeProperty(card.Id, "Name", true, true);
		if (string.IsNullOrEmpty(localizedName))
			localizedName = card.Id;

		var rarity = card.Config?.Rarity ?? Rarity.Common;
		return rarity switch
		{
			Rarity.Rare => StringDecorator.Decorate("|" + localizedName + "|"),
			Rarity.Uncommon => StringDecorator.Decorate("|s:" + localizedName + "|"),
			_ => localizedName,
		};
	}

	private static string BuildChallengerHistoryTooltip(LiteShop shop, List<(string, int)> modifiers)
	{
		var modifierMap = new Dictionary<string, int>(StringComparer.Ordinal);
		foreach (var entry in modifiers)
		{
			if (!string.IsNullOrEmpty(entry.Item1))
				modifierMap[entry.Item1] = entry.Item2;
		}

		var sb = new StringBuilder();

		foreach (var prefix in LocalisationKeys.CategoryOrder)
		{
			var items = shop.Items?.Values
				.Where(item => item != null && item.Id.StartsWith(prefix, StringComparison.Ordinal))
				.Select(item => item.Id)
				.Where(modifierMap.ContainsKey)
				.ToList();
			if (items == null || items.Count == 0)
				continue;

			string categoryKey = $"{LocalisationKeys.ShopPrefix}{prefix[..^1]}";
			string categoryName = GetShopLocalizedText(categoryKey);
			sb.Append(categoryName).Append('\n');

			foreach (var itemId in items)
			{
				int tier = modifierMap[itemId];
				int maxTier = shop.Items != null && shop.Items.TryGetValue(itemId, out var item) ? item.MaxTier : 0;
				string nameKey = $"{LocalisationKeys.ShopPrefix}{itemId}";
				string name = GetShopLocalizedText(nameKey);
				if (name == nameKey)
					name = itemId;
				string coloredName = LocalisationKeys.ColorizeTierName(name, tier, maxTier);
				string line = maxTier > 1 ? $"  {coloredName} {tier}" : $"  {coloredName}";
				sb.Append(line).Append('\n');
			}

			sb.Append('\n');
		}

		return sb.ToString().TrimEnd();
	}

	private static string GetShopLocalizedText(string key) => LocalisationKeys.Get(key);
}

// custom save file deletion isn't really necessary
// [HarmonyPatch(typeof(GameMaster), nameof(GameMaster.TryDeleteSaveData))]
// class GameMaster_Patch
// {
// 	static void Prefix(string filename)
// 	{
// 		var index = GameMaster.Instance.CurrentSaveIndex;
// 		if (index == null)
// 			return;
// 		if (GameMaster.GetGameRunFileName(index.Value) != filename)
// 			return;

// 		var customData = MiniTracker.Instance.CustomGrSaveData;
// 		var csd = customData;
// 		var fileName = "lvalonmimaShopSave.txt";
// 		var filePath = Path.Combine(GameMaster.PlatformHandler.GetSaveDataFolder(), fileName);
// 		if (!File.Exists(filePath))
// 			return;

// 		try
// 		{
// 			csd.OnGamerunEnded();

// 			if (csd.DeleteFileOnGamerunEnd)
// 				File.Delete(filePath);
// 		}
// 		catch (Exception)
// 		{
// 		}
// 	}
// }
