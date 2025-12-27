using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core;
using lvalonmima.StatusEffects;
using LBoL.EntityLib.StatusEffects.Sakuya;

namespace lvalonmima.Cards
{
	public sealed class cardconversionDef : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig();
			config.Colors = new List<ManaColor>() { ManaColor.Black, ManaColor.Colorless };
			config.Cost = new ManaGroup() { Any = 2, Black = 1, Colorless = 1 };
			config.UpgradedCost = new ManaGroup() { Black = 1, Colorless = 1 };
			config.Rarity = Rarity.Uncommon;
			config.Type = CardType.Ability;
			config.TargetType = TargetType.Self;

			config.Keywords = Keyword.Retain;
			config.UpgradedKeywords = Keyword.Retain;

			config.RelativeEffects = new List<string>() { nameof(TimeAuraSe), nameof(seunder) };
			config.UpgradedRelativeEffects = new List<string>() { nameof(TimeAuraSe), nameof(seunder) };

			config.Value1 = 1;
			config.Value2 = 12;

			config.Illustrator = "Xiirus";

			config.Index = CardIndexGenerator.GetUniqueIndex(config);
			return config;
		}
	}

	[EntityLogic(typeof(cardconversionDef))]
	public sealed class cardconversion : lvalonmimaCard.trigger25card
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new ApplyStatusEffectAction<seconversion>(Battle.Player, Value1, 0, 0, 0);
			if (Battle.BattleShouldEnd) { yield break; }
			yield return new ApplyStatusEffectAction<TimeAuraSe>(Battle.Player, Value2 * (BepinexPlugin.u25 ? 2 : 1), 0, 0, 0);
		}
	}
}


