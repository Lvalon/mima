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
using LBoL.EntityLib.StatusEffects.Others;
using LBoLEntitySideloader.Attributes;

namespace lvalonmima.StatusEffects
{
	public sealed class seDollDef : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Positive;
			return config;
		}
	}

	[EntityLogic(typeof(seDollDef))]
	public sealed class seDoll : StatusEffect
	{
		public override bool ForceNotShowDownText => true;
		protected override void OnAdded(Unit unit)
		{
			ReactOwnerEvent(unit.DamageDealt, OnDamageDealt);
		}

		private IEnumerable<BattleAction> OnDamageDealt(DamageEventArgs args)
		{
			if (args.DamageInfo.Damage <= 0)
				yield break;
			NotifyActivating();
			yield return new ApplyStatusEffectAction<Poison>(Battle.Player, 1);
			yield return new ApplyStatusEffectAction<Vulnerable>(Battle.Player, 0, 1);
		}
	}
}