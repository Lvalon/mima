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
    public sealed class cardrewinddef : CardTemplate
    {
        public override IdContainer GetId()
        {
            return nameof(cardrewind);
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
               Colors: new List<ManaColor>() { ManaColor.Blue, ManaColor.Black, ManaColor.Green },
               IsXCost: false,
               Cost: new ManaGroup() { Blue = 1, Black = 1, Green = 1 },
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
               Value2: 5,
               UpgradedValue2: 7,
               Mana: null,
               UpgradedMana: null,
               Scry: 2,
               UpgradedScry: 3,
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
               RelativeKeyword: Keyword.Scry,
               UpgradedRelativeKeyword: Keyword.Scry,

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
            public int Value3 => 1;
            protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
            {
                List<Card> hand = (from card in Battle.HandZone
                                   where card != this
                                   select card).ToList();
                if (hand.Count > 0)
                {
                    yield return new DiscardManyAction(hand);
                }
                List<Card> list = Battle.DrawZone.Reverse().ToList();
                List<Card> oriDiscard = Battle.DiscardZone.ToList();
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
                for (int i = 0; i < Value1; i++)
                {
                    manaGroup += ManaGroup.Single(ManaColors.Colors.Sample(GameRun.BattleRng));
                }
                yield return new GainManaAction(manaGroup);
                yield return new ScryAction(Scry);
                yield return new DrawManyCardAction(Value2);
                //yield return BuffAction<SE.mburstmodifiers.accumulationdef.accumulation>(Value3, 0, 0, 0, 0.2f);
                yield break;
            }
        }
    }
}
