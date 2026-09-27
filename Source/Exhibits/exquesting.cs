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

public sealed class exquestingDef : lvalonmimaExhibitTemplate
{
	public override ExhibitConfig MakeConfig()
	{
		ExhibitConfig exhibitConfig = GetDefaultExhibitConfig();
		exhibitConfig.LosableType = ExhibitLosableType.CantLose;
		exhibitConfig.Owner = null;
		exhibitConfig.BaseManaColor = ManaColor.Colorless;
		exhibitConfig.RelativeEffects = [nameof(sequest)];
		exhibitConfig.Rarity = Rarity.Rare;
		// This exhibit is UI-only: clicking it will open an empty shop UI.
		return exhibitConfig;
	}
}

[EntityLogic(typeof(exquestingDef))]
public sealed partial class exquesting : Exhibit
{
	public Dictionary<string, int> PendingQuestProgress = [];

	public Dictionary<string, string> QuestRequirements = new(StringComparer.Ordinal);

	public HashSet<string> CompletedQuestCards = new(StringComparer.Ordinal);

	private readonly HashSet<string> FreshlyCompletedQuestCards = new(StringComparer.Ordinal);

	public Dictionary<int, Card> RolledQuestCards = [];

	public HashSet<int> SoldOutQuestSlots = [];

	// Temporarily track quest IDs whose rolled slots were just cleared so we avoid re-rolling them immediately
	public HashSet<string> RecentlyClearedRolledQuestIds = new(StringComparer.Ordinal);

	private static readonly int[] VisibleQuestSlots = [1, 2, 3, 5, 6, 7];

	public static readonly string[] CardQuest2TypeKeys = ["TypeAttack", "TypeDefense", "TypeSkill", "TypeAbility"];

	public Dictionary<string, int> PendingQuestModifiers = [];

	private string FormatPendingQuestProgress()
	{
		if (PendingQuestProgress == null || PendingQuestProgress.Count == 0)
			return "<empty>";

		return string.Join(", ", PendingQuestProgress
			.Where(kvp => !string.IsNullOrEmpty(kvp.Key))
			.Select(kvp => $"{kvp.Key}:{kvp.Value}"));
	}

	private string FormatCompletedQuestCards()
	{
		if (CompletedQuestCards == null || CompletedQuestCards.Count == 0)
			return "<empty>";

		return string.Join(", ", CompletedQuestCards.Where(id => !string.IsNullOrEmpty(id)));
	}

	private string FormatRolledQuestSlots()
	{
		if (RolledQuestCards == null || RolledQuestCards.Count == 0)
			return "<empty>";

		List<string> parts = new(RolledQuestCards.Count);
		foreach (var kvp in RolledQuestCards.OrderBy(k => k.Key))
		{
			int slot = kvp.Key;
			Card card = kvp.Value;
			string cardId = card?.Id ?? "<null>";
			bool accepted = card != null && !string.IsNullOrEmpty(card.Id) && PendingQuestProgress.ContainsKey(card.Id);
			bool completed = false;
			bool soldOut = IsQuestSlotSoldOut(slot);
			parts.Add($"{slot}:{cardId}(accepted={accepted},completed={completed},soldOut={soldOut})");
		}

		return string.Join(" | ", parts);
	}

	public bool TryGetQuestRequirement(string questCardId, out string encodedRequirement)
	{
		if (string.IsNullOrEmpty(questCardId))
		{
			encodedRequirement = null;
			return false;
		}

		return QuestRequirements.TryGetValue(questCardId, out encodedRequirement) && !string.IsNullOrEmpty(encodedRequirement);
	}

	public void ClearQuestRequirement(string questCardId)
	{
		if (!string.IsNullOrEmpty(questCardId))
			QuestRequirements.Remove(questCardId);
	}

	public string EnsureRequirementLockedForQuest(string questCardId)
	{
		if (string.IsNullOrEmpty(questCardId))
			return string.Empty;

		if (TryGetQuestRequirement(questCardId, out string existing))
			return existing;

		string created = CreateQuestRequirement(questCardId);
		if (!string.IsNullOrEmpty(created))
			QuestRequirements[questCardId] = created;

		return created;
	}

	private string CreateQuestRequirement(string questCardId)
	{
		return questCardId switch
		{
			nameof(cardquest2) => quest2(),
			nameof(cardquest20) => GameRun.RollCards(GameRun.CardRng, new CardWeightTable(RarityWeightTable.EnemyCard, OwnerWeightTable.Valid, CardTypeWeightTable.CanBeLoot), 1, false, false, null)[0].Id,
			_ => string.Empty,
		};
		string quest2()
		{
			float typeRoll = GameRun.CardRng.NextFloat();
			int typeIndex = Math.Min(CardQuest2TypeKeys.Length - 1, Math.Max(0, (int)(typeRoll * CardQuest2TypeKeys.Length)));
			string typeKey = CardQuest2TypeKeys[typeIndex];
			string rarityKey = typeKey != "TypeAbility" ? "RarityCommon" : "RarityUncommon";
			return cardquest2.EncodeRequirement(rarityKey, typeKey);
		}
	}

	private bool IsQuestCardCurrentlyRolled(string questCardId)
	{
		if (string.IsNullOrEmpty(questCardId) || RolledQuestCards == null || RolledQuestCards.Count == 0)
			return false;

		foreach (var value in RolledQuestCards.Values)
		{
			Card card = value;
			if (card != null && string.Equals(card.Id, questCardId, StringComparison.Ordinal))
			{
				return true;
			}
		}

		return false;
	}

	public Card GetRolledQuestCard(int slotIndex)
	{
		EnsureRolledQuestCards();
		if (RolledQuestCards.TryGetValue(slotIndex, out Card card) && card != null)
			return card;

		return null;
	}

	public void MarkQuestSlotSoldOut(int slotIndex)
	{
		if (slotIndex >= 0)
			SoldOutQuestSlots.Add(slotIndex);
	}

	public bool IsQuestSlotSoldOut(int slotIndex)
	{
		return SoldOutQuestSlots.Contains(slotIndex);
	}

	public bool IsQuestCardSoldOut(string questCardId)
	{
		if (string.IsNullOrEmpty(questCardId))
			return false;

		if (RolledQuestCards == null || RolledQuestCards.Count == 0)
			return false;

		foreach (var (key, value) in RolledQuestCards)
		{
			Card card = value;
			if (card != null
				&& string.Equals(card.Id, questCardId, StringComparison.Ordinal)
				&& IsQuestSlotSoldOut(key))
			{
				return true;
			}
		}

		return false;
	}

	public bool IsQuestSlotAccepted(int slotIndex)
	{
		if (IsQuestSlotSoldOut(slotIndex))
			return false;

		Card card = GetRolledQuestCard(slotIndex);
		if (card == null || string.IsNullOrEmpty(card.Id))
			return false;

		return PendingQuestProgress.ContainsKey(card.Id);
	}

	public bool IsQuestCardCompleted(string questCardId)
	{
		return !string.IsNullOrEmpty(questCardId) && CompletedQuestCards.Contains(questCardId);
	}

	public void MarkQuestCompleted(string questCardId)
	{
		if (string.IsNullOrEmpty(questCardId))
			return;

		CompletedQuestCards.Add(questCardId);
		FreshlyCompletedQuestCards.Add(questCardId);
		PendingQuestProgress.Remove(questCardId);
		ClearQuestRequirement(questCardId);
	}

	public void ClearQuestCompleted(string questCardId)
	{
		if (!string.IsNullOrEmpty(questCardId))
		{
			CompletedQuestCards.Remove(questCardId);
			FreshlyCompletedQuestCards.Remove(questCardId);
		}
	}

	public bool IsFreshlyCompletedQuestCard(string questCardId)
	{
		return !string.IsNullOrEmpty(questCardId) && FreshlyCompletedQuestCards.Contains(questCardId);
	}

	public void ClearFreshQuestCompletion(string questCardId)
	{
		if (!string.IsNullOrEmpty(questCardId))
			FreshlyCompletedQuestCards.Remove(questCardId);
	}

	public void FinalizeQuestByCardId(string questCardId)
	{
		if (string.IsNullOrEmpty(questCardId))
			return;

		CompletedQuestCards.Add(questCardId);

		if (RolledQuestCards == null || RolledQuestCards.Count == 0)
			EnsureRolledQuestCards();

		int acceptedMatch = -1;
		foreach (var (key, value) in RolledQuestCards)
		{
			int slot = key;
			Card card = value;
			if (card != null && string.Equals(card.Id, questCardId, StringComparison.Ordinal) && IsQuestSlotAccepted(slot))
			{
				acceptedMatch = slot;
				break;
			}
		}

		if (acceptedMatch >= 0)
		{
			SoldOutQuestSlots.Add(acceptedMatch);
			return;
		}

		foreach (var (key, value) in RolledQuestCards)
		{
			int slot = key;
			Card card = value;
			if (card != null && string.Equals(card.Id, questCardId, StringComparison.Ordinal) && !IsQuestSlotSoldOut(slot))
			{
				SoldOutQuestSlots.Add(slot);
				return;
			}
		}
	}

	public void UnlockCompletedQuestSlots()
	{
		if (PendingQuestProgress == null || PendingQuestProgress.Count == 0)
			return;

		List<string> completed = [];
		foreach (var (key, value) in PendingQuestProgress)
		{
			Card card = Library.TryCreateCard(key, false);
			if (card == null)
			{
				continue;
			}

			int goal = card.Config.Value1 ?? -1;
			if (goal > 0 && value >= goal)
			{
				completed.Add(key);
			}
		}

		for (int i = 0; i < completed.Count; i++)
		{
			string cardId = completed[i];
			FinalizeQuestByCardId(cardId);
			MarkQuestCompleted(cardId);
		}

		CleanupStaleQuestRequirements();
	}
}
