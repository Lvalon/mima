using System;
using System.Collections.Generic;
using LBoL.ConfigData;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using LBoLEntitySideloader.Attributes;
using static lvalonmima.BepinexPlugin;
using UnityEngine;
using LBoL.Base;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle;
using LBoL.Core;
using LBoL.Core.Units;
using static lvalonmima.SE.evilspiritdef;
using LBoL.Base.Extensions;
using System.Linq;
using LBoL.Core.Randoms;
using static lvalonmima.SE.transcendeddef;
using static lvalonmima.SE.magicalburstdef;
using static lvalonmima.NotImages.Starter.cardchannellingdef;
using LBoL.Core.Stations;
using LBoL.Core.Cards;
using static lvalonmima.NotImages.Starter.cardmountaindef;
using static lvalonmima.NotImages.Starter.cardreminidef;
using static lvalonmima.NotImages.Starter.carderosiondef;

namespace lvalonmima.NotRelics
{
    public sealed class mimaadef : ExhibitTemplate
    {
        public override IdContainer GetId()
        {
            return nameof(mimaa);
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
                Value3: 2,
                Mana: new ManaGroup() { Philosophy = 1 },
                BaseManaRequirement: null,
                BaseManaColor: null,
                BaseManaAmount: 0,
                HasCounter: false,
                InitialCounter: null,
                Keywords: Keyword.Philosophy,
                RelativeEffects: new List<string>() { nameof(evilspirit), nameof(transcended), nameof(magicalburst) },
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
                GameRunController gameRun = base.GameRun;
                int rewardAndShopCardColorLimitFlag = gameRun.RewardAndShopCardColorLimitFlag + 1;
                gameRun.RewardAndShopCardColorLimitFlag = rewardAndShopCardColorLimitFlag;
                base.HandleGameRunEvent<StationEventArgs>(base.GameRun.StationEntered, delegate (StationEventArgs args)
                {
                    EntryStation entryStation = args.Station as EntryStation;
                    if (entryStation != null && base.GameRun.Stages.IndexOf(entryStation.Stage) == 0)
                    {
                        int addcard = 12;
                        gameRun.RemoveDeckCards(from card in GameRun.BaseDeck select card, false);
                        List<Card> card1 = new List<Card>
                        {
                            Library.CreateCard<cardchannelling>(),
                            Library.CreateCard<cardmountain>(),
                            Library.CreateCard<cardremini>(),
                            Library.CreateCard<carderosion>()
                        };
                        base.GameRun.AddDeckCards(card1, false, null);
                        for (int i = 0; i < addcard; i++)
                        {
                            Card[] cards = gameRun.RollCards(gameRun.CardRng, new CardWeightTable(RarityWeightTable.EnemyCard, OwnerWeightTable.OnlyPlayer, CardTypeWeightTable.OnlySkill), 1, false, false, (CardConfig config) => config.Rarity != Rarity.Rare);
                            gameRun.AddDeckCards(cards, false, null);
                        }
                        //fallback to no rarity limit
                        if (GameRun.BaseDeck.Count < addcard + 4)
                        {
                            int j = (addcard + 4 - GameRun.BaseDeck.Count);
                            for (int i = 0; i < j; i++)
                            {
                                Card[] card2 = gameRun.RollCards(gameRun.CardRng, new CardWeightTable(RarityWeightTable.EnemyCard, OwnerWeightTable.OnlyPlayer, CardTypeWeightTable.OnlySkill), 1, false, false, null);
                                gameRun.AddDeckCards(card2, false, null);
                            }
                        }
                        //fallback to no rarity and type limit
                        if (GameRun.BaseDeck.Count < addcard + 4)
                        {
                            int k = (addcard + 4 - GameRun.BaseDeck.Count);
                            for (int i = 0; i < k; i++)
                            {
                                Card[] card2 = gameRun.RollCards(gameRun.CardRng, new CardWeightTable(RarityWeightTable.OnlyRare, OwnerWeightTable.OnlyPlayer, CardTypeWeightTable.CanBeLoot), 1, false, false, null);
                                gameRun.AddDeckCards(card2, false, null);
                            }
                        }
                    }

                    //if (GameRun.BaseMana.Total > 0)
                    //{
                    //    log.LogDebug("HERE WE GO");
                    //    ManaGroup pendingMana = GameRun.BaseMana.With(null, null, null, null, null, null, null, null);
                    //    SelectBaseManaInteraction interaction = new SelectBaseManaInteraction(1, 1, pendingMana)
                    //    {
                    //        Description = "Select Base Mana to remove",
                    //        CanCancel = false
                    //    };
                    //    GameRun.InteractionViewer.View(interaction);
                    //    ManaGroup selectedMana = interaction.SelectedMana;
                    //    GameRun.SetBaseMana(base.GameRun.BaseMana - selectedMana, true);
                    //}
                });
            }

            //class CoroutineExtender : IEnumerable
            //{
            //    public IEnumerator target_enumerator;
            //    public List<IEnumerator> preItems = new List<IEnumerator>();
            //    public List<IEnumerator> postItems = new List<IEnumerator>();
            //    public List<IEnumerator> midItems = new List<IEnumerator>();


            //    public CoroutineExtender() { }

            //    public CoroutineExtender(IEnumerator target_enumerator) { this.target_enumerator = target_enumerator; }

            //    public IEnumerator GetEnumerator()
            //    {
            //        foreach (var e in preItems) yield return e;
            //        int i = 0;
            //        while (target_enumerator.MoveNext())
            //        {
            //            yield return target_enumerator.Current;
            //            i++;
            //        }
            //        foreach (var e in postItems) yield return e;
            //    }
            //}

            //[HarmonyPatch(typeof(Exhibit), "TriggerGain")]
            //[HarmonyDebug]
            //class Exhibit_Patch
            //{
            //    static void Postfix(ref IEnumerator __result)
            //    {
            //        var extendedRez = new CoroutineExtender(__result);

            //        extendedRez.postItems.Add(yolo());

            //        __result = extendedRez.GetEnumerator();
            //    }
            //}


            //    static IEnumerator yolo()
            //{
            //    var run = GameMaster.Instance.CurrentGameRun;
            //    Exhibit exhibit = run.Player.GetExhibit<mimapassives>();
            //    if (GameMaster.Instance != null && GameMaster.Instance.CurrentGameRun != null && exhibit != null && exhibit is mimapassives mimapassive)
            //    {
            //        if (run.BaseMana.Colorless > 0)
            //        {
            //            run.LoseBaseMana(ManaGroup.Colorlesses(1), false);
            //        }
            //    }
            //    yield break;
            //}

            protected override void OnEnterBattle()
            {
                base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new EventSequencedReactor<GameEventArgs>(this.OnBattleStarted));
                base.HandleBattleEvent<DamageEventArgs>(base.Battle.Player.DamageTaking, new GameEventHandler<DamageEventArgs>(this.OnPlayerDamageTaking));
                base.HandleBattleEvent<DamageEventArgs>(base.Battle.Player.DamageReceived, new GameEventHandler<DamageEventArgs>(this.OnPlayerDamageReceived));
                base.ReactBattleEvent<CardUsingEventArgs>(base.Battle.CardUsed, new EventSequencedReactor<CardUsingEventArgs>(this.OnCardUsed));
                base.ReactBattleEvent<UnitEventArgs>(base.Owner.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnOwnerTurnStarted));
            }

            private IEnumerable<BattleAction> OnCardUsed(CardUsingEventArgs args)
            {
                if (base.Owner.IsInTurn && args.Card.CardType == CardType.Skill)
                {
                    base.NotifyActivating();
                    yield return new ApplyStatusEffectAction<magicalburst>(base.Owner, new int?(base.Value3), null, null, null, 0f, true);
                }
                yield break;
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
                    React(new ApplyStatusEffectAction<transcended>(base.Owner, new int?(base.Value2), null, null, null, 0f, true));
                }
            }

            private IEnumerable<BattleAction> OnOwnerTurnStarted(UnitEventArgs args)
            {
                base.NotifyActivating();
                ManaGroup value = ManaGroup.Single(ManaColors.Colors.Sample(base.GameRun.BattleRng));
                yield return new GainManaAction(value);
                yield break;
            }
        }
    }
}
