using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using lvalonmima.StatusEffects;

namespace lvalonmima.Cards
{
	public sealed class cardquest8Def : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig(true);
			config.Colors = new List<ManaColor>() { ManaColor.Green, ManaColor.White };
			config.Rarity = Rarity.Common;

			config.Value1 = 23;
			config.Value2 = 1;

			config.Keywords = Keyword.Forbidden;

			config.RelativeEffects = new List<string>() { nameof(sequest) };

			config.Illustrator = "an2a";

			config.Index = CardIndexGenerator.GetUniqueIndex(config, 8);
			return config;
		}
	}

	[EntityLogic(typeof(cardquest8Def))]
	public sealed class cardquest8 : questCard
	{
	}
}


