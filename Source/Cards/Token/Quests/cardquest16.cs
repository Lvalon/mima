using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using lvalonmima.StatusEffects;

namespace lvalonmima.Cards
{
	public sealed class cardquest16Def : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig(true);
			config.Colors = new List<ManaColor>() { ManaColor.Blue, ManaColor.Black };
			config.Rarity = Rarity.Uncommon;

			config.Value1 = 2;
			config.Value2 = 1;

			config.Keywords = Keyword.Forbidden;

			config.RelativeEffects = new List<string>() { nameof(sequest) };

			config.Illustrator = "";

			config.Index = CardIndexGenerator.GetUniqueIndex(config, 16);
			return config;
		}
	}

	[EntityLogic(typeof(cardquest16Def))]
	public sealed class cardquest16 : questCard
	{
	}
}


