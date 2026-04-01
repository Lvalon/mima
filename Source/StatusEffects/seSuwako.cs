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
	public sealed class seSuwakoDef : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Positive;
			config.HasCount = true;
			return config;
		}
	}

	[EntityLogic(typeof(seSuwakoDef))]
	public sealed class seSuwako : StatusEffect
	{
		public int limit => lim;
		int lim = 5;
		public override bool ForceNotShowDownText => true;
		protected override void OnAdded(Unit unit)
		{
			lim = 4 + Battle.AllAliveEnemies.Count();
			Count = lim;
			ReactOwnerEvent(Battle.CardDrawn, OnCardDrawn);
		}

		private IEnumerable<BattleAction> OnCardDrawn(CardEventArgs args)
		{
			if (args.Cause != ActionCause.TurnStart && !(args.ActionSource is Card card && card.IsReplenish))
			{
				if (Count >= 1)
				{
					Count--;
					if (Count == 0)
					{
						List<Card> cards = Battle.HandZone.Where(c => !(c is Frog)).ToList();
						if (cards.Count > 0)
						{
							NotifyActivating();
							yield return PerformAction.Wait(0.2f, unscale: true);
							NotifyActivating();
							yield return PerformAction.UiSound("Frog");
							Card card2 = cards.Sample(GameRun.EnemyBattleRng);
							if (card2 != null)
							{
								Frog frog = Library.CreateCard<Frog>();
								frog.OriginalCard = card2;
								yield return new TransformCardAction(card2, frog);
							}
						}
						Count = lim;
					}
				}
				Highlight = Count == 1;
			}
		}
	}
}