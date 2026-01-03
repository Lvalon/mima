using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core;
using lvalonmima.StatusEffects;
using LBoL.EntityLib.StatusEffects.Others;
using LBoL.Core.Units;

namespace lvalonmima.Cards
{
	public sealed class cardbackgroundDef : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig();
			config.Colors = new List<ManaColor>() { ManaColor.Black, ManaColor.Green };
			config.Cost = new ManaGroup() { Any = 1, Black = 1, Green = 1 };
			config.Rarity = Rarity.Rare;
			config.Type = CardType.Ability;
			config.TargetType = TargetType.AllEnemies;

			config.RelativeEffects = new List<string>() { nameof(Poison) };
			config.UpgradedRelativeEffects = new List<string>() { nameof(Poison) };

			config.Value1 = 1;
			config.Value2 = 6;

			config.Illustrator = "yohane";

			config.Index = CardIndexGenerator.GetUniqueIndex(config);
			return config;
		}
	}

	[EntityLogic(typeof(cardbackgroundDef))]
	public sealed class cardbackground : lvalonmimaCard
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new ApplyStatusEffectAction<sebackground>(Battle.Player, Value1, 0, 0, 0);
			foreach (Unit unit in Battle.AllAliveEnemies)
			{
				if (!unit.IsAlive || Battle.BattleShouldEnd || !IsUpgraded) { continue; }
				yield return new ApplyStatusEffectAction<Poison>(unit, Value2, 0, 0, 0);
			}
		}
	}
}


