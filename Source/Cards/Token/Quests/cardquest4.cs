using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using lvalonmima.Cards.Template;
using lvalonmima.StatusEffects;

namespace lvalonmima.Cards;

public sealed class cardquest4Def : lvalonmimaCardTemplate
{
	public override CardConfig MakeConfig()
	{
		CardConfig config = GetCardDefaultConfig(true);
		config.Colors = [ManaColor.Green];
		config.Rarity = Rarity.Common;

		config.Value1 = 5;
		config.Value2 = 36;

		config.Keywords = Keyword.Forbidden;

		config.RelativeEffects = [nameof(sequest)];

		config.RelativeCards = [nameof(cardgenji)];

		config.Illustrator = "Men-dont-scream";

		config.Index = CardIndexGenerator.GetUniqueIndex(config, 4);
		return config;
	}
}

[EntityLogic(typeof(cardquest4Def))]
public sealed class cardquest4 : questCard
{
}
