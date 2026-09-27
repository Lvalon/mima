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
using lvalonmima.StatusEffects;

namespace lvalonmima.Cards;

public sealed class cardrpolarityDef : lvalonmimaCardTemplate
{
	public override CardConfig MakeConfig()
	{
		CardConfig config = GetCardDefaultConfig();
		config.Colors = [ManaColor.White, ManaColor.Black];
		config.Cost = new ManaGroup() { White = 1, Black = 1, Hybrid = 2, HybridColor = 1 };
		config.UpgradedCost = new ManaGroup() { Hybrid = 2, HybridColor = 1 };
		config.Rarity = Rarity.Rare;
		config.Mana = new ManaGroup() { Colorless = 1 };
		config.Type = CardType.Ability;
		config.TargetType = TargetType.Self;
		config.Value1 = 1;
		config.Keywords = config.UpgradedKeywords = Keyword.Initial | Keyword.Retain | Keyword.Replenish;

		config.Illustrator = "camellia";

		config.Index = CardIndexGenerator.GetUniqueIndex(config);
		return config;
	}
}

[EntityLogic(typeof(cardrpolarityDef))]
public sealed class cardrpolarity : lvalonmimaCard
{
	public ManaGroup Mana2 => new() { Philosophy = 1 };
	protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
	{
		List<Card> exile = [.. Battle.EnumerateAllCards().Where(c => c.IsExile)];
		List<Card> noexile = [.. Battle.EnumerateAllCards().Where(c => !c.IsExile && c != this)];
		foreach (Card c in exile)
			c.IsExile = false;
		foreach (Card c in noexile)
			c.IsExile = true;
		yield return new ApplyStatusEffectAction<serpolarity>(Battle.Player, Value1, 0, 0, 0);
	}
}
