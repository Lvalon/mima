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
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Randoms;
using LBoL.EntityLib.PlayerUnits;

namespace lvalonmima.Cards
{
	public sealed class cardmarisaskillDef : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig();
			config.Colors = new List<ManaColor>() { ManaColor.Black, ManaColor.Red };
			config.Cost = new ManaGroup() { Any = 1, Hybrid = 2, HybridColor = 7 };
			config.Rarity = Rarity.Uncommon;
			config.Type = CardType.Skill;
			config.TargetType = TargetType.Nobody;
			config.RelativeKeyword = Keyword.TempMorph;
			config.UpgradedRelativeKeyword = Keyword.TempMorph;

			config.Value1 = 1;
			config.Value2 = 3;
			config.UpgradedValue2 = 5;

			config.Mana = new ManaGroup() { Any = 0 };

			config.Illustrator = "kitsunenahou";

			config.Index = CardIndexGenerator.GetUniqueIndex(config);
			return config;
		}
	}

	[EntityLogic(typeof(cardmarisaskillDef))]
	public sealed class cardmarisaskill : lvalonmimaCard
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			List<Card> list = new List<Card>();
			list = Battle.RollCardsWithoutManaLimit(new CardWeightTable(RarityWeightTable.BattleCard, OwnerWeightTable.AllOnes, CardTypeWeightTable.OnlySkill), Value2, (config) => config.Owner == nameof(Marisa) && !config.Keywords.HasFlag(Keyword.Forbidden)).ToList();
			if (list.NotEmpty())
			{
				SelectCardInteraction interaction = new SelectCardInteraction(0, Value1, list, SelectedCardHandling.DoNothing)
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
						card.IsExile = true;
					}
					if (Battle.BattleShouldEnd) { yield break; }
					yield return new AddCardsToHandAction(selectedCards);
				}
			}
		}
	}
}


