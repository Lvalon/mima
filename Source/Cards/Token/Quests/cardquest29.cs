using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using lvalonmima.StatusEffects;

namespace lvalonmima.Cards
{
	public sealed class cardquest29Def : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig(true);
			config.Colors = new List<ManaColor>() { ManaColor.Blue, ManaColor.Red, ManaColor.White, ManaColor.Green, ManaColor.Black };
			config.Rarity = Rarity.Rare;

			config.Value1 = 1;
			config.Value2 = 1;

			config.Keywords = Keyword.Forbidden;

			config.RelativeEffects = new List<string>() { nameof(sequest) };

			config.Illustrator = "会帆";

			config.Index = CardIndexGenerator.GetUniqueIndex(config, 29);
			return config;
		}
	}

	[EntityLogic(typeof(cardquest29Def))]
	public sealed class cardquest29 : questCard
	{
	}
}


