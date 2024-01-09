using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core;
using LBoL.Core.StatusEffects;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Attributes;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static lvalonmima.BepinexPlugin;
using LBoL.Core.Battle.BattleActions;
using LBoL.Base.Extensions;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Enemy;
using System.Linq;
using LBoL.Core.Battle.Interactions;
using LBoL.Presentation.UI.Panels;
using LBoL.EntityLib.StatusEffects.ExtraTurn;
using LBoL.EntityLib.Cards.Character.Sakuya;
using LBoL.Core.Randoms;
using LBoL.EntityLib.JadeBoxes;

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
            return toolbox.locse();
        }

        public override Sprite LoadSprite()
        {
            return ResourceLoader.LoadSprite("sedawntime.png", embeddedSource);
        }

        public override StatusEffectConfig MakeConfig()
        {
            var statusEffectConfig = new StatusEffectConfig(
                Index: sequenceTable.Next(typeof(CardConfig)),
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
                base.ThisTurnActivating = false;
                base.HandleOwnerEvent<UnitEventArgs>(base.Battle.Player.TurnStarting, delegate (UnitEventArgs _)
                {
                    if (base.Battle.Player.IsExtraTurn && !base.Battle.Player.IsSuperExtraTurn && base.Battle.Player.GetStatusEffectExtend<ExtraTurnPartner>() == this)
                    {
                        base.ThisTurnActivating = true;
                    }
                });
                base.HandleOwnerEvent<DamageEventArgs>(Owner.DamageTaking, new GameEventHandler<DamageEventArgs>(this.OnDamageTaking));
                base.HandleOwnerEvent<DamageDealingEventArgs>(base.Owner.DamageDealing, new GameEventHandler<DamageDealingEventArgs>(this.OnDamageDealing));
                base.HandleOwnerEvent<CardEventArgs>(base.Battle.Predraw, new GameEventHandler<CardEventArgs>(this.OnPredraw));
                base.ReactOwnerEvent<UnitEventArgs>(base.Owner.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnOwnerTurnStarted));
                ReactOwnerEvent(Battle.Player.TurnEnding, new EventSequencedReactor<GameEventArgs>(OnPlayerTurnEnding));
                base.HandleOwnerEvent<ManaEventArgs>(base.Battle.ManaGaining, new GameEventHandler<ManaEventArgs>(this.OnManaGaining));
                this.CardToFree(base.Battle.EnumerateAllCards());
                base.HandleOwnerEvent<CardsEventArgs>(base.Battle.CardsAddedToDiscard, new GameEventHandler<CardsEventArgs>(this.OnAddCard));
                base.HandleOwnerEvent<CardsEventArgs>(base.Battle.CardsAddedToHand, new GameEventHandler<CardsEventArgs>(this.OnAddCard));
                base.HandleOwnerEvent<CardsEventArgs>(base.Battle.CardsAddedToExile, new GameEventHandler<CardsEventArgs>(this.OnAddCard));
                base.ReactOwnerEvent<CardMovingEventArgs>(base.Battle.CardMoved, new EventSequencedReactor<CardMovingEventArgs>(this.OnCardMoved));
                base.HandleOwnerEvent<CardsAddingToDrawZoneEventArgs>(base.Battle.CardsAddedToDrawZone, new GameEventHandler<CardsAddingToDrawZoneEventArgs>(this.OnAddCardToDraw));
            }
            private void OnDamageTaking(DamageEventArgs args)
            {
                if (base.ThisTurnActivating)
                {
                    int num = args.DamageInfo.Damage.RoundToInt();
                    if (num > 0)
                    {
                        base.NotifyActivating();
                        args.DamageInfo = args.DamageInfo.ReduceActualDamageBy(num);
                        args.AddModifier(this);
                    }
                }
            }
            private void OnDamageDealing(DamageDealingEventArgs args)
            {
                if (base.ThisTurnActivating)
                {
                    DamageInfo damageInfo = args.DamageInfo;
                    damageInfo.Damage = damageInfo.Amount * (0);
                    args.DamageInfo = damageInfo;
                    args.AddModifier(this);
                }
            }
            private void OnManaGaining(ManaEventArgs args)
            {
                if (base.ThisTurnActivating)
                {
                    //if (base.Battle.ExtraTurnMana.IsEmpty)
                    //{
                    args.CancelBy(this);
                    //    return;
                    //}
                    //args.Value = base.Battle.ExtraTurnMana;
                    //args.AddModifier(this);
                }
            }
            private void OnAddCardToDraw(CardsAddingToDrawZoneEventArgs args)
            {
                if (base.ThisTurnActivating) { this.CardToFree(args.Cards); }
            }
            private void OnAddCard(CardsEventArgs args)
            {
                if (base.ThisTurnActivating) { this.CardToFree(args.Cards); }
            }
            private IEnumerable<BattleAction> OnCardMoved(CardMovingEventArgs args)
            {
                if (base.ThisTurnActivating)
                {
                    Card card = args.Card;
                    if (card.Config.IsXCost == false) { card.FreeCost = true; }
                    yield break;
                }
            }
            private void CardToFree(IEnumerable<Card> cards)
            {
                if (base.ThisTurnActivating)
                {
                    foreach (Card card in cards)
                    {
                        if (card.Config.IsXCost == false) { card.FreeCost = true; }
                    }
                }
            }
            protected override void OnRemoved(Unit unit)
            {
                foreach (Card card in base.Battle.EnumerateAllCards())
                {
                    card.FreeCost = false;
                }
            }
            private IEnumerable<BattleAction> OnOwnerTurnStarted(UnitEventArgs args)
            {
                if (base.ThisTurnActivating)
                {
                    List<Card> hand = (from card in base.Battle.HandZone select card).ToList<Card>();
                    if (hand.Count > 0)
                    {
                        foreach (Card card2 in hand)
                        {
                            yield return new RemoveCardAction(card2);
                        }
                    }
                    List<Card> list = base.Battle.RollCardsWithoutManaLimit(new CardWeightTable(RarityWeightTable.BattleCard, OwnerWeightTable.OnlyPlayer, CardTypeWeightTable.CanBeLoot), base.Level, (CardConfig config) => !config.Keywords.HasFlag(Keyword.Forbidden) && config.Id != base.Id).ToList<Card>();
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
                if (base.ThisTurnActivating)
                {
                    args.CancelBy(this);
                }
            }
            private IEnumerable<BattleAction> OnPlayerTurnEnding(GameEventArgs args)
            {
                if (base.ThisTurnActivating)
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
