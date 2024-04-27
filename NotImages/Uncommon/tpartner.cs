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
using System;
using UnityEngine;
using static lvalonmima.BepinexPlugin;
using LBoL.Core.Units;
using System.Xml.Linq;
using static lvalonmima.SE.evilspiritdef;
using Unity.IO.LowLevel.Unsafe;
using LBoL.EntityLib.StatusEffects.Cirno;
using LBoL.Core.Randoms;
using LBoL.EntityLib.Cards;

namespace lvalonmima.NotImages.Uncommon
{
    public sealed class cardtpartnerdef : CardTemplate
    {
        public override IdContainer GetId()
        {
            return nameof(cardtpartner);
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
               Type: CardType.Skill,
               TargetType: TargetType.All,
               Colors: new List<ManaColor>() { ManaColor.White, ManaColor.Blue, ManaColor.Colorless },
               IsXCost: false,
               Cost: new ManaGroup() { Any = 1, White = 1, Blue = 1, Colorless = 1 },
               UpgradedCost: null,
               MoneyCost: null,
               Damage: null,
               UpgradedDamage: null,
               Block: null,
               UpgradedBlock: null,
               Shield: null,
               UpgradedShield: null,
               Value1: null,
               UpgradedValue1: null,
               Value2: null,
               UpgradedValue2: null,
               Mana: new ManaGroup() { Any = 1 },
               UpgradedMana: new ManaGroup() { Any = 0 },
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
               RelativeKeyword: Keyword.TempMorph | Keyword.Exile,
               UpgradedRelativeKeyword: Keyword.TempMorph | Keyword.Exile,

               RelativeEffects: new List<string>() { "TimeIsLimited" },
               UpgradedRelativeEffects: new List<string>() { "TimeIsLimited" },
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

        [EntityLogic(typeof(cardtpartnerdef))]
        public sealed class cardtpartner : mimaextensions.mimacard
        {
            protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
            {
                List<Card> list = Battle.RollCardsWithoutManaLimit(new CardWeightTable(RarityWeightTable.AllOnes, OwnerWeightTable.AllOnes, CardTypeWeightTable.CanBeLoot), 1, (CardConfig config) => config.Id != base.Id && (config.RelativeEffects.Contains("TimeIsLimited") || config.UpgradedRelativeEffects.Contains("TimeIsLimited"))).ToList();
                foreach (Card card in list)
                {
                    card.SetTurnCost(base.Mana);
                    card.IsEthereal = true;
                    card.IsExile = true;
                }
                yield return new AddCardsToHandAction(list);
            }
        }
    }
}
