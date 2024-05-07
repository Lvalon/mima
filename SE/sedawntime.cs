using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Attributes;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using System.Collections.Generic;
using UnityEngine;
using LBoL.Core.Battle.BattleActions;
using LBoL.Base.Extensions;
using LBoL.Core.Units;
using System.Linq;
using LBoL.EntityLib.StatusEffects.ExtraTurn;
using LBoL.Core.Randoms;

namespace lvalonmima.SE
{
    public sealed class sedawntimedef : StatusEffectTemplate
    {
        public override IdContainer GetId()
        {
            return nameof(sedawntime);
        }

        public override LocalizationOption LoadLocalization()
        {
            return BepinexPlugin.sebatchloc.AddEntity(this);
        }

        public override Sprite LoadSprite()
        {
            return ResourceLoader.LoadSprite("sedawntime.png", BepinexPlugin.embeddedSource);
        }

        public override StatusEffectConfig MakeConfig()
        {
            StatusEffectConfig statusEffectConfig = new StatusEffectConfig(
                Index: BepinexPlugin.sequenceTable.Next(typeof(CardConfig)),
                Id: "",
                Order: 420,
                Type: StatusEffectType.Special,
                IsVerbose: true,
                IsStackable: false,
                StackActionTriggerLevel: null,
                HasLevel: true,
                LevelStackType: StackType.Add,
                HasDuration: false,
                DurationStackType: null,
                DurationDecreaseTiming: DurationDecreaseTiming.Custom,
                HasCount: false,
                CountStackType: StackType.Keep,
                LimitStackType: StackType.Keep,
                ShowPlusByLimit: false,
                Keywords: Keyword.TempMorph | Keyword.Exile | Keyword.Ethereal,
                RelativeEffects: new List<string>() { },
                VFX: "Default",
                VFXloop: "Default",
                SFX: "Default"
            );
            return statusEffectConfig;
        }

        [EntityLogic(typeof(sedawntimedef))]
        public sealed class sedawntime : mimaextensions.mimaexpartner
        {
            protected override void OnAdded(Unit unit)
            {
                ThisTurnActivating = false;
                HandleOwnerEvent(Battle.Player.TurnStarting, delegate (UnitEventArgs _)
                {
                    if (Battle.Player.IsExtraTurn && !Battle.Player.IsSuperExtraTurn && Battle.Player.GetStatusEffectExtend<ExtraTurnPartner>() == this)
                    {
                        ThisTurnActivating = true;
                    }
                });
                HandleOwnerEvent(Owner.DamageTaking, new GameEventHandler<DamageEventArgs>(OnDamageTaking));
                HandleOwnerEvent(Owner.DamageDealing, new GameEventHandler<DamageDealingEventArgs>(OnDamageDealing));
                HandleOwnerEvent(Battle.Predraw, new GameEventHandler<CardEventArgs>(OnPredraw));
                ReactOwnerEvent(Owner.TurnStarted, new EventSequencedReactor<UnitEventArgs>(OnOwnerTurnStarted));
                ReactOwnerEvent(Battle.Player.TurnEnding, new EventSequencedReactor<GameEventArgs>(OnPlayerTurnEnding));
                HandleOwnerEvent(Battle.ManaGaining, new GameEventHandler<ManaEventArgs>(OnManaGaining));
                CardToFree(Battle.EnumerateAllCards());
                HandleOwnerEvent(Battle.CardsAddedToDiscard, new GameEventHandler<CardsEventArgs>(OnAddCard));
                HandleOwnerEvent(Battle.CardsAddedToHand, new GameEventHandler<CardsEventArgs>(OnAddCard));
                HandleOwnerEvent(Battle.CardsAddedToExile, new GameEventHandler<CardsEventArgs>(OnAddCard));
                ReactOwnerEvent(Battle.CardMoved, new EventSequencedReactor<CardMovingEventArgs>(OnCardMoved));
                HandleOwnerEvent(Battle.CardsAddedToDrawZone, new GameEventHandler<CardsAddingToDrawZoneEventArgs>(OnAddCardToDraw));
            }
            private void OnDamageTaking(DamageEventArgs args)
            {
                if (ThisTurnActivating)
                {
                    int num = args.DamageInfo.Damage.RoundToInt();
                    if (num > 0)
                    {
                        NotifyActivating();
                        args.DamageInfo = args.DamageInfo.ReduceActualDamageBy(num);
                        args.AddModifier(this);
                    }
                }
            }
            private void OnDamageDealing(DamageDealingEventArgs args)
            {
                if (ThisTurnActivating)
                {
                    DamageInfo damageInfo = args.DamageInfo;
                    damageInfo.Damage = damageInfo.Amount * 0;
                    args.DamageInfo = damageInfo;
                    args.AddModifier(this);
                }
            }
            private void OnManaGaining(ManaEventArgs args)
            {
                if (ThisTurnActivating)
                {
                    args.CancelBy(this);
                }
            }
            private void OnAddCardToDraw(CardsAddingToDrawZoneEventArgs args)
            {
                if (ThisTurnActivating) { CardToFree(args.Cards); }
            }
            private void OnAddCard(CardsEventArgs args)
            {
                if (ThisTurnActivating) { CardToFree(args.Cards); }
            }
            private IEnumerable<BattleAction> OnCardMoved(CardMovingEventArgs args)
            {
                if (ThisTurnActivating)
                {
                    Card card = args.Card;
                    if (card.Config.IsXCost == false) { card.FreeCost = true; }
                    yield break;
                }
            }
            private void CardToFree(IEnumerable<Card> cards)
            {
                if (ThisTurnActivating)
                {
                    foreach (Card card in cards)
                    {
                        if (card.Config.IsXCost == false) { card.FreeCost = true; }
                    }
                }
            }
            protected override void OnRemoved(Unit unit)
            {
                foreach (Card card in Battle.EnumerateAllCards())
                {
                    card.FreeCost = false;
                }
            }
            private IEnumerable<BattleAction> OnOwnerTurnStarted(UnitEventArgs args)
            {
                if (ThisTurnActivating)
                {
                    List<Card> hand = (from card in Battle.HandZone select card).ToList<Card>();
                    if (hand.Count > 0)
                    {
                        foreach (Card card2 in hand)
                        {
                            yield return new RemoveCardAction(card2);
                        }
                    }
                    List<Card> list = toolbox.RollCardsCustomIgnore(GameRun.BattleCardRng, new CardWeightTable(RarityWeightTable.BattleCard, OwnerWeightTable.OnlyPlayer, CardTypeWeightTable.CanBeLoot), Level, null, false, false, false, false, (Card card) => card is mimaextensions.mimacard && !card.Keywords.HasFlag(Keyword.Forbidden) && card.Config.Id != "carddawntime").ToList<Card>();
                    if (list.Count > 0)
                    {
                        foreach (Card card in list)
                        {
                            card.SetTurnCost(new ManaGroup() { Any = 0 });
                            card.IsEthereal = true;
                            card.IsExile = true;
                        }
                        yield return new AddCardsToHandAction(list);
                    }
                }
                yield break;
            }
            private void OnPredraw(CardEventArgs args)
            {
                if (ThisTurnActivating)
                {
                    args.CancelBy(this);
                }
            }
            private IEnumerable<BattleAction> OnPlayerTurnEnding(GameEventArgs args)
            {
                if (ThisTurnActivating)
                {
                    NotifyActivating();
                    yield return new RemoveStatusEffectAction(this, true);
                    yield break;
                }
            }
            public ManaGroup Mana = ManaGroup.Empty;
        }
    }
}
