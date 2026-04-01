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
using LBoL.EntityLib.EnemyUnits.Character;
using LBoL.EntityLib.StatusEffects.Cirno;
using LBoL.EntityLib.StatusEffects.Enemy;
using LBoLEntitySideloader.Attributes;

namespace lvalonmima.StatusEffects
{
	public sealed class seTenshiDef : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Positive;
			return config;
		}
	}

	[EntityLogic(typeof(seTenshiDef))]
	public sealed class seTenshi : StatusEffect
	{
		public override bool ForceNotShowDownText => true;
		protected override void OnAdded(Unit unit)
		{
			HandleOwnerEvent(unit.DamageReceiving, OnDamageReceiving);
			HandleOwnerEvent(unit.TurnEnded, OnTurnEnded);
		}

		private void OnTurnEnded(UnitEventArgs args)
		{
			Highlight = Owner is Tianzi self && self.Next == Tianzi.MoveType.SpellAttack;
		}

		private void OnDamageReceiving(DamageEventArgs args)
		{
			if (!(Owner is Tianzi self)) return;
			if (self.Next != Tianzi.MoveType.SpellAttack) return;
			DamageInfo damageInfo = args.DamageInfo;
			damageInfo.Damage = damageInfo.Amount * 0.5f;
			args.DamageInfo = damageInfo;
			args.AddModifier(this);
		}
	}
}