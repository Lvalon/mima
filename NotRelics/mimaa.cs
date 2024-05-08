using System;
using System.Collections.Generic;
using LBoL.ConfigData;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using LBoLEntitySideloader.Attributes;
using UnityEngine;
using LBoL.Base;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle;
using LBoL.Core;
using LBoL.Core.Units;
using LBoL.Base.Extensions;
using System.Linq;
using LBoL.Core.Randoms;
using LBoL.Core.Stations;
using LBoL.Core.Cards;
using lvalonmima.NotImages;

namespace lvalonmima.NotRelics
{
    public sealed class mimaadef : ExhibitTemplate
    {
        public override IdContainer GetId()
        {
            return nameof(mimaa);
        }

        public override LocalizationOption LoadLocalization()
        {
            return BepinexPlugin.exbatchloc.AddEntity(this);
        }

        public override ExhibitSprites LoadSprite()
        {
            // embedded resource folders are separated by a dot
            // this means exhibit image name must be exhibitid.png
            string folder = "Resources.";
            ExhibitSprites exhibitSprites = new ExhibitSprites();
            Func<string, Sprite> wrap = (s) => ResourceLoader.LoadSprite(folder + GetId() + s + ".png", BepinexPlugin.embeddedSource);
            exhibitSprites.main = wrap("");
            return exhibitSprites;
        }

        public override ExhibitConfig MakeConfig()
        {
            ExhibitConfig exhibitConfig = new ExhibitConfig(
                Index: BepinexPlugin.sequenceTable.Next(typeof(CardConfig)),
                Id: "",
                Order: 1000,
                IsDebug: false,
                IsPooled: false,
                IsSentinel: false,
                Revealable: false,
                Appearance: AppearanceType.Nowhere,
                Owner: "Mima",
                LosableType: ExhibitLosableType.CantLose,
                Rarity: Rarity.Mythic,
                Value1: 3,
                Value2: 1,
                Value3: 2,
                Mana: new ManaGroup() { Philosophy = 1 },
                BaseManaRequirement: null,
                BaseManaColor: null,
                BaseManaAmount: 0,
                HasCounter: false,
                InitialCounter: null,
                Keywords: Keyword.Philosophy,
                RelativeEffects: new List<string>() { nameof(SE.evilspiritdef.evilspirit), nameof(SE.transcendeddef.transcended), nameof(SE.magicalburstdef.magicalburst) },
                RelativeCards: new List<string>() { }
            );
            return exhibitConfig;
        }

        [EntityLogic(typeof(mimaadef))]
        public sealed class mimaa : mimaextensions.mimasexhibit
        {
            private int oghp;
            private int ogmax;
            private int ogdmg;

            public ManaGroup losemana { get; set; } = new ManaGroup() { Colorless = 1 };

            protected override void OnAdded(PlayerUnit player)
            {
                GameRunController gameRun = GameRun;
                int rewardAndShopCardColorLimitFlag = gameRun.RewardAndShopCardColorLimitFlag + 1;
                gameRun.RewardAndShopCardColorLimitFlag = rewardAndShopCardColorLimitFlag;
                HandleGameRunEvent(GameRun.StationEntered, delegate (StationEventArgs args)
                {
                    EntryStation entryStation = args.Station as EntryStation;
                    if (entryStation != null && GameRun.Stages.IndexOf(entryStation.Stage) == 0)
                    {
                        int addcard = 12;
                        gameRun.RemoveDeckCards(from card in GameRun.BaseDeck select card, false);
                        List<Card> card1 = new List<Card>
                        {
                            Library.CreateCard<NotImages.Starter.cardchannellingdef.cardchannelling>(),
                            Library.CreateCard<NotImages.Starter.cardmountaindef.cardmountain>(),
                            Library.CreateCard<NotImages.Starter.cardreminidef.cardremini>(),
                            Library.CreateCard<NotImages.Starter.carderosiondef.carderosion>()
                        };
                        GameRun.AddDeckCards(card1, false, null);
                        for (int i = 0; i < addcard; i++)
                        {
                            Card[] cards = toolbox.RollCardsCustom(gameRun.CardRng, new CardWeightTable(RarityWeightTable.EnemyCard, OwnerWeightTable.OnlyPlayer, CardTypeWeightTable.OnlySkill), 1, null, true, true, false, false, (Card card) => card is mimaextensions.mimacard mimacard && mimacard.Config.Rarity != Rarity.Rare && !mimacard.ispassive && !mimacard.isblitz);
                            gameRun.AddDeckCards(cards, false, null);
                        }
                        //fallback to no rarity limit
                        if (GameRun.BaseDeck.Count < addcard + 4)
                        {
                            int j = addcard + 4 - GameRun.BaseDeck.Count;
                            for (int i = 0; i < j; i++)
                            {
                                Card[] card2 = toolbox.RollCardsCustom(gameRun.CardRng, new CardWeightTable(RarityWeightTable.EnemyCard, OwnerWeightTable.OnlyPlayer, CardTypeWeightTable.OnlySkill), 1, null, true, true, false, false, (Card card) => card is mimaextensions.mimacard mimacard && !mimacard.ispassive && !mimacard.isblitz);
                                gameRun.AddDeckCards(card2, false, null);
                            }
                            BepinexPlugin.log.LogDebug("mimaa_fallback: fallback to no rarity limit");
                        }
                        //fallback to no rarity and type limit
                        if (GameRun.BaseDeck.Count < addcard + 4)
                        {
                            int k = addcard + 4 - GameRun.BaseDeck.Count;
                            for (int i = 0; i < k; i++)
                            {
                                Card[] card3 = toolbox.RollCardsCustom(gameRun.CardRng, new CardWeightTable(RarityWeightTable.EnemyCard, OwnerWeightTable.OnlyPlayer, CardTypeWeightTable.CanBeLoot), 1, null, true, true, false, false, (Card card) => card is mimaextensions.mimacard mimacard && !mimacard.ispassive && !mimacard.isblitz);
                                gameRun.AddDeckCards(card3, false, null);
                            }
                            BepinexPlugin.log.LogDebug("mimaa_fallback: fallback to no rarity and type limit");
                        }
                    }
                });
            }
            protected override void OnEnterBattle()
            {
                ReactBattleEvent(Battle.BattleStarted, new EventSequencedReactor<GameEventArgs>(OnBattleStarted));
                HandleBattleEvent(Battle.Player.DamageTaking, new GameEventHandler<DamageEventArgs>(OnPlayerDamageTaking));
                HandleBattleEvent(Battle.Player.DamageReceived, new GameEventHandler<DamageEventArgs>(OnPlayerDamageReceived));
                ReactBattleEvent(Battle.CardUsed, new EventSequencedReactor<CardUsingEventArgs>(OnCardUsed));
                ReactBattleEvent(Owner.TurnStarted, new EventSequencedReactor<UnitEventArgs>(OnOwnerTurnStarted));
            }

            private IEnumerable<BattleAction> OnCardUsed(CardUsingEventArgs args)
            {
                if (Owner.IsInTurn && args.Card.CardType == CardType.Skill)
                {
                    NotifyActivating();
                    yield return new ApplyStatusEffectAction<SE.magicalburstdef.magicalburst>(Owner, new int?(Value3), null, null, null, 0f, true);
                }
                yield break;
            }

            private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs args)
            {
                NotifyActivating();
                yield return new ApplyStatusEffectAction<SE.evilspiritdef.evilspirit>(Owner, new int?(Value1), null, null, null, 0f, true);
                yield break;
            }

            private void OnPlayerDamageTaking(DamageEventArgs args)
            {
                oghp = Owner.Hp;
                ogmax = Owner.MaxHp;
                ogdmg = args.DamageInfo.Damage.RoundToInt();
            }

            private void OnPlayerDamageReceived(DamageEventArgs args)
            {
                if (Owner.Hp == Owner.MaxHp && ogdmg > 0 && ((oghp - ogdmg != Owner.Hp - ogdmg) || ogdmg >= ogmax))
                {
                    NotifyActivating();
                    React(new ApplyStatusEffectAction<SE.transcendeddef.transcended>(Owner, new int?(Value2), null, null, null, 0f, true));
                }
            }

            private IEnumerable<BattleAction> OnOwnerTurnStarted(UnitEventArgs args)
            {
                NotifyActivating();
                ManaGroup value = ManaGroup.Single(ManaColors.Colors.Sample(GameRun.BattleRng));
                yield return new GainManaAction(value);
                yield break;
            }
        }
    }
}
