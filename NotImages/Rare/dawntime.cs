using LBoL.Base;
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

namespace lvalonmima.NotImages.Rare
{
    public sealed class carddawntimedef : CardTemplate
    {
        public override IdContainer GetId()
        {
            return nameof(carddawntime);
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
               Rarity: Rarity.Rare,
               Type: CardType.Ability,
               TargetType: TargetType.All,
               Colors: new List<ManaColor>() { ManaColor.Red, ManaColor.White, ManaColor.Blue, ManaColor.Green, ManaColor.Black },
               IsXCost: false,
               Cost: new ManaGroup() { Hybrid = 1, HybridColor = 1, Colorless = 1, Red = 1, Blue = 1, Green = 1 },
               UpgradedCost: null,
               MoneyCost: null,
               Damage: null,
               UpgradedDamage: null,
               Block: null,
               UpgradedBlock: null,
               Shield: null,
               UpgradedShield: null,
               Value1: 7,
               UpgradedValue1: 10,
               Value2: null,
               UpgradedValue2: null,
               Mana: new ManaGroup() { Any = 0 },
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

               Keywords: Keyword.Ethereal,
               UpgradedKeywords: Keyword.Ethereal,
               EmptyDescription: false,
               RelativeKeyword: Keyword.TempMorph,
               UpgradedRelativeKeyword: Keyword.TempMorph,

               RelativeEffects: new List<string>() { nameof(ExtraTurn), nameof(SE.sedawntimedef.sedawntime) },
               UpgradedRelativeEffects: new List<string>() { nameof(ExtraTurn), nameof(SE.sedawntimedef.sedawntime) },
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

        [EntityLogic(typeof(carddawntimedef))]
        public sealed class carddawntime : mimaextensions.mimacard
        {
            protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
            {
                yield return PerformAction.Effect(Battle.Player, "ExtraTime", 0f, null, 0f, PerformAction.EffectBehavior.PlayOneShot, 0f);
                yield return PerformAction.Sfx("ExtraTurnLaunch", 0f);
                yield return PerformAction.Animation(Battle.Player, "spell", 1.6f, null, 0f, -1);
                yield return BuffAction<ExtraTurn>(1, 0, 0, 0, 0.2f);
                yield return BuffAction<SE.sedawntimedef.sedawntime>(Value1, 0, 0, 0, 0.2f);
                yield return new RequestEndPlayerTurnAction();
                yield break;
            }
        }
    }
}
