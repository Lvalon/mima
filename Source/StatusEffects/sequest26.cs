using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoLEntitySideloader.Attributes;

namespace lvalonmima.StatusEffects
{
	public sealed class sequest26Def : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Positive;
			return config;
		}
	}

	[EntityLogic(typeof(sequest26Def))]
	public sealed class sequest26 : StatusEffect
	{
		public override bool ForceNotShowDownText => true;
		protected override void OnAdded(Unit unit)
		{
			Highlight = true;
			HandleOwnerEvent(unit.TurnEnding, OnTurnEnding);
			ReactOwnerEvent(unit.TurnStarted, OnTurnStarted);
			HandleOwnerEvent(unit.DamageTaking, OnDamageTaking, GameEventPriority.Lowest - 9);
		}

		private void OnTurnEnding(UnitEventArgs args)
		{
			if (Battle.HandZone.Count > 0)
				Highlight = false;
		}

		private void OnDamageTaking(DamageEventArgs args)
		{
			int num = args.DamageInfo.Damage.RoundToInt();
			if (num > 0)
			{
				NotifyActivating();
				args.DamageInfo = args.DamageInfo.ReduceActualDamageBy(num);
				args.AddModifier(this);
			}
		}

		private IEnumerable<BattleAction> OnTurnStarted(UnitEventArgs args)
		{
			if (!Highlight)
			{
				NotifyActivating();
				yield return new RemoveStatusEffectAction(this);
			}
		}
	}
}