using System;
using System.Collections.Generic;
using System.Text;
using LBoL.ConfigData;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using LBoLEntitySideloader.Attributes;
using static lvalonmima.BepinexPlugin;
using Mono.Cecil;
using UnityEngine;
using LBoL.Base;
using LBoL.EntityLib.Exhibits;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle;
using LBoL.Core.StatusEffects;
using LBoL.Core;
using LBoL.Core.Units;
using static lvalonmima.SE.evilspiritdef;
using LBoL.Base.Extensions;
using LBoL.EntityLib.StatusEffects.Enemy;
using System.Linq;
using static lvalonmima.SE.theabyssdef;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Enemy.SeijaItems;
using LBoL.EntityLib.Mixins;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Randoms;
using System.Collections;
using LBoL.Core.Stations;
using LBoL.Presentation;
using LBoLEntitySideloader.PersistentValues;
using LBoL.EntityLib.EnemyUnits.Lore;
using static lvalonmima.playermima;

namespace lvalonmima.NotRelics
{
    public sealed class mimabdef : ExhibitTemplate
    {
        public override IdContainer GetId()
        {
            return nameof(mimab);
        }

        public override LocalizationOption LoadLocalization() => exbatchloc.AddEntity(this);

        public override ExhibitSprites LoadSprite()
        {
            // embedded resource folders are separated by a dot
            // this means exhibit image name must be exhibitid.png
            var folder = "Resources.";
            var exhibitSprites = new ExhibitSprites();
            Func<string, Sprite> wrap = (s) => ResourceLoader.LoadSprite(folder + GetId() + s + ".png", embeddedSource);
            exhibitSprites.main = wrap("");
            return exhibitSprites;
        }

        public override ExhibitConfig MakeConfig()
        {
            var exhibitConfig = new ExhibitConfig(
                Index: sequenceTable.Next(typeof(CardConfig)),
                Id: "",
                Order: 10,
                IsDebug: false,
                IsPooled: false,
                IsSentinel: false,
                Revealable: false,
                Appearance: AppearanceType.Nowhere,
                Owner: "Mima", //"Mima"
                LosableType: ExhibitLosableType.CantLose,
                Rarity: Rarity.Mythic,
                Value1: 3,
                Value2: 1,
                Value3: 3,
                Mana: new ManaGroup() { Colorless = 1 },
                BaseManaRequirement: null,
                BaseManaColor: ManaColor.Colorless,
                BaseManaAmount: 1,
                HasCounter: false,
                InitialCounter: null,
                Keywords: Keyword.None,
                RelativeEffects: new List<string>() { "evilspirit", "theabyss" },
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

            protected override void OnAdded(PlayerUnit player)
            {
                GameRunController gameRun = base.GameRun;
                int rewardAndShopCardColorLimitFlag = gameRun.RewardAndShopCardColorLimitFlag + 1;
                gameRun.RewardAndShopCardColorLimitFlag = rewardAndShopCardColorLimitFlag;
                //base.GameRun.AdditionalRewardCardCount += base.Value3;
                gameRun.RemoveDeckCards(from card in GameRun.BaseDeck select card, false);
                HandleGameRunEvent(GameRun.StationEntered, new GameEventHandler<StationEventArgs>(OnGamerunStationEntered));
            }
            private void OnGamerunStationEntered(StationEventArgs args)
            {
                //if (args.Station.Type == StationType.Entry && args.Station.Stage.Level == 1)
                //{
                //    GameMaster.Instance.StartCoroutine(Draft());
                //}
                EntryStation entryStation = args.Station as EntryStation;
                if (entryStation != null && base.GameRun.Stages.IndexOf(entryStation.Stage) == 0)
                {
                    GameMaster.Instance.StartCoroutine(Draft());
                }
            }
            private IEnumerator Draft()
            {
                Card[] array1 = base.GameRun.RollCards(base.GameRun.CardRng, new CardWeightTable(RarityWeightTable.EnemyCard, OwnerWeightTable.OnlyPlayer, CardTypeWeightTable.CanBeLoot), 3, false, false, (CardConfig config) => config.Rarity != Rarity.Rare);
                base.GameRun.UpgradeNewDeckCardOnFlags(array1);
                MiniSelectCardInteraction interaction1 = new MiniSelectCardInteraction(array1, false, false, false)
                {
                    Source = this,
                    CanCancel = false
                };
                yield return base.GameRun.InteractionViewer.View(interaction1);
                base.GameRun.AddDeckCards(new Card[] { interaction1.SelectedCard }, false, null);
                Card[] array2 = base.GameRun.RollCards(base.GameRun.CardRng, new CardWeightTable(RarityWeightTable.EnemyCard, OwnerWeightTable.OnlyPlayer, CardTypeWeightTable.CanBeLoot), 3, false, false, (CardConfig config) => config.Rarity != Rarity.Rare);
                base.GameRun.UpgradeNewDeckCardOnFlags(array2);
                MiniSelectCardInteraction interaction2 = new MiniSelectCardInteraction(array2, false, false, false)
                {
                    Source = this,
                    CanCancel = false
                };
                yield return base.GameRun.InteractionViewer.View(interaction2);
                base.GameRun.AddDeckCards(new Card[] { interaction2.SelectedCard }, false, null);
                Card[] array3 = base.GameRun.RollCards(base.GameRun.CardRng, new CardWeightTable(RarityWeightTable.EnemyCard, OwnerWeightTable.OnlyPlayer, CardTypeWeightTable.CanBeLoot), 3, false, false, (CardConfig config) => config.Rarity != Rarity.Rare);
                base.GameRun.UpgradeNewDeckCardOnFlags(array3);
                MiniSelectCardInteraction interaction3 = new MiniSelectCardInteraction(array3, false, false, false)
                {
                    Source = this,
                    CanCancel = false
                };
                yield return base.GameRun.InteractionViewer.View(interaction3);
                base.GameRun.AddDeckCards(new Card[] { interaction3.SelectedCard }, false, null);
                Card[] array4 = base.GameRun.RollCards(base.GameRun.CardRng, new CardWeightTable(RarityWeightTable.EnemyCard, OwnerWeightTable.OnlyPlayer, CardTypeWeightTable.CanBeLoot), 3, false, false, (CardConfig config) => config.Rarity != Rarity.Rare);
                base.GameRun.UpgradeNewDeckCardOnFlags(array4);
                MiniSelectCardInteraction interaction4 = new MiniSelectCardInteraction(array4, false, false, false)
                {
                    Source = this,
                    CanCancel = false
                };
                yield return base.GameRun.InteractionViewer.View(interaction4);
                base.GameRun.AddDeckCards(new Card[] { interaction4.SelectedCard }, false, null);
                Card[] array5 = base.GameRun.RollCards(base.GameRun.CardRng, new CardWeightTable(RarityWeightTable.EnemyCard, OwnerWeightTable.OnlyPlayer, CardTypeWeightTable.CanBeLoot), 3, false, false, (CardConfig config) => config.Rarity != Rarity.Rare);
                base.GameRun.UpgradeNewDeckCardOnFlags(array5);
                MiniSelectCardInteraction interaction5 = new MiniSelectCardInteraction(array5, false, false, false)
                {
                    Source = this,
                    CanCancel = false
                };
                yield return base.GameRun.InteractionViewer.View(interaction5);
                base.GameRun.AddDeckCards(new Card[] { interaction5.SelectedCard }, false, null);
                Card[] array6 = base.GameRun.RollCards(base.GameRun.CardRng, new CardWeightTable(RarityWeightTable.EnemyCard, OwnerWeightTable.OnlyPlayer, CardTypeWeightTable.CanBeLoot), 3, false, false, (CardConfig config) => config.Rarity != Rarity.Rare);
                base.GameRun.UpgradeNewDeckCardOnFlags(array6);
                MiniSelectCardInteraction interaction6 = new MiniSelectCardInteraction(array6, false, false, false)
                {
                    Source = this,
                    CanCancel = false
                };
                yield return base.GameRun.InteractionViewer.View(interaction6);
                base.GameRun.AddDeckCards(new Card[] { interaction6.SelectedCard }, false, null);
                yield break;
            }
            protected override void OnEnterBattle()
            {
                base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new EventSequencedReactor<GameEventArgs>(this.OnBattleStarted));
                base.HandleBattleEvent<DamageEventArgs>(base.Battle.Player.DamageTaking, new GameEventHandler<DamageEventArgs>(this.OnPlayerDamageTaking));
                base.HandleBattleEvent<DamageEventArgs>(base.Battle.Player.DamageReceived, new GameEventHandler<DamageEventArgs>(this.OnPlayerDamageReceived));
            }

            private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs args)
            {
                base.NotifyActivating();
                yield return new ApplyStatusEffectAction<evilspirit>(base.Owner, new int?(base.Value1), null, null, null, 0f, true);
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
                    base.NotifyActivating();
                    React(new ApplyStatusEffectAction<theabyss>(base.Owner, new int?(base.Value2), null, null, null, 0f, true));
                }
            }
        }
    }
}
