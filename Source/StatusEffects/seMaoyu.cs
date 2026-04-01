using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Enemy;
using LBoLEntitySideloader.Attributes;
using LBoLEntitySideloader.CustomKeywords;
using lvalonmima.Cards;
using lvalonmima.Cards.Template;
using lvalonmima.JadeBoxes;

namespace lvalonmima.StatusEffects
{
	public sealed class seMaoyuDef : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Positive;
			return config;
		}
	}

	[EntityLogic(typeof(seMaoyuDef))]
	public sealed class seMaoyu : StatusEffect
	{
		public override bool ForceNotShowDownText => true;
		bool has = true;
		protected override void OnAdded(Unit unit)
		{
			has = true;
			HandleOwnerEvent(unit.DamageTaking, OnDamageTaking);
			ReactOwnerEvent(unit.StatisticalTotalDamageReceived, OnDamageReceived, GameEventPriority.ConfigDefault - 1); // quicker than maoyublock?
		}

		private void OnDamageTaking(DamageEventArgs args)
		{
			has = Owner.HasStatusEffect<MaoyuBlock>() && args.DamageInfo.Damage > 0;
		}

		private IEnumerable<BattleAction> OnDamageReceived(StatisticalDamageEventArgs args)
		{
			if (Owner.IsNotAlive)
				yield break;
			if (args.ArgsTable.Any(
				kv => kv.Value.Any(ev => ev.DamageInfo.DamageType == DamageType.Attack && ev.DamageInfo.Amount > 0)))
				NotifyActivating();
			foreach (var (_, readOnlyList2) in args.ArgsTable)
			{
				foreach (DamageEventArgs item in readOnlyList2)
				{
					DamageInfo damageInfo = item.DamageInfo;
					if (damageInfo.DamageType == DamageType.Attack && damageInfo.Damage > 0 && !Owner.HasStatusEffect<MaoyuBlock>())
					{
						NotifyActivating();
						yield return new ApplyStatusEffectAction<MaoyuBlock>(Owner, (int?)damageInfo.Damage);
						yield break;
					}
				}
			}
			has = Owner.HasStatusEffect<MaoyuBlock>();
		}
	}
}