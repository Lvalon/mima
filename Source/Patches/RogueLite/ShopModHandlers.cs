using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
using LBoL.Core.GapOptions;
using LBoL.Core.Randoms;
using LBoL.Core.Stations;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.Cards.Character.Cirno;
using LBoL.EntityLib.EnemyUnits.Character;
using LBoL.EntityLib.EnemyUnits.Normal;
using LBoL.EntityLib.EnemyUnits.Normal.Bats;
using LBoL.EntityLib.EnemyUnits.Normal.Drones;
using LBoL.EntityLib.EnemyUnits.Normal.Guihuos;
using LBoL.EntityLib.EnemyUnits.Normal.Maoyus;
using LBoL.EntityLib.EnemyUnits.Normal.Ravens;
using LBoL.EntityLib.EnemyUnits.Normal.Shenlings;
using LBoL.EntityLib.EnemyUnits.Normal.Yinyangyus;
using LBoL.EntityLib.EnemyUnits.Opponent;
using LBoL.EntityLib.Exhibits.Common;
using LBoL.EntityLib.StatusEffects.Enemy.Seija;
using LBoL.EntityLib.StatusEffects.Marisa;
using LBoL.Presentation;
using LBoL.Presentation.UI.Panels;
using LBoLEntitySideloader.CustomHandlers;
using lvalonmima.Cards;
using lvalonmima.Exhibits;
using lvalonmima.StatusEffects;

namespace lvalonmima.Source.Patches;

public partial class ShopModHandlers
{
	private const string QuestProgressFlagPrefix = "exquesting.quest:";

	private const string QuestRequirementFlagPrefix = "exquesting.requirement:";

	private const string QuestCompletedFlagPrefix = "exquesting.completed:";

	private const string QuestModifierFlagPrefix = "exquesting.modifier:";

	private const string QuestRolledFlagPrefix = "exquesting.rolled:"; // format: exquesting.rolled:slot=cardId

	private const string QuestSoldFlagPrefix = "exquesting.sold:";   // format: exquesting.sold:slot

	// Yields the text after `prefix` for every run flag that starts with it.
	private static IEnumerable<string> RunFlagPayloads(GameRunController gameRun, string prefix)
	{
		if (gameRun?.ExtraFlags == null)
			yield break;
		foreach (string flag in gameRun.ExtraFlags)
		{
			if (flag != null && flag.StartsWith(prefix, StringComparison.Ordinal))
				yield return flag[prefix.Length..];
		}
	}

	// Splits "key=value" at the first or last '='; false when either side would be empty.
	private static bool TrySplitPayload(string payload, bool splitAtLast, out string key, out string value)
	{
		int split = splitAtLast ? payload.LastIndexOf('=') : payload.IndexOf('=');
		if (split <= 0 || split >= payload.Length - 1)
		{
			key = value = null;
			return false;
		}
		key = payload[..split];
		value = payload[(split + 1)..];
		return true;
	}

	public static Dictionary<string, int> ReadQuestProgressFromRun(GameRunController gameRun)
	{
		var result = new Dictionary<string, int>(StringComparer.Ordinal);
		foreach (string payload in RunFlagPayloads(gameRun, QuestProgressFlagPrefix))
		{
			if (TrySplitPayload(payload, splitAtLast: true, out string cardId, out string progressText) && int.TryParse(progressText, out int progress))
				result[cardId] = progress;
		}
		return result;
	}

	public static void PersistQuestProgress(GameRunController gameRun, IDictionary<string, int> pendingQuestProgress, bool syncToLiteShop, bool saveToDisk)
	{
		PersistQuestProgress(gameRun, pendingQuestProgress, syncToLiteShop, saveToDisk, null, null, writeToRunFlags: true, questModifiers: null);
	}

	public static Dictionary<string, string> ReadQuestRequirementsFromRun(GameRunController gameRun)
	{
		var result = new Dictionary<string, string>(StringComparer.Ordinal);
		foreach (string payload in RunFlagPayloads(gameRun, QuestRequirementFlagPrefix))
		{
			if (TrySplitPayload(payload, splitAtLast: true, out string cardId, out string encodedRequirement))
				result[cardId] = encodedRequirement;
		}
		return result;
	}

	public static Dictionary<int, string> ReadRolledQuestCardsFromRun(GameRunController gameRun)
	{
		var result = new Dictionary<int, string>();
		foreach (string payload in RunFlagPayloads(gameRun, QuestRolledFlagPrefix))
		{
			if (TrySplitPayload(payload, splitAtLast: false, out string slotText, out string cardId) && int.TryParse(slotText, out int slot))
				result[slot] = cardId;
		}
		return result;
	}

	public static HashSet<int> ReadSoldQuestSlotsFromRun(GameRunController gameRun)
	{
		var result = new HashSet<int>();
		foreach (string payload in RunFlagPayloads(gameRun, QuestSoldFlagPrefix))
		{
			if (int.TryParse(payload, out int slot))
				result.Add(slot);
		}
		return result;
	}

	public static HashSet<string> ReadCompletedQuestCardsFromRun(GameRunController gameRun)
	{
		var result = new HashSet<string>(StringComparer.Ordinal);
		foreach (string questCardId in RunFlagPayloads(gameRun, QuestCompletedFlagPrefix))
		{
			if (questCardId.Length > 0)
				result.Add(questCardId);
		}
		return result;
	}

	public static Dictionary<int, string> ReadRolledQuestCardsFromLiteShop()
	{
		var result = new Dictionary<int, string>();
		var shop = MiniTracker.Instance?.CustomGrSaveData?.GetShopForCurrentProfile();
		if (shop?.QuestRolledSlots == null)
			return result;

		foreach (var (key, value) in shop.QuestRolledSlots)
		{
			if (value == null)
				continue;
			result[key] = value;
		}
		return result;
	}

	public static HashSet<int> ReadSoldQuestSlotsFromLiteShop()
	{
		var result = new HashSet<int>();
		var shop = MiniTracker.Instance?.CustomGrSaveData?.GetShopForCurrentProfile();
		if (shop?.QuestSoldSlots == null)
			return result;
		foreach (int slot in shop.QuestSoldSlots)
			result.Add(slot);
		return result;
	}

	public static Dictionary<string, int> ReadQuestModifiersFromRun(GameRunController gameRun)
	{
		var result = new Dictionary<string, int>(StringComparer.Ordinal);
		foreach (string payload in RunFlagPayloads(gameRun, QuestModifierFlagPrefix))
		{
			if (TrySplitPayload(payload, splitAtLast: true, out string cardId, out string stackText) && int.TryParse(stackText, out int stack))
				result[cardId] = stack;
		}
		return result;
	}

	public static Dictionary<string, int> ReadQuestProgressFromLiteShop()
	{
		var result = new Dictionary<string, int>(StringComparer.Ordinal);
		var shop = MiniTracker.Instance?.CustomGrSaveData?.GetShopForCurrentProfile();
		if (shop?.QuestProgress == null)
			return result;

		foreach (var (key, value) in shop.QuestProgress)
		{
			if (string.IsNullOrEmpty(key))
				continue;

			result[key] = value;
		}

		return result;
	}

	public static Dictionary<string, string> ReadQuestRequirementsFromLiteShop()
	{
		var result = new Dictionary<string, string>(StringComparer.Ordinal);
		var shop = MiniTracker.Instance?.CustomGrSaveData?.GetShopForCurrentProfile();
		if (shop?.QuestRequirements == null)
			return result;

		foreach (var (key, value) in shop.QuestRequirements)
		{
			if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value))
				continue;

			result[key] = value;
		}

		return result;
	}

	public static HashSet<string> ReadCompletedQuestCardsFromLiteShop()
	{
		var result = new HashSet<string>(StringComparer.Ordinal);
		var shop = MiniTracker.Instance?.CustomGrSaveData?.GetShopForCurrentProfile();
		if (shop?.QuestCompletedCards == null)
			return result;

		foreach (string questCardId in shop.QuestCompletedCards)
		{
			if (!string.IsNullOrEmpty(questCardId))
			{
				result.Add(questCardId);
			}
		}

		return result;
	}

	public static Dictionary<string, int> ReadQuestModifiersFromLiteShop()
	{
		var result = new Dictionary<string, int>(StringComparer.Ordinal);
		var shop = MiniTracker.Instance?.CustomGrSaveData?.GetShopForCurrentProfile();
		if (shop?.QuestModifiers == null)
			return result;

		foreach (var (key, value) in shop.QuestModifiers)
		{
			if (string.IsNullOrEmpty(key))
				continue;

			result[key] = value;
		}

		return result;
	}

	public static void RecordRewardedQuestCompletion(string questCardId)
	{
		if (string.IsNullOrEmpty(questCardId))
			return;

		var customData = MiniTracker.Instance?.CustomGrSaveData;
		var shop = customData?.GetShopForCurrentProfile();
		if (shop == null || !shop.ChallengerModeEnabled)
			return;

		shop.CurrentRunCompletedQuests ??= [];
		shop.CurrentRunCompletedQuests.Add(questCardId);

		customData.Save(0, false);
		ShopSaveLoader.Save();
	}

	public static void ResetCurrentRunRewardedQuestCompletions(LiteShop shop)
	{
		if (shop == null)
			return;

		shop.CurrentRunCompletedQuests = [];
	}

	public static void PersistQuestProgress(GameRunController gameRun, IDictionary<string, int> pendingQuestProgress, bool syncToLiteShop, bool saveToDisk, IDictionary<string, string> questRequirements, ISet<string> completedQuestCards = null, bool writeToRunFlags = true, IDictionary<string, int> questModifiers = null)
	{
		HashSet<string> completedToPersist = completedQuestCards == null
			? new HashSet<string>(StringComparer.Ordinal)
			: new HashSet<string>(completedQuestCards.Where(id => !string.IsNullOrEmpty(id)), StringComparer.Ordinal);

		Dictionary<string, int> modifiersToPersist = null;
		if (questModifiers != null)
		{
			modifiersToPersist = new Dictionary<string, int>(StringComparer.Ordinal);
			foreach (var (key, value) in questModifiers)
			{
				if (string.IsNullOrEmpty(key))
					continue;
				modifiersToPersist[key] = value;
			}
		}

		IEnumerable<string> completedForLog = completedQuestCards ?? Enumerable.Empty<string>();

		if (writeToRunFlags && gameRun?.ExtraFlags != null)
		{
			gameRun.ExtraFlags.RemoveWhere(flag =>
				!string.IsNullOrEmpty(flag) &&
				flag.StartsWith(QuestProgressFlagPrefix, StringComparison.Ordinal));

			gameRun.ExtraFlags.RemoveWhere(flag =>
				!string.IsNullOrEmpty(flag) &&
				flag.StartsWith(QuestRequirementFlagPrefix, StringComparison.Ordinal));

			gameRun.ExtraFlags.RemoveWhere(flag =>
				!string.IsNullOrEmpty(flag) &&
				flag.StartsWith(QuestCompletedFlagPrefix, StringComparison.Ordinal));

			// remove old rolled / sold flags as well
			gameRun.ExtraFlags.RemoveWhere(flag =>
				!string.IsNullOrEmpty(flag) &&
				flag.StartsWith(QuestRolledFlagPrefix, StringComparison.Ordinal));

			gameRun.ExtraFlags.RemoveWhere(flag =>
				!string.IsNullOrEmpty(flag) &&
				flag.StartsWith(QuestSoldFlagPrefix, StringComparison.Ordinal));

			// remove old modifier flags as well
			gameRun.ExtraFlags.RemoveWhere(flag =>
				!string.IsNullOrEmpty(flag) &&
				flag.StartsWith(QuestModifierFlagPrefix, StringComparison.Ordinal));

			if (pendingQuestProgress != null)
			{
				foreach (var (key, value) in pendingQuestProgress)
				{
					if (string.IsNullOrEmpty(key))
						continue;
					gameRun.ExtraFlags.Add($"{QuestProgressFlagPrefix}{key}={value}");
				}
			}

			if (questRequirements != null)
			{
				foreach (var (key, value) in questRequirements)
				{
					if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value))
						continue;
					gameRun.ExtraFlags.Add($"{QuestRequirementFlagPrefix}{key}={value}");
				}
			}

			if (completedToPersist.Count > 0)
			{
				foreach (string questCardId in completedToPersist)
				{
					gameRun.ExtraFlags.Add($"{QuestCompletedFlagPrefix}{questCardId}");
				}
			}

			// persist modifiers to run flags when requested
			Dictionary<string, int> modifiersToWrite = questModifiers != null
				? new Dictionary<string, int>(questModifiers, StringComparer.Ordinal)
				: gameRun?.Player?.GetExhibit<exquesting>()?.PendingQuestModifiers;

			if (modifiersToWrite != null)
			{
				foreach (var (key, value) in modifiersToWrite)
				{
					if (string.IsNullOrEmpty(key))
						continue;
					gameRun.ExtraFlags.Add($"{QuestModifierFlagPrefix}{key}={value}");
				}
			}

			// persist current rolled slots and sold slots from runtime exhibit if available
			try
			{
				exquesting runtimeExhibit = gameRun?.Player?.GetExhibit<exquesting>();
				if (runtimeExhibit != null && runtimeExhibit.RolledQuestCards != null)
				{
					foreach (var (key, value) in runtimeExhibit.RolledQuestCards)
					{
						if (value == null || string.IsNullOrEmpty(value.Id))
							continue;
						gameRun.ExtraFlags.Add($"{QuestRolledFlagPrefix}{key}={value.Id}");
					}
				}
				if (runtimeExhibit != null && runtimeExhibit.SoldOutQuestSlots != null)
				{
					foreach (int slot in runtimeExhibit.SoldOutQuestSlots)
					{
						gameRun.ExtraFlags.Add($"{QuestSoldFlagPrefix}{slot}");
					}
				}
			}
			catch (Exception e)
			{
				BepinexPlugin.log?.LogDebug(e);
			}

		}

		if (syncToLiteShop)
		{
			var shop = MiniTracker.Instance?.CustomGrSaveData?.GetShopForCurrentProfile();
			if (shop != null)
			{
				shop.QuestProgress = pendingQuestProgress == null
					? new Dictionary<string, int>(StringComparer.Ordinal)
					: new Dictionary<string, int>(pendingQuestProgress, StringComparer.Ordinal);

				shop.QuestRequirements = questRequirements == null
					? new Dictionary<string, string>(StringComparer.Ordinal)
					: new Dictionary<string, string>(questRequirements, StringComparer.Ordinal);

				shop.QuestCompletedCards = new HashSet<string>(completedToPersist, StringComparer.Ordinal);

				if (modifiersToPersist != null)
				{
					shop.QuestModifiers = new Dictionary<string, int>(modifiersToPersist, StringComparer.Ordinal);
				}
				else
				{
					var mods = gameRun?.Player?.GetExhibit<exquesting>()?.PendingQuestModifiers;
					if (mods != null)
						shop.QuestModifiers = new Dictionary<string, int>(mods, StringComparer.Ordinal);
					else
						shop.QuestModifiers ??= new Dictionary<string, int>(StringComparer.Ordinal);
				}

				// Persist rolled slots and sold slots to lite shop
				try
				{
					exquesting runtimeExhibit = gameRun?.Player?.GetExhibit<exquesting>();
					if (runtimeExhibit != null && runtimeExhibit.RolledQuestCards != null)
					{
						shop.QuestRolledSlots = runtimeExhibit.RolledQuestCards.ToDictionary(k => k.Key, v => v.Value?.Id);
					}
					else
					{
						shop.QuestRolledSlots = [];
					}

					if (runtimeExhibit != null && runtimeExhibit.SoldOutQuestSlots != null)
					{
						shop.QuestSoldSlots = [.. runtimeExhibit.SoldOutQuestSlots];
					}
					else
					{
						shop.QuestSoldSlots = [];
					}
				}
				catch (Exception e)
				{
					BepinexPlugin.log?.LogDebug(e);
				}

				// persist rolled and sold slots into lite shop as well
				var exhibit = gameRun?.Player?.GetExhibit<exquesting>();
				if (exhibit != null)
				{
					shop.QuestRolledSlots = exhibit.RolledQuestCards?.ToDictionary(k => k.Key, v => v.Value?.Id) ?? [];
					shop.QuestSoldSlots = exhibit.SoldOutQuestSlots != null ? [.. exhibit.SoldOutQuestSlots] : [];
				}
			}
		}

		if (saveToDisk)
		{
			MiniTracker.Instance?.CustomGrSaveData?.Save(0, false);
			ShopSaveLoader.Save();
		}
	}

	public static void QueueResolveCompletedQuestEffectsOnStationEnter(GameRunController gameRun, exquesting exhibit)
	{
		if (gameRun == null || exhibit == null || exhibit.CompletedQuestCards == null || exhibit.CompletedQuestCards.Count == 0)
			return;

		HashSet<string> completedSnapshot = new(exhibit.CompletedQuestCards, StringComparer.Ordinal);
		GameMaster.Instance?.StartCoroutine(CoResolveCompletedQuestEffectsOnStationEnter(gameRun, exhibit, completedSnapshot));
	}

	private static IEnumerator CoResolveCompletedQuestEffectsOnStationEnter(GameRunController gameRun, exquesting exhibit, HashSet<string> completedSnapshot)
	{
		if (gameRun == null || exhibit == null || completedSnapshot == null || completedSnapshot.Count == 0)
			yield break;

		// Run after station enter initialization so vanilla restore/state hydration does not overwrite effects.
		yield return null;
		HashSet<string> consumedCompleted = new(StringComparer.Ordinal);

		foreach (string questCardId in completedSnapshot)
		{
			if (string.IsNullOrEmpty(questCardId))
				continue;

			if (exhibit.IsFreshlyCompletedQuestCard(questCardId))
			{
				exhibit.ClearFreshQuestCompletion(questCardId);
				continue;
			}

			switch (questCardId)
			{
				case nameof(cardquest4):
					Card card = Library.CreateCard<cardquest4>();
					Card genji = null;
					for (int attempt = 0; attempt < 60; attempt++)
					{
						genji = gameRun.BaseDeck?.FirstOrDefault(c => c.Id == nameof(cardgenji));
						if (genji != null)
						{
							break;
						}

						if (attempt < 59)
						{
							yield return null;
						}
					}
					if (genji != null && card != null)
					{
						gameRun.RemoveDeckCard(genji);
						gameRun.GainPower((int)card.Config.Value2);
						gameRun.Heal((int)card.Config.Value2);
					}
					consumedCompleted.Add(questCardId);
					break;
				case nameof(cardquest10):
					cardquest10 card2 = Library.CreateCard<cardquest10>();
					if (card2 != null)
					{
						int requiredShadows = card2.Value20;
						for (int attempt = 0; attempt < 60; attempt++)
						{
							int shadowCount = gameRun.BaseDeck?.Count(c => c.Id == nameof(LBoL.EntityLib.Cards.Neutral.Black.Shadow)) ?? 0;
							if (shadowCount >= requiredShadows)
							{
								break;
							}

							if (attempt < 59)
							{
								yield return null;
							}
						}

						List<Card> shadows = [.. gameRun.BaseDeck
							.Where(c => c.Id == nameof(LBoL.EntityLib.Cards.Neutral.Black.Shadow))
							.Take(card2.Value20)];
						if (shadows.Count >= card2.Value20)
						{
							gameRun.RemoveDeckCards(shadows);
							gameRun.Heal((int)card2.Config.Value2);
							gameRun.GainMoney((int)card2.Config.Value2 * 10, true, new VisualSourceData
							{
								SourceType = VisualSourceType.Entity,
								Source = exhibit,
							});
						}
					}
					consumedCompleted.Add(questCardId);
					break;
				case nameof(cardquest26) when gameRun.CurrentStation.Type == StationType.Boss:
					// exhibit.PendingQuestModifiers.TryGetValue(questCardId, out int stack);
					// exhibit.PendingQuestModifiers[questCardId] = ++stack;
					consumedCompleted.Add(questCardId);
					break;
				case nameof(cardquest27) when gameRun.CurrentStation.Type == StationType.Boss:
					consumedCompleted.Add(questCardId);
					break;
				case nameof(cardquest28) when gameRun.CurrentStation.Type == StationType.Boss:
					consumedCompleted.Add(questCardId);
					break;
				case nameof(cardquest29) when gameRun.CurrentStation.Type == StationType.Boss:
					consumedCompleted.Add(questCardId);
					break;
				case nameof(cardquest30) when gameRun.CurrentStation.Type == StationType.Boss:
					consumedCompleted.Add(questCardId);
					break;
				default:
					break;
			}
		}

		if (consumedCompleted.Count > 0)
		{
			foreach (string questCardId in consumedCompleted)
			{
				exhibit.ClearQuestCompleted(questCardId);
				foreach (var (key, value) in exhibit.RolledQuestCards)
				{
					Card slotCard = value;
					if (slotCard != null && string.Equals(slotCard.Id, questCardId, StringComparison.Ordinal))
					{
						exhibit.SoldOutQuestSlots.Remove(key);
					}
				}
			}

		}

		// Persist post-resolution state to avoid regression on immediate reload.
		PersistQuestProgress(gameRun, exhibit.PendingQuestProgress, syncToLiteShop: true, saveToDisk: true, questRequirements: exhibit.QuestRequirements, completedQuestCards: exhibit.CompletedQuestCards, questModifiers: exhibit.PendingQuestModifiers);
	}

	private static string FormatQuestProgress(IEnumerable<KeyValuePair<string, int>> data)
	{
		if (data == null)
			return "";

		return string.Join(", ", data
			.Where(kvp => !string.IsNullOrEmpty(kvp.Key))
			.Select(kvp => $"{kvp.Key}={kvp.Value}"));
	}

	private static void ResetQuestStateForNewRun(GameRunController gameRun, LiteShop shop)
	{
		if (shop != null)
		{
			shop.QuestProgress = new Dictionary<string, int>(StringComparer.Ordinal);
			shop.QuestRequirements = new Dictionary<string, string>(StringComparer.Ordinal);
			shop.QuestCompletedCards = new HashSet<string>(StringComparer.Ordinal);
			shop.QuestModifiers = new Dictionary<string, int>(StringComparer.Ordinal);
			ResetCurrentRunRewardedQuestCompletions(shop);
		}

		if (gameRun?.ExtraFlags != null)
		{
			gameRun.ExtraFlags.RemoveWhere(flag =>
				!string.IsNullOrEmpty(flag)
				&& (flag.StartsWith(QuestProgressFlagPrefix, StringComparison.Ordinal)
					|| flag.StartsWith(QuestRequirementFlagPrefix, StringComparison.Ordinal)
					|| flag.StartsWith(QuestCompletedFlagPrefix, StringComparison.Ordinal)));
		}

	}
}
