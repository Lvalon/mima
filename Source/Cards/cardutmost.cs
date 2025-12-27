using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core;
using lvalonmima.StatusEffects;

namespace lvalonmima.Cards
{
	public sealed class cardutmostDef : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig();
			config.Colors = new List<ManaColor>() { ManaColor.Red };
			config.Cost = new ManaGroup() { Any = 2, Red = 1 };
			config.UpgradedCost = new ManaGroup() { Red = 1 };
			config.Rarity = Rarity.Uncommon;
			config.Type = CardType.Ability;
			config.TargetType = TargetType.Self;
			config.RelativeEffects = new List<string>() { nameof(semburst) };
			config.UpgradedRelativeEffects = new List<string>() { nameof(semburst) };

			config.Value1 = 1;

			config.Illustrator = "灯跡（ヒセキ）";

			config.Index = CardIndexGenerator.GetUniqueIndex(config);
			return config;
		}
	}

	[EntityLogic(typeof(cardutmostDef))]
	public sealed class cardutmost : lvalonmimaCard
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new ApplyStatusEffectAction<seutmost>(Battle.Player, Value1, 0, 0, 0);
		}
	}
}


