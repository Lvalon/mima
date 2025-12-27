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

namespace lvalonmima.StatusEffects
{
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
		// private readonly List<(Card, CardUsingEventArgs)> AttackEchoArgs = new List<(Card, CardUsingEventArgs)>();
		// protected override void OnAdded(Unit unit)
		// {
		// 	HandleOwnerEvent(Battle.CardUsing, OnCardUsing);
		// 	ReactOwnerEvent(Battle.CardUsed, OnCardUsed);
		// }

		// private void OnCardUsing(CardUsingEventArgs args)
		// {
		// 	if (!args.Card.IsPurified || args.Card.IsBasic) { return; }
		// 	Card token = args.Card.Clone();
		// 	token.IsPlayTwiceToken = true;
		// 	token.PlayTwiceSourceCard = args.Card;
		// 	AttackEchoArgs.Add((token, args.Clone()));
		// }

		// private IEnumerable<BattleAction> OnCardUsed(CardUsingEventArgs args)
		// {
		// 	foreach ((Card card, CardUsingEventArgs aargs) in AttackEchoArgs)
		// 	{
		// 		NotifyActivating();
		// 		yield return new PlayTwiceAction(card, aargs);
		// 	}
		// 	AttackEchoArgs.Clear();
		// 	yield break;
		// }

		protected override void OnAdded(Unit unit)
		{
			ReactOwnerEvent(Battle.CardUsed, OnCardUsed);
		}
		private IEnumerable<BattleAction> OnCardUsed(CardUsingEventArgs args)
		{
			if (!args.Card.IsPurified || args.Card.IsBasic) { yield break; }
			Card token = args.Card.Clone();
			token.IsPlayTwiceToken = true;
			token.PlayTwiceSourceCard = args.Card;

			NotifyActivating();
			yield return new PlayTwiceAction(token, args.Clone());
			yield break;
		}

		// bool hasabyss = false;
		// public ManaGroup Mana => ManaGroup.Anys(base.Level);
		// protected override void OnAdded(Unit unit)
		// {
		// 	hasabyss = Battle.Player.HasStatusEffect<seabyss>();
		// 	if (hasabyss)
		// 	{
		// 		SetMana(Battle.EnumerateAllCards());
		// 	}
		// 	HandleOwnerEvent(Battle.CardsAddedToDiscard, OnAddCard);
		// 	HandleOwnerEvent(Battle.CardsAddedToHand, OnAddCard);
		// 	HandleOwnerEvent(Battle.CardsAddedToExile, OnAddCard);
		// 	HandleOwnerEvent(Battle.CardsAddedToDrawZone, OnAddCardToDraw);
		// 	HandleOwnerEvent(Battle.CardTransformed, OnCardTransformed);
		// 	HandleOwnerEvent(Battle.Player.StatusEffectRemoved, OnStatusEffectRemoved);
		// 	HandleOwnerEvent(Battle.Player.StatusEffectAdded, OnStatusEffectAdded);
		// }

		// private void OnStatusEffectAdded(StatusEffectApplyEventArgs args)
		// {
		// 	if (hasabyss)
		// 	{
		// 		return;
		// 	}
		// 	hasabyss = Battle.Player.HasStatusEffect<seabyss>();
		// 	if (hasabyss)
		// 	{
		// 		SetMana(Battle.EnumerateAllCards());
		// 	}
		// }

		// private void OnStatusEffectRemoved(StatusEffectEventArgs args)
		// {
		// 	hasabyss = Battle.Player.HasStatusEffect<seabyss>();
		// 	if (args.Effect is seabyss)
		// 	{
		// 		privateOnRemoved();
		// 		return;
		// 	}
		// }
		// private void privateOnRemoved()
		// {
		// 	if (Battle.EnumerateAllCards().Count() > 0)
		// 	{
		// 		NotifyActivating();
		// 	}
		// 	foreach (Card item in Battle.EnumerateAllCards())
		// 	{
		// 		item.AuraCost += Mana;
		// 	}
		// }

		// protected override void OnRemoved(Unit unit)
		// {
		// 	if (hasabyss)
		// 	{
		// 		foreach (Card item in Battle.EnumerateAllCards())
		// 		{
		// 			item.AuraCost += Mana;
		// 		}
		// 	}
		// }

		// private void OnAddCard(CardsEventArgs args)
		// {
		// 	if (!hasabyss)
		// 	{
		// 		return;
		// 	}
		// 	SetMana(args.Cards);
		// }

		// private void OnAddCardToDraw(CardsAddingToDrawZoneEventArgs args)
		// {
		// 	if (!hasabyss)
		// 	{
		// 		return;
		// 	}
		// 	SetMana(args.Cards);
		// }

		// private void OnCardTransformed(CardTransformEventArgs args)
		// {
		// 	if (!hasabyss)
		// 	{
		// 		return;
		// 	}
		// 	SetMana(args.DestinationCard);
		// }

		// private void SetMana(Card card)
		// {
		// 	NotifyActivating();
		// 	card.AuraCost -= Mana;
		// }

		// private void SetMana(IEnumerable<Card> cards)
		// {
		// 	bool flag = true;
		// 	foreach (Card card in cards)
		// 	{
		// 		if (flag)
		// 		{
		// 			NotifyActivating();
		// 			flag = false;
		// 		}

		// 		card.AuraCost -= Mana;
		// 	}
		// }

		// public override bool Stack(StatusEffect other)
		// {
		// 	bool flag = base.Stack(other);
		// 	if (flag && hasabyss)
		// 	{
		// 		bool flag2 = true;
		// 		foreach (Card item in Battle.EnumerateAllCards())
		// 		{
		// 			if (flag2)
		// 			{
		// 				NotifyActivating();
		// 				flag2 = false;
		// 			}
		// 			item.AuraCost -= ManaGroup.Anys(other.Level);
		// 		}
		// 	}
		// 	return flag;
		// }
	}
}