using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Attributes;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using System.Collections.Generic;

namespace lvalonmima.NotImages.Passive.Rare
{
    public sealed class passiverpolaritydef : CardTemplate
    {
        public override IdContainer GetId()
        {
            return nameof(passiverpolarity);
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
               FindInBattle: false,
               HideMesuem: false,
               IsUpgradable: false,
               Rarity: Rarity.Rare,
               Type: CardType.Ability,
               TargetType: TargetType.All,
               Colors: new List<ManaColor>() { ManaColor.Black, ManaColor.Red },
               IsXCost: false,
               Cost: new ManaGroup() { },
               UpgradedCost: new ManaGroup() { },
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
               Kicker: null,
               UpgradedKicker: null,
               ActiveCost2: null,
               UpgradedActiveCost2: null,

               Keywords: Keyword.Forbidden,
               UpgradedKeywords: Keyword.None,
               EmptyDescription: false,
               RelativeKeyword: Keyword.None,
               UpgradedRelativeKeyword: Keyword.None,

               RelativeEffects: new List<string>() { nameof(SE.sepassivedef.sepassive), nameof(SE.mburstmodifiers.retributiondef.retribution) },
               UpgradedRelativeEffects: new List<string>() { },
               RelativeCards: new List<string>() { },
               UpgradedRelativeCards: new List<string>() { },
               Owner: "Mima",
               ImageId: "",
               UpgradeImageId: "",
               Unfinished: false,
               Illustrator: "Lvalon",
               SubIllustrator: new List<string>() { }
            );
            return cardConfig;
        }

        [EntityLogic(typeof(passiverpolaritydef))]
        public sealed class passiverpolarity : mimaextensions.mimacard.passivecard
        {
            // protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
            // {
            //     yield return BuffAction<SE.mburstmodifiers.retributiondef.retribution>(Value1, 0, 0, 0, 0.2f);
            //     yield break;
            // }

        }
    }
}
