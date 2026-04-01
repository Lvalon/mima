using System;
using System.Collections.Generic;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Cirno;
using LBoLEntitySideloader.Attributes;

namespace lvalonmima.StatusEffects
{
	public sealed class seCirnoDef : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Positive;
			config.HasCount = true;
			return config;
		}
	}

	[EntityLogic(typeof(seCirnoDef))]
	public sealed class seCirno : StatusEffect
	{
		int lim = 99;
		public override bool ForceNotShowDownText => true;
		protected override void OnAdded(Unit unit)
		{
			lim = 99;
			Count = lim;
			ReactOwnerEvent(unit.DamageReceived, OnDamageReceived);
		}

		private IEnumerable<BattleAction> OnDamageReceived(DamageEventArgs args)
		{
			Count = Math.Max(0, Count - args.DamageInfo.Damage.ToInt());
			if (Count == 0)
			{
				NotifyActivating();
				yield return new ApplyStatusEffectAction<Immune>(Owner, 0, 1);
				Count = lim;
			}
		}
	}
}