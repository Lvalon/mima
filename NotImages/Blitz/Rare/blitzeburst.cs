using LBoL.Base;
using LBoL.Core.Cards;
using LBoL.ConfigData;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Attributes;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using System.Collections.Generic;
using lvalonmima.SE;
using LBoL.Core;
using System.Collections;

namespace lvalonmima.NotImages.Blitz.Rare
{
    public sealed class blitzeburstdef : CardTemplate
    {
        public override IdContainer GetId()
        {
            return nameof(blitzeburst);
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
               IsPooled: false,
               FindInBattle: false,
               HideMesuem: false,
               IsUpgradable: false,
               Rarity: Rarity.Rare,
               Type: CardType.Skill,
               TargetType: TargetType.All,
               Colors: new List<ManaColor>() { ManaColor.Black },
               IsXCost: false,
               Cost: new ManaGroup() { },
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
               Mana: new ManaGroup() { Philosophy = 3 } ,
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

               Keywords: Keyword.Forbidden,
               UpgradedKeywords: Keyword.None,
               EmptyDescription: false,
               RelativeKeyword: Keyword.None,
               UpgradedRelativeKeyword: Keyword.None,

               RelativeEffects: new List<string>() { nameof(seblitzdef.seblitz) },
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

        [EntityLogic(typeof(blitzeburstdef))]
        public class blitzeburst : mimaextensions.mimacard
        {
            public IEnumerator OnConfirmed(CardsEventArgs args)
            {
                foreach (Card card in args.Cards)
                {
                    if (card is mimaextensions.mimacard mimascard && blitzcards.Contains(mimascard.Id) && mimascard.Id == nameof(blitzeburst))
                    {
                        if (Battle != null)
                        {
                            Battle.GainMana(Mana);
                        }
                        GameRun.RemoveDeckCard(card, false);
                    }
                }
                yield break;
            }
        }
    }
}
