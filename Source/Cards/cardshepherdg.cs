using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core;
using lvalonmima.StatusEffects;
using LBoL.EntityLib.StatusEffects.Cirno;
using LBoL.Core.Units;

namespace lvalonmima.Cards
{
	public sealed class cardshepherdgDef : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig();
			config.Colors = new List<ManaColor>() { ManaColor.Blue, ManaColor.Green };
			config.Cost = new ManaGroup() { Any = 2, Blue = 1, Green = 1 };
			config.UpgradedCost = new ManaGroup() { Any = 1, Hybrid = 1, HybridColor = 6 };
			config.Rarity = Rarity.Uncommon;
			config.Type = CardType.Ability;
			config.TargetType = TargetType.AllEnemies;
			config.RelativeEffects = new List<string>() { nameof(Cold) };
			config.UpgradedRelativeEffects = new List<string>() { nameof(Cold) };

			config.Value1 = 1;
			config.Mana = new ManaGroup() { Green = 1 };

			config.Illustrator = "kazuha (ichiwa)";

			config.Index = CardIndexGenerator.GetUniqueIndex(config);
			return config;
		}
	}

	[EntityLogic(typeof(cardshepherdgDef))]
	public sealed class cardshepherdg : lvalonmimaCard
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new ApplyStatusEffectAction<seshepherdg>(Battle.Player, Value1, 0, 0, 0);
			foreach (Unit unit in Battle.AllAliveUnits)
			{
				if (!unit.IsAlive || Battle.BattleShouldEnd) { continue; }
				yield return new ApplyStatusEffectAction<Cold>(unit, Value1, 0, 0, 0);
			}
		}
	}
}


