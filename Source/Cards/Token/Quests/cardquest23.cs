using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using lvalonmima.StatusEffects;
using LBoL.EntityLib.StatusEffects.Marisa;

namespace lvalonmima.Cards
{
	public sealed class cardquest23Def : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig(true);
			config.Colors = new List<ManaColor>() { ManaColor.White, ManaColor.Black };
			config.Rarity = Rarity.Uncommon;

			config.Value1 = 4;
			config.Value2 = 20;

			config.Keywords = Keyword.Forbidden;

			config.RelativeEffects = new List<string>() { nameof(sequest) };

			config.Illustrator = "米诺";

			config.Index = CardIndexGenerator.GetUniqueIndex(config, 23);
			return config;
		}
	}

	[EntityLogic(typeof(cardquest23Def))]
	public sealed class cardquest23 : questCard
	{
	}
}


