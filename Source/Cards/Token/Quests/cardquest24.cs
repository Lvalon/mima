using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using lvalonmima.Cards.Template;
using lvalonmima.StatusEffects;

namespace lvalonmima.Cards;

public sealed class cardquest24Def : lvalonmimaCardTemplate
{
	public override CardConfig MakeConfig()
	{
		CardConfig config = GetCardDefaultConfig(true);
		config.Colors = [ManaColor.Red, ManaColor.White];
		config.Rarity = Rarity.Uncommon;

		config.Value1 = 10;
		config.Value2 = 10;

		config.Keywords = Keyword.Forbidden;

		config.RelativeEffects = [nameof(sequest), nameof(seholddamage), nameof(sedelaydamage)];

		config.Illustrator = "海源";

		config.Index = CardIndexGenerator.GetUniqueIndex(config, 24);
		return config;
	}
}

[EntityLogic(typeof(cardquest24Def))]
public sealed class cardquest24 : questCard
{
}
