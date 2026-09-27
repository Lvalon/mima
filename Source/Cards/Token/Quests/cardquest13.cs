using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using lvalonmima.Cards.Template;
using lvalonmima.StatusEffects;

namespace lvalonmima.Cards;

public sealed class cardquest13Def : lvalonmimaCardTemplate
{
	public override CardConfig MakeConfig()
	{
		CardConfig config = GetCardDefaultConfig(true);
		config.Colors = [ManaColor.Colorless, ManaColor.Red];
		config.Rarity = Rarity.Common;

		config.Value1 = 2;
		config.Value2 = 1;

		config.Keywords = Keyword.Forbidden;

		config.RelativeEffects = [nameof(sequest)];

		config.Illustrator = "はるときくれ";

		config.Index = CardIndexGenerator.GetUniqueIndex(config, 13);
		return config;
	}
}

[EntityLogic(typeof(cardquest13Def))]
public sealed class cardquest13 : questCard
{
}
