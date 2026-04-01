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
using LBoL.EntityLib.Cards.Enemy;
using LBoL.EntityLib.EnemyUnits.Normal.Ravens;
using LBoL.EntityLib.StatusEffects.Enemy;
using LBoLEntitySideloader.Attributes;

namespace lvalonmima.StatusEffects
{
	public sealed class seBatDef : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Positive;
			return config;
		}
	}

	[EntityLogic(typeof(seBatDef))]
	public sealed class seBat : StatusEffect
	{
		public override bool ForceNotShowDownText => true;
		protected override void OnAdded(Unit unit)
		{
			ReactOwnerEvent(Battle.CardDrawn, OnCardDrawn);
		}

		private IEnumerable<BattleAction> OnCardDrawn(CardEventArgs args)
		{
			if (!Owner.HasStatusEffect<Graze>() && args.Cause != ActionCause.TurnStart && !(args.ActionSource is Card card && card.IsReplenish))
			{
				NotifyActivating();
				yield return new ApplyStatusEffectAction<Graze>(Owner, 1);
			}
		}
	}
}