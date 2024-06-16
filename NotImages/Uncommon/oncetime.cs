using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Attributes;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using System.Collections.Generic;
using System.Linq;

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
               Colors: new List<ManaColor>() { ManaColor.White, ManaColor.Black },
               IsXCost: false,
               Cost: new ManaGroup() { Hybrid = 2, HybridColor = 1 },
               UpgradedCost: new ManaGroup() { Any = 1, Hybrid = 1, HybridColor = 1 },
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
               RelativeCards: new List<string>() { nameof(Rare.cardpurediamonddef.cardpurediamond) },
               UpgradedRelativeCards: new List<string>() { nameof(Rare.cardpurediamonddef.cardpurediamond) },
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
                List<cardoncetime> list = Library.CreateCards<cardoncetime>(2, IsUpgraded).ToList();
                cardoncetime first = list[0];
                cardoncetime cardoncetime = list[1];
                first.ShowWhichDescription = 2;
                cardoncetime.ShowWhichDescription = 1;
                first.SetBattle(Battle);
                cardoncetime.SetBattle(Battle);
                MiniSelectCardInteraction interaction = new MiniSelectCardInteraction(list, false, false, false)
                {
                    Source = this
                };
                yield return new InteractionAction(interaction, false);
                if (interaction.SelectedCard == first)
                {
                    yield return new AddCardsToDrawZoneAction(Library.CreateCards<Rare.cardpurediamonddef.cardpurediamond>(Value1, false), DrawZoneTarget.Random, AddCardsType.Normal);
                }
                else
                {
                    bool flag = false;
                    List<Card> list4 = (from card in Battle.HandZone
                                        where !card.IsPurified && card.Cost.HasTrivial
                                        select card).ToList();
                    if (list4.Count > 0)
                    {
                        Card card3 = list4.Sample(GameRun.BattleRng);
                        card3.NotifyActivating();
                        card3.IsPurified = true;
                        if (!flag)
                        {
                            NotifyActivating();
                            flag = true;
                        }
                    }
                    else
                    {
                        List<Card> list2 = (from card in Battle.HandZone
                                            where !card.IsPurified
                                            select card).ToList();
                        if (list2.Count <= 0)
                        {
                            yield break;
                        }
                        Card card2 = list2.Sample(GameRun.BattleRng);
                        card2.NotifyActivating();
                        card2.IsPurified = true;
                        if (!flag)
                        {
                            NotifyActivating();
                            flag = true;
                        }
                    }
                }
                yield break;
            }
        }
    }
}
