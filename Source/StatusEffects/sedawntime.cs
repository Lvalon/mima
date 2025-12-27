using System.Collections.Generic;
using System.Linq;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.Randoms;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.ExtraTurn;
using LBoLEntitySideloader.Attributes;

namespace lvalonmima.StatusEffects
{
	public sealed class sedawntimeDef : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Special;
			config.Keywords = Keyword.TempMorph;
			config.IsStackable = false;
			config.Order = int.MaxValue;
			config.HasCount = true;
			config.RelativeEffects = new List<string>() { nameof(ExtraTurn) };
			return config;
		}
	}

	[EntityLogic(typeof(sedawntimeDef))]
	public sealed class sedawntime : exhl10
	{
		protected override void OnAdded(Unit unit)
		{
			if (!(unit is PlayerUnit))
			{
				BepinexPlugin.log.LogWarning(DebugName + " should not apply to non-player unit.");
				React(new RemoveStatusEffectAction(this));
				return;
			}
			base.ThisTurnActivating = false;
			HandleOwnerEvent(base.Battle.Player.TurnStarting, delegate
			{
				if (base.Battle.Player.IsExtraTurn && !base.Battle.Player.IsSuperExtraTurn && base.Battle.Player.GetStatusEffectExtend<ExtraTurnPartner>() == this)
				{
					base.ThisTurnActivating = true;
				}
			});
			HandleOwnerEvent(Owner.DamageTaking, new GameEventHandler<DamageEventArgs>(OnDamageTaking), GameEventPriority.Lowest - 1);
			HandleOwnerEvent(Owner.DamageDealing, new GameEventHandler<DamageDealingEventArgs>(OnDamageDealing), GameEventPriority.Lowest - 1);
			HandleOwnerEvent(Battle.Predraw, new GameEventHandler<CardEventArgs>(OnPredraw), GameEventPriority.Lowest - 1);
			ReactOwnerEvent(Owner.TurnStarted, new EventSequencedReactor<UnitEventArgs>(OnOwnerTurnStarted));
			ReactOwnerEvent(Battle.Player.TurnEnding, new EventSequencedReactor<GameEventArgs>(OnPlayerTurnEnding));
			HandleOwnerEvent(Battle.ManaGaining, new GameEventHandler<ManaEventArgs>(OnManaGaining), GameEventPriority.Lowest - 1);
			CardToFree(Battle.EnumerateAllCards());
			HandleOwnerEvent(Battle.CardsAddedToDiscard, new GameEventHandler<CardsEventArgs>(OnAddCard), GameEventPriority.Lowest - 1);
			HandleOwnerEvent(Battle.CardsAddedToHand, new GameEventHandler<CardsEventArgs>(OnAddCard), GameEventPriority.Lowest - 1);
			HandleOwnerEvent(Battle.CardsAddedToExile, new GameEventHandler<CardsEventArgs>(OnAddCard), GameEventPriority.Lowest - 1);
			ReactOwnerEvent(Battle.CardMoved, new EventSequencedReactor<CardMovingEventArgs>(OnCardMoved), GameEventPriority.Lowest - 1);
			HandleOwnerEvent(Battle.CardsAddedToDrawZone, new GameEventHandler<CardsAddingToDrawZoneEventArgs>(OnAddCardToDraw), GameEventPriority.Lowest - 1);
			foreach (EnemyUnit enemyUnit in base.Battle.AllAliveEnemies)
			{
				HandleOwnerEvent(enemyUnit.DamageTaking, OnDamageTaking);
			}
			base.HandleOwnerEvent<UnitEventArgs>(base.Battle.EnemySpawned, new GameEventHandler<UnitEventArgs>(this.OnEnemySpawned));
		}

		private void OnEnemySpawned(UnitEventArgs args)
		{
			HandleOwnerEvent(args.Unit.DamageTaking, OnDamageTaking, GameEventPriority.Lowest - 1);
		}

		private void OnDamageTaking(DamageEventArgs args)
		{
			if (ThisTurnActivating && Level == 0)
			{
				NotifyActivating();
				DamageInfo damageInfo = args.DamageInfo;
				damageInfo.Damage = damageInfo.Amount * 0;
				args.DamageInfo = damageInfo;
				args.AddModifier(this);
			}
		}
		private void OnDamageDealing(DamageDealingEventArgs args)
		{
			if (ThisTurnActivating && Level == 0)
			{
				NotifyActivating();
				DamageInfo damageInfo = args.DamageInfo;
				damageInfo.Damage = damageInfo.Amount * 0;
				args.DamageInfo = damageInfo;
				args.AddModifier(this);
			}
		}
		private void OnManaGaining(ManaEventArgs args)
		{
			if (ThisTurnActivating && Level == 0)
			{
				NotifyActivating();
				args.CancelBy(this);
			}
		}
		private void OnAddCardToDraw(CardsAddingToDrawZoneEventArgs args)
		{
			if (ThisTurnActivating) { NotifyActivating(); CardToFree(args.Cards); }
		}
		private void OnAddCard(CardsEventArgs args)
		{
			if (ThisTurnActivating) { NotifyActivating(); CardToFree(args.Cards); }
		}
		private IEnumerable<BattleAction> OnCardMoved(CardMovingEventArgs args)
		{
			if (ThisTurnActivating)
			{
				Card card = args.Card;
				if (card.Config.IsXCost == false) { NotifyActivating(); card.FreeCost = true; }
				yield break;
			}
		}
		private void CardToFree(IEnumerable<Card> cards)
		{
			if (ThisTurnActivating)
			{
				foreach (Card card in cards)
				{
					if (card.Config.IsXCost == false) { card.FreeCost = true; }
				}
			}
		}
		protected override void OnRemoved(Unit unit)
		{
			foreach (Card card in Battle.EnumerateAllCards())
			{
				card.FreeCost = false;
			}
		}
		private IEnumerable<BattleAction> OnOwnerTurnStarted(UnitEventArgs args)
		{
			if (ThisTurnActivating)
			{
				List<Card> hand = (from card in Battle.HandZone select card).ToList();
				if (hand.Count > 0)
				{
					foreach (Card card2 in hand)
					{
						yield return new RemoveCardAction(card2);
					}
				}
				List<Card> list = Battle.RollCardsWithoutManaLimit(new CardWeightTable(RarityWeightTable.BattleCard, OwnerWeightTable.OnlyPlayer, CardTypeWeightTable.CanBeLoot), Count, (CardConfig config) => config.Id != Id).ToList();
				if (list.NotEmpty())
				{
					foreach (Card card in list)
					{
						if (!card.IsXCost)
						{
							card.SetTurnCost(new ManaGroup());
						}
						card.IsEthereal = true;
						card.IsExile = true;
					}
					NotifyActivating();
					yield return new AddCardsToHandAction(list);
				}
			}
			yield break;
		}
		private void OnPredraw(CardEventArgs args)
		{
			if (ThisTurnActivating && Level == 0)
			{
				NotifyActivating();
				args.CancelBy(this);
			}
		}
		private IEnumerable<BattleAction> OnPlayerTurnEnding(GameEventArgs args)
		{
			if (ThisTurnActivating)
			{
				NotifyActivating();
				yield return new RemoveStatusEffectAction(this, true);
				yield break;
			}
		}
		public ManaGroup Mana = ManaGroup.Empty;
	}
}