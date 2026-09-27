using System;
using System.Linq;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoLEntitySideloader.Attributes;

namespace lvalonmima.StatusEffects;

public sealed class seDoremyDef : lvalonmimaStatusEffectTemplate
{
	public override StatusEffectConfig MakeConfig() => Cfg(StatusEffectType.Positive);
}

[EntityLogic(typeof(seDoremyDef))]
public sealed class seDoremy : StatusEffect
{
	bool has = false;
	public override bool ForceNotShowDownText => true;
	protected override void OnAdded(Unit unit)
	{
		has = false;
		HandleOwnerEvent(Battle.CardUsing, OnCardUsing);
		HandleOwnerEvent(Battle.CardUsed, OnCardUsed);
	}

	private void OnCardUsed(CardUsingEventArgs args)
	{
		if (has && Battle.EnumerateAllCards().Contains(args.Card))
			args.Card.IsEthereal = true;
		has = false;
	}

	private void OnCardUsing(CardUsingEventArgs args)
	{
		has = Owner.Shield > 0;
	}
}
