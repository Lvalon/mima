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

namespace lvalonmima.Exhibits
{
	public sealed class exquestingDef : lvalonmimaExhibitTemplate
	{
		public override ExhibitConfig MakeConfig()
		{
			ExhibitConfig exhibitConfig = GetDefaultExhibitConfig();
			exhibitConfig.LosableType = ExhibitLosableType.CantLose;
			exhibitConfig.Owner = null;
			exhibitConfig.BaseManaColor = ManaColor.Colorless;
			exhibitConfig.RelativeEffects = new List<string>() { nameof(sequest) };
			exhibitConfig.Rarity = Rarity.Rare;
			// This exhibit is UI-only: clicking it will open an empty shop UI.
			return exhibitConfig;
		}
	}

	[EntityLogic(typeof(exquestingDef))]
	public sealed class exquesting : Exhibit
	{
		private static readonly HashSet<exquesting> DeferredRestoreHydration = new HashSet<exquesting>();
		private bool NeedsDeferredOpenSlotReroll;
		public Dictionary<string, int> PendingQuestProgress = new Dictionary<string, int>();
		public Dictionary<string, string> QuestRequirements = new Dictionary<string, string>(StringComparer.Ordinal);
		public HashSet<string> CompletedQuestCards = new HashSet<string>(StringComparer.Ordinal);
		private readonly HashSet<string> FreshlyCompletedQuestCards = new HashSet<string>(StringComparer.Ordinal);
		public Dictionary<int, Card> RolledQuestCards = new Dictionary<int, Card>();
		public HashSet<int> SoldOutQuestSlots = new HashSet<int>();
		// Temporarily track quest IDs whose rolled slots were just cleared so we avoid re-rolling them immediately
		public HashSet<string> RecentlyClearedRolledQuestIds = new HashSet<string>(StringComparer.Ordinal);
		private static readonly int[] VisibleQuestSlots = { 1, 2, 3, 5, 6, 7 };
		public static readonly string[] CardQuest2TypeKeys = { "TypeAttack", "TypeDefense", "TypeSkill", "TypeAbility" };
		public Dictionary<string, int> PendingQuestModifiers = new Dictionary<string, int>();

		private string FormatPendingQuestProgress()
		{
			if (PendingQuestProgress == null || PendingQuestProgress.Count == 0)
			{
				return "<empty>";
			}

			return string.Join(", ", PendingQuestProgress
				.Where(kvp => !string.IsNullOrEmpty(kvp.Key))
				.Select(kvp => $"{kvp.Key}:{kvp.Value}"));
		}

		private string FormatCompletedQuestCards()
		{
			if (CompletedQuestCards == null || CompletedQuestCards.Count == 0)
			{
				return "<empty>";
			}

			return string.Join(", ", CompletedQuestCards.Where(id => !string.IsNullOrEmpty(id)));
		}

		private string FormatRolledQuestSlots()
		{
			if (RolledQuestCards == null || RolledQuestCards.Count == 0)
			{
				return "<empty>";
			}

			List<string> parts = new List<string>(RolledQuestCards.Count);
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
			{
				QuestRequirements.Remove(questCardId);
			}
		}

		public string EnsureRequirementLockedForQuest(string questCardId)
		{
			if (string.IsNullOrEmpty(questCardId))
			{
				return string.Empty;
			}

			if (TryGetQuestRequirement(questCardId, out string existing))
			{
				return existing;
			}

			string created = CreateQuestRequirement(questCardId);
			if (!string.IsNullOrEmpty(created))
			{
				QuestRequirements[questCardId] = created;
			}

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
			{
				return false;
			}

			foreach (var kvp in RolledQuestCards)
			{
				Card card = kvp.Value;
				if (card != null && string.Equals(card.Id, questCardId, StringComparison.Ordinal))
				{
					return true;
				}
			}

			return false;
		}

		private void PreRollQuestRequirementsForRolledCards(bool useGameRunRng)
		{
			if (RolledQuestCards == null || RolledQuestCards.Count == 0)
			{
				return;
			}

			foreach (var kvp in RolledQuestCards)
			{
				Card card = kvp.Value;
				if (card == null || string.IsNullOrEmpty(card.Id))
				{
					continue;
				}

				List<string> requiredCards = new List<string>() { nameof(cardquest2), nameof(cardquest20) };

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
			Dictionary<string, string> snapshot = new Dictionary<string, string>(StringComparer.Ordinal);
			if (RolledQuestCards == null || RolledQuestCards.Count == 0 || QuestRequirements == null || QuestRequirements.Count == 0)
			{
				return snapshot;
			}

			foreach (var kvp in RolledQuestCards)
			{
				Card card = kvp.Value;
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
			{
				return;
			}

			foreach (var kvp in RolledQuestCards)
			{
				Card card = kvp.Value;
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
			{
				return;
			}

			List<string> stale = new List<string>();
			foreach (var kvp in QuestRequirements)
			{
				if (string.IsNullOrEmpty(kvp.Key))
				{
					stale.Add(kvp.Key);
					continue;
				}

				if (!PendingQuestProgress.ContainsKey(kvp.Key) && !IsQuestCardCurrentlyRolled(kvp.Key))
				{
					stale.Add(kvp.Key);
				}
			}

			for (int i = 0; i < stale.Count; i++)
			{
				QuestRequirements.Remove(stale[i]);
			}
		}

		public void RefreshRolledQuestRequirementsForSave()
		{
			EnsureRolledQuestCards();

			if (RolledQuestCards == null || RolledQuestCards.Count == 0)
			{
				return;
			}

			foreach (var kvp in RolledQuestCards)
			{
				Card card = kvp.Value;
				if (card == null || string.IsNullOrEmpty(card.Id))
				{
					continue;
				}

				List<string> requiredCards = new List<string>() { nameof(cardquest2), nameof(cardquest20) };

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
			{
				return;
			}

			bool preserveAccepted = PendingQuestProgress != null && PendingQuestProgress.Count > 0;
			BepinexPlugin.log.LogInfo($"[EXQUESTING SAVE] EnsureRolledQuestCards rolling preserveAccepted={preserveAccepted}, pendingCount={PendingQuestProgress?.Count ?? 0}");
			RollQuestCards(preserveAccepted);
		}

		private List<Card> RollQuestCardsForSlots(int count, ISet<string> excludedQuestCardIds)
		{
			List<Card> result = new List<Card>(Math.Max(0, count));
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
				BepinexPlugin.log.LogInfo($"[EXQUESTING SAVE] RollQuestCardsForSlots marked deferred reroll reason=GameRunNull count={count}");
				BepinexPlugin.log.LogInfo($"[EXQUESTING SAVE] RollQuestCardsForSlots fallback reason=GameRunNull count={count}");
				for (int i = 0; i < count; i++)
				{
					result.Add(Library.CreateCard<cardmimaexa>());
				}
				return result;
			}

			int stationLevel = Math.Max(0, gameRun.CurrentStation?.Level ?? 0);
			float levelDeduct = 1f - stationLevel % 16 / 16f;
			Card[] rolledCards = null;
			bool runNotReadyFallback = false;

			HashSet<string> conditionalExcludes = new HashSet<string>();
			if (!gameRun.BaseDeck.Any(c => c.IsBasic))
			{
				conditionalExcludes.Add(nameof(cardquest2));
			}
			if (gameRun.RollCard(new RandomGen(), new CardWeightTable(RarityWeightTable.EnemyCard, OwnerWeightTable.Valid, CardTypeWeightTable.CanBeLoot), false, false, config => config.RelativeEffects.Contains(nameof(Graze)) || config.UpgradedRelativeEffects.Contains(nameof(Graze))) == null
			|| gameRun.Player.HasExhibit<LouguanJian>())
			{
				conditionalExcludes.Add(nameof(cardquest8));
			}
			if (gameRun.Player.HasExhibit<ChuRenou>())
			{
				conditionalExcludes.Add(nameof(cardquest13));
			}
			if (gameRun.Player.HasExhibit<Huiyuanka>())
			{
				conditionalExcludes.Add(nameof(cardquest19));
			}
			if (!gameRun.BaseDeck.Any(c => (c.Config.RelativeKeyword.HasFlag(Keyword.Shield) && !c.IsUpgraded) || (c.Config.UpgradedRelativeKeyword.HasFlag(Keyword.Shield) && c.IsUpgraded)))
			{
				conditionalExcludes.Add(nameof(cardquest25));
			}
			var mods = MiniTracker.Instance?.CustomGrSaveData?.GetShopForCurrentProfile()?.QuestModifiers ?? new Dictionary<string, int>();
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
					new CardWeightTable(new RarityWeightTable(10f, 5f, levelDeduct, 0f), OwnerWeightTable.AllOnes, CardTypeWeightTable.AllOnes),
					count,
					false,
					c => c != null
						&& !string.IsNullOrEmpty(c.Id)
						&& c.Config.RelativeEffects.Contains(nameof(sequest))
						&& !excludedQuestCardIds.Contains(c.Id)
						&& !conditionalExcludes.Contains(c.Id));
			}
			catch (InvalidOperationException ex)
			{
				runNotReadyFallback = true;
				NeedsDeferredOpenSlotReroll = true;
				BepinexPlugin.log.LogInfo($"[EXQUESTING SAVE] RollQuestCardsForSlots marked deferred reroll reason=RunNotStarted count={count}");
				BepinexPlugin.log.LogWarning($"[EXQUESTING SAVE] RollQuestCardsForSlots fallback reason=RunNotStarted count={count} stationLevel={stationLevel} error={ex.Message}");
			}

			if (!runNotReadyFallback)
			{
				NeedsDeferredOpenSlotReroll = false;
			}

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
			{
				result.Add(Library.CreateCard<cardmimaexa>());
			}

			return result;
		}

		private int GetNextOpenVisibleSlot(List<int> slotsToPopulate)
		{
			if (slotsToPopulate == null || slotsToPopulate.Count == 0)
			{
				return -1;
			}

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
			{
				BepinexPlugin.log.LogInfo($"[EXQUESTING SAVE] RestoreAcceptedSlotsFromPendingProgress skipped: acceptedSlotCardsNull={acceptedSlotCards == null}, slotsNull={slotsToPopulate == null}, pendingCount={PendingQuestProgress?.Count ?? 0}");
				return;
			}

			HashSet<string> assignedQuestIds = new HashSet<string>(StringComparer.Ordinal);
			foreach (Card existingAccepted in acceptedSlotCards.Values)
			{
				if (existingAccepted != null && !string.IsNullOrEmpty(existingAccepted.Id))
				{
					assignedQuestIds.Add(existingAccepted.Id);
				}
			}

			foreach (var kvp in PendingQuestProgress)
			{
				string questCardId = kvp.Key;
				if (string.IsNullOrEmpty(questCardId) || assignedQuestIds.Contains(questCardId))
				{
					BepinexPlugin.log.LogInfo($"[EXQUESTING SAVE] RestoreAcceptedSlotsFromPendingProgress skip quest={questCardId ?? "<null>"} alreadyAssigned={assignedQuestIds.Contains(questCardId ?? string.Empty)}");
					continue;
				}

				int slot = GetNextOpenVisibleSlot(slotsToPopulate);
				if (slot < 0)
				{
					BepinexPlugin.log.LogInfo($"[EXQUESTING SAVE] RestoreAcceptedSlotsFromPendingProgress no open visible slot for quest={questCardId}");
					break;
				}

				Card restoredCard = Library.TryCreateCard(questCardId, false);
				if (restoredCard == null)
				{
					BepinexPlugin.log.LogWarning($"[EXQUESTING SAVE] RestoreAcceptedSlotsFromPendingProgress failed to create card for quest={questCardId}");
					continue;
				}

				restoredCard.GameRun = GameRun;
				acceptedSlotCards[slot] = restoredCard;
				slotsToPopulate.Remove(slot);
				assignedQuestIds.Add(questCardId);
				BepinexPlugin.log.LogInfo($"[EXQUESTING SAVE] RestoreAcceptedSlotsFromPendingProgress restored quest={questCardId} to slot={slot}");
			}

			BepinexPlugin.log.LogInfo($"[EXQUESTING SAVE] RestoreAcceptedSlotsFromPendingProgress result acceptedSlots={acceptedSlotCards.Count}, remainingOpenSlots={string.Join(",", slotsToPopulate)}");
		}

		private bool ShouldRecoverRolledCardsAfterSync()
		{
			if (GameRun == null)
			{
				BepinexPlugin.log.LogInfo("[EXQUESTING SAVE] ShouldRecoverRolledCardsAfterSync=false reason=GameRunNull");
				return false;
			}

			if (NeedsDeferredOpenSlotReroll)
			{
				BepinexPlugin.log.LogInfo("[EXQUESTING SAVE] ShouldRecoverRolledCardsAfterSync=true reason=DeferredOpenSlotReroll");
				return true;
			}

			if (RolledQuestCards == null || RolledQuestCards.Count == 0)
			{
				BepinexPlugin.log.LogInfo("[EXQUESTING SAVE] ShouldRecoverRolledCardsAfterSync=true reason=NoRolledCards");
				return true;
			}

			foreach (var kvp in PendingQuestProgress)
			{
				string questCardId = kvp.Key;
				if (string.IsNullOrEmpty(questCardId))
				{
					continue;
				}

				bool foundAcceptedSlot = false;
				foreach (var slotCard in RolledQuestCards)
				{
					int slot = slotCard.Key;
					Card card = slotCard.Value;
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
					BepinexPlugin.log.LogInfo($"[EXQUESTING SAVE] ShouldRecoverRolledCardsAfterSync=true reason=MissingAcceptedQuest quest={questCardId}");
					return true;
				}
			}

			BepinexPlugin.log.LogInfo("[EXQUESTING SAVE] ShouldRecoverRolledCardsAfterSync=false reason=AllAcceptedFound");
			return false;
		}


		public void RollQuestCards(bool preserveAcceptedSlots)
		{
			BepinexPlugin.log.LogInfo($"[EXQUESTING SAVE] RollQuestCards begin preserveAcceptedSlots={preserveAcceptedSlots}, pending=[{FormatPendingQuestProgress()}], completed=[{FormatCompletedQuestCards()}], previousRolled=[{FormatRolledQuestSlots()}]");
			Dictionary<int, Card> previousCards = new Dictionary<int, Card>(RolledQuestCards);
			HashSet<int> previousSoldOutSlots = new HashSet<int>(SoldOutQuestSlots);
			List<int> slotsToPopulate = new List<int>();
			Dictionary<int, Card> acceptedSlotCards = new Dictionary<int, Card>();

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

			HashSet<string> excludedQuestCardIds = new HashSet<string>(StringComparer.Ordinal);
			foreach (Card acceptedCard in acceptedSlotCards.Values)
			{
				if (acceptedCard != null && !string.IsNullOrEmpty(acceptedCard.Id))
				{
					excludedQuestCardIds.Add(acceptedCard.Id);
				}
			}

			// Also exclude any quests that were just cleared from rolled slots during this runtime
			if (RecentlyClearedRolledQuestIds != null && RecentlyClearedRolledQuestIds.Count > 0)
			{
				foreach (var id in RecentlyClearedRolledQuestIds)
				{
					if (!string.IsNullOrEmpty(id))
						excludedQuestCardIds.Add(id);
				}
				// clear after applying exclusion so it's only a one-roll protection
				RecentlyClearedRolledQuestIds.Clear();
			}

			BepinexPlugin.log.LogInfo($"[EXQUESTING SAVE] RollQuestCards acceptedSlots={acceptedSlotCards.Count}, openVisibleSlots={slotsToPopulate.Count}, excludedQuestIds=[{string.Join(",", excludedQuestCardIds)}]");

			List<Card> rolledForOpenSlots = RollQuestCardsForSlots(slotsToPopulate.Count, excludedQuestCardIds);
			for (int i = 0; i < slotsToPopulate.Count; i++)
			{
				int slot = slotsToPopulate[i];
				Card rolledCard = i < rolledForOpenSlots.Count ? rolledForOpenSlots[i] : null;
				Card finalCard = rolledCard ?? Library.CreateCard<cardmimaexa>();
				finalCard.GameRun = GameRun;
				RolledQuestCards[slot] = finalCard;
				BepinexPlugin.log.LogInfo($"[EXQUESTING SAVE] RollQuestCards assigned slot={slot} card={finalCard?.Id ?? "<null>"}");
			}

			PreRollQuestRequirementsForRolledCards(useGameRunRng: true);
			CleanupStaleQuestRequirements();

			BepinexPlugin.log.LogInfo($"[EXQUESTING UI] Rolled quest cards for station. entries={RolledQuestCards.Count}, slots=[{FormatRolledQuestSlots()}]");
		}

		public Card GetRolledQuestCard(int slotIndex)
		{
			EnsureRolledQuestCards();
			if (RolledQuestCards.TryGetValue(slotIndex, out Card card) && card != null)
			{
				return card;
			}

			return null;
		}

		public void MarkQuestSlotSoldOut(int slotIndex)
		{
			if (slotIndex >= 0)
			{
				SoldOutQuestSlots.Add(slotIndex);
			}
		}

		public bool IsQuestSlotSoldOut(int slotIndex)
		{
			return SoldOutQuestSlots.Contains(slotIndex);
		}

		public bool IsQuestCardSoldOut(string questCardId)
		{
			if (string.IsNullOrEmpty(questCardId))
			{
				return false;
			}

			if (RolledQuestCards == null || RolledQuestCards.Count == 0)
			{
				return false;
			}

			foreach (var kvp in RolledQuestCards)
			{
				Card card = kvp.Value;
				if (card != null
					&& string.Equals(card.Id, questCardId, StringComparison.Ordinal)
					&& IsQuestSlotSoldOut(kvp.Key))
				{
					return true;
				}
			}

			return false;
		}

		public bool IsQuestSlotAccepted(int slotIndex)
		{
			if (IsQuestSlotSoldOut(slotIndex))
			{
				return false;
			}

			Card card = GetRolledQuestCard(slotIndex);
			if (card == null || string.IsNullOrEmpty(card.Id))
			{
				return false;
			}

			return PendingQuestProgress.ContainsKey(card.Id);
		}

		public bool IsQuestCardCompleted(string questCardId)
		{
			return !string.IsNullOrEmpty(questCardId) && CompletedQuestCards.Contains(questCardId);
		}

		public void MarkQuestCompleted(string questCardId)
		{
			if (string.IsNullOrEmpty(questCardId))
			{
				return;
			}

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
			{
				FreshlyCompletedQuestCards.Remove(questCardId);
			}
		}

		public void FlushCompletedQuestStateAfterFullSave()
		{
			if (CompletedQuestCards == null || CompletedQuestCards.Count == 0)
			{
				return;
			}

			HashSet<string> completedSnapshot = new HashSet<string>(
				CompletedQuestCards.Where(id => !string.IsNullOrEmpty(id)),
				StringComparer.Ordinal);

			if (completedSnapshot.Count == 0)
			{
				CompletedQuestCards.Clear();
				return;
			}

			if (RolledQuestCards != null && RolledQuestCards.Count > 0 && SoldOutQuestSlots != null && SoldOutQuestSlots.Count > 0)
			{
				foreach (var kvp in RolledQuestCards)
				{
					int slot = kvp.Key;
					Card card = kvp.Value;
					if (card != null && !string.IsNullOrEmpty(card.Id) && completedSnapshot.Contains(card.Id))
					{
						SoldOutQuestSlots.Remove(slot);
					}
				}
			}

			CompletedQuestCards.Clear();
			BepinexPlugin.log.LogInfo($"[EXQUESTING SAVE] FlushCompletedQuestStateAfterFullSave cleared completed=[{string.Join(", ", completedSnapshot)}]");
		}

		public void FinalizeQuestByCardId(string questCardId)
		{
			if (string.IsNullOrEmpty(questCardId))
			{
				return;
			}

			CompletedQuestCards.Add(questCardId);

			if (RolledQuestCards == null || RolledQuestCards.Count == 0)
			{
				EnsureRolledQuestCards();
			}

			int acceptedMatch = -1;
			foreach (var kvp in RolledQuestCards)
			{
				int slot = kvp.Key;
				Card card = kvp.Value;
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

			foreach (var kvp in RolledQuestCards)
			{
				int slot = kvp.Key;
				Card card = kvp.Value;
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
			{
				return;
			}

			List<string> completed = new List<string>();
			foreach (var kvp in PendingQuestProgress)
			{
				Card card = Library.TryCreateCard(kvp.Key, false);
				if (card == null)
				{
					continue;
				}

				int goal = card.Config.Value1 ?? -1;
				if (goal > 0 && kvp.Value >= goal)
				{
					completed.Add(kvp.Key);
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

		public List<ShopItem<Card>> BuildRolledShopCards(GameRunController run)
		{
			EnsureRolledQuestCards();
			BepinexPlugin.log.LogInfo($"[EXQUESTING SAVE] BuildRolledShopCards begin pending=[{FormatPendingQuestProgress()}], rolled=[{FormatRolledQuestSlots()}]");
			List<ShopItem<Card>> shopCards = new List<ShopItem<Card>>(10);
			for (int i = 0; i < 10; i++)
			{
				if (!RolledQuestCards.TryGetValue(i, out Card rolledCard) || rolledCard == null)
				{
					shopCards.Add(null);
					continue;
				}

				rolledCard.GameRun = run;

				ShopItem<Card> item = new ShopItem<Card>(run, rolledCard, 0, false, false)
				{
					IsSoldOut = IsQuestSlotSoldOut(i)
				};
				shopCards.Add(item);
			}

			return shopCards;
		}

		public void SyncPendingQuestProgressFromPersistence(string reason)
		{
			BepinexPlugin.log.LogInfo($"[EXQUESTING SAVE] Sync begin reason={reason}, pendingBefore=[{FormatPendingQuestProgress()}], completedBefore=[{FormatCompletedQuestCards()}], rolledBefore=[{FormatRolledQuestSlots()}], requirementsBefore={QuestRequirements?.Count ?? 0}, modifiersBefore={PendingQuestModifiers?.Count ?? 0}");
			bool allowRecoveryRoll = string.Equals(reason, "OnStationEntered", StringComparison.Ordinal)
				|| string.Equals(reason, "OnExhibitClicked", StringComparison.Ordinal);
			Dictionary<string, int> previousPendingProgress = new Dictionary<string, int>(PendingQuestProgress, StringComparer.Ordinal);
			Dictionary<string, string> previousQuestRequirements = new Dictionary<string, string>(QuestRequirements, StringComparer.Ordinal);
			HashSet<string> previousCompletedQuestCards = new HashSet<string>(CompletedQuestCards, StringComparer.Ordinal);
			Dictionary<string, int> previousPendingQuestModifiers = new Dictionary<string, int>(PendingQuestModifiers, StringComparer.Ordinal);
			Dictionary<string, string> rolledRequirementSnapshot = CaptureCurrentRolledRequirements();
			PendingQuestProgress.Clear();
			QuestRequirements.Clear();
			CompletedQuestCards.Clear();
			PendingQuestModifiers.Clear();
			bool useRestoreSnapshot = string.Equals(reason, "CreateExhibitWidget", StringComparison.Ordinal)
				&& ShopSaveLoader.ConsumePendingRestoreQuestHydration();

			Dictionary<string, int> runProgress = ShopModHandlers.ReadQuestProgressFromRun(GameRun);
			Dictionary<string, string> runRequirements = ShopModHandlers.ReadQuestRequirementsFromRun(GameRun);
			HashSet<string> runCompleted = ShopModHandlers.ReadCompletedQuestCardsFromRun(GameRun);
			Dictionary<string, int> runModifiers = ShopModHandlers.ReadQuestModifiersFromRun(GameRun);
			Dictionary<string, int> liteModifiers = ShopModHandlers.ReadQuestModifiersFromLiteShop();
			// also read rolled/sold snapshot presence so we can restore rolled slots even when no progress/requirements/completed exist
			Dictionary<int, string> runRolled = ShopModHandlers.ReadRolledQuestCardsFromRun(GameRun);
			Dictionary<int, string> liteRolled = ShopModHandlers.ReadRolledQuestCardsFromLiteShop();
			HashSet<int> runSold = ShopModHandlers.ReadSoldQuestSlotsFromRun(GameRun);
			HashSet<int> liteSold = ShopModHandlers.ReadSoldQuestSlotsFromLiteShop();
			BepinexPlugin.log.LogInfo($"[EXQUESTING SAVE] Sync runFlags reason={reason} runProgress=[{string.Join(", ", runProgress.Select(kvp => $"{kvp.Key}:{kvp.Value}"))}] runRequirements={runRequirements.Count} runCompleted={runCompleted.Count} runModifiers={runModifiers.Count}");

			Dictionary<string, int> sourceProgress = runProgress;
			Dictionary<string, string> sourceRequirements = runRequirements;
			HashSet<string> sourceCompleted = runCompleted;
			string sourceName = "RunFlags";
			BepinexPlugin.log.LogInfo($"[EXQUESTING SAVE] Sync candidates reason={reason} useRestoreSnapshot={useRestoreSnapshot} runProgress={runProgress.Count} runRequirements={runRequirements.Count} runCompleted={runCompleted.Count}");
			bool allowLiteFallback = !string.Equals(reason, "OnStationEntered", StringComparison.Ordinal);

			if (useRestoreSnapshot)
			{
				Dictionary<string, int> liteProgress = ShopModHandlers.ReadQuestProgressFromLiteShop();
				Dictionary<string, string> liteRequirements = ShopModHandlers.ReadQuestRequirementsFromLiteShop();
				HashSet<string> liteCompleted = ShopModHandlers.ReadCompletedQuestCardsFromLiteShop();
				BepinexPlugin.log.LogInfo($"[EXQUESTING SAVE] Sync restore snapshot reason={reason} liteProgress=[{string.Join(", ", liteProgress.Select(kvp => $"{kvp.Key}:{kvp.Value}"))}] liteRequirements={liteRequirements.Count} liteCompleted={liteCompleted.Count} runProgress={runProgress.Count} runRequirements={runRequirements.Count} runCompleted={runCompleted.Count}");

				bool runHasQuestState = runProgress.Count > 0 || runRequirements.Count > 0 || runCompleted.Count > 0;
				if (runHasQuestState)
				{
					sourceProgress = runProgress;
					sourceRequirements = runRequirements;
					sourceCompleted = runCompleted;
					sourceName = "RunFlagsRestoreSnapshot";
				}
				else
				{
					sourceProgress = liteProgress;
					sourceRequirements = liteRequirements;
					sourceCompleted = liteCompleted;
					sourceName = "LiteShopRestoreFallback";
				}
			}

			if (!useRestoreSnapshot && allowLiteFallback && sourceProgress.Count == 0 && sourceCompleted.Count == 0)
			{
				Dictionary<string, int> liteProgress = ShopModHandlers.ReadQuestProgressFromLiteShop();
				Dictionary<string, string> liteRequirements = ShopModHandlers.ReadQuestRequirementsFromLiteShop();
				HashSet<string> liteCompleted = ShopModHandlers.ReadCompletedQuestCardsFromLiteShop();
				BepinexPlugin.log.LogInfo($"[EXQUESTING SAVE] Sync lite candidates reason={reason} liteProgress={liteProgress.Count} liteRequirements={liteRequirements.Count} liteCompleted={liteCompleted.Count} runProgress={runProgress.Count} runRequirements={runRequirements.Count} runCompleted={runCompleted.Count}");
				if (liteProgress.Count > 0)
				{
					sourceProgress = liteProgress;
					sourceRequirements = liteRequirements;
					sourceCompleted = liteCompleted;
					sourceName = runRequirements.Count > 0 ? "LiteShopProgressOverRunRequirements" : "LiteShop";
				}
				else if (liteCompleted.Count > 0)
				{
					sourceProgress = liteProgress;
					sourceRequirements = liteRequirements;
					sourceCompleted = liteCompleted;
					sourceName = "LiteShopCompletedOnly";
				}
				else if (sourceRequirements.Count == 0 && liteRequirements.Count > 0)
				{
					sourceProgress = liteProgress;
					sourceRequirements = liteRequirements;
					sourceCompleted = liteCompleted;
					sourceName = "LiteShopRequirementsOnly";
				}
			}

			if (sourceProgress.Count > 0 || sourceRequirements.Count > 0 || sourceCompleted.Count > 0 || runModifiers.Count > 0 || liteModifiers.Count > 0 || runRolled.Count > 0 || liteRolled.Count > 0)
			{
				BepinexPlugin.log.LogInfo($"[EXQUESTING SAVE] Sync source decision reason={reason} source={sourceName} selectedProgress={sourceProgress.Count} selectedRequirements={sourceRequirements.Count} selectedCompleted={sourceCompleted.Count}");
				foreach (var kvp in sourceProgress)
				{
					PendingQuestProgress[kvp.Key] = kvp.Value;
				}

				foreach (var kvp in sourceRequirements)
				{
					QuestRequirements[kvp.Key] = kvp.Value;
				}

				foreach (string questCardId in sourceCompleted)
				{
					if (!string.IsNullOrEmpty(questCardId))
					{
						CompletedQuestCards.Add(questCardId);
					}
				}

				// prefer run-sourced modifiers, then fall back / merge lite-shop modifiers
				foreach (var kvp in runModifiers)
				{
					if (!string.IsNullOrEmpty(kvp.Key))
					{
						PendingQuestModifiers[kvp.Key] = kvp.Value;
					}
				}

				foreach (var kvp in liteModifiers)
				{
					if (!string.IsNullOrEmpty(kvp.Key) && !PendingQuestModifiers.ContainsKey(kvp.Key))
					{
						PendingQuestModifiers[kvp.Key] = kvp.Value;
					}
				}

				// Restore rolled quest slots and sold slots from chosen persistence source (run flags or lite shop)
				try
				{
					Dictionary<int, string> sourceRolled = string.Equals(sourceName, "RunFlags", StringComparison.Ordinal) || sourceName.StartsWith("RunFlags", StringComparison.Ordinal)
						? ShopModHandlers.ReadRolledQuestCardsFromRun(GameRun)
						: ShopModHandlers.ReadRolledQuestCardsFromLiteShop();
					HashSet<int> sourceSold = string.Equals(sourceName, "RunFlags", StringComparison.Ordinal) || sourceName.StartsWith("RunFlags", StringComparison.Ordinal)
						? ShopModHandlers.ReadSoldQuestSlotsFromRun(GameRun)
						: ShopModHandlers.ReadSoldQuestSlotsFromLiteShop();

					RolledQuestCards.Clear();
					SoldOutQuestSlots.Clear();
					foreach (var kvp in sourceRolled)
					{
						if (kvp.Value == null)
							continue;
						Card card = toolbox.createcardwithid(kvp.Value);
						if (card != null)
						{
							card.GameRun = GameRun ?? GameMaster.Instance?.CurrentGameRun;
							RolledQuestCards[kvp.Key] = card;
						}
						else
						{
							BepinexPlugin.log.LogInfo($"[EXQUESTING SAVE] Sync could not create rolled card id={kvp.Value} for slot={kvp.Key}");
						}
					}

					if (sourceSold != null)
					{
						foreach (int slot in sourceSold)
						{
							SoldOutQuestSlots.Add(slot);
						}
					}
				}
				catch (Exception ex)
				{
					BepinexPlugin.log.LogInfo($"[EXQUESTING SAVE] Sync rolled restore failed: {ex.Message}");
				}

				bool preserveRuntimePendingOnStationEntered =
					string.Equals(reason, "OnStationEntered", StringComparison.Ordinal)
					&& sourceProgress.Count == 0
					&& sourceCompleted.Count == 0
					&& previousPendingProgress.Count > 0;

				if (preserveRuntimePendingOnStationEntered)
				{
					foreach (var kvp in previousPendingProgress)
					{
						PendingQuestProgress[kvp.Key] = kvp.Value;
					}

					foreach (var kvp in previousQuestRequirements)
					{
						if (!QuestRequirements.ContainsKey(kvp.Key))
						{
							QuestRequirements[kvp.Key] = kvp.Value;
						}
					}

					BepinexPlugin.log.LogInfo($"[EXQUESTING SAVE] Sync OnStationEntered preserved runtime pending because source had requirements-only fallback pending=[{FormatPendingQuestProgress()}]");
				}

				if (string.Equals(reason, "OnStationEntered", StringComparison.Ordinal) && previousCompletedQuestCards.Count > 0)
				{
					foreach (string questCardId in previousCompletedQuestCards)
					{
						if (!string.IsNullOrEmpty(questCardId))
						{
							CompletedQuestCards.Add(questCardId);
						}
					}
					BepinexPlugin.log.LogInfo($"[EXQUESTING SAVE] Sync OnStationEntered preserved runtime completed cards=[{string.Join(", ", previousCompletedQuestCards)}]");
				}

				if (string.Equals(reason, "OnStationEntered", StringComparison.Ordinal) && PendingQuestModifiers.Count == 0 && previousPendingQuestModifiers.Count > 0)
				{
					foreach (var kvp in previousPendingQuestModifiers)
					{
						PendingQuestModifiers[kvp.Key] = kvp.Value;
					}
					BepinexPlugin.log.LogInfo($"[EXQUESTING SAVE] Sync OnStationEntered preserved runtime modifiers because persisted modifiers were empty count={PendingQuestModifiers.Count}");
				}

				if (CompletedQuestCards.Count > 0)
				{
					foreach (string completedId in CompletedQuestCards)
					{
						PendingQuestProgress.Remove(completedId);
						ClearQuestRequirement(completedId);
					}
				}

				RestoreCurrentRolledRequirements(rolledRequirementSnapshot);

				if (allowRecoveryRoll && ShouldRecoverRolledCardsAfterSync())
				{
					BepinexPlugin.log.LogInfo($"[EXQUESTING SAVE] Sync triggering RollQuestCards(preserveAcceptedSlots=true) reason={reason}");
					RollQuestCards(true);
				}

				CleanupStaleQuestRequirements();

				BepinexPlugin.log.LogInfo($"[EXQUESTING SAVE] Sync source={sourceName} reason={reason} entries={PendingQuestProgress.Count}, completed={CompletedQuestCards.Count}, pending=[{FormatPendingQuestProgress()}], completedCards=[{FormatCompletedQuestCards()}], rolled=[{FormatRolledQuestSlots()}], requirements={QuestRequirements.Count}, modifiers={PendingQuestModifiers.Count}");
				bool syncLiteShop = !string.Equals(sourceName, "RunFlags", StringComparison.Ordinal)
					|| PendingQuestProgress.Count > 0
					|| CompletedQuestCards.Count > 0
					|| PendingQuestModifiers.Count > 0;
				ShopModHandlers.PersistQuestProgress(GameRun, PendingQuestProgress, syncToLiteShop: syncLiteShop, saveToDisk: false, questRequirements: QuestRequirements, completedQuestCards: CompletedQuestCards, writeToRunFlags: false, questModifiers: PendingQuestModifiers);
				return;
			}

			RestoreCurrentRolledRequirements(rolledRequirementSnapshot);

			if (previousPendingProgress.Count > 0 || previousQuestRequirements.Count > 0 || previousCompletedQuestCards.Count > 0 || previousPendingQuestModifiers.Count > 0)
			{
				PendingQuestProgress.Clear();
				QuestRequirements.Clear();
				CompletedQuestCards.Clear();
				PendingQuestModifiers.Clear();
				foreach (var kvp in previousPendingProgress)
				{
					PendingQuestProgress[kvp.Key] = kvp.Value;
				}

				foreach (var kvp in previousQuestRequirements)
				{
					QuestRequirements[kvp.Key] = kvp.Value;
				}

				foreach (string questCardId in previousCompletedQuestCards)
				{
					if (!string.IsNullOrEmpty(questCardId))
					{
						CompletedQuestCards.Add(questCardId);
					}
				}

				foreach (var kvp in previousPendingQuestModifiers)
				{
					PendingQuestModifiers[kvp.Key] = kvp.Value;
				}

				if (CompletedQuestCards.Count > 0)
				{
					foreach (string completedId in CompletedQuestCards)
					{
						PendingQuestProgress.Remove(completedId);
						ClearQuestRequirement(completedId);
					}
				}

				RestoreCurrentRolledRequirements(rolledRequirementSnapshot);
				CleanupStaleQuestRequirements();
				BepinexPlugin.log.LogInfo($"[EXQUESTING SAVE] Sync source=EmptyPreservePrevious reason={reason} preservedPending=[{FormatPendingQuestProgress()}], preservedCompleted=[{FormatCompletedQuestCards()}], requirements={QuestRequirements.Count}, modifiers={PendingQuestModifiers.Count}");
				return;
			}

			CleanupStaleQuestRequirements();

			BepinexPlugin.log.LogInfo($"[EXQUESTING SAVE] Sync source=Empty reason={reason} entries={PendingQuestProgress.Count}, completed={CompletedQuestCards.Count}, pending=[{FormatPendingQuestProgress()}], completedCards=[{FormatCompletedQuestCards()}], rolled=[{FormatRolledQuestSlots()}], requirements={QuestRequirements.Count}, modifiers={PendingQuestModifiers.Count}");
			ShopModHandlers.PersistQuestProgress(GameRun, PendingQuestProgress, syncToLiteShop: false, saveToDisk: false, questRequirements: QuestRequirements, completedQuestCards: CompletedQuestCards, writeToRunFlags: false, questModifiers: PendingQuestModifiers);
		}

		protected override string GetBaseDescription()
		{
			return base.GetBaseDescription() + QuestDescriptions() + BuffDescriptions();
		}

		private string BuffDescriptions()
		{
			var buffs = PendingQuestModifiers;
			BepinexPlugin.log.LogInfo($"[EXQUESTING UI] showingshowing QuestModifiers content foreach buff in buffs=[{(buffs != null ? string.Join(", ", buffs.Select(kvp => $"{kvp.Key}:{kvp.Value}")) : "null")}]");
			if (buffs == null || buffs.Count == 0)
			{
				return string.Empty;
			}
			StringBuilder sb = new StringBuilder();
			foreach (var buff in buffs)
			{
				Card buffCard = Library.TryCreateCard(buff.Key, false);
				if (buffCard == null)
					continue;
				sb.Append("\n").Append(buff.Value + "× ").Append(ResolveQuestExtraDescription(buffCard, 2));
			}
			return StringDecorator.Decorate(sb.ToString());
		}

		private string QuestDescriptions()
		{
			StringBuilder sb = new StringBuilder();
			foreach (var kvp in PendingQuestProgress)
			{
				int progress = kvp.Value;
				var card = Library.TryCreateCard(kvp.Key, false);
				if (card == null)
					continue;
				card.GameRun = GameMaster.Instance?.CurrentGameRun;
				int goal = card.Config.Value1 ?? -1;
				if (goal == -1)
					continue;
				string prog = " (|c:" + progress + "| / |c:" + goal + "|)";

				if (card.Id == nameof(cardquest23))
				{
					if (progress > 0)
						prog = " (|f:" + progress + "|)";
					if (progress < 0)
						prog = " (|u:" + progress + "|)";
					if (progress == 0)
						prog = " (|" + progress + "|)"; ;
				}

				prog = StringDecorator.Decorate(prog);
				string extraDescription = ResolveQuestExtraDescription(card, 1); // desc 1 for condition
				if (string.IsNullOrEmpty(extraDescription))
					continue;
				sb.Append("\n").Append(extraDescription).Append(prog); // effect + progress
			}
			return StringDecorator.Decorate(sb.ToString());
		}

		private static string ResolveQuestExtraDescription(Card card, int desc)
		{
			if (card == null || string.IsNullOrEmpty(card.Id))
				return string.Empty;

			try
			{
				string field = "ExtraDescription" + desc;
				string rawText = TypeFactory<Card>.LocalizeProperty(card.Id, field, true, true);
				if (string.IsNullOrEmpty(rawText))
					return string.Empty;
				return rawText.RuntimeFormat(card.FormatWrapper);
			}
			catch (Exception ex)
			{
				BepinexPlugin.log.LogWarning($"[EXQUESTING UI] Failed to resolve quest description for card '{card.Id}' (ExtraDescription{desc}): {ex.Message}");
				return string.Empty;
			}
		}

		protected override void OnAdded(PlayerUnit player)
		{
			if (ShopSaveLoader.GetGameRunRestoreInProgress())
			{
				DeferredRestoreHydration.Add(this);
				BepinexPlugin.log.LogInfo("[EXQUESTING SAVE] OnAdded deferred hydration until GameRunController.Restore postfix.");
			}
			else
			{
				SyncPendingQuestProgressFromPersistence("OnAdded");
				BepinexPlugin.log.LogInfo($"[EXQUESTING SAVE] OnAdded registered state only pending=[{FormatPendingQuestProgress()}], requirements={QuestRequirements.Count}");
			}

			HandleGameRunEvent(GameRun.StationEntered, OnStationEntered, GameEventPriority.Lowest);
		}

		public static void ProcessDeferredRestoreHydration()
		{
			if (DeferredRestoreHydration.Count == 0)
			{
				return;
			}

			exquesting[] pending = DeferredRestoreHydration.ToArray();
			DeferredRestoreHydration.Clear();

			for (int i = 0; i < pending.Length; i++)
			{
				exquesting exhibit = pending[i];
				if (exhibit == null)
				{
					continue;
				}

				exhibit.SyncPendingQuestProgressFromPersistence("OnAddedDeferredAfterRestoreHydrateOnly");
				BepinexPlugin.log.LogInfo($"[EXQUESTING SAVE] OnAddedDeferredAfterRestoreHydrateOnly complete pending=[{exhibit.FormatPendingQuestProgress()}], requirements={exhibit.QuestRequirements.Count}");
			}
		}

		private void OnStationEntered(StationEventArgs args)
		{
			BepinexPlugin.log.LogInfo("[EXQUESTING SAVE] OnStationEntered begin: hydrate quest state first, then roll with accepted-slot lock.");
			SyncPendingQuestProgressFromPersistence("OnStationEntered");
			UnlockCompletedQuestSlots();
			BepinexPlugin.log.LogInfo($"[EXQUESTING SAVE] OnStationEntered post-sync pending=[{FormatPendingQuestProgress()}], completed=[{FormatCompletedQuestCards()}], requirements={QuestRequirements.Count}");
			ShopModHandlers.QueueResolveCompletedQuestEffectsOnStationEnter(GameRun, this);
			if (CompletedQuestCards.Count > 0)
			{
				BepinexPlugin.log.LogInfo($"[EXQUESTING SAVE] OnStationEntered preserving completed cards for deferred resolution=[{FormatCompletedQuestCards()}]");
			}
			bool preserveAccepted = PendingQuestProgress.Count > 0;
			RollQuestCards(preserveAccepted);
			BepinexPlugin.log.LogInfo($"[EXQUESTING SAVE] OnStationEntered rolled preserveAccepted={preserveAccepted}, pending=[{FormatPendingQuestProgress()}], rolled=[{FormatRolledQuestSlots()}]");
			RefreshRolledQuestRequirementsForSave();
			ShopModHandlers.PersistQuestProgress(GameRun, PendingQuestProgress, syncToLiteShop: true, saveToDisk: true, questRequirements: QuestRequirements, completedQuestCards: CompletedQuestCards, questModifiers: PendingQuestModifiers);
		}
	}
}
