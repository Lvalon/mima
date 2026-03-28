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
using LBoLEntitySideloader.Attributes;

namespace lvalonmima.StatusEffects
{
	public sealed class seplayleftDef : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Special;
			return config;
		}
	}

	[EntityLogic(typeof(seplayleftDef))]
	public sealed class seplayleft : StatusEffect
	{
		public override bool ForceNotShowDownText => true;
		public override string PreventCardUsageMessage
		{
			get
			{
				return TypeFactory<StatusEffect>.LocalizeProperty(Id, "seerror", true, true).RuntimeFormat(FormatWrapper);
			}
		}
		protected override void OnAdded(Unit unit)
		{
			ReactOwnerEvent(Battle.CardUsed, OnCardUsed);
		}

		private IEnumerable<BattleAction> OnCardUsed(CardUsingEventArgs args)
		{
			yield return new RemoveStatusEffectAction(this);
		}

		public override bool ShouldPreventCardUsage(Card card)
		{
			return card != Battle.HandZone.FirstOrDefault(CanPlay);
		}
		bool CanPlay(Card card)
		{
			bool rez = Battle.IsWaitingPlayerInput
			&& !Battle.DoesHandCardPreventUse(card, out var _)
			&& !card.IsForbidden
			&& card.CanUse
			&& ((card.Config.MoneyCost.HasValue ? card.Config.MoneyCost : 0) <= Battle.GameRun.Money);
			foreach (StatusEffect statusEffect in Battle.Player.StatusEffects.Where(se => se.Id != nameof(seplayleft)))
			{
				if (statusEffect.ShouldPreventCardUsage(card))
				{
					return false;
				}
			}
			return rez;
		}
	}
}