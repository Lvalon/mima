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
using LBoLEntitySideloader.Attributes;

namespace lvalonmima.StatusEffects
{
	public sealed class seSPPurpleDef : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Positive;
			return config;
		}
	}

	[EntityLogic(typeof(seSPPurpleDef))]
	public sealed class seSPPurple : StatusEffect
	{
		public override bool ForceNotShowDownText => true;
		protected override void OnAdded(Unit unit)
		{
			ReactOwnerEvent(Battle.CardDrawn, OnCardDrawn);
		}

		private IEnumerable<BattleAction> OnCardDrawn(CardEventArgs args)
		{
			if (args.Cause != ActionCause.TurnStart && !(args.ActionSource is Card card && card.IsReplenish))
			{
				NotifyActivating();
				yield return DamageAction.LoseLife(Battle.Player, 1);
			}
		}
	}
}