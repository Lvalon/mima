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
using LBoL.EntityLib.EnemyUnits.Normal.Ravens;
using LBoL.EntityLib.StatusEffects.Enemy;
using LBoLEntitySideloader.Attributes;

namespace lvalonmima.StatusEffects
{
	public sealed class seClownpieceDef : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Positive;
			config.HasCount = true;
			return config;
		}
	}

	[EntityLogic(typeof(seClownpieceDef))]
	public sealed class seClownpiece : StatusEffect
	{
		public override bool ForceNotShowDownText => true;
		protected override void OnAdded(Unit unit)
		{
			Count = Battle.AllAliveEnemies.Count() / 2; // for every 2
			HandleOwnerEvent(Battle.EnemyDied, OnEnemyDied);
			HandleOwnerEvent(Battle.EnemyEscaped, OnEnemyEscaped);
			HandleOwnerEvent(Battle.EnemySpawned, OnSpawned);
			HandleOwnerEvent(Battle.RoundEnded, OnRoundEnded);
		}

		private void OnSpawned(UnitEventArgs args)
		{
			Count = Battle.AllAliveEnemies.Count() / 2;
		}

		private void OnEnemyEscaped(UnitEventArgs args)
		{
			Count = Battle.AllAliveEnemies.Count() / 2;
		}

		private void OnEnemyDied(DieEventArgs args)
		{
			Count = Battle.AllAliveEnemies.Count() / 2;
		}

		private void OnRoundEnded(GameEventArgs args)
		{
			if (Count > 0 && Battle.EnumerateAllCardsButExile().Any(c => c.CardType == CardType.Status && c.IsExile))
			{
				NotifyActivating();
				Card[] cards = Battle.EnumerateAllCardsButExile().Where(c => c.CardType == CardType.Status && c.IsExile).SampleManyOrAll(Count, GameRun.EnemyBattleRng);
				foreach (Card card in cards)
				{
					card.IsExile = false;
				}
			}
		}
	}
}