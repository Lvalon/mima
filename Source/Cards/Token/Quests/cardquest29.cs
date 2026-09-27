using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using lvalonmima.Cards.Template;
using lvalonmima.StatusEffects;

namespace lvalonmima.Cards;

public sealed class cardquest29Def : lvalonmimaCardTemplate
{
	public override CardConfig MakeConfig()
	{
		CardConfig config = GetCardDefaultConfig(true);
		config.Colors = [ManaColor.Blue, ManaColor.Red, ManaColor.White, ManaColor.Green, ManaColor.Black];
		config.Rarity = Rarity.Rare;

		config.Value1 = 1;
		config.Value2 = 1;

		config.Keywords = Keyword.Forbidden;

		config.RelativeEffects = [nameof(sequest)];

		config.Illustrator = "会帆";

		config.Index = CardIndexGenerator.GetUniqueIndex(config, 29);
		return config;
	}
}

[EntityLogic(typeof(cardquest29Def))]
public sealed class cardquest29 : questCard
{
}
