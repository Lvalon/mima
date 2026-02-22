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
			ReactOwnerEvent(unit.TurnStarted, OnTurnStarted, GameEventPriority.ConfigDefault + 1); // slower than holddamage
		}

		private IEnumerable<BattleAction> OnTurnStarted(GameEventArgs args)
		{
			int gunid = 15160;
			int[] thresholds = { 0, 10, 25, 50, 100 };
			gunid += thresholds.Count(t => Level > toolbox.hpfrompercent(Owner, t));
			if (Level > 0)
			{
				NotifyActivating();
				yield return DamageAction.Reaction(Owner, Level, GunNameID.GetGunFromId(gunid));
			}
			yield return new RemoveStatusEffectAction(this);
		}
	}
}