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
using LBoL.Core.Cards;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Randoms;
using System.Collections;
using LBoL.Core.Stations;
using LBoL.Presentation;

namespace lvalonmima.NotRelics
{
    public sealed class mimabdef : ExhibitTemplate
    {
        public override IdContainer GetId()
        {
            return nameof(mimab);
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
                Value3: 3,
                Mana: new ManaGroup() { Philosophy = 1 },
                BaseManaRequirement: null,
                BaseManaColor: ManaColor.Philosophy,
                BaseManaAmount: 1,
                HasCounter: false,
                InitialCounter: null,
                Keywords: Keyword.Philosophy,
                RelativeEffects: new List<string>() { nameof(SE.evilspiritdef.evilspirit), nameof(SE.sepassiveshopdef.sepassiveshop), nameof(SE.seblitzshopdef.seblitzshop) },
                RelativeCards: new List<string>() { }
            );
            return exhibitConfig;
        }

        [EntityLogic(typeof(mimabdef))]
        public sealed class mimab : mimaextensions.mimasexhibit
        {
            private int oghp;
            private int ogmax;
            private int ogdmg;

            public int Value4 => 10;

            protected override void OnAdded(PlayerUnit player)
            {
                GameRunController gameRun = GameRun;
                int rewardAndShopCardColorLimitFlag = gameRun.RewardAndShopCardColorLimitFlag + 1;
                gameRun.RewardAndShopCardColorLimitFlag = rewardAndShopCardColorLimitFlag;
                HandleGameRunEvent(GameRun.StationEntered, new GameEventHandler<StationEventArgs>(OnGamerunStationEntered));
            }
            private void OnGamerunStationEntered(StationEventArgs args)
            {
                EntryStation entryStation = args.Station as EntryStation;
                if (entryStation != null && GameRun.Stages.IndexOf(entryStation.Stage) == 0)
                {
                    GameRunController gameRun = GameRun;
                    gameRun.RemoveDeckCards(from card in GameRun.BaseDeck select card, false);
                    GameMaster.Instance.StartCoroutine(Draft());
                }
            }
            private IEnumerator Draft()
            {
                int draftamount = 3;
                Card[] array1 = toolbox.RollCardsCustom(GameMaster.Instance.CurrentGameRun.BattleCardRng, new CardWeightTable(RarityWeightTable.EnemyCard, OwnerWeightTable.AllOnes, CardTypeWeightTable.CanBeLoot), draftamount, null, true, true, false, false, (Card card) => card is mimaextensions.mimacard mimascard && card.Config.Rarity != Rarity.Rare);
                //Card[] array1 = GameRun.RollCards(GameRun.CardRng, new CardWeightTable(RarityWeightTable.EnemyCard, OwnerWeightTable.OnlyPlayer, CardTypeWeightTable.CanBeLoot), draftamount, false, false, (CardConfig config) => config.Rarity != Rarity.Rare);
                GameRun.UpgradeNewDeckCardOnFlags(array1);
                MiniSelectCardInteraction interaction1 = new MiniSelectCardInteraction(array1, false, false, false)
                {
                    Source = this,
                    CanCancel = false
                };
                yield return GameRun.InteractionViewer.View(interaction1);
                GameRun.AddDeckCards(new Card[] { interaction1.SelectedCard }, false, null);
                Card[] array2 = toolbox.RollCardsCustom(GameMaster.Instance.CurrentGameRun.BattleCardRng, new CardWeightTable(RarityWeightTable.EnemyCard, OwnerWeightTable.AllOnes, CardTypeWeightTable.CanBeLoot), draftamount, null, true, true, false, false, (Card card) => card is mimaextensions.mimacard mimascard && card.Config.Rarity != Rarity.Rare);
                GameRun.UpgradeNewDeckCardOnFlags(array2);
                MiniSelectCardInteraction interaction2 = new MiniSelectCardInteraction(array2, false, false, false)
                {
                    Source = this,
                    CanCancel = false
                };
                yield return GameRun.InteractionViewer.View(interaction2);
                GameRun.AddDeckCards(new Card[] { interaction2.SelectedCard }, false, null);
                Card[] array3 = toolbox.RollCardsCustom(GameMaster.Instance.CurrentGameRun.BattleCardRng, new CardWeightTable(RarityWeightTable.EnemyCard, OwnerWeightTable.AllOnes, CardTypeWeightTable.CanBeLoot), draftamount, null, true, true, false, false, (Card card) => card is mimaextensions.mimacard mimascard && card.Config.Rarity != Rarity.Rare);
                GameRun.UpgradeNewDeckCardOnFlags(array3);
                MiniSelectCardInteraction interaction3 = new MiniSelectCardInteraction(array3, false, false, false)
                {
                    Source = this,
                    CanCancel = false
                };
                yield return GameRun.InteractionViewer.View(interaction3);
                GameRun.AddDeckCards(new Card[] { interaction3.SelectedCard }, false, null);
                Card[] array4 = toolbox.RollCardsCustom(GameMaster.Instance.CurrentGameRun.BattleCardRng, new CardWeightTable(RarityWeightTable.EnemyCard, OwnerWeightTable.AllOnes, CardTypeWeightTable.CanBeLoot), draftamount, null, true, true, false, false, (Card card) => card is mimaextensions.mimacard mimascard && card.Config.Rarity != Rarity.Rare);
                GameRun.UpgradeNewDeckCardOnFlags(array4);
                MiniSelectCardInteraction interaction4 = new MiniSelectCardInteraction(array4, false, false, false)
                {
                    Source = this,
                    CanCancel = false
                };
                yield return GameRun.InteractionViewer.View(interaction4);
                GameRun.AddDeckCards(new Card[] { interaction4.SelectedCard }, false, null);
                Card[] array5 = toolbox.RollCardsCustom(GameMaster.Instance.CurrentGameRun.BattleCardRng, new CardWeightTable(RarityWeightTable.EnemyCard, OwnerWeightTable.AllOnes, CardTypeWeightTable.CanBeLoot), draftamount, null, true, true, false, false, (Card card) => card is mimaextensions.mimacard mimascard && card.Config.Rarity != Rarity.Rare);
                GameRun.UpgradeNewDeckCardOnFlags(array5);
                MiniSelectCardInteraction interaction5 = new MiniSelectCardInteraction(array5, false, false, false)
                {
                    Source = this,
                    CanCancel = false
                };
                yield return GameRun.InteractionViewer.View(interaction5);
                GameRun.AddDeckCards(new Card[] { interaction5.SelectedCard }, false, null);
                Card[] array6 = toolbox.RollCardsCustom(GameMaster.Instance.CurrentGameRun.BattleCardRng, new CardWeightTable(RarityWeightTable.EnemyCard, OwnerWeightTable.AllOnes, CardTypeWeightTable.CanBeLoot), draftamount, null, true, true, false, false, (Card card) => card is mimaextensions.mimacard mimascard && card.Config.Rarity != Rarity.Rare);
                GameRun.UpgradeNewDeckCardOnFlags(array6);
                MiniSelectCardInteraction interaction6 = new MiniSelectCardInteraction(array6, false, false, false)
                {
                    Source = this,
                    CanCancel = false
                };
                yield return GameRun.InteractionViewer.View(interaction6);
                GameRun.AddDeckCards(new Card[] { interaction6.SelectedCard }, false, null);
                Card[] array7 = toolbox.RollCardsCustom(GameMaster.Instance.CurrentGameRun.BattleCardRng, new CardWeightTable(RarityWeightTable.EnemyCard, OwnerWeightTable.AllOnes, CardTypeWeightTable.CanBeLoot), draftamount, null, true, true, false, false, (Card card) => card is mimaextensions.mimacard mimascard && card.Config.Rarity != Rarity.Rare);
                GameRun.UpgradeNewDeckCardOnFlags(array7);
                MiniSelectCardInteraction interaction7 = new MiniSelectCardInteraction(array7, false, false, false)
                {
                    Source = this,
                    CanCancel = false
                };
                yield return GameRun.InteractionViewer.View(interaction7);
                GameRun.AddDeckCards(new Card[] { interaction7.SelectedCard }, false, null);
                Card[] array8 = toolbox.RollCardsCustom(GameMaster.Instance.CurrentGameRun.BattleCardRng, new CardWeightTable(RarityWeightTable.EnemyCard, OwnerWeightTable.AllOnes, CardTypeWeightTable.CanBeLoot), draftamount, null, true, true, false, false, (Card card) => card is mimaextensions.mimacard mimascard && card.Config.Rarity != Rarity.Rare);
                GameRun.UpgradeNewDeckCardOnFlags(array8);
                MiniSelectCardInteraction interaction8 = new MiniSelectCardInteraction(array8, false, false, false)
                {
                    Source = this,
                    CanCancel = false
                };
                yield return GameRun.InteractionViewer.View(interaction8);
                GameRun.AddDeckCards(new Card[] { interaction8.SelectedCard }, false, null);
                Card[] array9 = toolbox.RollCardsCustom(GameMaster.Instance.CurrentGameRun.BattleCardRng, new CardWeightTable(RarityWeightTable.EnemyCard, OwnerWeightTable.AllOnes, CardTypeWeightTable.CanBeLoot), draftamount, null, true, true, false, false, (Card card) => card is mimaextensions.mimacard mimascard && card.Config.Rarity != Rarity.Rare);
                GameRun.UpgradeNewDeckCardOnFlags(array9);
                MiniSelectCardInteraction interaction9 = new MiniSelectCardInteraction(array9, false, false, false)
                {
                    Source = this,
                    CanCancel = false
                };
                yield return GameRun.InteractionViewer.View(interaction9);
                GameRun.AddDeckCards(new Card[] { interaction9.SelectedCard }, false, null);
                Card[] array10 = toolbox.RollCardsCustom(GameMaster.Instance.CurrentGameRun.BattleCardRng, new CardWeightTable(RarityWeightTable.EnemyCard, OwnerWeightTable.AllOnes, CardTypeWeightTable.CanBeLoot), draftamount, null, true, true, false, false, (Card card) => card is mimaextensions.mimacard mimascard && card.Config.Rarity != Rarity.Rare);
                GameRun.UpgradeNewDeckCardOnFlags(array10);
                MiniSelectCardInteraction interaction10 = new MiniSelectCardInteraction(array10, false, false, false)
                {
                    Source = this,
                    CanCancel = false
                };
                yield return GameRun.InteractionViewer.View(interaction10);
                GameRun.AddDeckCards(new Card[] { interaction10.SelectedCard }, false, null);
                yield break;
            }
            protected override void OnEnterBattle()
            {
                ReactBattleEvent(Battle.BattleStarted, new EventSequencedReactor<GameEventArgs>(OnBattleStarted));
                HandleBattleEvent(Battle.Player.DamageTaking, new GameEventHandler<DamageEventArgs>(OnPlayerDamageTaking));
                HandleBattleEvent(Battle.Player.DamageReceived, new GameEventHandler<DamageEventArgs>(OnPlayerDamageReceived));
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
                }
            }
        }
    }
}
