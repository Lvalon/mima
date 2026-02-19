using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using lvalonmima.StatusEffects;

namespace lvalonmima.Cards
{
	public sealed class cardquest13Def : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig(true);
			config.Colors = new List<ManaColor>() { ManaColor.Colorless, ManaColor.Red };
			config.Rarity = Rarity.Common;

			config.Value1 = 2;
			config.Value2 = 1;

			config.Keywords = Keyword.Forbidden;

			config.RelativeEffects = new List<string>() { nameof(sequest) };

			config.Illustrator = "はるときくれ";

			config.Index = CardIndexGenerator.GetUniqueIndex(config, 13);
			return config;
		}
	}

	[EntityLogic(typeof(cardquest13Def))]
	public sealed class cardquest13 : questCard
	{
	}
}


