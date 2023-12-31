﻿using LBoL.Base;
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
using static lvalonmima.SE.mburstmodifiers.implosiondef;
using Unity.IO.LowLevel.Unsafe;
using LBoL.EntityLib.StatusEffects.Cirno;
using LBoL.EntityLib.Cards.Character.Sakuya;
using static lvalonmima.SE.mburstmodifiers.everlastingmagicdef;
using LBoL.Core.Randoms;

namespace lvalonmima.NotImages.Rare
{
    public sealed class cardcardtabledef : CardTemplate
    {
        public override IdContainer GetId()
        {
            return nameof(cardcardtable);
        }

        public override CardImages LoadCardImages()
        {
            var imgs = new CardImages(embeddedSource);
            imgs.AutoLoad(this, extension: ".png");
            return imgs;
        }

        public override LocalizationOption LoadLocalization()
        {
            return toolbox.loccard();
        }

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
               IsPooled: true,
               HideMesuem: false,
               IsUpgradable: true,
               Rarity: Rarity.Rare,
               Type: CardType.Skill,
               TargetType: TargetType.All,
               Colors: new List<ManaColor>() { ManaColor.Blue, ManaColor.Green },
               IsXCost: false,
               Cost: new ManaGroup() { Any = 1, Blue = 2, Green = 1 },
               UpgradedCost: new ManaGroup() { Any = 1, Blue = 1, Green = 1 },
               MoneyCost: null,
               Damage: null,
               UpgradedDamage: null,
               Block: null,
               UpgradedBlock: null,
               Shield: null,
               UpgradedShield: null,
               Value1: 1,
               UpgradedValue1: null,
               Value2: null,
               UpgradedValue2: null,
               Mana: null,
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

               Keywords: Keyword.Exile,
               UpgradedKeywords: Keyword.Exile,
               EmptyDescription: false,
               RelativeKeyword: Keyword.TempMorph,
               UpgradedRelativeKeyword: Keyword.TempMorph,

               RelativeEffects: new List<string>() { },
               UpgradedRelativeEffects: new List<string>() { },
               RelativeCards: new List<string>() { },
               UpgradedRelativeCards: new List<string>() { },
               Owner: "Mima",
               ImageId: "",
               UpgradeImageId: "",
               Unfinished: false,
               Illustrator: "Dairi",
               SubIllustrator: new List<string>() { }
            );
            return cardConfig;
        }

        [EntityLogic(typeof(cardcardtabledef))]
        public sealed class cardcardtable : mimaextensions.mimacard
        {
            protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
            {
                int num = base.Battle.MaxHand - base.Battle.HandZone.Count;
                List<Card> list = base.Battle.RollCards(new CardWeightTable(RarityWeightTable.BattleCard, OwnerWeightTable.Valid, CardTypeWeightTable.OnlySkill), num, (CardConfig config) => !config.Keywords.HasFlag(Keyword.Forbidden)).ToList<Card>();
                if (list.Count > 0)
                {
                    foreach (Card card in list)
                    {
                        ManaColor[] components = card.Cost.EnumerateComponents().SampleManyOrAll(1, base.GameRun.BattleRng);
                        card.DecreaseTurnCost(ManaGroup.FromComponents(components));
                    }
                    yield return new AddCardsToHandAction(list);
                }
                yield break;
            }

        }
    }
}
