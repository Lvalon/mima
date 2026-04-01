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
using LBoL.EntityLib.StatusEffects.Enemy;
using LBoLEntitySideloader.Attributes;

namespace lvalonmima.StatusEffects
{
	public sealed class seSickGirlDef : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Positive;
			return config;
		}
	}

	[EntityLogic(typeof(seSickGirlDef))]
	public sealed class seSickGirl : StatusEffect
	{
		public override bool ForceNotShowDownText => true;
		protected override void OnAdded(Unit unit)
		{
			if (Owner is EnemyUnit owner && owner.Intentions.Any(i => i.Type == IntentionType.Attack))
				Highlight = true;
			HandleOwnerEvent(Battle.RoundStarted, OnRoundStarted);
			ReactOwnerEvent(unit.DamageDealt, OnDamageDealt);
		}

		private void OnRoundStarted(GameEventArgs args)
		{
			Highlight = Owner is EnemyUnit owner && owner.Intentions.Any(i => i.Type == IntentionType.Attack);
		}

		private IEnumerable<BattleAction> OnDamageDealt(DamageEventArgs args)
		{
			if (args.DamageInfo.Damage <= 0)
				yield break;
			NotifyActivating();
			yield return new ApplyStatusEffectAction<Weak>(Battle.Player, 0, 2);
		}
	}
}