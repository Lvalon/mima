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
using LBoL.EntityLib.Cards.Enemy;
using LBoLEntitySideloader.Attributes;

namespace lvalonmima.StatusEffects
{
	public sealed class seSakuyaDef : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Positive;
			config.Order = 9;
			config.HasCount = true;
			return config;
		}
	}

	[EntityLogic(typeof(seSakuyaDef))]
	public sealed class seSakuya : StatusEffect
	{
		public override bool ForceNotShowDownText => true;
		protected override void OnAdded(Unit unit)
		{
			Count = Battle.EnumerateAllCardsButExile().Count(c => c is SakuyaLock) * 10;
			HandleOwnerEvent(unit.DamageReceiving, OnDamageReceiving);
			HandleOwnerEvent(unit.DamageDealing, OnDamageDealing);

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

		private void OnAddedDraw(CardsAddingToDrawZoneEventArgs args)
		{
			Count = Battle.EnumerateAllCardsButExile().Count(c => c is SakuyaLock) * 10;
		}

		private void OnAdded(CardsEventArgs args)
		{
			Count = Battle.EnumerateAllCardsButExile().Count(c => c is SakuyaLock) * 10;
		}

		private void OnUsed(CardMovingEventArgs args)
		{
			Count = Battle.EnumerateAllCardsButExile().Count(c => c is SakuyaLock) * 10;
		}

		private void OnUsed(CardEventArgs args)
		{
			Count = Battle.EnumerateAllCardsButExile().Count(c => c is SakuyaLock) * 10;
		}

		private void OnUsed(CardUsingEventArgs args)
		{
			Count = Battle.EnumerateAllCardsButExile().Count(c => c is SakuyaLock) * 10;
		}

		private void OnDamageDealing(DamageDealingEventArgs args)
		{
			if (Count <= 0) return;
			DamageInfo damageInfo = args.DamageInfo;
			damageInfo.Damage = damageInfo.Amount * (100f + Count) / 100f;
			args.DamageInfo = damageInfo;
			args.AddModifier(this);
		}

		private void OnDamageReceiving(DamageEventArgs args)
		{
			if (Count <= 0) return;
			DamageInfo damageInfo = args.DamageInfo;
			damageInfo.Damage = damageInfo.Amount * Math.Max((100f - Count) / 100f, 0);
			args.DamageInfo = damageInfo;
			args.AddModifier(this);
		}
	}
}