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
using lvalonmima.StatusEffects;

namespace lvalonmima.Cards
{
	public sealed class cardreminiDef : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig();
			config.Colors = new List<ManaColor>() { ManaColor.Colorless };
			config.Cost = new ManaGroup() { Colorless = 1 };
			config.Rarity = Rarity.Common;
			config.Type = CardType.Skill;
			config.TargetType = TargetType.Nobody;

			config.Value1 = 1;
			config.Value2 = 3;
			config.UpgradedValue2 = 4;

			config.RelativeEffects = new List<string>() { nameof(seunder) };
			config.UpgradedRelativeEffects = new List<string>() { nameof(seunder) };

			config.Illustrator = "Radal";

			config.Index = CardIndexGenerator.GetUniqueIndex(config);
			return config;
		}
	}

	[EntityLogic(typeof(cardreminiDef))]
	public sealed class cardremini : lvalonmimaCard.trigger50card
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			List<Card> list = Battle.RollCards(new CardWeightTable(RarityWeightTable.NoneRare, OwnerWeightTable.OnlyPlayer, CardTypeWeightTable.OnlySkill), Value2, (config) => !config.Keywords.HasFlag(Keyword.Forbidden) && config.Id != Id).ToList();
			if (list.NotEmpty())
			{
				SelectCardInteraction interaction = new SelectCardInteraction(Value1, Value1 + (BepinexPlugin.u50 ? 1 : 0), list, SelectedCardHandling.DoNothing)
				{
					Source = this
				};
				yield return new InteractionAction(interaction, false);
				if (interaction.SelectedCards.NotEmpty())
				{
					yield return new AddCardsToHandAction(interaction.SelectedCards.ToList());
				}
			}
		}
	}
}


