using System.Linq;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoLEntitySideloader.Attributes;

namespace lvalonmima.StatusEffects;

public sealed class seMarisaDef : lvalonmimaStatusEffectTemplate
{
	public override StatusEffectConfig MakeConfig() => Cfg(StatusEffectType.Positive, hasCount: true);
}

[EntityLogic(typeof(seMarisaDef))]
public sealed class seMarisa : StatusEffect
{
	public override bool ForceNotShowDownText => true;
	protected override void OnAdded(Unit unit)
	{
		Count = GetDistinctStatusCardCount() * 2;
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
		Count = GetDistinctStatusCardCount() * 2;
	}

	private void OnAddedDraw(CardsAddingToDrawZoneEventArgs args)
	{
		Count = GetDistinctStatusCardCount() * 2;
	}

	private void OnAdded(CardsEventArgs args)
	{
		Count = GetDistinctStatusCardCount() * 2;
	}

	private void OnUsed(CardEventArgs args)
	{
		Count = GetDistinctStatusCardCount() * 2;
	}

	private void OnUsed(CardUsingEventArgs args)
	{
		Count = GetDistinctStatusCardCount() * 2;
	}

	private void OnDealing(DamageDealingEventArgs args)
	{
		if (args.DamageInfo.DamageType != DamageType.Attack) return;
		args.DamageInfo = args.DamageInfo.IncreaseBy(GetDistinctStatusCardCount() * 2);
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
