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
using LBoL.EntityLib.EnemyUnits.Character;
using LBoL.EntityLib.StatusEffects.Enemy;
using LBoL.EntityLib.StatusEffects.Enemy.Seija;
using LBoLEntitySideloader.Attributes;

namespace lvalonmima.StatusEffects
{
	public sealed class seSeijaDef : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Positive;
			config.HasCount = true;
			return config;
		}
	}

	[EntityLogic(typeof(seSeijaDef))]
	public sealed class seSeija : StatusEffect
	{
		int lim;
		int tier;
		public void AddPool()
		{
			if (!(Owner is Seija seija)) return;

			if (tier == 2)
			{
				if (!seija.HasStatusEffect<InfinityGemsSe>())
					seija._pool.Add(typeof(InfinityGemsSe));
				if (!seija.HasStatusEffect<SakuraWandSe>())
					seija._pool.Add(typeof(SakuraWandSe));
			}
			if (tier == 1)
			{
				if (!seija.HasStatusEffect<HolyGrailSe>())
					seija._pool.Add(typeof(HolyGrailSe));
				if (!seija.HasStatusEffect<QiannianShenqiSe>())
					seija._pool.Add(typeof(QiannianShenqiSe));
			}
		}
		public BattleAction RandomBuff()
		{
			if (!(Owner is Seija seija)) return null;

			Type type = seija._pool.Sample(seija.SeijaRng);
			seija._pool.Remove(type);

			if (type == typeof(ShendengSe))
			{
				return new ApplyStatusEffectAction<ShendengSe>(Owner, 3, null, null, null, 1f);
			}

			if (type == typeof(MadokaBowSe))
			{
				return new ApplyStatusEffectAction<MadokaBowSe>(Owner, 2, null, null, null, 1f);
			}

			if (type == typeof(QiannianShenqiSe))
			{
				int? level = 2;
				int? limit = 10;
				return new ApplyStatusEffectAction<QiannianShenqiSe>(Owner, level, null, null, limit, 1f);
			}

			return new ApplyStatusEffectAction(type, Owner, null, null, null, null, 1f);
		}
		public override bool ForceNotShowDownText => true;
		protected override void OnAdded(Unit unit)
		{
			lim = 300;
			tier = 1;
			Count = lim;
			ReactOwnerEvent(unit.DamageReceived, OnDamageReceived);
		}

		private IEnumerable<BattleAction> OnDamageReceived(DamageEventArgs args)
		{
			int totalDamage = args.DamageInfo.Damage.ToInt();

			(int result, int remainder) = totalDamage.DivRem(lim);
			Count -= remainder;
			if (result > 0)
			{
				NotifyActivating();
				for (int i = 0; i < result; i++)
				{
					if (Battle.BattleShouldEnd) { yield break; }
					yield return RandomBuff();
					yield return PerformAction.Animation(Owner, "spell", 1f);
					AddPool();
					tier++;
				}
			}
		}
	}
}