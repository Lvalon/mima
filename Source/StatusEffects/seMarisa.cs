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
using LBoL.EntityLib.EnemyUnits.Normal.Yinyangyus;
using LBoLEntitySideloader.Attributes;

namespace lvalonmima.StatusEffects
{
	public sealed class seMarisaDef : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Positive;
			config.HasCount = true;
			return config;
		}
	}

	[EntityLogic(typeof(seMarisaDef))]
	public sealed class seMarisa : StatusEffect
	{
		public override bool ForceNotShowDownText => true;
		protected override void OnAdded(Unit unit)
		{
			Count = GetDistinctStatusCardCount();
			HandleOwnerEvent(unit.DamageDealing, OnDealing);

			HandleOwnerEvent(Battle.CardUsed, OnUsed);
			HandleOwnerEvent(Battle.CardExiled, OnUsed);
			HandleOwnerEvent(Battle.CardMoved, OnUsed);
			HandleOwnerEvent(Battle.CardPlayed, OnUsed);
			HandleOwnerEvent(Battle.CardRemoved, OnUsed);
			HandleOwnerEvent(Battle.CardsAddedToDiscard, OnAdded);
			HandleOwnerEvent(Battle.CardsAddedToDrawZone, OnAddedDraw);
			HandleOwnerEvent(Battle.CardsAddedToExile, OnAdded);
			HandleOwnerEvent(Battle.CardsAddedToHand, OnAdded);
		}

		private void OnUsed(CardMovingEventArgs args)
		{
			Count = GetDistinctStatusCardCount();
		}

		private void OnAddedDraw(CardsAddingToDrawZoneEventArgs args)
		{
			Count = GetDistinctStatusCardCount();
		}

		private void OnAdded(CardsEventArgs args)
		{
			Count = GetDistinctStatusCardCount();
		}

		private void OnUsed(CardEventArgs args)
		{
			Count = GetDistinctStatusCardCount();
		}

		private void OnUsed(CardUsingEventArgs args)
		{
			Count = GetDistinctStatusCardCount();
		}

		private void OnDealing(DamageDealingEventArgs args)
		{
			if (args.DamageInfo.DamageType != DamageType.Attack) return;
			args.DamageInfo = args.DamageInfo.IncreaseBy(GetDistinctStatusCardCount());
			args.AddModifier(this);
		}

		private int GetDistinctStatusCardCount()
		{
			return Battle.EnumerateAllCardsButExile()
				.Where(c => c.CardType == CardType.Status)
				.GroupBy(c => c.Id)
				.Count();
		}
	}
}