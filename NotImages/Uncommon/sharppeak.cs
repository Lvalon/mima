using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Attributes;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using System.Collections.Generic;
using System.Linq;

namespace lvalonmima.NotImages.Uncommon
{
    public sealed class cardsharppeakdef : CardTemplate
    {
        public override IdContainer GetId()
        {
            return nameof(cardsharppeak);
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
               Colors: new List<ManaColor>() { ManaColor.White, ManaColor.Red },
               IsXCost: true,
               Cost: new ManaGroup() { Red = 1, White = 1 },
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
               Kicker: null,
               UpgradedKicker: null,
               ActiveCost2: null,
               UpgradedActiveCost2: null,

               Keywords: Keyword.None,
               UpgradedKeywords: Keyword.None,
               EmptyDescription: false,
               RelativeKeyword: Keyword.XCost | Keyword.Synergy,
               UpgradedRelativeKeyword: Keyword.XCost | Keyword.Synergy,

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

        [EntityLogic(typeof(cardsharppeakdef))]
        public sealed class cardsharppeak : mimaextensions.mimacard
        {
            public override ManaGroup GetXCostFromPooled(ManaGroup pooledMana)
            {
                return new ManaGroup
                {
                    Red = pooledMana.Red,
                    White = pooledMana.White,
                    Philosophy = pooledMana.Philosophy
                };
            }
            protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
            {
                ManaGroup manaGroup = ManaGroup.Empty;
                yield return SacrificeAction(SynergyAmount(consumingMana, ManaColor.Red, 1) * Value1);
                //yield return BuffAction<SE.magicalburstdef.magicalburst>(SynergyAmount(consumingMana, ManaColor.Red, 1) * Value1, 0, 0, 0, 0.2f);
                for (int i = 1; i <= SynergyAmount(consumingMana, ManaColor.White, !IsUpgraded ? 2 : 1) * Value2; i++)
                {
                    Card[] cards = (from c in Battle.HandZone
                                    where c.CanUpgrade
                                    select c).ToList().SampleManyOrAll(1, GameRun.BattleRng);
                    NotifyActivating();
                    yield return new UpgradeCardsAction(cards);
                }
                // List<Card> list = (from card in Battle.HandZone
                //                    where card.CanUpgradeAndPositive
                //                    select card).ToList().SampleManyOrAll(SynergyAmount(consumingMana, ManaColor.White, !IsUpgraded ? 2 : 1) * Value2, GameRun.BattleRng).ToList();
                // if (list.Count > 0)
                // {
                //     NotifyActivating();
                //     yield return new UpgradeCardsAction(list);
                // }
                yield break;
            }
        }
    }
}
