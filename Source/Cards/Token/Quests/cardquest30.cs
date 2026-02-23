using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using lvalonmima.StatusEffects;

namespace lvalonmima.Cards
{
	public sealed class cardquest30Def : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig(true);
			config.Colors = new List<ManaColor>() { ManaColor.White, ManaColor.Black };
			config.Rarity = Rarity.Rare;

			config.Value1 = 1;
			config.Value2 = 3;

			config.Keywords = Keyword.Forbidden;

			config.RelativeEffects = new List<string>() { nameof(sequest), nameof(selockinstance) };

			config.Illustrator = "lyiet";

			config.Index = CardIndexGenerator.GetUniqueIndex(config, 30);
			return config;
		}
	}

	[EntityLogic(typeof(cardquest30Def))]
	public sealed class cardquest30 : questCard
	{
		public int Value7 => 7;
	}
}


