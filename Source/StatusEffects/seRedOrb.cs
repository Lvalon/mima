using System;
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
using LBoL.EntityLib.EnemyUnits.Normal.Yinyangyus;
using LBoL.EntityLib.StatusEffects.Enemy;
using LBoLEntitySideloader.Attributes;

namespace lvalonmima.StatusEffects
{
	public sealed class seRedOrbDef : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Positive;
			config.HasCount = true;
			return config;
		}
	}

	[EntityLogic(typeof(seRedOrbDef))]
	public sealed class seRedOrb : StatusEffect
	{
		public override bool ForceNotShowDownText => true;
		protected override void OnAdded(Unit unit)
		{
			Count = 0;
			foreach (Unit enemy in Battle.AllAliveEnemies.Where(e => e is YinyangyuBlueOrigin))
				HandleOwnerEvent(enemy.Dying, OnBlueDying, GameEventPriority.ConfigDefault + 100);
			HandleOwnerEvent(Battle.EnemySpawned, OnEnemySpawned);
			ReactOwnerEvent(unit.TurnEnded, OnTurnEnded);
		}

		private IEnumerable<BattleAction> OnTurnEnded(UnitEventArgs args)
		{
			if (Count > 0 && Highlight)
			{
				yield return new CastBlockShieldAction(Owner, new ShieldInfo(Count++));
			}
		}

		private void OnEnemySpawned(UnitEventArgs args)
		{
			if (args.Unit is YinyangyuBlueOrigin)
				HandleOwnerEvent(args.Unit.Dying, OnBlueDying, GameEventPriority.ConfigDefault + 100);
		}

		private void OnBlueDying(DieEventArgs args)
		{
			if (args.Unit.HasStatusEffect<AbsorbSpirit>() && args.Unit.TryGetStatusEffect<Spirit>(out var se))
			{
				NotifyActivating();
				Count += se.Level;
				Highlight = true;
			}
		}
	}
}