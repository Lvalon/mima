﻿using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Attributes;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using System.Collections.Generic;

namespace lvalonmima.NotImages.Uncommon
{
    public sealed class cardstakedef : CardTemplate
    {
        public override IdContainer GetId()
        {
            return nameof(cardstake);
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
               Type: CardType.Skill,
               TargetType: TargetType.All,
               Colors: new List<ManaColor>() { ManaColor.White, ManaColor.Green },
               IsXCost: false,
               Cost: new ManaGroup() { Any = 1, Hybrid = 2, HybridColor = 3 },
               UpgradedCost: null,
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
               Mana: new ManaGroup() { Philosophy = 2 },
               UpgradedMana: new ManaGroup() { Philosophy = 3 },
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

               Keywords: Keyword.Exile,
               UpgradedKeywords: Keyword.Exile,
               EmptyDescription: false,
               RelativeKeyword: Keyword.Philosophy,
               UpgradedRelativeKeyword: Keyword.Philosophy,

               RelativeEffects: new List<string>() { nameof(Invincible) },
               UpgradedRelativeEffects: new List<string>() { nameof(Invincible) },
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

        [EntityLogic(typeof(cardstakedef))]
        public sealed class cardstake : mimaextensions.mimacard
        {
            protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
            {
                yield return BuffAction<Invincible>(0, Value1, 0, 0, 0.2f);
                yield return new GainManaAction(Mana);
                yield return UpgradeAllHandsAction();
            }
        }
    }
}
