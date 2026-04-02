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
	public sealed class seBigCrowDef : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Positive;
			return config;
		}
	}

	[EntityLogic(typeof(seBigCrowDef))]
	public sealed class seBigCrow : StatusEffect
	{
		public override bool ForceNotShowDownText => true;
		protected override void OnAdded(Unit unit)
		{
			ReactOwnerEvent(unit.DamageReceived, OnDamageReceived);
		}

		private IEnumerable<BattleAction> OnDamageReceived(DamageEventArgs args)
		{
			if (args.DamageInfo.Damage > 0)
			{
				NotifyActivating();
				yield return new DamageAction(Owner, new List<Unit> { Battle.Player }, new DamageInfo(toolbox.Round(args.DamageInfo.Damage * 0.5), DamageType.Attack), "狼天狗双刀");
			}
		}
	}
}