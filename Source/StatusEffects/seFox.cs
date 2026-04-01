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
	public sealed class seFoxDef : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Positive;
			return config;
		}
	}

	[EntityLogic(typeof(seFoxDef))]
	public sealed class seFox : StatusEffect
	{
		public override bool ForceNotShowDownText => true;
		protected override void OnAdded(Unit unit)
		{
			Highlight = true;
			ReactOwnerEvent(unit.TurnEnded, OnTurnEnded);
			HandleOwnerEvent(unit.DamageTaking, OnDamageTaking);
		}

		private IEnumerable<BattleAction> OnTurnEnded(UnitEventArgs args)
		{
			if (Highlight)
			{
				NotifyActivating();
				yield return new ApplyStatusEffectAction<Firepower>(Owner, 1);
			}
			Highlight = true;
		}

		private void OnDamageTaking(DamageEventArgs args)
		{
			int num = args.DamageInfo.Damage.RoundToInt();
			if (num > 0)
			{
				Highlight = false;
			}
		}
	}
}