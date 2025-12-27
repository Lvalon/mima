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
using LBoL.Core.Randoms;
using LBoL.Core.Battle.Interactions;

namespace lvalonmima.Cards
{
	public sealed class cardexistpDef : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig();
			config.Colors = new List<ManaColor>() { ManaColor.Black, ManaColor.Green };
			config.Cost = new ManaGroup() { Any = 1, Hybrid = 3, HybridColor = 8 };
			config.UpgradedCost = new ManaGroup() { Any = 1, Hybrid = 1, HybridColor = 8 };
			config.Rarity = Rarity.Uncommon;
			config.Type = CardType.Skill;
			config.TargetType = TargetType.Nobody;
			config.Keywords = Keyword.Exile;
			config.UpgradedKeywords = Keyword.Exile;
			config.RelativeKeyword = Keyword.FollowAttack | Keyword.FollowCard | Keyword.TempMorph;
			config.UpgradedRelativeKeyword = Keyword.FollowAttack | Keyword.FollowCard | Keyword.TempMorph;
			config.Mana = new ManaGroup() { Any = 0 };

			config.Value1 = 1;
			config.Value2 = 3;

			config.Illustrator = "camellia";

			config.Index = CardIndexGenerator.GetUniqueIndex(config);
			return config;
		}
	}

	[EntityLogic(typeof(cardexistpDef))]
	public sealed class cardexistp : lvalonmimaCard
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			foreach (Card card in Battle.EnumerateAllCards().Where(c => c.CardType == CardType.Attack && !c.IsFollowCard).ToList())
			{
				card.IsFollowCard = true;
			}
			List<Card> list = Battle.RollCardsWithoutManaLimit(new CardWeightTable(RarityWeightTable.AllOnes, OwnerWeightTable.AllOnes, CardTypeWeightTable.CanBeLoot), Value2, (CardConfig config) => (config.RelativeKeyword.HasFlag(Keyword.FollowAttack) || config.UpgradedRelativeKeyword.HasFlag(Keyword.FollowAttack)) && config.Id != Id).ToList();
			if (list.NotEmpty())
			{
				SelectCardInteraction interaction = new SelectCardInteraction(Value1, Value1, list, SelectedCardHandling.DoNothing)
				{
					Source = this
				};
				yield return new InteractionAction(interaction, false);
				IReadOnlyList<Card> selectedCards = interaction.SelectedCards;
				if (selectedCards != null)
				{
					foreach (Card card in selectedCards)
					{
						if (!card.IsXCost)
						{
							card.SetTurnCost(Mana);
						}
						yield return new AddCardsToHandAction(card);
					}
				}
			}
		}
	}
}


