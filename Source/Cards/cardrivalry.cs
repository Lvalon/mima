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
	public sealed class cardrivalryDef : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig();
			config.Colors = new List<ManaColor>() { ManaColor.White, ManaColor.Red };
			config.Cost = new ManaGroup() { Any = 2, Hybrid = 1, HybridColor = 2 };
			config.UpgradedCost = new ManaGroup() { Any = 1, Hybrid = 1, HybridColor = 2 };
			config.Rarity = Rarity.Uncommon;
			config.Type = CardType.Skill;
			config.TargetType = TargetType.Nobody;

			config.Value1 = 2;
			config.Value2 = 1;
			config.Mana = new ManaGroup() { Any = 0 };

			config.Keywords = Keyword.Exile | Keyword.Ethereal;
			config.UpgradedKeywords = Keyword.Exile | Keyword.Ethereal;

			config.RelativeKeyword = Keyword.TempMorph;
			config.UpgradedRelativeKeyword = Keyword.TempMorph;

			config.Illustrator = "夜覩カタリ";

			config.Index = CardIndexGenerator.GetUniqueIndex(config);
			return config;
		}
	}

	[EntityLogic(typeof(cardrivalryDef))]
	public sealed class cardrivalry : lvalonmimaCard
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			List<Card> list = new List<Card>();

			list = Battle.RollCardsWithoutManaLimit(new CardWeightTable(RarityWeightTable.OnlyUncommon, OwnerWeightTable.OnlyPlayer, CardTypeWeightTable.OnlyAbility), Value1).ToList();
			if (list.NotEmpty())
			{
				SelectCardInteraction interaction = new SelectCardInteraction(Value2, Value2, list, SelectedCardHandling.DoNothing)
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
						card.IsEthereal = true;
					}
					yield return new AddCardsToHandAction(selectedCards);
				}
			}
		}
	}
}


