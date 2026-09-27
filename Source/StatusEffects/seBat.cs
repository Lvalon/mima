using System.Collections.Generic;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoLEntitySideloader.Attributes;

namespace lvalonmima.StatusEffects;

public sealed class seBatDef : lvalonmimaStatusEffectTemplate
{
	public override StatusEffectConfig MakeConfig() => Cfg(StatusEffectType.Positive);
}

[EntityLogic(typeof(seBatDef))]
public sealed class seBat : StatusEffect
{
	public override bool ForceNotShowDownText => true;
	protected override void OnAdded(Unit unit)
	{
		ReactOwnerEvent(Battle.CardDrawn, OnCardDrawn);
	}

	private IEnumerable<BattleAction> OnCardDrawn(CardEventArgs args)
	{
		if (!Owner.HasStatusEffect<Graze>() && args.Cause != ActionCause.TurnStart && args.ActionSource is not Card { IsReplenish: true })
		{
			NotifyActivating();
			yield return new ApplyStatusEffectAction<Graze>(Owner, 1);
		}
	}
}
