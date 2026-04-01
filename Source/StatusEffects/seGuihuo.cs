using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.Cards.Enemy;
using LBoL.EntityLib.EnemyUnits.Normal.Ravens;
using LBoL.EntityLib.StatusEffects.Enemy;
using LBoLEntitySideloader.Attributes;

namespace lvalonmima.StatusEffects
{
	public sealed class seGuihuoDef : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Positive;
			return config;
		}
	}

	[EntityLogic(typeof(seGuihuoDef))]
	public sealed class seGuihuo : StatusEffect
	{
		public override bool ForceNotShowDownText => true;
		protected override void OnAdded(Unit unit)
		{
			ReactOwnerEvent(unit.Died, OnDied);
		}

		private IEnumerable<BattleAction> OnDied(DieEventArgs args)
		{
			bool isCount = Owner.HasStatusEffect<DeathExplodeCount>();
			bool isNotCount = Owner.HasStatusEffect<DeathExplodeNotCount>();
			if (!isCount && !isNotCount)
				yield break;

			NotifyActivating();

			int enemyCount = Battle.AllAliveEnemies.Count();
			if (enemyCount == 0)
				yield break;
			int counts = 0;
			int notCounts = 0;
			int countDown = 0;
			if (isCount)
			{
				counts = toolbox.Round(Owner.GetStatusEffect<DeathExplodeCount>().Level * 1.0 / 2);
				countDown = Owner.GetStatusEffect<DeathExplodeCount>().Count;
			}
			if (isNotCount)
			{
				notCounts = toolbox.Round(Owner.GetStatusEffect<DeathExplodeNotCount>().Level * 1.0 / 2);
			}
			int baseCountShare = enemyCount > 0 ? counts / enemyCount : 0;
			int countRemainder = enemyCount > 0 ? counts % enemyCount : 0;
			int baseNotCountShare = enemyCount > 0 ? notCounts / enemyCount : 0;
			int notCountRemainder = enemyCount > 0 ? notCounts % enemyCount : 0;
			int index = 0;
			foreach (EnemyUnit unit in Battle.AllAliveEnemies)
			{
				int countShare = baseCountShare + (index < countRemainder ? 1 : 0);
				int notCountShare = baseNotCountShare + (index < notCountRemainder ? 1 : 0);
				if (countShare > 0)
				{
					yield return new ApplyStatusEffectAction<DeathExplodeCount>(unit, countShare, 0, 0, Math.Max(1, countDown));
				}
				if (notCountShare > 0)
				{
					yield return new ApplyStatusEffectAction<DeathExplodeNotCount>(unit, notCountShare);
				}
				index++;
			}
		}
	}
}