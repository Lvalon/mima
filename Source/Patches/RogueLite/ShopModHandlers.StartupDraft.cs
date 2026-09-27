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
	private sealed class StartupDraftWorkItem
	{
		public GameRunController GameRun;
		public IEnumerator Routine;
		public Action OnCompleted;
	}

	private static readonly Queue<StartupDraftWorkItem> StartupDraftQueue = new();

	private static bool StartupDraftQueueRunning;

	private static IEnumerator DraftCardFromPrev(int num, GameRunController gameRun)
	{
		if (gameRun == null || num <= 0)
			yield break;

		bool completed = false;
		EnqueueStartupDraft(gameRun, CoDraftCardFromPrev(num, gameRun), () => completed = true);
		while (!completed)
			yield return null;
	}

	private static IEnumerator CoDraftCardFromPrev(int num, GameRunController gameRun)
	{
		var historyLast = GameMaster.GetGameRunHistory()?.LastOrDefault();
		if (historyLast == null)
			yield break;
		var prevIds = historyLast?.Cards?.Select(rec => rec.Id).ToArray() ?? [];
		var filteredCards = prevIds
			.Select(id => toolbox.createcardwithid(id))
			.Where(card => card != null && card.CardType == CardType.Ability)
			.ToArray();
		if (filteredCards.Length == 0)
			yield break;
		//GameRun.UpgradeNewDeckCardOnFlags(array1);
		SelectCardInteraction interaction = new(0, Math.Min(filteredCards.Length, num), filteredCards)
		{
			Source = null,
			CanCancel = false,
			Description = GetLocalizedText($"{LocalisationKeys.ShopPrefix}{LocalisationKeys.InitPrefix}card")
		};
		yield return gameRun.InteractionViewer.View(interaction);
		if (interaction.SelectedCards == null || interaction.SelectedCards.Count == 0)
			yield break;
		gameRun.AddDeckCards(interaction.SelectedCards, true, null);
	}

	private static IEnumerator DraftExhibitFromPrev(int num, GameRunController gameRun)
	{
		if (gameRun == null || num <= 0)
			yield break;

		bool completed = false;
		EnqueueStartupDraft(gameRun, CoDraftExhibitFromPrev(num, gameRun), () => completed = true);
		while (!completed)
			yield return null;
	}

	private static IEnumerator CoDraftExhibitFromPrev(int num, GameRunController gameRun)
	{
		yield return null;
		var historyLast = GameMaster.GetGameRunHistory()?.LastOrDefault()?.Exhibits ?? [];
		Stage stage = gameRun.CurrentStage;
		for (int i = 0; i < num; i++)
		{
			var exhibit = gameRun.RollNormalExhibit(
				gameRun.ExhibitRng,
				new ExhibitWeightTable(new RarityWeightTable(0.5f, 0.33f, 0.17f, 0f), AppearanceWeightTable.NotInShop),
				new Func<Exhibit>(stage.GetSentinelExhibit),
				c => c.Rarity != Rarity.Mythic && c.Rarity != Rarity.Shining && !historyLast.Contains(c.Id));
			if (exhibit != null)
				GameMaster.DebugGainExhibit(exhibit);
		}
		yield break;
	}

	private static void EnqueueStartupDraft(GameRunController gameRun, IEnumerator routine, Action onCompleted)
	{
		if (routine == null)
		{
			onCompleted?.Invoke();
			return;
		}

		StartupDraftQueue.Enqueue(new StartupDraftWorkItem
		{
			GameRun = gameRun,
			Routine = routine,
			OnCompleted = onCompleted,
		});

		if (StartupDraftQueueRunning)
			return;

		GameMaster.Instance?.StartCoroutine(CoRunStartupDraftQueue());
	}

	private static IEnumerator CoRunStartupDraftQueue()
	{
		if (StartupDraftQueueRunning)
			yield break;

		StartupDraftQueueRunning = true;
		while (StartupDraftQueue.Count > 0)
		{
			StartupDraftWorkItem item = StartupDraftQueue.Dequeue();
			if (item == null)
				continue;

			while (HasActiveInteraction(item.GameRun))
				yield return null;

			try
			{
				yield return item.Routine;
			}
			finally
			{
				item.OnCompleted?.Invoke();
			}
		}

		StartupDraftQueueRunning = false;
	}

	private static bool HasActiveInteraction(GameRunController gameRun)
	{
		object viewer = gameRun?.InteractionViewer;
		if (viewer == null)
			return false;

		foreach (string name in new[] { "IsInteracting", "InInteraction", "HasInteraction", "HasActiveInteraction", "IsBusy", "Busy", "IsViewing" })
		{
			if (TryGetMemberValue(viewer, name, out object value) && value is bool b)
				return b;
		}

		foreach (string name in new[] { "CurrentInteraction", "Current", "Interaction", "ActiveInteraction" })
		{
			if (TryGetMemberValue(viewer, name, out object value) && value != null)
				return true;
		}

		foreach (string name in new[] { "Count", "PendingCount", "InteractionCount" })
		{
			if (TryGetMemberValue(viewer, name, out object value) && value is int count && count > 0)
				return true;
		}

		foreach (string name in new[] { "Interactions", "PendingInteractions", "Queue", "InteractionQueue" })
		{
			if (!TryGetMemberValue(viewer, name, out object value) || value == null)
				continue;

			if (value is ICollection collection && collection.Count > 0)
				return true;

			if (TryGetMemberValue(value, "Count", out object nestedCount) && nestedCount is int queueCount && queueCount > 0)
				return true;
		}

		return false;
	}

	private static bool TryGetMemberValue(object target, string memberName, out object value)
	{
		value = null;
		if (target == null || string.IsNullOrEmpty(memberName))
			return false;

		BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
		Type type = target.GetType();
		PropertyInfo property = type.GetProperty(memberName, flags);
		if (property != null && property.GetIndexParameters().Length == 0)
		{
			try
			{
				value = property.GetValue(target);
				return true;
			}
			catch
			{
				return false;
			}
		}

		FieldInfo field = type.GetField(memberName, flags);
		if (field == null)
			return false;

		try
		{
			value = field.GetValue(target);
			return true;
		}
		catch
		{
			return false;
		}
	}

	private static IEnumerator GainQuestExhibit()
	{
		yield return null;
		GameMaster.DebugGainExhibit(Library.CreateExhibit<exquesting>());
		yield break;
	}
}
