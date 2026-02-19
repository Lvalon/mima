using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using lvalonmima.StatusEffects;

namespace lvalonmima.Cards
{
	public sealed class cardquest12Def : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig(true);
			config.Colors = new List<ManaColor>() { ManaColor.Colorless, ManaColor.Black };
			config.Rarity = Rarity.Common;

			config.Value1 = 5;
			config.Value2 = 1;

			config.Keywords = Keyword.Forbidden;

			config.RelativeEffects = new List<string>() { nameof(sequest) };

			config.Illustrator = "なまうに";

			config.Index = CardIndexGenerator.GetUniqueIndex(config, 12);
			return config;
		}
	}

	[EntityLogic(typeof(cardquest12Def))]
	public sealed class cardquest12 : questCard
	{
		public int Value30 => 3;
	}
}


