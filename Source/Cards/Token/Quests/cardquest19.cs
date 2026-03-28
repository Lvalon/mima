using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using lvalonmima.StatusEffects;

namespace lvalonmima.Cards
{
	public sealed class cardquest19Def : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig(true);
			config.Colors = new List<ManaColor>() { ManaColor.Green };
			config.Rarity = Rarity.Uncommon;

			config.Value1 = 6;
			config.Value2 = 1;

			config.Keywords = Keyword.Forbidden;

			config.RelativeEffects = new List<string>() { nameof(sequest) };

			config.Illustrator = "しょぺ▼C107(火)Z-04b";

			config.Index = CardIndexGenerator.GetUniqueIndex(config, 19);
			return config;
		}
	}

	[EntityLogic(typeof(cardquest19Def))]
	public sealed class cardquest19 : questCard
	{
	}
}


