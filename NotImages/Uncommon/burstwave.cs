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
using static lvalonmima.SE.mburstmodifiers.splitburstdef;
using Unity.IO.LowLevel.Unsafe;
using LBoL.EntityLib.StatusEffects.Cirno;
using LBoL.Core.Randoms;
using LBoL.EntityLib.StatusEffects.Neutral.Red;
using lvalonmima.SE.mburstmodifiers;
using static lvalonmima.NotImages.Uncommon.cardutmostdef;

namespace lvalonmima.NotImages.Uncommon
{
    public sealed class cardburstwavedef : CardTemplate
    {
        public override IdContainer GetId()
        {
            return nameof(cardburstwave);
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
               IsPooled: true,
               FindInBattle: true,
               HideMesuem: false,
               IsUpgradable: true,
               Rarity: Rarity.Uncommon,
               Type: CardType.Ability,
               TargetType: TargetType.All,
               Colors: new List<ManaColor>() { ManaColor.Red, ManaColor.Colorless },
               IsXCost: false,
               Cost: new ManaGroup() { Red = 1, Colorless = 1 },
               UpgradedCost: null,
               MoneyCost: null,
               Damage: null,
               UpgradedDamage: null,
               Block: null,
               UpgradedBlock: null,
               Shield: null,
               UpgradedShield: null,
               Value1: 1,
               UpgradedValue1: 2,
               Value2: 2,
               UpgradedValue2: 3,
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

               Keywords: Keyword.None,
               UpgradedKeywords: Keyword.None,
               EmptyDescription: false,
               RelativeKeyword: Keyword.None,
               UpgradedRelativeKeyword: Keyword.None,

               RelativeEffects: new List<string>() { nameof(splitburst), nameof(Firepower) },
               UpgradedRelativeEffects: new List<string>() { nameof(splitburst), nameof(Firepower) },
               RelativeCards: new List<string>() { nameof(cardutmost) },
               UpgradedRelativeCards: new List<string>() { nameof(cardutmost) },
               Owner: "Mima",
               ImageId: "",
               UpgradeImageId: "",
               Unfinished: false,
               Illustrator: "Dairi",
               SubIllustrator: new List<string>() { }
            );
            return cardConfig;
        }

        [EntityLogic(typeof(cardburstwavedef))]
        public sealed class cardburstwave : mimaextensions.mimacard
        {
            public int Value3 { get { return Value1 + 1; } }
            protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
            {
                yield return base.BuffAction<splitburst>(base.Value1, 0, 0, 0, 0.2f);
                yield return base.BuffAction<Firepower>(base.Value2, 0, 0, 0, 0.2f);
            }
        }
    }
}
