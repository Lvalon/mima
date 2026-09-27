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

public sealed class semimaexbDef : lvalonmimaStatusEffectTemplate
{
	public override StatusEffectConfig MakeConfig()
	{
		StatusEffectConfig config = GetDefaultStatusEffectConfig();
		config.Type = StatusEffectType.Positive;
		config.Keywords = Keyword.Purified;
		return config;
	}
}

[EntityLogic(typeof(semimaexbDef))]
public sealed class semimaexb : StatusEffect
{
	public override bool ForceNotShowDownText => true;

	protected override void OnAdded(Unit unit)
	{
		ReactOwnerEvent(Battle.CardUsed, OnCardUsed);
	}
	private IEnumerable<BattleAction> OnCardUsed(CardUsingEventArgs args)
	{
		if (!args.Card.IsPurified || args.Card.IsBasic) yield break;
		Card token = args.Card.Clone();
		token.IsPlayTwiceToken = true;
		token.PlayTwiceSourceCard = args.Card;

		NotifyActivating();
		yield return new PlayTwiceAction(token, args.Clone());
		yield break;
	}
}
