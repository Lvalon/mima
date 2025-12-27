using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core;
using System;
using LBoL.Core.Units;

namespace lvalonmima.Cards
{
	public sealed class cardtruefalseDef : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig();
			config.Colors = new List<ManaColor>() { ManaColor.White, ManaColor.Black };
			config.Cost = new ManaGroup() { White = 2, Black = 2, Hybrid = 1, HybridColor = 1 };
			config.UpgradedCost = new ManaGroup() { White = 1, Black = 1, Hybrid = 1, HybridColor = 1 };
			config.FindInBattle = false;
			config.Rarity = Rarity.Uncommon;
			config.Type = CardType.Attack;
			config.TargetType = TargetType.SingleEnemy;

			config.Damage = 0;

			config.Value1 = 2;
			config.Value2 = 6;

			config.Illustrator = "六夜";

			config.Index = CardIndexGenerator.GetUniqueIndex(config);
			return config;
		}
	}

	[EntityLogic(typeof(cardtruefalseDef))]
	public sealed class cardtruefalse : lvalonmimaCard
	{
		public string eff1
		{
			get
			{
				if (PendingTarget != null)
				{
					return " (" + Math.Abs(PendingTarget.MaxHp - PendingTarget.Hp) + ")";
				}
				return "";
			}
		}
		public string eff2
		{
			get
			{
				if (PendingTarget != null)
				{
					return " (" + Math.Abs(PendingTarget.Hp - PendingTarget.MaxHp + PendingTarget.Hp) + ")";
				}
				return "";
			}
		}

		public override bool CanUse
		{
			get
			{
				return GameRun != null && GameRun.Player.MaxHp > Value2;
			}
		}

		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			if (GameRun.Player.MaxHp > 6)
			{
				GameRun.LoseMaxHp(Value2, true);
				EnemyUnit target = selector.SelectedEnemy;
				if (Battle.BattleShouldEnd || !target.IsAlive) { yield break; }
				int diff = Math.Abs(target.Hp - target.MaxHp + target.Hp);
				int lifelost = target.MaxHp - target.Hp;
				if (lifelost == 0)
				{
					yield return new ForceKillAction(Battle.Player, target);
				}
				else
				{
					GameRun.SetEnemyHpAndMaxHp(lifelost, target.MaxHp, target, true);
				}
				yield return SacrificeAction(diff);
				yield return new RemoveCardAction(this);
			}
		}
	}
}


