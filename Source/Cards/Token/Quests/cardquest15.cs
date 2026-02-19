using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using lvalonmima.StatusEffects;

namespace lvalonmima.Cards
{
	public sealed class cardquest15Def : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig(true);
			config.Colors = new List<ManaColor>() { ManaColor.Blue, ManaColor.Red, ManaColor.White, ManaColor.Green, ManaColor.Black };
			config.Rarity = Rarity.Common;

			config.Value1 = 5;
			config.Value2 = 15;

			config.Keywords = Keyword.Forbidden;

			config.RelativeEffects = new List<string>() { nameof(sequest) };

			config.Illustrator = "ZOIYA";

			config.Index = CardIndexGenerator.GetUniqueIndex(config, 15);
			return config;
		}
	}

	[EntityLogic(typeof(cardquest15Def))]
	public sealed class cardquest15 : questCard
	{
		public int Value10 => 10;
	}
}


