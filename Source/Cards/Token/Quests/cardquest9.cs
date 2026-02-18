using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using lvalonmima.StatusEffects;
using LBoL.EntityLib.Cards.Character.Cirno;

namespace lvalonmima.Cards
{
	public sealed class cardquest9Def : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig(true);
			config.Colors = new List<ManaColor>() { ManaColor.Blue, ManaColor.Black };
			config.Rarity = Rarity.Common;

			config.Value1 = 9;
			config.Value2 = 1;

			config.Keywords = Keyword.Forbidden;

			config.RelativeEffects = new List<string>() { nameof(sequest) };

			config.RelativeCards = new List<string>() { nameof(IceWing) };

			config.Illustrator = "ういrふぃ８え８w８ぢ";

			config.Index = CardIndexGenerator.GetUniqueIndex(config, 9);
			return config;
		}
	}

	[EntityLogic(typeof(cardquest9Def))]
	public sealed class cardquest9 : questCard
	{
	}
}


