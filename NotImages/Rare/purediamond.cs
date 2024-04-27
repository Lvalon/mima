using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Attributes;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static lvalonmima.BepinexPlugin;
using LBoL.Core.Units;
using System.Xml.Linq;
using static lvalonmima.SE.evilspiritdef;
using Unity.IO.LowLevel.Unsafe;
using LBoL.EntityLib.StatusEffects.Cirno;
using LBoL.Core.Randoms;
using LBoL.EntityLib.Cards.Character.Marisa;
using LBoL.EntityLib.StatusEffects.Marisa;
using LBoL.EntityLib.StatusEffects.Others;
using LBoL.EntityLib.StatusEffects.Reimu;

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
            var imgs = new CardImages(embeddedSource);
            imgs.AutoLoad(this, extension: ".png");
            return imgs;
        }

        public override LocalizationOption LoadLocalization() => cardbatchloc.AddEntity(this);

        public override CardConfig MakeConfig()
        {
            var cardConfig = new CardConfig(
               Index: sequenceTable.Next(typeof(CardConfig)),
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
               Colors: new List<ManaColor>() {},
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
                if (dstZone != CardZone.Hand)
                {
                    return null;
                }
                return EnterHandReactor();
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
                    //action = null;
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
                //yield return base.DamageSelfAction(Value1);
                //yield return DamageAction.Reaction((Unit)Battle.AllAliveUnits, Value1, "星落");
                yield return new DamageAction(Battle.Player, Battle.AllAliveUnits, DamageInfo.Reaction(Value1, false), "星落", GunType.Single);
                yield return new ExileCardAction(this);
                //yield return DamageAction.Reaction(base.Owner, base.Level, (base.Level >= 15) ? "溺水BuffB" : "溺水BuffA");

                //foreach (Unit in battle)
                //{
                //}
                yield break;
            }
        }
    }
}
