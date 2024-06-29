using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Attributes;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using System.Collections.Generic;

namespace lvalonmima.NotImages.Starter
{
    public sealed class cardmountaindef : CardTemplate
    {
        public override IdContainer GetId()
        {
            return nameof(cardmountain);
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
               Rarity: Rarity.Common,
               Type: CardType.Skill,
               TargetType: TargetType.All,
               Colors: new List<ManaColor>() { ManaColor.White, ManaColor.Red },
               IsXCost: false,
               Cost: new ManaGroup() { Colorless = 1, Hybrid = 1, HybridColor = 2 },
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
               Value2: 4,
               UpgradedValue2: 6,
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

               Keywords: Keyword.Replenish | Keyword.Exile | Keyword.AutoExile,
               UpgradedKeywords: Keyword.Replenish | Keyword.Exile | Keyword.AutoExile,
               EmptyDescription: false,
               RelativeKeyword: Keyword.AutoExile | Keyword.Replenish,
               UpgradedRelativeKeyword: Keyword.AutoExile | Keyword.Replenish,

               RelativeEffects: new List<string>() { nameof(SE.magicalburstdef.magicalburst) },
               UpgradedRelativeEffects: new List<string>() { nameof(SE.magicalburstdef.magicalburst) },
               RelativeCards: new List<string>() { nameof(Rare.cardpurediamonddef.cardpurediamond) },
               UpgradedRelativeCards: new List<string>() { nameof(Rare.cardpurediamonddef.cardpurediamond) },
               Owner: "Mima",
               ImageId: "",
               UpgradeImageId: "",
               Unfinished: false,
               Illustrator: "Lvalon",
               SubIllustrator: new List<string>() { }
            );
            return cardConfig;
        }

        [EntityLogic(typeof(cardmountaindef))]
        public sealed class cardmountain : mimaextensions.mimacard
        {
            protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
            {
                yield return new AddCardsToDrawZoneAction(Library.CreateCards<Rare.cardpurediamonddef.cardpurediamond>(Value1, false), DrawZoneTarget.Random, AddCardsType.Normal);
                yield return BuffAction<SE.magicalburstdef.magicalburst>(Value2, 0, 0, 0, 0.2f);
            }
        }
    }
}
