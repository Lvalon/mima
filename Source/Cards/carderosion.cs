using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core;
using LBoL.EntityLib.StatusEffects.Others;
using LBoL.Core.Units;

namespace lvalonmima.Cards
{
	public sealed class carderosionDef : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig();
			config.Colors = new List<ManaColor>() { ManaColor.Black, ManaColor.Green };
			config.Cost = new ManaGroup() { Black = 1, Green = 1 };
			config.UpgradedCost = new ManaGroup() { Any = 1, Hybrid = 1, HybridColor = 8 };
			config.Rarity = Rarity.Common;
			config.Type = CardType.Skill;
			config.TargetType = TargetType.All;
			config.RelativeEffects = new List<string>() { nameof(Poison) };
			config.UpgradedRelativeEffects = new List<string>() { nameof(Poison) };

			config.Value1 = 3;
			config.UpgradedValue1 = 4;
			config.Value2 = 6;
			config.UpgradedValue2 = 8;

			config.Illustrator = "Radal";

			config.Index = CardIndexGenerator.GetUniqueIndex(config);
			return config;
		}
	}

	[EntityLogic(typeof(carderosionDef))]
	public sealed class carderosion : lvalonmimaCard
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return SacrificeAction(Value1);
			foreach (Unit unit in Battle.AllAliveEnemies)
			{
				if (!unit.IsAlive || Battle.BattleShouldEnd) { yield break; }
				yield return new ApplyStatusEffectAction<Poison>(unit, Value2, 0, 0, 0);
			}
		}
	}
}


