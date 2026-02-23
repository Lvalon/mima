using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using lvalonmima.StatusEffects;

namespace lvalonmima.Cards
{
	public sealed class cardquest27Def : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig(true);
			config.Colors = new List<ManaColor>() { ManaColor.Green, ManaColor.Red };
			config.Rarity = Rarity.Rare;

			config.Value1 = 1;
			config.Value2 = 1;

			config.Keywords = Keyword.Forbidden;

			config.RelativeEffects = new List<string>() { nameof(sequest) };

			config.Illustrator = "めろん２２";

			config.Index = CardIndexGenerator.GetUniqueIndex(config, 27);
			return config;
		}
	}

	[EntityLogic(typeof(cardquest27Def))]
	public sealed class cardquest27 : questCard
	{
	}
}


