using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using lvalonmima.Cards.Template;

namespace lvalonmima.Cards;

public sealed class cardrerolldiscardDef : lvalonmimaCardTemplate
{
	public override CardConfig MakeConfig()
	{
		CardConfig config = GetCardDefaultConfig();
		config.Owner = null;
		config.IsPooled = false;
		config.Colors = [ManaColor.Colorless];
		config.Cost = new ManaGroup() { Any = 0 };
		config.Rarity = Rarity.Common;
		config.IsUpgradable = false;

		config.Type = CardType.Status;
		config.HideMesuem = true;
		config.TargetType = TargetType.Nobody;

		config.Illustrator = "";

		config.Index = CardIndexGenerator.GetUniqueIndex(config);
		return config;
	}
}

[EntityLogic(typeof(cardrerolldiscardDef))]
public sealed class cardrerolldiscard : lvalonmimaCard
{
}
