﻿using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Attributes;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using System.Collections.Generic;
using LBoL.EntityLib.StatusEffects.Reimu;

namespace lvalonmima.NotImages.Uncommon
{
    public sealed class cardextratickdef : CardTemplate
    {
        public override IdContainer GetId()
        {
            return nameof(cardextratick);
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
               IsPooled: true,
               FindInBattle: true,
               HideMesuem: false,
               IsUpgradable: true,
               Rarity: Rarity.Uncommon,
               Type: CardType.Ability,
               TargetType: TargetType.All,
               Colors: new List<ManaColor>() { ManaColor.Blue, ManaColor.Green },
               IsXCost: false,
               Cost: new ManaGroup() { Any = 2, Hybrid = 1, HybridColor = 6 },
               UpgradedCost: new ManaGroup() { Any = 1, Hybrid = 1, HybridColor = 6 },
               MoneyCost: null,
               Damage: null,
               UpgradedDamage: null,
               Block: null,
               UpgradedBlock: null,
               Shield: null,
               UpgradedShield: null,
               Value1: 5,
               UpgradedValue1: null,
               Value2: 3,
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

               Keywords: Keyword.None,
               UpgradedKeywords: Keyword.None,
               EmptyDescription: false,
               RelativeKeyword: Keyword.Philosophy,
               UpgradedRelativeKeyword: Keyword.Philosophy,

               RelativeEffects: new List<string>() { "fastburst" },
               UpgradedRelativeEffects: new List<string>() { "fastburst" },
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

        [EntityLogic(typeof(cardextratickdef))]
        public sealed class cardextratick : mimaextensions.mimacard
        {
            public int Value3 => 1;
            protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
            {
                yield return BuffAction<SE.mburstmodifiers.fastburstdef.fastburst>(Value1, 0, 0, 0, 0.2f);
                yield return BuffAction<FreeFlySe>(Value3, 0, 0, 0, 0.2f);
            }
        }
    }
}
