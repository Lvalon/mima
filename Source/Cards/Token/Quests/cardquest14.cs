using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using lvalonmima.Cards.Template;
using lvalonmima.StatusEffects;

namespace lvalonmima.Cards;

public sealed class cardquest14Def : lvalonmimaCardTemplate
{
	public override CardConfig MakeConfig()
	{
		CardConfig config = GetCardDefaultConfig(true);
		config.Colors = [ManaColor.Blue, ManaColor.Red];
		config.Rarity = Rarity.Common;

		config.Value1 = 1;
		config.Value2 = 1;

		config.Keywords = Keyword.Forbidden;

		config.RelativeEffects = [nameof(sequest)];

		config.Illustrator = "しょぺ@冬Z04b";

		config.Index = CardIndexGenerator.GetUniqueIndex(config, 14);
		return config;
	}
}

[EntityLogic(typeof(cardquest14Def))]
public sealed class cardquest14 : questCard
{
}
