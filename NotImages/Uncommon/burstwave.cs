using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.StatusEffects;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Attributes;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using System.Collections.Generic;

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

               RelativeEffects: new List<string>() { nameof(SE.mburstmodifiers.splitburstdef.splitburst), nameof(Firepower) },
               UpgradedRelativeEffects: new List<string>() { nameof(SE.mburstmodifiers.splitburstdef.splitburst), nameof(Firepower) },
               RelativeCards: new List<string>() { nameof(cardutmostdef.cardutmost) },
               UpgradedRelativeCards: new List<string>() { nameof(cardutmostdef.cardutmost) },
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
            public int Value3 => Value1 + 1;
            protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
            {
                yield return BuffAction<SE.mburstmodifiers.splitburstdef.splitburst>(Value1, 0, 0, 0, 0.2f);
                yield return BuffAction<Firepower>(Value2, 0, 0, 0, 0.2f);
            }
        }
    }
}
