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
using static lvalonmima.SE.magicalburstdef;
using static lvalonmima.SE.extmpfiredef;

namespace lvalonmima.NotImages.Uncommon
{
    public sealed class cardrewinddef : CardTemplate
    {
        public override IdContainer GetId()
        {
            return nameof(cardrewind);
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
               Colors: new List<ManaColor>() { ManaColor.Blue, ManaColor.Green },
               IsXCost: false,
               Cost: new ManaGroup() { Any = 1, Blue = 1, Green = 1 },
               UpgradedCost: new ManaGroup() { Any = 2 },
               MoneyCost: null,
               Damage: null,
               UpgradedDamage: null,
               Block: null,
               UpgradedBlock: null,
               Shield: null,
               UpgradedShield: null,
               Value1: 2,
               UpgradedValue1: null,
               Value2: 4,
               UpgradedValue2: 5,
               Mana: null,
               UpgradedMana: null,
               Scry: 7,
               UpgradedScry: 8,
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

        [EntityLogic(typeof(cardrewinddef))]
        public sealed class cardrewind : mimaextensions.mimacard
        {
            public int Value3 { get { return 1; } }
            protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
            {
                List<Card> hand = (from card in base.Battle.HandZone
                                   where card != this
                                   select card).ToList<Card>();
                if (hand.Count > 0)
                {
                    yield return new DiscardManyAction(hand);
                }
                List<Card> list = base.Battle.DrawZone.Reverse<Card>().ToList<Card>();
                List<Card> oriDiscard = base.Battle.DiscardZone.ToList<Card>();
                foreach (Card card in list)
                {
                    if (card.Zone == CardZone.Draw)
                    {
                        yield return new MoveCardAction(card, CardZone.Discard);
                    }
                }
                foreach (Card card2 in oriDiscard)
                {
                    if (card2.Zone == CardZone.Discard)
                    {
                        yield return new MoveCardToDrawZoneAction(card2, DrawZoneTarget.Top);
                    }
                }
                ManaGroup manaGroup = ManaGroup.Empty;
                for (int i = 0; i < base.Value1; i++)
                {
                    manaGroup += ManaGroup.Single(ManaColors.Colors.Sample(base.GameRun.BattleRng));
                }
                yield return new GainManaAction(manaGroup);
                yield return new ScryAction(base.Scry);
                yield return new DrawManyCardAction(base.Value2);
                yield break;
            }
        }
    }
}
