using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
using LBoL.Core.Randoms;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoLEntitySideloader.Attributes;
using lvalonmima.Cards;

namespace lvalonmima.StatusEffects
{
	public sealed class seshinkiDef : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Positive;
			config.HasCount = true;
			config.CountStackType = StackType.Add;
			config.Keywords = Keyword.TempMorph | Keyword.Replenish;
			return config;
		}
	}

	[EntityLogic(typeof(seshinkiDef))]
	public sealed class seshinki : StatusEffect
	{
		public ManaGroup Mana => new ManaGroup() { Any = 0 };
		protected override void OnAdded(Unit unit)
		{
			ReactOwnerEvent(Battle.Player.TurnStarted, OnTurnStarted);
		}

		private IEnumerable<BattleAction> OnTurnStarted(UnitEventArgs args)
		{
			if (Battle.BattleShouldEnd) { yield break; }
			NotifyActivating();
			List<Card> list = Battle.RollCardsWithoutManaLimit(new CardWeightTable(RarityWeightTable.NonCommon, OwnerWeightTable.Valid, CardTypeWeightTable.CanBeLoot), Count, c => c.Id != nameof(cardshinki)).ToList();
			if (list != null && list.Count > 0)
			{
				SelectCardInteraction interaction = new SelectCardInteraction(0, Math.Min(Level, list.Count), list)
				{
					Source = this
				};
				yield return new InteractionAction(interaction);
				IReadOnlyList<Card> cards = interaction.SelectedCards;
				// List<Card> cards2 = new List<Card>();
				if (cards.Count > 0)
				{
					foreach (Card card in cards)
					{
						if (!card.IsXCost)
						{
							card.SetTurnCost(Mana);
						}
						card.IsExile = true;
						card.IsEthereal = true;
						card.IsReplenish = true;
						// cards2.Add(card);
					}
					yield return new AddCardsToDrawZoneAction(cards, DrawZoneTarget.Top);
				}
				// foreach (Card card in cards2)
				// {
				// }
			}
		}
	}
}