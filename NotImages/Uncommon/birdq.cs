using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Attributes;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using System.Collections.Generic;

namespace lvalonmima.NotImages.Uncommon
{
    public sealed class cardbirdqdef : CardTemplate
    {
        public override IdContainer GetId()
        {
            return nameof(cardbirdq);
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
               Colors: new List<ManaColor>() { ManaColor.Blue },
               IsXCost: false,
               Cost: new ManaGroup() { Any = 1, Blue = 1 },
               UpgradedCost: new ManaGroup() { Any = 1 },
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

               RelativeEffects: new List<string>() { nameof(SE.mburstmodifiers.accumulationdef.accumulation) },
               UpgradedRelativeEffects: new List<string>() { nameof(SE.mburstmodifiers.accumulationdef.accumulation) },
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

        [EntityLogic(typeof(cardbirdqdef))]
        public sealed class cardbirdq : mimaextensions.mimacard
        {
            protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
            {
                yield return new DrawManyCardAction(Value1);
                yield return BuffAction<SE.mburstmodifiers.accumulationdef.accumulation>(Value2, 0, 0, 0, 0.2f);

                // List<Card> cards = new List<Card>
                //         {
                //             Library.CreateCard<Monster.Rare.monstertearkitdef.monstertearkit>(),
                //             Library.CreateCard<Monster.Rare.monstertearkitdef.monstertearkit>(),
                //             Library.CreateCard<Monster.Rare.monstertearkitdef.monstertearkit>(),
                //             Library.CreateCard<Monster.Rare.monstertearkitdef.monstertearkit>(),
                //             Library.CreateCard<Monster.Rare.monstertearkitdef.monstertearkit>()
                //         };
                // List<Card> cards2 = new List<Card>
                //         {
                //             Library.CreateCard<Monster.Rare.monstertearkitdef.monstertearkit>(),
                //             Library.CreateCard<Monster.Rare.monstertearkitdef.monstertearkit>(),
                //             Library.CreateCard<Monster.Rare.monstertearkitdef.monstertearkit>(),
                //             Library.CreateCard<Monster.Rare.monstertearkitdef.monstertearkit>(),
                //             Library.CreateCard<Monster.Rare.monstertearkitdef.monstertearkit>()
                //         };
                //yield return new AddCardsToDrawZoneAction(Library.CreateCards<Monster.Rare.monstertearkitdef.monstertearkit>(5, false), DrawZoneTarget.Random, AddCardsType.Normal);
                //yield return new AddCardsToHandAction(cards); //deck 8
                //yield return new AddCardsToDeckAction(cards); //hand 8

                //yield return new AddCardsToDeckAction(cards); //deck 4
                //yield return new AddCardsToHandAction(cards2); //hand 4
                //yield return new AddCardsToDrawZoneAction(Library.CreateCards<Monster.Rare.monstertearkitdef.monstertearkit>(5, false), DrawZoneTarget.Random, AddCardsType.Normal);

                // yield return new AddCardsToDeckAction(cards); //deck 4
                // yield return new AddCardsToDrawZoneAction(Library.CreateCards<Monster.Rare.monstertearkitdef.monstertearkit>(5, false), DrawZoneTarget.Random, AddCardsType.Normal);
                // yield return new AddCardsToHandAction(cards); //draw pile 9
            }
        }
    }
}
