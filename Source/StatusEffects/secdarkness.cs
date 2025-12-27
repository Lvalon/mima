using System.Collections.Generic;
using System.Linq;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoLEntitySideloader.Attributes;

namespace lvalonmima.StatusEffects
{
	public sealed class secdarknessDef : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Special;
			return config;
		}
	}

	[EntityLogic(typeof(secdarknessDef))]
	public sealed class secdarkness : StatusEffect
	{
		bool aoe = false;
		protected override void OnAdded(Unit unit)
		{
			aoe = false;
			ReactOwnerEvent(Battle.RoundEnded, OnRoundEnded);
			HandleOwnerEvent(Battle.EnemySpawned, OnEnemySpawned);
			HandleOwnerEvent(Battle.Player.DamageDealing, OnDamageDealing);
			HandleOwnerEvent(Battle.Player.DamageGiving, OnDamageGiving, GameEventPriority.Lowest - 10);
			HandleOwnerEvent(Battle.Player.StatisticalTotalDamageDealt, OnStatisticalTotalDamageDealt);
			foreach (EnemyUnit allAliveEnemy in Battle.AllAliveEnemies)
			{
				HandleOwnerEvent(allAliveEnemy.DamageGiving, OnDamageGiving, GameEventPriority.Lowest - 10);
			}
		}

		private void OnStatisticalTotalDamageDealt(StatisticalDamageEventArgs args)
		{
			aoe = false;
		}

		private void OnDamageDealing(DamageDealingEventArgs args)
		{
			if (args.Cause != ActionCause.OnlyCalculate && args.DamageInfo.DamageType == DamageType.Attack)
			{
				aoe = args.Targets.Count() > 1;
			}
		}

		private void OnEnemySpawned(UnitEventArgs args)
		{
			HandleOwnerEvent(args.Unit.DamageGiving, OnDamageGiving, GameEventPriority.Lowest - 10);
		}

		private void OnDamageGiving(DamageEventArgs args)
		{
			if (args.Cause != ActionCause.OnlyCalculate
			&& args.DamageInfo.DamageType == DamageType.Attack
			&& !aoe
			&& args.DamageInfo.Damage > 0)
			{
				NotifyActivating();
				args.Target = Battle.AllAliveUnits.ToList().Sample(GameRun.BattleRng);
				args.AddModifier(this);
			}
		}
		private IEnumerable<BattleAction> OnRoundEnded(GameEventArgs args)
		{
			if (Level <= 1)
			{
				yield return new RemoveStatusEffectAction(this);
			}
			else
			{
				Level--;
			}
		}
	}
}