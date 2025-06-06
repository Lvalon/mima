﻿using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Attributes;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using System.Collections.Generic;
using UnityEngine;

namespace lvalonmima.NotImages.Rare
{
    public sealed class cardpurediamonddef : CardTemplate
    {
        public override IdContainer GetId()
        {
            return nameof(cardpurediamond);
        }

        public override CardImages LoadCardImages()
        {
            CardImages imgs = new CardImages(BepinexPlugin.embeddedSource);
            imgs.AutoLoad(this, extension: ".png");
            return imgs;
        }

        public override LocalizationOption LoadLocalization()
        {
            return BepinexPlugin.cardbatchloc.AddEntity(this);
        }

        public override CardConfig MakeConfig()
        {
            CardConfig cardConfig = new CardConfig(
               Index: BepinexPlugin.sequenceTable.Next(typeof(CardConfig)),
               Id: "",
               Order: 10,
               AutoPerform: true,
               Perform: new string[0][],
               GunName: "Simple1",
               GunNameBurst: "Simple1",
               DebugLevel: 0,
               Revealable: false,
               IsPooled: false,
               FindInBattle: true,
               HideMesuem: false,
               IsUpgradable: false,
               Rarity: Rarity.Rare,
               Type: CardType.Status,
               TargetType: TargetType.All,
               Colors: new List<ManaColor>() { },
               IsXCost: false,
               Cost: new ManaGroup() { Any = 0 },
               UpgradedCost: null,
               MoneyCost: null,
               Damage: null,
               UpgradedDamage: null,
               Block: null,
               UpgradedBlock: null,
               Shield: null,
               UpgradedShield: null,
               Value1: 5,
               UpgradedValue1: null,
               Value2: null,
               UpgradedValue2: null,
               Mana: new ManaGroup() { Philosophy = 1 },
               UpgradedMana: null,
               Scry: null,
               UpgradedScry: null,
               ToolPlayableTimes: null,
               Loyalty: null,
               UpgradedLoyalty: null,
               PassiveCost: null,
               UpgradedPassiveCost: null,
               ActiveCost: null,
               UpgradedActiveCost: null,
               UltimateCost: null,
               UpgradedUltimateCost: null,
               Kicker: null,
               UpgradedKicker: null,
               ActiveCost2: null,
               UpgradedActiveCost2: null,

               Keywords: Keyword.Forbidden,
               UpgradedKeywords: Keyword.Forbidden,
               EmptyDescription: false,
               RelativeKeyword: Keyword.Philosophy,
               UpgradedRelativeKeyword: Keyword.Philosophy,

               RelativeEffects: new List<string>() { },
               UpgradedRelativeEffects: new List<string>() { },
               RelativeCards: new List<string>() { },
               UpgradedRelativeCards: new List<string>() { },
               Owner: null,
               ImageId: "",
               UpgradeImageId: "",
               Unfinished: false,
               Illustrator: "DeepAI",
               SubIllustrator: new List<string>() { }
            );
            return cardConfig;
        }

        [EntityLogic(typeof(cardpurediamonddef))]
        public sealed class cardpurediamond : mimaextensions.mimacard
        {
            public override IEnumerable<BattleAction> OnDraw()
            {
                return EnterHandReactor();
            }

            public override IEnumerable<BattleAction> OnMove(CardZone srcZone, CardZone dstZone)
            {
                return dstZone != CardZone.Hand ? null : EnterHandReactor();
            }

            protected override void OnEnterBattle(BattleController battle)
            {
                if (Zone == CardZone.Hand)
                {
                    React(new LazySequencedReactor(AddToHandReactor));
                }
            }
            private IEnumerable<BattleAction> AddToHandReactor()
            {
                NotifyActivating();
                List<DamageAction> list = new List<DamageAction>();
                foreach (BattleAction action in EnterHandReactor())
                {
                    yield return action;
                    DamageAction damageAction = action as DamageAction;
                    if (damageAction != null)
                    {
                        list.Add(damageAction);
                    }
                }
                if (list.NotEmpty())
                {
                    yield return new StatisticalTotalDamageAction(list);
                }
                yield break;
            }
            private IEnumerable<BattleAction> EnterHandReactor()
            {
                if (Battle.BattleShouldEnd)
                {
                    yield break;
                }
                if (Zone != CardZone.Hand)
                {
                    Debug.LogWarning(Name + " is not in hand.");
                    yield break;
                }
                yield return new GainManaAction(Mana);
                yield return new DamageAction(Battle.Player, Battle.AllAliveUnits, DamageInfo.Reaction(Value1, false), "星落", GunType.Single);
                yield return new ExileCardAction(this);
                yield break;
            }
        }
    }
}
