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
using LBoLEntitySideloader.Attributes;

namespace lvalonmima.StatusEffects
{
	public sealed class seextratickDef : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Special;
			config.HasCount = true;
			config.CountStackType = StackType.Add;
			config.Keywords = Keyword.NaturalTurn | Keyword.FollowAttack | Keyword.Purified;
			config.RelativeEffects = new List<string>() { nameof(ExtraTurn), nameof(seunder) };
			return config;
		}
	}

	[EntityLogic(typeof(seextratickDef))]
	public sealed class seextratick : sehl50
	{
		bool activated = false;
		bool activating = false;
		protected override void OnAdded(Unit unit)
		{
			activating = false;
			activated = false;
			// HandleOwnerEvent(base.Battle.Player.TurnStarting, delegate
			// {
			// 	//if ((base.Battle.Player.IsExtraTurn || base.Battle.Player.IsSuperExtraTurn) && base.Battle.Player.GetStatusEffectExtend<ExtraTurnPartner>() == this)
			// 	if (!activated && base.Battle.Player.IsExtraTurn && !base.Battle.Player.IsSuperExtraTurn)
			// 	{
			// 		activating = true;
			// 	}
			// });
			ReactOwnerEvent(Battle.Player.TurnStarted, OnTurnStarted);
			HandleOwnerEvent(Battle.Player.TurnEnding, OnPlayerTurnEnding, GameEventPriority.Lowest - 10);
			HandleOwnerEvent(Battle.RoundEnded, OnRoundEnded);
		}

		private void OnRoundEnded(GameEventArgs args)
		{
			activated = false;
			activating = false;
		}

		private IEnumerable<BattleAction> OnTurnStarted(UnitEventArgs args)
		{
			if (activating && Battle.AllAliveEnemies.Count() > 0)
			{
				yield return new FollowAttackAction(UnitSelector.RandomEnemy, Count + (BepinexPlugin.u50 ? Level : 0));

				int togo = Level;
				List<Card> list = base.Battle.HandZone.Where((Card card4) => !card4.IsPurified && card4.Cost.HasTrivialOrHybrid && !card4.IsXCost).ToList();
				if (list.Count > 0)
				{
					List<Card> card2 = list.SampleManyOrAll(Level, base.GameRun.BattleRng).ToList();
					foreach (Card card in card2)
					{
						if (!card.IsXCost && Battle.AllAliveEnemies.Count() > 0)
						{
							card.NotifyActivating();
							card.IsPurified = true;
						}
						togo--;
					}
					if (togo == 0)
					{
						activated = true;
						yield return new RequestEndPlayerTurnAction();
						yield break;
					}
				}
				List<Card> list2 = base.Battle.HandZone.Where((Card card4) => !card4.IsPurified).ToList();
				if (list2.Count > 0 && Battle.AllAliveEnemies.Count() > 0)
				{
					List<Card> card3 = list2.SampleManyOrAll(togo, base.GameRun.BattleRng).ToList();
					foreach (Card card in card3.Where(c => !c.IsXCost))
					{
						card.NotifyActivating();
						card.IsPurified = true;
					}
				}
				activated = true;
				if (Battle.BattleShouldEnd) { yield break; }
				yield return new RequestEndPlayerTurnAction();
			}
		}

		public void OnPlayerTurnEnding(UnitEventArgs args)
		{
			//if (!Battle.Player.IsExtraTurn && !Battle.Player.IsSuperExtraTurn && !Battle.Player.HasStatusEffect<ExtraTurn>() && !activated)
			if (!Battle.Player.HasStatusEffect<ExtraTurn>() && !activated)
			{
				if (Battle.Player.TryGetStatusEffect(out SuperExtraTurn se))
				{
					if (se.Status != TurnStatus.NaturalTurn)
					{
						React(BuffAction<ExtraTurn>(1));
						activating = true;
					}
				}
				else
				{
					React(BuffAction<ExtraTurn>(1));
					activating = true;
				}
			}
		}
	}
}