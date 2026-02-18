using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using lvalonmima.StatusEffects;

namespace lvalonmima.Cards
{
	public sealed class cardquest10Def : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig(true);
			config.Colors = new List<ManaColor>() { ManaColor.Black };
			config.Rarity = Rarity.Common;

			config.Value1 = 5;
			config.Value2 = 10; // and 100 gold

			config.Keywords = Keyword.Forbidden;

			config.RelativeEffects = new List<string>() { nameof(sequest) };

			config.RelativeCards = new List<string>() { nameof(LBoL.EntityLib.Cards.Neutral.Black.Shadow) };

			config.Illustrator = "Redlikeroses7";

			config.Index = CardIndexGenerator.GetUniqueIndex(config, 10);
			return config;
		}
	}

	[EntityLogic(typeof(cardquest10Def))]
	public sealed class cardquest10 : questCard
	{
		public int Value20 => 2;
	}
}


