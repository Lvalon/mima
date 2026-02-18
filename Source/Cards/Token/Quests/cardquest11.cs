using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using lvalonmima.StatusEffects;

namespace lvalonmima.Cards
{
	public sealed class cardquest11Def : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig(true);
			config.Colors = new List<ManaColor>() { ManaColor.Colorless, ManaColor.White };
			config.Rarity = Rarity.Common;

			config.Value1 = 5;
			config.Value2 = 2;

			config.Keywords = Keyword.Forbidden;

			config.RelativeEffects = new List<string>() { nameof(sequest) };

			config.Illustrator = "(仮)";

			config.Index = CardIndexGenerator.GetUniqueIndex(config, 10);
			return config;
		}
	}

	[EntityLogic(typeof(cardquest11Def))]
	public sealed class cardquest11 : questCard
	{
		public int Value440 => 440;
	}
}


