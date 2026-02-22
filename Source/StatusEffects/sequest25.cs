using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoLEntitySideloader.Attributes;

namespace lvalonmima.StatusEffects
{
	public sealed class sequest25Def : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Positive;
			return config;
		}
	}

	[EntityLogic(typeof(sequest25Def))]
	public sealed class sequest25 : StatusEffect
	{
		protected override void OnAdded(Unit unit)
		{
			ReactOwnerEvent(Battle.CardUsed, OnCardUsed);
		}

		private IEnumerable<BattleAction> OnCardUsed(CardUsingEventArgs args)
		{
			if (args.Card.CardType != CardType.Ability || args.Card.IsPlayTwiceToken)
			{
				yield break;
			}
			Card token = args.Card.CloneTwiceToken();
			token.IsPlayTwiceToken = true;
			token.PlayTwiceSourceCard = args.Card;
			yield return new PlayTwiceAction(token, args.Clone());
			if (Level > 0)
				Level--;
			if (Level == 0)
				yield return new RemoveStatusEffectAction(this);
		}
	}
}