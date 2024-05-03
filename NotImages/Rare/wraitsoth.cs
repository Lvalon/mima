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
using static lvalonmima.SE.magicalburstdef;
using static lvalonmima.SE.mburstmodifiers.everlastingmagicdef;

namespace lvalonmima.NotImages.Rare
{
    public sealed class cardwraitsothdef : CardTemplate
    {
        public override IdContainer GetId()
        {
            return nameof(cardwraitsoth);
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
               Rarity: Rarity.Rare,
               Type: CardType.Ability,
               TargetType: TargetType.All,
               Colors: new List<ManaColor>() { ManaColor.Red, ManaColor.Green },
               IsXCost: false,
               Cost: new ManaGroup() { Any = 1, Red = 2, Green = 2 },
               UpgradedCost: new ManaGroup() { Any = 1, Red = 1, Green = 1 },
               MoneyCost: null,
               Damage: null,
               UpgradedDamage: null,
               Block: null,
               UpgradedBlock: null,
               Shield: null,
               UpgradedShield: null,
               Value1: 3,
               UpgradedValue1: null,
               Value2: 1,
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

               Keywords: Keyword.None,
               UpgradedKeywords: Keyword.None,
               EmptyDescription: false,
               RelativeKeyword: Keyword.None,
               UpgradedRelativeKeyword: Keyword.None,

               RelativeEffects: new List<string>() { nameof(everlastingmagic) },
               UpgradedRelativeEffects: new List<string>() { nameof(everlastingmagic) },
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

        [EntityLogic(typeof(cardwraitsothdef))]
        public sealed class cardwraitsoth : mimaextensions.mimacard
        {
            //public int gainred { get { return (100 - (Value1 * 20)); } }
            public int gainred { get { return (Value1 * 20); } }
            protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
            {
                if (Battle.Player.TryGetStatusEffect<magicalburst>(out var tmp) && tmp is mimaextensions.mimase magicalburst)
                {
                    magicalburst.truecounter = 0;
                    yield return BuffAction<magicalburst>(0, 1, 0, 0, 0.2f);
                }
                yield return BuffAction<everlastingmagic>(Value1, 0, 0, 0, 0.2f);
                //yield return BuffAction<evilspirit>(Value2, 0, 0, 0, 0.2f);
                yield break;
            }

        }
    }
}
