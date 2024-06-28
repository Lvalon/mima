using LBoL.Base;
using LBoL.Core.Cards;
using LBoL.Core.Battle;
using LBoL.ConfigData;
using LBoL.Presentation;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Attributes;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using System.Collections.Generic;
using lvalonmima.SE;
using LBoL.Core;
using LBoL.Core.StatusEffects;
using System.Collections;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle.Interactions;
using System.Linq;
using LBoL.Base.Extensions;

namespace lvalonmima.NotImages.Monster.Rare
{
    public sealed class monstertearkitdef : CardTemplate
    {
        public override IdContainer GetId()
        {
            return nameof(monstertearkit);
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
               Order: 7,
               AutoPerform: true,
               Perform: new string[0][],
               GunName: "Simple1",
               GunNameBurst: "Simple1",
               DebugLevel: 0,
               Revealable: false,
               IsPooled: true,
               FindInBattle: false,
               HideMesuem: false,
               IsUpgradable: true,
               Rarity: Rarity.Rare,
               Type: CardType.Friend,
               TargetType: TargetType.All,
               Colors: new List<ManaColor>() { ManaColor.Blue, ManaColor.Black },
               IsXCost: false,
               Cost: new ManaGroup() { Blue = 2, Black = 2 },
               UpgradedCost: null,
               MoneyCost: null,
               Damage: 10,
               UpgradedDamage: null,
               Block: null,
               UpgradedBlock: null,
               Shield: null,
               UpgradedShield: null,
               Value1: null,
               UpgradedValue1: null,
               Value2: null,
               UpgradedValue2: null,
               Mana: new ManaGroup() { Blue = 2, Black = 2 },
               UpgradedMana: null,
               Scry: 2,
               UpgradedScry: null,
               ToolPlayableTimes: null,
               Loyalty: 4,
               UpgradedLoyalty: null,
               PassiveCost: 1,
               UpgradedPassiveCost: null,
               ActiveCost: -2,
               UpgradedActiveCost: null,
               UltimateCost: -2,
               UpgradedUltimateCost: null,

               Keywords: Keyword.Scry,
               UpgradedKeywords: Keyword.Scry,
               EmptyDescription: false,
               RelativeKeyword: Keyword.None,
               UpgradedRelativeKeyword: Keyword.None,

               RelativeEffects: new List<string>() { nameof(semonsterdef.semonster) },
               UpgradedRelativeEffects: new List<string>() { nameof(semonsterdef.semonster) },
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

        [EntityLogic(typeof(monstertearkitdef))]
        public sealed class monstertearkit : mimaextensions.mimacard.monstercard
        {
            public override bool CanUpgrade
            {
                get
                {
                    int? upgradeCounter = UpgradeCounter;
                    int num = 2147483647;
                    return upgradeCounter.GetValueOrDefault() < num & upgradeCounter != null;
                }
            }
            public override bool IsUpgraded
            {
                get
                {
                    return UpgradeCounter > 0;
                }
            }
            public override void Initialize()
            {
                base.Initialize();
                UpgradeCounter = new int?(0);
            }
            public override void Upgrade()
            {
                int? upgradeCounter = UpgradeCounter + 1;
                UpgradeCounter = upgradeCounter;
                ProcessKeywordUpgrade();
                CostChangeInUpgrading();
                NotifyChanged();
            }
            public int activevalue
            {
                get
                {
                    return UpgradeCounter.Value + 1;
                }
            }
            public int scryadd
            {
                get
                {
                    return UpgradeCounter.Value;
                }
            }
            public int scryshow {
                get {
                    return UpgradeCounter.Value + 2;
                }
            }
            public int ultvalue
            {
                get
                {
                    return UpgradeCounter.Value + 2;
                }
            }
            protected override void OnEnterBattle(BattleController battle)
            {
                HandleBattleEvent(Battle.Scrying, delegate (ScryEventArgs args)
                {
                    if (args.ActionSource == this) {
                        args.ScryInfo = args.ScryInfo.IncreasedBy(scryadd);
                    }
                });
            }
            //friend effect starts now
            public override IEnumerable<BattleAction> OnTurnStartedInHand()
            {
                return GetPassiveActions();
            }

            public override IEnumerable<BattleAction> GetPassiveActions()
            {
                if (!Summoned || Battle.BattleShouldEnd)
                {
                    yield break;
                }
                NotifyActivating();
                Loyalty += PassiveCost;
                int num;
                for (int i = 0; i < Battle.FriendPassiveTimes; i = num + 1)
                {
                    if (Battle.BattleShouldEnd)
                    {
                        yield break;
                    }
                    yield return PerformAction.Sfx("FairySupport", 0f);
                    yield return new ScryAction(Scry);
                    num = i;
                }
                yield break;
            }
            //used when on summon action is wanted

            // public override IEnumerable<BattleAction> SummonActions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
            // {
            //     foreach (BattleAction battleAction in SummonActions(selector, consumingMana, precondition))
            //     {
            //         yield return battleAction;
            //     }
            //     yield break;
            // }
            protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
            {
                if (precondition == null || ((MiniSelectCardInteraction)precondition).SelectedCard.FriendToken == FriendToken.Active)
                {
                    Loyalty += ActiveCost;
                    yield return SkillAnime;
                    if (Battle.HandZone.Count < Battle.MaxHand && Battle.DrawZone.Count > 0)
                    {
                        List<Card> cards = (from card in Battle.DrawZone
                                            where card.Config.Colors.Contains(ManaColor.Blue) || card.Config.Colors.Contains(ManaColor.Black)
                                            select card).Take(activevalue).ToList();
                        if (cards.Count > 0)
                        {
                            foreach (Card card3 in cards)
                            {
                                if (Battle.HandZone.Count == Battle.MaxHand)
                                {
                                    break;
                                }
                                yield return new MoveCardAction(card3, CardZone.Hand);
                            }
                        }
                        cards = null;
                    }
                    if (Loyalty > 0)
                    {
                        yield return new MoveCardAction(this, CardZone.Hand);
                    }
                }
                else
                {
                    Loyalty += UltimateCost;
                    UltimateUsed = true;
                    yield return SkillAnime;
                    List<Card> list = (from card2 in Battle.DrawZone.Concat(Battle.DiscardZone).Concat(Battle.ExileZone)
                                       where card2 != this && (card2.Config.Colors.Contains(ManaColor.Blue) || card2.Config.Colors.Contains(ManaColor.Black))
                                       select card2).ToList();
                    if (list.Count > 0)
                    {
                        SelectCardInteraction interaction = new SelectCardInteraction(1, ultvalue, list, SelectedCardHandling.DoNothing)
                        {
                            Source = this
                        };
                        yield return new InteractionAction(interaction, false);
                        foreach (Card card in interaction.SelectedCards)
                        {
                            yield return new MoveCardAction(card, CardZone.Hand);
                        }
                        interaction = null;
                    }
                }
                yield break;
            }
        }
    }
}
