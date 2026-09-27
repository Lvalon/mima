using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using lvalonmima.Cards.Template;
using lvalonmima.StatusEffects;

namespace lvalonmima.Cards;

public sealed class cardquest6Def : lvalonmimaCardTemplate
{
	public override CardConfig MakeConfig()
	{
		CardConfig config = GetCardDefaultConfig(true);
		config.Colors = [ManaColor.Black, ManaColor.Green];
		config.Rarity = Rarity.Common;

		config.Value1 = 1;
		config.Value2 = 3;

		config.Keywords = Keyword.Forbidden;

		config.RelativeEffects = [nameof(sequest), nameof(sehaunted)];

		config.Illustrator = "yange";

		config.Index = CardIndexGenerator.GetUniqueIndex(config, 6);
		return config;
	}
}

[EntityLogic(typeof(cardquest6Def))]
public sealed class cardquest6 : questCard
{
}
