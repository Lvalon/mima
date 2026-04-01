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
using LBoL.EntityLib.EnemyUnits.Character;
using LBoL.EntityLib.StatusEffects.Cirno;
using LBoL.EntityLib.StatusEffects.Enemy;
using LBoLEntitySideloader.Attributes;

namespace lvalonmima.StatusEffects
{
	public sealed class seMegumuDef : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Positive;
			return config;
		}
	}

	[EntityLogic(typeof(seMegumuDef))]
	public sealed class seMegumu : StatusEffect
	{
		bool went;
		public override bool ForceNotShowDownText => true;
		protected override void OnAdded(Unit unit)
		{
			went = false;
			Highlight = Owner is Long damn && damn.Next == Long.MoveType.Spell;
			HandleOwnerEvent(unit.TurnStarting, OnTurnStarting);
			HandleOwnerEvent(unit.TurnEnded, OnTurnEnded);
			HandleOwnerEvent(Battle.CardUsed, OnCardUsed);
		}

		private void OnCardUsed(CardUsingEventArgs args)
		{
			if (args.Card is Bribery)
			{
				NotifyActivating();
				foreach (Card card in Battle.EnumerateAllCards().Where(c => c.CardType == CardType.Status))
				{
					card.IsEthereal = true;
				}
			}
		}

		private void OnTurnStarting(UnitEventArgs args)
		{
			went = Owner is Long damn && damn.Next == Long.MoveType.Spell;
			Highlight = Owner is Long damn2 && damn2.Next == Long.MoveType.Spell;
		}

		private void OnTurnEnded(UnitEventArgs args)
		{
			if (went)
			{
				bool notified = false;
				foreach (Card card in Battle.EnumerateAllCards().Where(c => c.CardType == CardType.Status))
				{
					if (!notified)
					{
						NotifyActivating();
						notified = true;
					}
					card.IsExile = false;
					card.IsEthereal = false;
				}
			}
			Highlight = Owner is Long damn && damn.Next == Long.MoveType.Spell;
		}
	}
}