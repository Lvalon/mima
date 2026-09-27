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

// The reasons SyncPendingQuestProgressFromPersistence can be called for. Not
// every value is used by a caller today (OnExhibitClicked, CreateExhibitWidget
// aren't currently passed anywhere), but the method's branches still check for
// them, so they're kept rather than pruned as dead — that's a separate, larger
// decision than this 1:1 string-to-enum substitution.
public enum QuestSyncReason
{
	OnAdded,
	OnAddedDeferredAfterRestoreHydrateOnly,
	OnStationEntered,
	OnExhibitClicked,
	CreateExhibitWidget,
}

public sealed partial class exquesting : Exhibit
{
	private static readonly HashSet<exquesting> DeferredRestoreHydration = [];

	public void FlushCompletedQuestStateAfterFullSave()
	{
		if (CompletedQuestCards == null || CompletedQuestCards.Count == 0)
			return;

		HashSet<string> completedSnapshot = new(
			CompletedQuestCards.Where(id => !string.IsNullOrEmpty(id)),
			StringComparer.Ordinal);

		if (completedSnapshot.Count == 0)
		{
			CompletedQuestCards.Clear();
			return;
		}

		if (RolledQuestCards is { Count: > 0 } && SoldOutQuestSlots is { Count: > 0 })
		{
			foreach (var (key, value) in RolledQuestCards)
			{
				int slot = key;
				Card card = value;
				if (card != null && !string.IsNullOrEmpty(card.Id) && completedSnapshot.Contains(card.Id))
				{
					SoldOutQuestSlots.Remove(slot);
				}
			}
		}

		CompletedQuestCards.Clear();
	}

	public void SyncPendingQuestProgressFromPersistence(QuestSyncReason reason)
	{
		bool allowRecoveryRoll = reason == QuestSyncReason.OnStationEntered
			|| reason == QuestSyncReason.OnExhibitClicked;
		Dictionary<string, int> previousPendingProgress = new(PendingQuestProgress, StringComparer.Ordinal);
		Dictionary<string, string> previousQuestRequirements = new(QuestRequirements, StringComparer.Ordinal);
		HashSet<string> previousCompletedQuestCards = new(CompletedQuestCards, StringComparer.Ordinal);
		Dictionary<string, int> previousPendingQuestModifiers = new(PendingQuestModifiers, StringComparer.Ordinal);
		Dictionary<string, string> rolledRequirementSnapshot = CaptureCurrentRolledRequirements();
		PendingQuestProgress.Clear();
		QuestRequirements.Clear();
		CompletedQuestCards.Clear();
		PendingQuestModifiers.Clear();
		bool useRestoreSnapshot = reason == QuestSyncReason.CreateExhibitWidget
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

		Dictionary<string, int> sourceProgress = runProgress;
		Dictionary<string, string> sourceRequirements = runRequirements;
		HashSet<string> sourceCompleted = runCompleted;
		string sourceName = "RunFlags";
		bool allowLiteFallback = reason != QuestSyncReason.OnStationEntered;

		if (useRestoreSnapshot)
		{
			Dictionary<string, int> liteProgress = ShopModHandlers.ReadQuestProgressFromLiteShop();
			Dictionary<string, string> liteRequirements = ShopModHandlers.ReadQuestRequirementsFromLiteShop();
			HashSet<string> liteCompleted = ShopModHandlers.ReadCompletedQuestCardsFromLiteShop();

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
			foreach (var (key, value) in sourceProgress)
			{
				PendingQuestProgress[key] = value;
			}

			foreach (var (key, value) in sourceRequirements)
			{
				QuestRequirements[key] = value;
			}

			foreach (string questCardId in sourceCompleted)
			{
				if (!string.IsNullOrEmpty(questCardId))
				{
					CompletedQuestCards.Add(questCardId);
				}
			}

			// prefer run-sourced modifiers, then fall back / merge lite-shop modifiers
			foreach (var (key, value) in runModifiers)
			{
				if (!string.IsNullOrEmpty(key))
				{
					PendingQuestModifiers[key] = value;
				}
			}

			foreach (var (key, value) in liteModifiers)
			{
				if (!string.IsNullOrEmpty(key) && !PendingQuestModifiers.ContainsKey(key))
				{
					PendingQuestModifiers[key] = value;
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
				foreach (var (key, value) in sourceRolled)
				{
					if (value == null)
						continue;
					Card card = toolbox.createcardwithid(value);
					if (card != null)
					{
						card.GameRun = GameRun ?? GameMaster.Instance?.CurrentGameRun;
						RolledQuestCards[key] = card;
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
			catch (Exception)
			{
			}

			bool preserveRuntimePendingOnStationEntered =
				reason == QuestSyncReason.OnStationEntered
				&& sourceProgress.Count == 0
				&& sourceCompleted.Count == 0
				&& previousPendingProgress.Count > 0;

			if (preserveRuntimePendingOnStationEntered)
			{
				foreach (var (key, value) in previousPendingProgress)
				{
					PendingQuestProgress[key] = value;
				}

				foreach (var (key, value) in previousQuestRequirements)
				{
					if (!QuestRequirements.ContainsKey(key))
					{
						QuestRequirements[key] = value;
					}
				}

			}

			if (reason == QuestSyncReason.OnStationEntered && previousCompletedQuestCards.Count > 0)
			{
				foreach (string questCardId in previousCompletedQuestCards)
				{
					if (!string.IsNullOrEmpty(questCardId))
					{
						CompletedQuestCards.Add(questCardId);
					}
				}
			}

			if (reason == QuestSyncReason.OnStationEntered && PendingQuestModifiers.Count == 0 && previousPendingQuestModifiers.Count > 0)
			{
				foreach (var (key, value) in previousPendingQuestModifiers)
				{
					PendingQuestModifiers[key] = value;
				}
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
				RollQuestCards(true);
			}

			CleanupStaleQuestRequirements();

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
			foreach (var (key, value) in previousPendingProgress)
			{
				PendingQuestProgress[key] = value;
			}

			foreach (var (key, value) in previousQuestRequirements)
			{
				QuestRequirements[key] = value;
			}

			foreach (string questCardId in previousCompletedQuestCards)
			{
				if (!string.IsNullOrEmpty(questCardId))
				{
					CompletedQuestCards.Add(questCardId);
				}
			}

			foreach (var (key, value) in previousPendingQuestModifiers)
			{
				PendingQuestModifiers[key] = value;
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
			return;
		}

		CleanupStaleQuestRequirements();

		ShopModHandlers.PersistQuestProgress(GameRun, PendingQuestProgress, syncToLiteShop: false, saveToDisk: false, questRequirements: QuestRequirements, completedQuestCards: CompletedQuestCards, writeToRunFlags: false, questModifiers: PendingQuestModifiers);
	}

	protected override void OnAdded(PlayerUnit player)
	{
		if (ShopSaveLoader.GetGameRunRestoreInProgress())
			DeferredRestoreHydration.Add(this);
		else
			SyncPendingQuestProgressFromPersistence(QuestSyncReason.OnAdded);

		HandleGameRunEvent(GameRun.StationEntered, OnStationEntered, GameEventPriority.Lowest);
	}

	public static void ProcessDeferredRestoreHydration()
	{
		if (DeferredRestoreHydration.Count == 0)
			return;

		exquesting[] pending = [.. DeferredRestoreHydration];
		DeferredRestoreHydration.Clear();

		for (int i = 0; i < pending.Length; i++)
		{
			exquesting exhibit = pending[i];
			if (exhibit == null)
			{
				continue;
			}

			exhibit.SyncPendingQuestProgressFromPersistence(QuestSyncReason.OnAddedDeferredAfterRestoreHydrateOnly);
		}
	}

	private void OnStationEntered(StationEventArgs args)
	{
		SyncPendingQuestProgressFromPersistence(QuestSyncReason.OnStationEntered);
		UnlockCompletedQuestSlots();
		ShopModHandlers.QueueResolveCompletedQuestEffectsOnStationEnter(GameRun, this);
		bool preserveAccepted = PendingQuestProgress.Count > 0;
		RollQuestCards(preserveAccepted);
		RefreshRolledQuestRequirementsForSave();
		ShopModHandlers.PersistQuestProgress(GameRun, PendingQuestProgress, syncToLiteShop: true, saveToDisk: true, questRequirements: QuestRequirements, completedQuestCards: CompletedQuestCards, questModifiers: PendingQuestModifiers);
	}
}
