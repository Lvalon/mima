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
using LBoL.EntityLib.Cards.Character.Cirno;
using static lvalonmima.NotImages.Rare.cardpurediamonddef;

namespace lvalonmima.NotImages.Uncommon
{
    public sealed class cardoncetimedef : CardTemplate
    {
        public override IdContainer GetId()
        {
            return nameof(cardoncetime);
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
               Colors: new List<ManaColor>() { ManaColor.White, ManaColor.Black },
               IsXCost: false,
               Cost: new ManaGroup() { White = 1, Black = 1 },
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
               Value2: 1,
               UpgradedValue2: null,
               Mana: new ManaGroup() { Philosophy = 1 },
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
               RelativeKeyword: Keyword.Purified,
               UpgradedRelativeKeyword: Keyword.Purified,

               RelativeEffects: new List<string>() { },
               UpgradedRelativeEffects: new List<string>() { },
               RelativeCards: new List<string>() { nameof(cardpurediamond)},
               UpgradedRelativeCards: new List<string>() { nameof(cardpurediamond) },
               Owner: "Mima",
               ImageId: "",
               UpgradeImageId: "",
               Unfinished: false,
               Illustrator: "Dairi",
               SubIllustrator: new List<string>() { }
            );
            return cardConfig;
        }

        [EntityLogic(typeof(cardoncetimedef))]
        public sealed class cardoncetime : mimaextensions.mimacard
        {
            protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
            {
                //yield return base.SacrificeAction(base.Value1);
                //yield return new GainManaAction(base.Mana);
                //if (IsUpgraded)
                //{
                //    List<Card> upgraded = null;
                //    DrawManyCardAction drawAction = new DrawManyCardAction(base.Value2);
                //    yield return drawAction;
                //    IReadOnlyList<Card> drawnCards = drawAction.DrawnCards;
                //    if (drawnCards != null && drawnCards.Count > 0)
                //    {
                //        upgraded = (from card in drawnCards
                //                    where card.IsUpgraded
                //                    select card).ToList<Card>();
                //        List<Card> list = (from card in drawnCards
                //                           where card.CanUpgradeAndPositive
                //                           select card).ToList<Card>();
                //        if (list.Count > 0)
                //        {
                //            yield return new UpgradeCardsAction(list);
                //        }
                //    }
                //}
                List<cardoncetime> list = Library.CreateCards<cardoncetime>(2, this.IsUpgraded).ToList<cardoncetime>();
                cardoncetime first = list[0];
                cardoncetime cardoncetime = list[1];
                first.ShowWhichDescription = 2;
                cardoncetime.ShowWhichDescription = 1;
                first.SetBattle(base.Battle);
                cardoncetime.SetBattle(base.Battle);
                MiniSelectCardInteraction interaction = new MiniSelectCardInteraction(list, false, false, false)
                {
                    Source = this
                };
                yield return new InteractionAction(interaction, false);
                if (interaction.SelectedCard == first)
                {
                    yield return new AddCardsToDrawZoneAction(Library.CreateCards<cardpurediamond>(Value1, false), DrawZoneTarget.Random, AddCardsType.Normal);
                }
                else
                {
                    bool flag = false;
                    List<Card> list4 = (from card in base.Battle.HandZone
                                       where !card.IsPurified && card.Cost.HasTrivial
                                       select card).ToList<Card>();
                    if (list4.Count > 0)
                    {
                        Card card3 = list4.Sample(base.GameRun.BattleRng);
                        card3.NotifyActivating();
                        card3.IsPurified = true;
                        if (!flag)
                        {
                            base.NotifyActivating();
                            flag = true;
                        }
                    }
                    else
                    {
                        List<Card> list2 = (from card in base.Battle.HandZone
                                            where !card.IsPurified
                                            select card).ToList<Card>();
                        if (list2.Count <= 0)
                        {
                            yield break;
                        }
                        Card card2 = list2.Sample(base.GameRun.BattleRng);
                        card2.NotifyActivating();
                        card2.IsPurified = true;
                        if (!flag)
                        {
                            base.NotifyActivating();
                            flag = true;
                        }
                    }
                }
                yield break;
            }
        }
    }
}
