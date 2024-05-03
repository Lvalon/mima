using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Attributes;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using System.Collections.Generic;
using static lvalonmima.BepinexPlugin;
using LBoL.EntityLib.StatusEffects.Sakuya;

namespace lvalonmima.NotImages.Starter
{
    public sealed class cardchannellingdef : CardTemplate
    {
        public override IdContainer GetId()
        {
            return nameof(cardchannelling);
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
               Rarity: Rarity.Common,
               Type: CardType.Skill,
               TargetType: TargetType.All,
               Colors: new List<ManaColor>() { ManaColor.White, ManaColor.Blue },
               IsXCost: false,
               Cost: new ManaGroup() { Any = 1, White = 1, Blue = 1 },
               UpgradedCost: null,
               MoneyCost: null,
               Damage: null,
               UpgradedDamage: null,
               Block: null,
               UpgradedBlock: null,
               Shield: null,
               UpgradedShield: null,
               Value1: 6,
               UpgradedValue1: 8,
               Value2: 8,
               UpgradedValue2: 10,
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
               RelativeKeyword: Keyword.None,
               UpgradedRelativeKeyword: Keyword.None,

               RelativeEffects: new List<string>() { "magicalburst", "TimeAuraSe" },
               UpgradedRelativeEffects: new List<string>() { "magicalburst", "TimeAuraSe" },
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

        [EntityLogic(typeof(cardchannellingdef))]
        public sealed class cardchannelling : mimaextensions.mimacard
        {
            protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
            {
                yield return SacrificeAction(Value1);
                yield return BuffAction<TimeAuraSe>(Value2, 0, 0, 0, 0.2f);
                //if (Battle.Player.TryGetStatusEffect<magicalburst>(out var tmp) && tmp is mimaextensions.mimase magicalburst)
                //{
                //    int countfr = magicalburst.truecounter;
                //    magicalburst.truecounter = 0;
                //    if (countfr > 0)
                //    {
                //        yield return base.BuffAction<TimeAuraSe>(countfr, 0, 0, 0, 0.2f);
                //        yield return base.BuffAction<magicalburst>(0, 1, 0, 0, 0.2f);
                //    }
                //}
            }
        }
    }
}
