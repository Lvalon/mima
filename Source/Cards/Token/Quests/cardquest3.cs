using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using lvalonmima.StatusEffects;

namespace lvalonmima.Cards
{
	public sealed class cardquest3Def : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig(true);
			config.Colors = new List<ManaColor>() { ManaColor.Colorless };
			config.Rarity = Rarity.Common;

			config.Value1 = 5;
			config.Value2 = 50;

			config.Keywords = Keyword.Forbidden;

			config.RelativeEffects = new List<string>() { nameof(sequest) };

			config.Illustrator = "torque";

			config.Index = CardIndexGenerator.GetUniqueIndex(config, 3);
			return config;
		}
	}

	[EntityLogic(typeof(cardquest3Def))]
	public sealed class cardquest3 : questCard
	{
	}
}


