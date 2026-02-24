// using System.Collections.Generic;
// using System.Linq;
// using LBoL.Base;
// using LBoL.ConfigData;
// using LBoL.Core;
// using LBoL.Core.Battle;
// using LBoL.Core.Battle.Interactions;
// using LBoL.Core.Cards;
// using LBoL.Core.StatusEffects;
// using LBoL.Core.Units;
// using LBoLEntitySideloader.Attributes;
// using HarmonyLib;
// using LBoL.Core.Battle.BattleActions;
// using System;

// namespace lvalonmima.StatusEffects
// {
// 	public sealed class MySelectCardListenerDef : lvalonmimaStatusEffectTemplate
// 	{
// 		public override StatusEffectConfig MakeConfig()
// 		{
// 			StatusEffectConfig config = GetDefaultStatusEffectConfig();
// 			config.Type = StatusEffectType.Positive;
// 			config.HasCount = true;
// 			config.CountStackType = StackType.Add;
// 			return config;
// 		}
// 	}

// 	[EntityLogic(typeof(MySelectCardListenerDef))]
// 	public sealed class MySelectCardListener : StatusEffect
// 	{
// 		protected override void OnAdded(Unit unit)
// 		{
// 			SelectCardInteractionPatch.Instance = this;
// 			HandleOwnerEvent(Battle.CardRemoved, OnCardRemoved);
// 		}

// 		private void OnCardRemoved(CardEventArgs args)
// 		{
// 			Count++;
// 		}

// 		public void ProcessSelectedCards(IReadOnlyList<Card> selectedCards)
// 		{
// 			if (selectedCards == null || Count <= 0)
// 				return;

// 			var validCards = selectedCards
// 				.Where(c => c.Config.IsPooled &&
// 							c.CanBeDuplicated)
// 				.ToList();

// 			if (!validCards.Any())
// 				return;

// 			for (int i = 0; i < Level && Count > 0 && !Battle.BattleShouldEnd; i++)
// 			{
// 				if (i == 0)
// 				{
// 					NotifyActivating();
// 				}

// 				var cards = validCards
// 					.Take(Count)
// 					.Select(c => c.CloneBattleCard())
// 					.ToList();

// 				var action = new AddCardsToDrawZoneAction(
// 					cards,
// 					DrawZoneTarget.Random,
// 					AddCardsType.Normal);

// 				PendingReactionQueue.Enqueue(action);
// 				if (i == 0)
// 					Count -= cards.Count;
// 			}
// 		}
// 	}

// 	[HarmonyPatch(typeof(SelectCardInteraction), nameof(SelectCardInteraction.SelectedCards), MethodType.Getter)]
// 	public static class SelectCardInteractionPatch
// 	{
// 		public static MySelectCardListener Instance { get; set; }

// 		static void Postfix(SelectCardInteraction __instance, IReadOnlyList<Card> __result)
// 		{
// 			if (Instance != null && __result?.Count > 0)
// 			{
// 				Instance.ProcessSelectedCards(__result);
// 			}
// 		}
// 	}

// 	[HarmonyPatch(typeof(MiniSelectCardInteraction), "SelectedCard", MethodType.Getter)]
// 	public static class MiniSelectCardInteractionPatch
// 	{
// 		static void Postfix(MiniSelectCardInteraction __instance, Card __result)
// 		{
// 			if (SelectCardInteractionPatch.Instance != null && __result != null)
// 			{
// 				SelectCardInteractionPatch.Instance.ProcessSelectedCards(new[] { __result });
// 			}
// 		}
// 	}

// 	[HarmonyPatch(typeof(Reactor), nameof(Reactor.EnumerateReactions))]
// 	public static class ReactorReactPatch
// 	{
// 		[HarmonyPostfix]
// 		static IEnumerable<BattleAction> EnumerateReactionsPostfix(IEnumerable<BattleAction> __result)
// 		{
// 			while (PendingReactionQueue.TryDequeue(out var action))
// 			{
// 				yield return action;
// 			}

// 			foreach (var action in __result)
// 			{
// 				yield return action;
// 			}
// 		}
// 	}

// 	public static class PendingReactionQueue
// 	{
// 		private static Queue<BattleAction> queue = new Queue<BattleAction>();

// 		public static void Enqueue(BattleAction action)
// 		{
// 			queue.Enqueue(action);
// 		}

// 		public static bool TryDequeue(out BattleAction action)
// 		{
// 			return queue.TryDequeue(out action);
// 		}

// 		public static int Count => queue.Count;
// 	}
// }