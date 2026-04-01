using System;
using System.Collections.Generic;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.Cards.Enemy;
using LBoLEntitySideloader.Attributes;

namespace lvalonmima.StatusEffects
{
	public sealed class seLoveGirlDef : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Positive;
			config.Order = 9;
			config.HasCount = true;
			return config;
		}
	}

	[EntityLogic(typeof(seLoveGirlDef))]
	public sealed class seLoveGirl : StatusEffect
	{
		public override bool ForceNotShowDownText => true;
		protected override void OnAdded(Unit unit)
		{
			Count = 0;
			HandleOwnerEvent(unit.DamageReceiving, OnDamageReceiving);
			HandleOwnerEvent(unit.DamageDealing, OnDamageDealing);
			HandleOwnerEvent(Battle.CardUsed, OnCardUsed);
		}

		private void OnCardUsed(CardUsingEventArgs args)
		{
			if (args.Card is LoveLetter)
			{
				NotifyActivating();
				Count += 10;
			}
		}

		private void OnDamageDealing(DamageDealingEventArgs args)
		{
			if (Count <= 0) return;
			DamageInfo damageInfo = args.DamageInfo;
			damageInfo.Damage = damageInfo.Amount * (100f + Count) / 100f;
			args.DamageInfo = damageInfo;
			args.AddModifier(this);
		}

		private void OnDamageReceiving(DamageEventArgs args)
		{
			if (Count <= 0) return;
			DamageInfo damageInfo = args.DamageInfo;
			damageInfo.Damage = damageInfo.Amount * (100f + Count) / 100f;
			args.DamageInfo = damageInfo;
			args.AddModifier(this);
		}
	}
}