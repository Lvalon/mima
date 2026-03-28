using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using lvalonmima.StatusEffects;

namespace lvalonmima.Cards
{
	public sealed class cardquest1Def : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig(true);
			config.Colors = new List<ManaColor>() { ManaColor.White };
			config.Rarity = Rarity.Common;

			config.Value1 = 8;
			config.Value2 = 40;

			config.Keywords = Keyword.Forbidden;

			config.RelativeKeyword = Keyword.Expel;

			config.RelativeEffects = new List<string>() { nameof(sequest) };

			config.Illustrator = "扇城ひな";

			config.Index = CardIndexGenerator.GetUniqueIndex(config, 1);
			return config;
		}
	}

	[EntityLogic(typeof(cardquest1Def))]
	public sealed class cardquest1 : questCard
	{
	}
}


