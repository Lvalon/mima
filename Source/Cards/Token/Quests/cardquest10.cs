using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using lvalonmima.Cards.Template;
using lvalonmima.StatusEffects;

namespace lvalonmima.Cards;

public sealed class cardquest10Def : lvalonmimaCardTemplate
{
	public override CardConfig MakeConfig()
	{
		CardConfig config = GetCardDefaultConfig(true);
		config.Colors = [ManaColor.Black];
		config.Rarity = Rarity.Common;

		config.Value1 = 3;
		config.Value2 = 10; // and 100 gold

		config.Keywords = Keyword.Forbidden;

		config.RelativeEffects = [nameof(sequest)];

		config.RelativeCards = [nameof(LBoL.EntityLib.Cards.Neutral.Black.Shadow)];

		config.Illustrator = "Redlikeroses7";

		config.Index = CardIndexGenerator.GetUniqueIndex(config, 10);
		return config;
	}
}

[EntityLogic(typeof(cardquest10Def))]
public sealed class cardquest10 : questCard
{
	public int Value20 => 2;
}
