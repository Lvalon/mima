using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using lvalonmima.StatusEffects;
using LBoL.EntityLib.StatusEffects.Marisa;

namespace lvalonmima.Cards
{
	public sealed class cardquest22Def : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig(true);
			config.Colors = new List<ManaColor>() { ManaColor.White, ManaColor.Red };
			config.Rarity = Rarity.Uncommon;

			config.Value1 = 6;
			config.Value2 = 2;
			config.Mana = new ManaGroup() { Philosophy = 1 };

			config.Keywords = Keyword.Forbidden;

			config.RelativeEffects = new List<string>() { nameof(sequest), nameof(ManaFreezed) };

			config.Illustrator = "reimu.yuyuko.combo";

			config.Index = CardIndexGenerator.GetUniqueIndex(config, 22);
			return config;
		}
	}

	[EntityLogic(typeof(cardquest22Def))]
	public sealed class cardquest22 : questCard
	{
		public ManaGroup Mana2 => new ManaGroup() { Any = 1 };
	}
}


