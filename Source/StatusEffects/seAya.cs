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
using LBoLEntitySideloader.Attributes;

namespace lvalonmima.StatusEffects
{
	public sealed class seAyaDef : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Positive;
			return config;
		}
	}

	[EntityLogic(typeof(seAyaDef))]
	public sealed class seAya : StatusEffect
	{
		public override bool ForceNotShowDownText => true;
		protected override void OnAdded(Unit unit)
		{
			ReactOwnerEvent(Battle.Player.TurnEnded, OnTurnEnded);
		}
		private IEnumerable<BattleAction> OnTurnEnded(UnitEventArgs args)
		{
			bool hasShield = false;
			if (Owner.Shield > 0)
			{
				hasShield = true;
				NotifyActivating();
				yield return new LoseBlockShieldAction(Owner, 0, Owner.Shield);
			}
			if (Battle.DiscardZone.Count(c => c.CardType == CardType.Status) > 0)
			{
				if (!hasShield) NotifyActivating();
				yield return new CastBlockShieldAction(Owner, new ShieldInfo(Battle.DiscardZone.Count(c => c.CardType == CardType.Status), BlockShieldType.Direct));
			}
		}
	}
}