using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using lvalonmima.Cards.Template;
using lvalonmima.StatusEffects;
using LBoL.EntityLib.Cards.Character.Cirno;

namespace lvalonmima.Cards;

public sealed class cardquest9Def : lvalonmimaCardTemplate
{
	public override CardConfig MakeConfig()
	{
		CardConfig config = GetCardDefaultConfig(true);
		config.Colors = [ManaColor.Blue, ManaColor.Black];
		config.Rarity = Rarity.Common;

		config.Value1 = 9;
		config.Value2 = 1;

		config.Keywords = Keyword.Forbidden;

		config.RelativeEffects = [nameof(sequest)];

		config.RelativeCards = [nameof(IceWing)];

		config.Illustrator = "ういrふぃ８え８w８ぢ";

		config.Index = CardIndexGenerator.GetUniqueIndex(config, 9);
		return config;
	}
}

[EntityLogic(typeof(cardquest9Def))]
public sealed class cardquest9 : questCard
{
}
