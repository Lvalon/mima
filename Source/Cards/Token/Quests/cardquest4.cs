using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using lvalonmima.StatusEffects;

namespace lvalonmima.Cards
{
	public sealed class cardquest4Def : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig(true);
			config.Colors = new List<ManaColor>() { ManaColor.Green };
			config.Rarity = Rarity.Common;

			config.Value1 = 5;
			config.Value2 = 36;

			config.Keywords = Keyword.Forbidden;

			config.RelativeEffects = new List<string>() { nameof(sequest) };

			config.RelativeCards = new List<string>() { nameof(cardgenji) };

			config.Illustrator = "Men-dont-scream";

			config.Index = CardIndexGenerator.GetUniqueIndex(config, 4);
			return config;
		}
	}

	[EntityLogic(typeof(cardquest4Def))]
	public sealed class cardquest4 : questCard
	{
	}
}


