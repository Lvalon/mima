using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using lvalonmima.StatusEffects;

namespace lvalonmima.Cards
{
	public sealed class cardquest14Def : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig(true);
			config.Colors = new List<ManaColor>() { ManaColor.Blue, ManaColor.Red };
			config.Rarity = Rarity.Common;

			config.Value1 = 1;
			config.Value2 = 1;

			config.Keywords = Keyword.Forbidden;

			config.RelativeEffects = new List<string>() { nameof(sequest) };

			config.Illustrator = "しょぺ@冬Z04b";

			config.Index = CardIndexGenerator.GetUniqueIndex(config, 14);
			return config;
		}
	}

	[EntityLogic(typeof(cardquest14Def))]
	public sealed class cardquest14 : questCard
	{
	}
}


