using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.Battle.BattleActions;
using LBoL.Base.Extensions;
using LBoL.Core;
using System.Linq;
using System;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.StatusEffects;

namespace lvalonmima.Cards
{
	public sealed class cardstakeDef : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig();
			config.Colors = new List<ManaColor>() { ManaColor.Green, ManaColor.Red };
			config.Cost = new ManaGroup() { Green = 1, Red = 1 };
			config.Rarity = Rarity.Uncommon;
			config.Type = CardType.Skill;
			config.TargetType = TargetType.Nobody;
			config.UpgradedKeywords = Keyword.Echo;
			config.RelativeKeyword = Keyword.Overdraft;
			config.UpgradedRelativeKeyword = Keyword.Overdraft;
			config.Value1 = 1;

			config.Illustrator = "ぱじ";

			config.Index = CardIndexGenerator.GetUniqueIndex(config);
			return config;
		}
	}

	[EntityLogic(typeof(cardstakeDef))]
	public sealed class cardstake : lvalonmimaCard
	{
		public override Interaction Precondition()
		{
			if (Battle.HandZone.Count > 1 || (IsUpgraded && (Battle.ExileZone.Count > 0 || Battle.DiscardZone.Count > 0)))
			{
				return new SelectCardInteraction(Value1, Value1, IsUpgraded ? Battle.EnumerateAllCardsButExile().Where(c => c != this) : Battle.HandZone.Where(c => c != this));
			}
			return null;
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			IEnumerable<ManaColor> cardcolor = Enumerable.Empty<ManaColor>();
			if (precondition != null)
			{
				IReadOnlyList<Card> selectedCards = ((SelectCardInteraction)precondition).SelectedCards;
				if (selectedCards != null)
				{
					Card card = selectedCards.FirstOrDefault();
					cardcolor = card.Config.Colors;
					yield return new ExileCardAction(card);
					if (card.ConfigCost.Amount > 0)
					{
						if (Battle.BattleShouldEnd) { yield break; }
						yield return new LockRandomTurnManaAction(card.ConfigCost.Amount);
						if (Battle.BattleShouldEnd) { yield break; }
						yield return new ApplyStatusEffectAction<Charging>(Battle.Player, card.ConfigCost.Amount, 0, 0, 0);
					}
				}
			}
			List<Card> pass = pass = Battle.EnumerateAllCards().Where(c => c.Zone != CardZone.Hand)
			.Where(c => c != this && c.Config.Colors.Intersect(cardcolor).Any() || c.Config.Colors == cardcolor || (c.Config.Colors.Contains(ManaColor.Colorless) && cardcolor.Count() == 0) || (c.Config.Colors.Count() == 0 && cardcolor.Contains(ManaColor.Colorless))) //edge case no color
			.ToList();

			if (pass.Count > 0 && precondition != null)
			{
				SelectCardInteraction interaction = new SelectCardInteraction(Value1, Value1, pass)
				{
					Source = this
				};
				yield return new InteractionAction(interaction);
				yield return new MoveCardAction(interaction.SelectedCards.FirstOrDefault(), CardZone.Hand);
			}
		}
	}
}


