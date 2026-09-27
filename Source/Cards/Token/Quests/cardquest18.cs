using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using lvalonmima.Cards.Template;
using lvalonmima.StatusEffects;

namespace lvalonmima.Cards;

public sealed class cardquest18Def : lvalonmimaCardTemplate
{
	public override CardConfig MakeConfig()
	{
		CardConfig config = GetCardDefaultConfig(true);
		config.Colors = [ManaColor.White];
		config.Rarity = Rarity.Uncommon;

		config.Value1 = 3;
		config.Value2 = 3;

		config.Keywords = Keyword.Forbidden;

		config.RelativeEffects = [nameof(sequest)];

		config.Illustrator = "yezhi na";

		config.Index = CardIndexGenerator.GetUniqueIndex(config, 18);
		return config;
	}
}

[EntityLogic(typeof(cardquest18Def))]
public sealed class cardquest18 : questCard
{
}
