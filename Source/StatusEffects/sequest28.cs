using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoLEntitySideloader.Attributes;

namespace lvalonmima.StatusEffects
{
	public sealed class sequest28Def : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Special;
			return config;
		}
	}

	[EntityLogic(typeof(sequest28Def))]
	public sealed class sequest28 : StatusEffect
	{
		public override bool ForceNotShowDownText => true;
		protected override void OnAdded(Unit unit)
		{
			HandleOwnerEvent(Battle.CardsAddedToDiscard, OnAddCard, GameEventPriority.ConfigDefault + 100);
			HandleOwnerEvent(Battle.CardsAddedToHand, OnAddCard, GameEventPriority.ConfigDefault + 100);
			HandleOwnerEvent(Battle.CardsAddedToExile, OnAddCard, GameEventPriority.ConfigDefault + 100);
			HandleOwnerEvent(Battle.CardsAddedToDrawZone, OnCardsAddedToDrawZone, GameEventPriority.ConfigDefault + 100);
		}

		private void OnCardsAddedToDrawZone(CardsAddingToDrawZoneEventArgs args)
		{
			Upgrade(args.Cards.Where(c => c.CardType != CardType.Misfortune && c.CardType != CardType.Status && Battle.GameRun.GetDeckCardByInstanceId(c.InstanceId) == null));
		}

		private void OnAddCard(CardsEventArgs args)
		{
			Upgrade(args.Cards.Where(c => c.CardType != CardType.Misfortune && c.CardType != CardType.Status && Battle.GameRun.GetDeckCardByInstanceId(c.InstanceId) == null));
		}

		public void Upgrade(IEnumerable<Card> cards)
		{
			bool go = false;
			bool went = false;
			foreach (Card card in cards)
			{
				if (card.Cost.Amount > 0)
				{
					ManaColor[] components = card.Cost.EnumerateComponents().SampleManyOrAll(Level, GameRun.BattleRng);
					card.DecreaseBaseCost(ManaGroup.FromComponents(components));
					go = true;
				}
				if (card.IsEthereal)
				{
					card.IsEthereal = false;
					go = true;
				}
				if (go && !went)
				{
					NotifyActivating();
					went = true;
				}
			}
		}
	}
}