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
		bool lastTurnGrazed = false;
		protected override void OnAdded(Unit unit)
		{
			lastTurnGrazed = false;
			Highlight = false;
			HandleOwnerEvent(Battle.Player.DamageReceived, OnDamageReceived);
			HandleOwnerEvent(unit.DamageDealing, OnDamageDealing);
			HandleOwnerEvent(Battle.RoundEnded, OnRoundEnded);
		}

		private void OnRoundEnded(GameEventArgs args)
		{
			Highlight = lastTurnGrazed;
			lastTurnGrazed = false;
		}

		private void OnDamageDealing(DamageDealingEventArgs args)
		{
			if (!args.DamageInfo.IsAccuracy && Highlight && args.DamageInfo.DamageType == DamageType.Attack)
			{
				DamageInfo damageInfo = args.DamageInfo;
				damageInfo.IsAccuracy = true;
				args.DamageInfo = damageInfo;
				args.AddModifier(this);
			}
		}

		private void OnDamageReceived(DamageEventArgs args)
		{
			if (args.DamageInfo.IsGrazed)
				lastTurnGrazed = true;
		}
	}
}