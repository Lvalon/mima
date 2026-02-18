using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using lvalonmima.StatusEffects;

namespace lvalonmima.Cards
{
	public sealed class cardquest5Def : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig(true);
			config.Colors = new List<ManaColor>() { ManaColor.Black };
			config.Rarity = Rarity.Common;

			config.Value1 = 5;
			config.Value2 = 20;

			config.Keywords = Keyword.Forbidden;

			config.RelativeEffects = new List<string>() { nameof(sequest) };

			config.Illustrator = "Niy";

			config.Index = CardIndexGenerator.GetUniqueIndex(config, 5);
			return config;
		}
	}

	[EntityLogic(typeof(cardquest5Def))]
	public sealed class cardquest5 : questCard
	{
	}
}


