using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.Cards.Enemy;
using LBoL.EntityLib.EnemyUnits.Normal.Ravens;
using LBoLEntitySideloader.Attributes;

namespace lvalonmima.StatusEffects
{
	public sealed class seRaven3Def : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Positive;
			return config;
		}
	}

	[EntityLogic(typeof(seRaven3Def))]
	public sealed class seRaven3 : StatusEffect
	{
		public ManaGroup Mana => ManaGroup.Anys(1);
		public override bool ForceNotShowDownText => true;
		IEnumerable<Card> TargetCard()
		{
			foreach (Card item in Battle.EnumerateAllCards()
			.Where(c => c.CardType != CardType.Status)
			.Where(c => c.BaseCost.Total < Battle.AllAliveEnemies.Count(e => e is Raven)))
			{
				yield return item;
			}
		}
		IEnumerable<Card> TargetCardOnSpawn()
		{
			foreach (Card item in Battle.EnumerateAllCards()
			.Where(c => c.CardType != CardType.Status)
			.Where(c => c.BaseCost.Total + 1 == Battle.AllAliveEnemies.Count(e => e is Raven)))
			{
				yield return item;
			}
		}
		IEnumerable<Card> TargetCardOnLeave()
		{
			foreach (Card item in Battle.EnumerateAllCards()
			.Where(c => c.CardType != CardType.Status)
			.Where(c => c.BaseCost.Total == Battle.AllAliveEnemies.Count(e => e is Raven)))
			{
				yield return item;
			}
		}
		bool IsTarget(Card card)
		{
			return card.CardType != CardType.Status && card.BaseCost.Total < Battle.AllAliveEnemies.Count(e => e is Raven);
		}
		protected override void OnAdded(Unit unit)
		{
			bool notified = false;
			foreach (Card item in TargetCard())
			{
				if (!notified)
				{
					NotifyActivating();
					notified = true;
				}
				item.AuraCost += Mana;
			}
			HandleOwnerEvent(Battle.CardsAddedToDiscard, OnAddCard);
			HandleOwnerEvent(Battle.CardsAddedToHand, OnAddCard);
			HandleOwnerEvent(Battle.CardsAddedToExile, OnAddCard);
			HandleOwnerEvent(Battle.CardsAddedToDrawZone, OnAddCardToDraw);
			HandleOwnerEvent(Battle.CardTransformed, OnCardTransformed);
			HandleOwnerEvent(Battle.EnemySpawned, OnEnemySpawned);
			HandleOwnerEvent(Battle.EnemyDied, OnEnemyDied);
			HandleOwnerEvent(Battle.EnemyEscaped, OnEnemyEscaped);
		}

		private void OnEnemyEscaped(UnitEventArgs args)
		{
			if (args.Unit is Raven)
			{
				foreach (Card item in TargetCardOnLeave())
				{
					item.AuraCost -= Mana;
				}
			}
		}

		private void OnEnemyDied(DieEventArgs args)
		{
			if (args.Unit is Raven)
			{
				foreach (Card item in TargetCardOnLeave())
				{
					item.AuraCost -= Mana;
				}
			}
		}

		private void OnEnemySpawned(UnitEventArgs args)
		{
			if (args.Unit is Raven)
			{
				foreach (Card item in TargetCardOnSpawn())
				{
					item.AuraCost += Mana;
				}
			}
		}

		private void OnCardTransformed(CardTransformEventArgs args)
		{
			Modify(new Card[] { args.DestinationCard });
		}

		private void OnAddCardToDraw(CardsAddingToDrawZoneEventArgs args)
		{
			Modify(args.Cards);
		}
		private void OnAddCard(CardsEventArgs args)
		{
			Modify(args.Cards);
		}
		void Modify(Card[] cards)
		{
			bool notified = false;
			for (int i = 0; i < cards.Length; i++)
			{
				if (IsTarget(cards[i]))
				{
					if (!notified)
					{
						NotifyActivating();
						notified = true;
					}
					cards[i].AuraCost += Mana;
				}
			}
		}
		protected override void OnRemoved(Unit unit)
		{
			foreach (Card item in TargetCard())
			{
				item.AuraCost -= Mana;
			}
		}
	}
}