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
using LBoL.EntityLib.StatusEffects.Cirno;
using LBoL.EntityLib.StatusEffects.Enemy;
using LBoLEntitySideloader.Attributes;

namespace lvalonmima.StatusEffects
{
	public sealed class sesbKoishiDef : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Positive;
			return config;
		}
	}

	[EntityLogic(typeof(sesbKoishiDef))]
	public sealed class sesbKoishi : StatusEffect
	{
		Card tmp;
		public override bool ForceNotShowDownText => true;
		protected override void OnAdded(Unit unit)
		{
			tmp = null;
			ReactOwnerEvent(unit.TurnStarted, OnTurnStarted);
			// ReactOwnerEvent(unit.DamageReceived, OnDamageReceived);
		}

		private IEnumerable<BattleAction> OnTurnStarted(UnitEventArgs args)
		{
			bool notified = false;
			tmp = Battle.ExileZone.Where(c => c.CardType != CardType.Status && c.CardType != CardType.Misfortune).SampleOrDefault(GameRun.EnemyBattleRng);
			if (tmp != null)
			{
				if (!notified)
				{
					NotifyActivating();
					notified = true;
				}
				yield return new MoveCardToDrawZoneAction(tmp, DrawZoneTarget.Random);
			}
			tmp = tmp == null ?
			Battle.EnumerateAllCardsButExile().Where(c => c.CardType != CardType.Status && c.CardType != CardType.Misfortune).SampleOrDefault(GameRun.EnemyBattleRng) :
			Battle.EnumerateAllCardsButExile().Where(c => c.CardType != CardType.Status && c.CardType != CardType.Misfortune && tmp != c).SampleOrDefault(GameRun.EnemyBattleRng);
			if (tmp != null)
			{
				if (!notified)
				{
					NotifyActivating();
					notified = true;
				}
				yield return new ExileCardAction(tmp);
			}
		}

		// private IEnumerable<BattleAction> OnDamageReceived(DamageEventArgs args)
		// {
		// 	yield return new ApplyStatusEffectAction<LoveGirlDamageIncrease>(Owner, 1); // <3
		// 	yield return new ApplyStatusEffectAction<Burst>(Owner, 0);
		// }
	}
}