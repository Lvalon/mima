using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using lvalonmima.StatusEffects;

namespace lvalonmima.Cards
{
	public sealed class cardquest7Def : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig(true);
			config.Colors = new List<ManaColor>() { ManaColor.Black, ManaColor.White };
			config.Rarity = Rarity.Common;

			config.Value1 = 2;
			config.Value2 = 85;

			config.Keywords = Keyword.Forbidden;

			config.RelativeEffects = new List<string>() { nameof(sequest) };

			config.Illustrator = "稀泥m";

			config.Index = CardIndexGenerator.GetUniqueIndex(config, 7);
			return config;
		}
	}

	[EntityLogic(typeof(cardquest7Def))]
	public sealed class cardquest7 : questCard
	{
	}
}


