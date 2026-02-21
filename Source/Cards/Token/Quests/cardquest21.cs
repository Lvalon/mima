using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using lvalonmima.StatusEffects;

namespace lvalonmima.Cards
{
	public sealed class cardquest21Def : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig(true);
			config.Colors = new List<ManaColor>() { ManaColor.Blue, ManaColor.Green };
			config.Rarity = Rarity.Uncommon;

			config.Value1 = 10;
			config.Value2 = 210;

			config.Keywords = Keyword.Forbidden;

			config.RelativeEffects = new List<string>() { nameof(sequest) };

			config.Illustrator = "Unkky";

			config.Index = CardIndexGenerator.GetUniqueIndex(config, 21);
			return config;
		}
	}

	[EntityLogic(typeof(cardquest21Def))]
	public sealed class cardquest21 : questCard
	{
		public int Value9 => 9;
	}
}


