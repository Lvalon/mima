using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using lvalonmima.StatusEffects;

namespace lvalonmima.Cards
{
	public sealed class cardquest28Def : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig(true);
			config.Colors = new List<ManaColor>() { ManaColor.Green, ManaColor.Blue };
			config.Rarity = Rarity.Rare;

			config.Value1 = 1;
			config.Value2 = 1;

			config.Keywords = Keyword.Forbidden;
			config.RelativeKeyword = Keyword.Copy;

			config.RelativeEffects = new List<string>() { nameof(sequest) };

			config.Illustrator = "狼巴子原型机";

			config.Index = CardIndexGenerator.GetUniqueIndex(config, 28);
			return config;
		}
	}

	[EntityLogic(typeof(cardquest28Def))]
	public sealed class cardquest28 : questCard
	{
	}
}


