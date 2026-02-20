using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using lvalonmima.StatusEffects;

namespace lvalonmima.Cards
{
	public sealed class cardquest17Def : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig(true);
			config.Colors = new List<ManaColor>() { ManaColor.White, ManaColor.Black };
			config.Rarity = Rarity.Uncommon;

			config.Value1 = 10;
			config.Value2 = 1;

			config.Mana = new ManaGroup { Any = 0 };

			config.Keywords = Keyword.Forbidden;

			config.RelativeKeyword = Keyword.TempMorph;

			config.RelativeEffects = new List<string>() { nameof(sequest) };

			config.Illustrator = "ノドグロ＠原稿";

			config.Index = CardIndexGenerator.GetUniqueIndex(config, 17);
			return config;
		}
	}

	[EntityLogic(typeof(cardquest17Def))]
	public sealed class cardquest17 : questCard
	{
	}
}


