using System;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace lvalonmima.StatusEffects;

// Shared abstract bases for the mod's status-effect logic classes. None of these
// are entity logic classes themselves (no [EntityLogic] attribute, so no Id impact);
// they only exist to be subclassed.
public abstract class mimaSe : StatusEffect
{
	public override bool ForceNotShowDownText => true;
}

// For the se{attack,skill,defense,ability,tool,status,misfortune,friend}played
// family, which differ only in which CardType they watch for. Derives directly
// from StatusEffect (not mimaSe): none of the 8 override ForceNotShowDownText
// today, so routing them through mimaSe would silently flip that to true.
public abstract class CardTypeCounterSe : StatusEffect
{
	protected abstract CardType WatchedType { get; }

	protected override void OnAdded(Unit unit)
	{
		Count = 0;
		HandleOwnerEvent(Battle.CardUsed, OnCardUsed);
	}

	protected virtual void OnCardUsed(CardUsingEventArgs args)
	{
		if (args.Card.CardType == WatchedType)
		{
			Count++;
			Highlight = Count == Level;
		}
	}
}

// Wraps the CardsAddedToDiscard/Hand/Exile/DrawZone + CardTransformed registration
// block. Only seRaven3 actually matches this exact shape (same events, same
// priority, same per-card callback, CardTransformed passed as the single
// destination card). sedawntime, semimaexb, seSakuya, secreative, seMarisa and
// sequest28 look similar at a glance but each differs in which events they hook,
// in priority, or in filtering applied before the callback — forcing them onto
// this base would either drop behavior or require an over-parameterized helper,
// so they stay as their own bespoke implementations.
public abstract class ZoneWatcherSe : StatusEffect
{
	protected void WatchCardsAdded(Action<Card[]> onAdded)
	{
		HandleOwnerEvent(Battle.CardsAddedToDiscard, args => onAdded(args.Cards));
		HandleOwnerEvent(Battle.CardsAddedToHand, args => onAdded(args.Cards));
		HandleOwnerEvent(Battle.CardsAddedToExile, args => onAdded(args.Cards));
		HandleOwnerEvent(Battle.CardsAddedToDrawZone, args => onAdded(args.Cards));
		HandleOwnerEvent(Battle.CardTransformed, args => onAdded(new[] { args.DestinationCard }));
	}
}
