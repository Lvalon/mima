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
using LBoL.EntityLib.Cards.Enemy;
using LBoL.EntityLib.EnemyUnits.Normal.Bats;
using LBoL.EntityLib.EnemyUnits.Normal.Ravens;
using LBoL.EntityLib.StatusEffects.Enemy;
using LBoLEntitySideloader.Attributes;

namespace lvalonmima.StatusEffects
{
	public sealed class seBatLordDef : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Positive;
			config.HasCount = true;
			return config;
		}
	}

	[EntityLogic(typeof(seBatLordDef))]
	public sealed class seBatLord : StatusEffect
	{
		public override bool ForceNotShowDownText => true;
		protected override void OnAdded(Unit unit)
		{
			Count = 1;
			ReactOwnerEvent(Battle.CardDrawn, OnCardDrawn);
			HandleOwnerEvent(Battle.Player.TurnEnded, OnTurnEnded);
		}

		private void OnTurnEnded(UnitEventArgs args)
		{
			Count = 1;
		}

		private IEnumerable<BattleAction> OnCardDrawn(CardEventArgs args)
		{
			if (args.Cause != ActionCause.TurnStart && !(args.ActionSource is Card card && card.IsReplenish))
			{
				NotifyActivating();
				yield return new HealAction(Owner, Battle.AllAliveEnemies.Where(e => e is BatOrigin).MaxBy(u => u.MaxHp - u.Hp), Count++);
			}
		}
	}
}