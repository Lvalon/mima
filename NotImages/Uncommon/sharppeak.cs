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
using static lvalonmima.SE.mburstmodifiers.fastburstdef;
using static lvalonmima.SE.magicalburstdef;
using Unity.IO.LowLevel.Unsafe;
using LBoL.EntityLib.StatusEffects.Cirno;
using LBoL.Core.Randoms;

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
               Colors: new List<ManaColor>() { ManaColor.Red, ManaColor.White },
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
               Value1: 2,
               UpgradedValue1: 3,
               Value2: 1,
               UpgradedValue2: 2,
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
               RelativeKeyword: Keyword.XCost | Keyword.Synergy,
               UpgradedRelativeKeyword: Keyword.XCost | Keyword.Synergy,

               RelativeEffects: new List<string>() { "fastburst" },
               UpgradedRelativeEffects: new List<string>() { "fastburst" },
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
            public int Value3
            {
                get
                {
                    return 1;
                }
            }
            public int Value3pct
            { get { return 20; } }
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
                yield return base.BuffAction<magicalburst>(base.SynergyAmount(consumingMana, ManaColor.Red, 1) * Value1, 0, 0, 0, 0.2f);
                //this one below doesnt fix the card applied vs status effect/debug applied problem
                //yield return new ApplyStatusEffectAction<magicalburst>(base.Battle.Player, new int?(base.SynergyAmount(consumingMana, ManaColor.Red, 1) * Value1), null, null, null, 0.4f, true);
                List<Card> list = (from card in base.Battle.HandZone
                                   where card.CanUpgradeAndPositive
                                   select card).ToList<Card>().SampleManyOrAll(base.SynergyAmount(consumingMana, ManaColor.White, 1) * Value2, base.GameRun.BattleRng).ToList<Card>();
                if (list.Count > 0)
                {
                    base.NotifyActivating();
                    yield return new UpgradeCardsAction(list);
                }
                yield return BuffAction<fastburst>(Value3, 0, 0, 0, 0.2f);
                yield break;
            }
        }
    }
}
