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
using static lvalonmima.BepinexPlugin;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;

namespace lvalonmima.SE
{
    public sealed class theabyssdef : StatusEffectTemplate
    {
        public override IdContainer GetId()
        {
            return nameof(theabyss);
        }

        public override LocalizationOption LoadLocalization() => sebatchloc.AddEntity(this);

        public override Sprite LoadSprite()
        {
            return ResourceLoader.LoadSprite("setheabyss.png", embeddedSource);
        }

        public override StatusEffectConfig MakeConfig()
        {
            var statusEffectConfig = new StatusEffectConfig(
                Index: 0,
                Id: "",
                Order: 1,
                Type: StatusEffectType.Special,
                IsVerbose: true,
                IsStackable: true,
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
                Keywords: Keyword.TempMorph,
                RelativeEffects: new List<string>() { },
                VFX: "Default",
                VFXloop: "Default",
                SFX: "Default"
            );
            return statusEffectConfig;
        }

        [EntityLogic(typeof(theabyssdef))]
        public sealed class theabyss : mimaextensions.mimase
        {
            int cardused = 0;
            //set up triggers to give a fuck on
            //they worked
            protected override void OnAdded(Unit unit)
            {
                this.CardToFree(base.Battle.EnumerateAllCards());
                ReactOwnerEvent(Battle.RoundEnding, new EventSequencedReactor<GameEventArgs>(OnRoundEnding));
                base.HandleOwnerEvent<CardsEventArgs>(base.Battle.CardsAddedToDiscard, new GameEventHandler<CardsEventArgs>(this.OnAddCard));
                base.HandleOwnerEvent<CardsEventArgs>(base.Battle.CardsAddedToHand, new GameEventHandler<CardsEventArgs>(this.OnAddCard));
                base.HandleOwnerEvent<CardsEventArgs>(base.Battle.CardsAddedToExile, new GameEventHandler<CardsEventArgs>(this.OnAddCard));
                base.HandleOwnerEvent<CardsAddingToDrawZoneEventArgs>(base.Battle.CardsAddedToDrawZone, new GameEventHandler<CardsAddingToDrawZoneEventArgs>(this.OnAddCardToDraw));
                base.ReactOwnerEvent<CardUsingEventArgs>(base.Battle.CardUsed, new EventSequencedReactor<CardUsingEventArgs>(this.OnCardUsed));
                React(PerformAction.Effect(unit, "JingHua", 0f, "", 0f, PerformAction.EffectBehavior.PlayOneShot, 0f));
            }
            private void CardToFree(IEnumerable<Card> cards)
            {
                foreach (Card card in cards)
                {
                    if (card.CardType == CardType.Skill && card.BaseCost.Amount > 2)
                    {
                        card.FreeCost = true;
                    }
                }
            }
            private void OnAddCardToDraw(CardsAddingToDrawZoneEventArgs args)
            {
                this.CardToFree(args.Cards);
            }
            private void OnAddCard(CardsEventArgs args)
            {
                this.CardToFree(args.Cards);
            }
            private IEnumerable<BattleAction> OnCardUsed(CardUsingEventArgs args)
            {
                if (args.Card.CardType == CardType.Skill)
                {
                    cardused = 1;
                }
                yield break;
            }
            protected override void OnRemoved(Unit unit)
            {
                foreach (Card card in base.Battle.EnumerateAllCards())
                {
                    if (card.CardType == CardType.Skill)
                    {
                        card.FreeCost = false;
                    }
                }
            }
            private IEnumerable<BattleAction> OnRoundEnding(GameEventArgs args)
            {
                if (cardused == 1)
                {
                    int num = Level - 1;
                    Level = num;
                    cardused = 0;
                }
                if (Level == 0)
                {
                    NotifyActivating();
                    yield return new RemoveStatusEffectAction(this, true);
                    yield break;
                }
                yield break;
            }
            public ManaGroup Mana = ManaGroup.Empty;
        }
    }
}
