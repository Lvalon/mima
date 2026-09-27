using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core;
using System.Linq;
using System;
using LBoL.Core.Randoms;
using LBoL.EntityLib.StatusEffects.ExtraTurn;

namespace lvalonmima.Cards;

public sealed class cardtpartnerDef : lvalonmimaCardTemplate
{
	public override CardConfig MakeConfig()
	{
		CardConfig config = GetCardDefaultConfig();
		config.Colors = [ManaColor.White, ManaColor.Blue, ManaColor.Colorless];
		config.Cost = new ManaGroup() { Any = 3, Hybrid = 1, HybridColor = 0, Colorless = 1 };
		config.UpgradedCost = new ManaGroup() { Any = 2, Hybrid = 1, HybridColor = 0, Colorless = 1 };
		config.Rarity = Rarity.Uncommon;
		config.Type = CardType.Skill;
		config.TargetType = TargetType.Nobody;
		config.Keywords = config.UpgradedKeywords = Keyword.Ethereal;
		config.RelativeKeyword = config.UpgradedRelativeKeyword = Keyword.TempMorph;
		config.Mana = new ManaGroup() { Any = 0 };

		config.RelativeEffects = config.UpgradedRelativeEffects = [nameof(TimeIsLimited)];

		config.Value1 = 1;

		config.Illustrator = "酒醉的蝴蝶";

		config.Index = CardIndexGenerator.GetUniqueIndex(config);
		return config;
	}
}

[EntityLogic(typeof(cardtpartnerDef))]
public sealed class cardtpartner : lvalonmimaCard
{
	protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
	{
		List<Card> list = [.. Battle.RollCardsWithoutManaLimit(new CardWeightTable(RarityWeightTable.AllOnes, OwnerWeightTable.AllOnes, CardTypeWeightTable.CanBeLoot), Value1, config => config.Id != Id && (config.RelativeEffects.Contains(nameof(TimeIsLimited)) || config.UpgradedRelativeEffects.Contains(nameof(TimeIsLimited))))];
		foreach (Card card in list)
		{
			if (!card.IsXCost)
			{
				card.SetTurnCost(Mana);
			}
			card.IsEthereal = true;
			card.IsExile = true;
		}
		yield return new AddCardsToHandAction(list);
	}
}
