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
using Unity.IO.LowLevel.Unsafe;
using LBoL.EntityLib.StatusEffects.Cirno;
using LBoL.Core.Randoms;

namespace lvalonmima.NotImages.Uncommon
{
    public sealed class cardmconvertdef : CardTemplate
    {
        public override IdContainer GetId()
        {
            return nameof(cardmconvert);
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
               Rarity: Rarity.Uncommon,
               Type: CardType.Skill,
               TargetType: TargetType.All,
               Colors: new List<ManaColor>() { ManaColor.Colorless },
               IsXCost: true,
               Cost: new ManaGroup() { Colorless = 1 },
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

               Keywords: Keyword.Exile,
               UpgradedKeywords: Keyword.Exile | Keyword.Echo,
               EmptyDescription: false,
               RelativeKeyword: Keyword.Philosophy | Keyword.XCost | Keyword.Synergy,
               UpgradedRelativeKeyword: Keyword.Philosophy | Keyword.XCost | Keyword.Synergy,

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

        [EntityLogic(typeof(cardmconvertdef))]
        public sealed class cardmconvert : mimaextensions.mimacard
        {
            public override ManaGroup GetXCostFromPooled(ManaGroup pooledMana)
            {
                return new ManaGroup
                {
                    Colorless = pooledMana.Colorless,
                    Philosophy = pooledMana.Philosophy
                };
            }
            protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
            {
                ManaGroup manaGroup = ManaGroup.Empty;
                for (int i = 0; i < base.SynergyAmount(consumingMana, ManaColor.Philosophy, 1); i++)
                {
                    manaGroup += ManaGroup.Single(ManaColor.Philosophy);
                }
                yield return new GainManaAction(manaGroup);
                yield return new DrawManyCardAction(base.SynergyAmount(consumingMana, ManaColor.Colorless, 2));
            }
        }
    }
}