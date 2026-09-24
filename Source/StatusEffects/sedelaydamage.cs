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
using LBoLEntitySideloader.Attributes;
using lvalonmima.Cards;
using lvalonmima.Exhibits;
using lvalonmima.GunName;

namespace lvalonmima.StatusEffects
{
	public sealed class sedelaydamageDef : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Special;
			return config;
		}
	}

	[EntityLogic(typeof(sedelaydamageDef))]
	public sealed class sedelaydamage : StatusEffect
	{
		protected override void OnAdded(Unit unit)
		{
			// like vanilla Poison/Cold: enemies resolve at AllEnemyTurnStarted so they never die inside their own StartEnemyTurnAction
			if (unit is EnemyUnit)
				ReactOwnerEvent(Battle.AllEnemyTurnStarted, TakeEffect, GameEventPriority.ConfigDefault + 1); // slower than holddamage
			else
				ReactOwnerEvent(unit.TurnStarted, TakeEffect, GameEventPriority.ConfigDefault + 1);
		}

		private IEnumerable<BattleAction> TakeEffect(GameEventArgs args)
		{
			if (Owner == null || Owner.IsDead || Battle.BattleShouldEnd)
				yield break;
			int gunid = 15160;
			int[] thresholds = { 0, 10, 25, 50, 100 };
			gunid += thresholds.Count(t => Level > toolbox.hpfrompercent(Owner, t));
			if (Level > 0)
			{
				NotifyActivating();
				// hp loss: enemy block isn't cleared yet at AllEnemyTurnStarted
				yield return DamageAction.LoseLife(Owner, Level, GunNameID.GetGunFromId(gunid));
			}
			if (Owner != null)
				yield return new RemoveStatusEffectAction(this);
		}
	}
}