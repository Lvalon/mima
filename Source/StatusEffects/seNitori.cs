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
	public sealed class seNitoriDef : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Positive;
			config.Order = 10;
			return config;
		}
	}

	[EntityLogic(typeof(seNitoriDef))]
	public sealed class seNitori : StatusEffect
	{
		int damage = 0;
		protected override void OnAdded(Unit unit)
		{
			damage = 0;
			HandleOwnerEvent(unit.DamageTaking, OnDamageTaking);
			foreach (EnemyUnit mf in Battle.AllAliveEnemies.Where(e => e != Owner))
			{
				HandleOwnerEvent(mf.DamageReceiving, OnDamageReceiving);
				ReactOwnerEvent(mf.DamageReceived, OnDamageReceived);
			}
			HandleOwnerEvent(Battle.EnemySpawned, OnSpawned);
		}

		private void OnDamageTaking(DamageEventArgs args)
		{
			int num = args.DamageInfo.Damage.RoundToInt();
			if (num > 0 && toolbox.Round(args.Target.MaxHp * 0.2) < num && Battle.AllAliveEnemies.Count() == 1)
			{
				NotifyActivating();
				args.DamageInfo = args.DamageInfo.ReduceActualDamageBy(num - toolbox.Round(args.Target.MaxHp * 0.2));
				args.AddModifier(this);
			}
		}

		private void OnSpawned(UnitEventArgs args)
		{
			HandleOwnerEvent(args.Unit.DamageReceiving, OnDamageReceiving);
			ReactOwnerEvent(args.Unit.DamageReceived, OnDamageReceived);
		}

		private IEnumerable<BattleAction> OnDamageReceived(DamageEventArgs args)
		{
			if (damage > 0)
			{
				NotifyActivating();
				yield return DamageAction.Reaction(Owner, damage);
			}
			damage = 0;
		}

		private void OnDamageReceiving(DamageEventArgs args)
		{
			if (args.DamageInfo.DamageType != DamageType.Attack || args.Cause == ActionCause.OnlyCalculate) return;

			DamageInfo damageInfo = args.DamageInfo;
			damage += (int)(damageInfo.Damage - damageInfo.Amount * 0.5f);
			damageInfo.Damage = damageInfo.Amount * 0.5f;
			args.DamageInfo = damageInfo;
			args.AddModifier(this);
		}
	}
}