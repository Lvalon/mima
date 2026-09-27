using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.Core.Randoms;
using LBoL.Core.Stations;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.Exhibits.Common;
using LBoL.Presentation;
using LBoLEntitySideloader.Attributes;
using lvalonmima.Cards;
using lvalonmima.Source.Patches;
using lvalonmima.StatusEffects;

namespace lvalonmima.Exhibits;

public sealed partial class exquesting : Exhibit
{
	private bool NeedsDeferredOpenSlotReroll;

	private void PreRollQuestRequirementsForRolledCards(bool useGameRunRng)
	{
		if (RolledQuestCards == null || RolledQuestCards.Count == 0)
			return;

		foreach (var value in RolledQuestCards.Values)
		{
			Card card = value;
			if (card == null || string.IsNullOrEmpty(card.Id))
			{
				continue;
			}

			List<string> requiredCards = [nameof(cardquest2), nameof(cardquest20)];

			if (!requiredCards.Contains(card.Id))
			{
				continue;
			}

			if (!QuestRequirements.ContainsKey(card.Id))
			{
				string created = CreateQuestRequirement(card.Id);
				if (!string.IsNullOrEmpty(created))
				{
					QuestRequirements[card.Id] = created;
				}
			}
		}
	}

	private Dictionary<string, string> CaptureCurrentRolledRequirements()
	{
		Dictionary<string, string> snapshot = new(StringComparer.Ordinal);
		if (RolledQuestCards == null || RolledQuestCards.Count == 0 || QuestRequirements == null || QuestRequirements.Count == 0)
			return snapshot;

		foreach (var value in RolledQuestCards.Values)
		{
			Card card = value;
			if (card == null || string.IsNullOrEmpty(card.Id))
			{
				continue;
			}

			if (QuestRequirements.TryGetValue(card.Id, out string requirement) && !string.IsNullOrEmpty(requirement))
			{
				snapshot[card.Id] = requirement;
			}
		}

		return snapshot;
	}

	private void RestoreCurrentRolledRequirements(IDictionary<string, string> snapshot)
	{
		if (snapshot == null || snapshot.Count == 0 || RolledQuestCards == null || RolledQuestCards.Count == 0)
			return;

		foreach (var value in RolledQuestCards.Values)
		{
			Card card = value;
			if (card == null || string.IsNullOrEmpty(card.Id) || QuestRequirements.ContainsKey(card.Id))
			{
				continue;
			}

			if (snapshot.TryGetValue(card.Id, out string requirement) && !string.IsNullOrEmpty(requirement))
			{
				QuestRequirements[card.Id] = requirement;
			}
		}
	}

	public void CleanupStaleQuestRequirements()
	{
		if (QuestRequirements == null || QuestRequirements.Count == 0)
			return;

		List<string> stale = [];
		foreach (var key in QuestRequirements.Keys)
		{
			if (string.IsNullOrEmpty(key))
			{
				stale.Add(key);
				continue;
			}

			if (!PendingQuestProgress.ContainsKey(key) && !IsQuestCardCurrentlyRolled(key))
			{
				stale.Add(key);
			}
		}

		for (int i = 0; i < stale.Count; i++)
			QuestRequirements.Remove(stale[i]);
	}

	public void RefreshRolledQuestRequirementsForSave()
	{
		EnsureRolledQuestCards();

		if (RolledQuestCards == null || RolledQuestCards.Count == 0)
			return;

		foreach (var value in RolledQuestCards.Values)
		{
			Card card = value;
			if (card == null || string.IsNullOrEmpty(card.Id))
			{
				continue;
			}

			List<string> requiredCards = [nameof(cardquest2), nameof(cardquest20)];

			if (!requiredCards.Contains(card.Id))
			{
				continue;
			}

			if (PendingQuestProgress.ContainsKey(card.Id))
			{
				continue;
			}

			string refreshed = CreateQuestRequirement(card.Id);
			if (!string.IsNullOrEmpty(refreshed))
			{
				QuestRequirements[card.Id] = refreshed;
			}
		}

		CleanupStaleQuestRequirements();
	}

	public void EnsureRolledQuestCards()
	{
		if (RolledQuestCards.Count > 0)
			return;

		bool preserveAccepted = PendingQuestProgress is { Count: > 0 };
		RollQuestCards(preserveAccepted);
	}

	private List<Card> RollQuestCardsForSlots(int count, ISet<string> excludedQuestCardIds)
	{
		List<Card> result = new(Math.Max(0, count));
		if (count <= 0)
		{
			NeedsDeferredOpenSlotReroll = false;
			return result;
		}

		excludedQuestCardIds ??= new HashSet<string>(StringComparer.Ordinal);

		GameRunController gameRun = GameRun ?? GameMaster.Instance?.CurrentGameRun;
		if (gameRun == null) // fallback
		{
			NeedsDeferredOpenSlotReroll = true;
			for (int i = 0; i < count; i++)
				result.Add(Library.CreateCard<cardmimaexa>());
			return result;
		}

		int stationLevel = Math.Max(0, gameRun.CurrentStation?.Level ?? (MiniTracker.Instance?.CustomGrSaveData?.GetShopForCurrentProfile()?.BPProgress.TryGetValue("level", out int recordedLevel) == true ? recordedLevel : 0));
		float fraction = stationLevel % 17 / 17f;
		float k = 3f; // steepness
		float expStart = MathF.Exp(-k * MathF.Max(0f, fraction));
		float expEnd = MathF.Exp(-k * 1f);
		float levelDeduct10 = (expStart - expEnd) / (1f - expEnd);
		if (gameRun.Stages?.IndexOf(gameRun.CurrentStage) == 3)
		{
			levelDeduct10 = 0f; //no rares in act 4 duh
		}
		Card[] rolledCards = null;
		bool runNotReadyFallback = false;

		HashSet<string> conditionalExcludes = [];
		if (!gameRun.BaseDeck.Any(c => c.IsBasic))
			conditionalExcludes.Add(nameof(cardquest2));
		if (//gameRun.RollCard(new RandomGen(), new CardWeightTable(RarityWeightTable.EnemyCard, OwnerWeightTable.Valid, CardTypeWeightTable.CanBeLoot), false, false, config => config.RelativeEffects.Contains(nameof(Graze)) || config.UpgradedRelativeEffects.Contains(nameof(Graze))) == null
		(!gameRun.BaseDeck.Any(c => (c.Config.RelativeEffects.Contains(nameof(Graze)) && !c.IsUpgraded) || (c.Config.UpgradedRelativeEffects.Contains(nameof(Graze)) && c.IsUpgraded)))
		|| gameRun.Player.HasExhibit<LouguanJian>())
		{
			conditionalExcludes.Add(nameof(cardquest8));
		}
		if (gameRun.Player.HasExhibit<ChuRenou>())
			conditionalExcludes.Add(nameof(cardquest13));
		if (gameRun.Player.HasExhibit<Huiyuanka>())
			conditionalExcludes.Add(nameof(cardquest19));
		if (!gameRun.BaseDeck.Any(c => (c.Config.RelativeKeyword.HasFlag(Keyword.Shield) && !c.IsUpgraded) || (c.Config.UpgradedRelativeKeyword.HasFlag(Keyword.Shield) && c.IsUpgraded)))
			conditionalExcludes.Add(nameof(cardquest25));
		var mods = MiniTracker.Instance?.CustomGrSaveData?.GetShopForCurrentProfile()?.QuestModifiers ?? [];
		foreach (string id in PendingQuestModifiers.Keys.Concat(mods.Keys))
		{
			Card card = Library.TryCreateCard(id, false);
			if (card != null && card.Config.Rarity == Rarity.Rare)
				conditionalExcludes.Add(id);
		}

		try
		{
			rolledCards = toolbox.UniqueAllCards(
				gameRun.CardRng,
				new CardWeightTable(new RarityWeightTable(15f, 12f, 9f * levelDeduct10, 0f), OwnerWeightTable.AllOnes, CardTypeWeightTable.AllOnes),
				count,
				false,
				c => c != null
					&& !string.IsNullOrEmpty(c.Id)
					&& c.Config.RelativeEffects.Contains(nameof(sequest))
					&& !excludedQuestCardIds.Contains(c.Id)
					&& !conditionalExcludes.Contains(c.Id));
		}
		catch (InvalidOperationException)
		{
			runNotReadyFallback = true;
			NeedsDeferredOpenSlotReroll = true;
		}

		if (!runNotReadyFallback)
			NeedsDeferredOpenSlotReroll = false;

		if (rolledCards != null)
		{
			for (int i = 0; i < rolledCards.Length; i++)
			{
				if (rolledCards[i] != null)
				{
					result.Add(rolledCards[i]);
				}
			}
		}

		// fallback
		while (result.Count < count)
			result.Add(Library.CreateCard<cardmimaexa>());

		return result;
	}

	private int GetNextOpenVisibleSlot(List<int> slotsToPopulate)
	{
		if (slotsToPopulate == null || slotsToPopulate.Count == 0)
			return -1;

		for (int i = 0; i < VisibleQuestSlots.Length; i++)
		{
			int slot = VisibleQuestSlots[i];
			if (slotsToPopulate.Contains(slot))
			{
				return slot;
			}
		}

		return -1;
	}

	private void RestoreAcceptedSlotsFromPendingProgress(Dictionary<int, Card> acceptedSlotCards, List<int> slotsToPopulate)
	{
		if (acceptedSlotCards == null || slotsToPopulate == null || !PendingQuestProgress.Any())
			return;

		HashSet<string> assignedQuestIds = new(StringComparer.Ordinal);
		foreach (Card existingAccepted in acceptedSlotCards.Values)
		{
			if (existingAccepted != null && !string.IsNullOrEmpty(existingAccepted.Id))
			{
				assignedQuestIds.Add(existingAccepted.Id);
			}
		}

		foreach (var key in PendingQuestProgress.Keys)
		{
			string questCardId = key;
			if (string.IsNullOrEmpty(questCardId) || assignedQuestIds.Contains(questCardId))
			{
				continue;
			}

			int slot = GetNextOpenVisibleSlot(slotsToPopulate);
			if (slot < 0)
			{
				break;
			}

			Card restoredCard = Library.TryCreateCard(questCardId, false);
			if (restoredCard == null)
			{
				continue;
			}

			restoredCard.GameRun = GameRun;
			acceptedSlotCards[slot] = restoredCard;
			slotsToPopulate.Remove(slot);
			assignedQuestIds.Add(questCardId);
		}

	}

	private bool ShouldRecoverRolledCardsAfterSync()
	{
		if (GameRun == null)
			return false;

		if (NeedsDeferredOpenSlotReroll)
			return true;

		if (RolledQuestCards == null || RolledQuestCards.Count == 0)
			return true;

		foreach (var kvp in PendingQuestProgress)
		{
			string questCardId = kvp.Key;
			if (string.IsNullOrEmpty(questCardId))
			{
				continue;
			}

			bool foundAcceptedSlot = false;
			foreach (var (key, value) in RolledQuestCards)
			{
				int slot = key;
				Card card = value;
				if (card == null || string.IsNullOrEmpty(card.Id) || IsQuestSlotSoldOut(slot))
				{
					continue;
				}

				if (string.Equals(card.Id, questCardId, StringComparison.Ordinal))
				{
					foundAcceptedSlot = true;
					break;
				}
			}

			if (!foundAcceptedSlot)
			{
				return true;
			}
		}

		return false;
	}

	public void RollQuestCards(bool preserveAcceptedSlots)
	{
		Dictionary<int, Card> previousCards = new(RolledQuestCards);
		HashSet<int> previousSoldOutSlots = [.. SoldOutQuestSlots];
		List<int> slotsToPopulate = [];
		Dictionary<int, Card> acceptedSlotCards = [];

		RolledQuestCards.Clear();
		SoldOutQuestSlots.Clear();

		for (int i = 0; i < 10; i++)
		{
			if (Array.IndexOf(VisibleQuestSlots, i) >= 0)
			{
				Card existingCard = null;
				bool keepExistingAccepted = preserveAcceptedSlots
					&& previousCards.TryGetValue(i, out existingCard)
					&& existingCard != null
					&& !previousSoldOutSlots.Contains(i)
					&& !string.IsNullOrEmpty(existingCard.Id)
					&& PendingQuestProgress.ContainsKey(existingCard.Id);

				if (keepExistingAccepted)
				{
					acceptedSlotCards[i] = existingCard;
				}
				else
				{
					slotsToPopulate.Add(i);
				}
			}
			else
			{
				RolledQuestCards[i] = Library.CreateCard<cardmimaexb>();
			}
		}

		for (int i = 0; i < VisibleQuestSlots.Length; i++)
		{
			int slot = VisibleQuestSlots[i];
			if (acceptedSlotCards.TryGetValue(slot, out Card acceptedCard))
			{
				acceptedCard.GameRun = GameRun;
				RolledQuestCards[slot] = acceptedCard;
			}
		}

		if (preserveAcceptedSlots)
		{
			RestoreAcceptedSlotsFromPendingProgress(acceptedSlotCards, slotsToPopulate);
			for (int i = 0; i < VisibleQuestSlots.Length; i++)
			{
				int slot = VisibleQuestSlots[i];
				if (acceptedSlotCards.TryGetValue(slot, out Card acceptedCard))
				{
					acceptedCard.GameRun = GameRun;
					RolledQuestCards[slot] = acceptedCard;
				}
			}
		}

		HashSet<string> excludedQuestCardIds = new(StringComparer.Ordinal);
		foreach (Card acceptedCard in acceptedSlotCards.Values)
		{
			if (acceptedCard != null && !string.IsNullOrEmpty(acceptedCard.Id))
			{
				excludedQuestCardIds.Add(acceptedCard.Id);
			}
		}

		// Also exclude any quests that were just cleared from rolled slots during this runtime
		if (RecentlyClearedRolledQuestIds is { Count: > 0 })
		{
			foreach (var id in RecentlyClearedRolledQuestIds)
			{
				if (!string.IsNullOrEmpty(id))
					excludedQuestCardIds.Add(id);
			}
			// clear after applying exclusion so it's only a one-roll protection
			RecentlyClearedRolledQuestIds.Clear();
		}

		List<Card> rolledForOpenSlots = RollQuestCardsForSlots(slotsToPopulate.Count, excludedQuestCardIds);
		for (int i = 0; i < slotsToPopulate.Count; i++)
		{
			int slot = slotsToPopulate[i];
			Card rolledCard = i < rolledForOpenSlots.Count ? rolledForOpenSlots[i] : null;
			Card finalCard = rolledCard ?? Library.CreateCard<cardmimaexa>();
			finalCard.GameRun = GameRun;
			RolledQuestCards[slot] = finalCard;
		}

		PreRollQuestRequirementsForRolledCards(useGameRunRng: true);
		CleanupStaleQuestRequirements();

		// Persist rolled slots and their generated requirements immediately so
		// they don't get regenerated on the next station restart.
		try
		{
			ShopModHandlers.PersistQuestProgress(GameRun, PendingQuestProgress, syncToLiteShop: false, saveToDisk: false, questRequirements: QuestRequirements, completedQuestCards: CompletedQuestCards, writeToRunFlags: true, questModifiers: PendingQuestModifiers);
		}
		catch (Exception)
		{
			// swallow to avoid breaking runtime rolls
		}

	}

	public List<ShopItem<Card>> BuildRolledShopCards(GameRunController run)
	{
		EnsureRolledQuestCards();
		List<ShopItem<Card>> shopCards = new(10);
		for (int i = 0; i < 10; i++)
		{
			if (!RolledQuestCards.TryGetValue(i, out Card rolledCard) || rolledCard == null)
			{
				shopCards.Add(null);
				continue;
			}

			rolledCard.GameRun = run;

			ShopItem<Card> item = new(run, rolledCard, 0, false, false)
			{
				IsSoldOut = IsQuestSlotSoldOut(i)
			};
			shopCards.Add(item);
		}

		return shopCards;
	}
}
