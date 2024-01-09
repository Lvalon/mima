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
using Unity.IO.LowLevel.Unsafe;
using LBoL.EntityLib.StatusEffects.Cirno;
using LBoL.Core.Randoms;
using static lvalonmima.NotImages.Rare.cardpurediamonddef;
using static lvalonmima.SE.mburstmodifiers.accumulationdef;

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

               RelativeEffects: new List<string>() { "accumulation" },
               UpgradedRelativeEffects: new List<string>() { "accumulation" },
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
            //public override bool DiscardCard
            //{
            //    get
            //    {
            //        return true;
            //    }
            //}
            //public override Interaction Precondition()
            //{
            //    List<Card> list = (from hand in base.Battle.HandZone
            //                       where hand != this
            //                       select hand).ToList<Card>();
            //    if (list.Count <= base.Value1)
            //    {
            //        this.allHand = list;
            //    }
            //    if (list.Count <= base.Value1)
            //    {
            //        return null;
            //    }
            //    return new SelectHandInteraction(base.Value1, base.Value1, list);
            //}
            protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
            {
                //if (precondition != null)
                //{
                //    IReadOnlyList<Card> selectedCards = ((SelectHandInteraction)precondition).SelectedCards;
                //    if (selectedCards != null)
                //    {
                //        yield return new DiscardManyAction(selectedCards);
                //    }
                //}
                //else if (this.allHand.Count > 0)
                //{
                //    yield return new DiscardManyAction(this.allHand);
                //    this.allHand = null;
                //}
                yield return new DrawManyCardAction(Value1);
                //yield return new AddCardsToDrawZoneAction(Library.CreateCards<cardpurediamond>(Value2, false), DrawZoneTarget.Random, AddCardsType.Normal);
                yield return BuffAction<accumulation>(Value2, 0, 0, 0, 0.2f);
            }
            //private List<Card> allHand;
        }
    }
}
