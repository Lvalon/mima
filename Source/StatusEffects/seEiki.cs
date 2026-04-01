using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Others;
using LBoLEntitySideloader.Attributes;

namespace lvalonmima.StatusEffects
{
	public sealed class seEikiDef : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Positive;
			config.Order = 10;
			return config;
		}
	}

	[EntityLogic(typeof(seEikiDef))]
	public sealed class seEiki : StatusEffect
	{
		public override bool ForceNotShowDownText => true;
		protected override void OnAdded(Unit unit)
		{
			foreach (EnemyUnit mf in Battle.AllAliveEnemies)
			{
				HandleOwnerEvent(mf.DamageReceiving, OnDamageReceiving);
			}
			HandleOwnerEvent(Battle.EnemySpawned, OnSpawned);
		}

		private void OnSpawned(UnitEventArgs args)
		{
			HandleOwnerEvent(args.Unit.DamageReceiving, OnDamageReceiving);
		}

		private void OnDamageReceiving(DamageEventArgs args)
		{
			if (!args.Target.HasStatusEffect<MirrorImage>() || Owner.IsNotAlive) return;
			DamageInfo damageInfo = args.DamageInfo;
			damageInfo.Damage = damageInfo.Amount * 0.5f;
			args.DamageInfo = damageInfo;
			args.AddModifier(this);
		}
	}
}