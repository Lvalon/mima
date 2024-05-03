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
using static lvalonmima.SE.magicalburstdef;
using LBoL.Core.Cards;
using static lvalonmima.mimaextensions;

namespace lvalonmima.NotRelics
{
    public sealed class mimapassivesdef : ExhibitTemplate
    {
        public override IdContainer GetId()
        {
            return nameof(mimapassives);
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
                Value1: 0,
                Value2: 0,
                Value3: 0,
                Mana: new ManaGroup() { },
                BaseManaRequirement: null,
                BaseManaColor: null,
                BaseManaAmount: 0,
                HasCounter: false,
                InitialCounter: null,
                Keywords: Keyword.None,
                RelativeEffects: new List<string>() { },
                RelativeCards: new List<string>() { }
            );
            return exhibitConfig;
        }

        [EntityLogic(typeof(mimapassivesdef))]
        public sealed class mimapassives : mimaextensions.mimasexhibit
        {
            public int passivegold = 0;
            public int passivepower = 0;
            public int passivemb = 0;
            public int passivembhand = 0;
            public mimapassives() : base()
            {
            }

            protected override string LocalizeProperty(string key, bool decorated = false, bool required = true)
            {
                return TypeFactory<Exhibit>.LocalizeProperty(Id, key, decorated, required);
            }
            //playername for future use
            public string theplayername
            {
                get
                {
                    if (GameRun == null) { return "The Player"; }
                    else
                    {
                        return "<color=" + GameRun.Player.Config.NarrativeColor + ">" + GameRun.Player.Name + "</color>";
                    }
                }
            }

            //START OF PASSIVES
            public string goldyaml
            {
                get
                {
                    if (passivegold > 0) { return Convert.ToString(passivegold); } //StringDecorator.Decorate(Convert.ToString("|k:" + passivegold + "|") + "<sprite=\"Point\" name=\"Gold\">");
                    else return "";
                }
            }
            public string poweryaml
            {
                get
                {
                    if (passivepower > 0) { return Convert.ToString(passivepower); } //StringDecorator.Decorate(Convert.ToString("|k:" + passivegold + "|") + "<sprite=\"Point\" name=\"Gold\">");
                    else return "";
                }
            }
            public string mbyaml
            {
                get
                {
                    if (passivemb > 0) { return Convert.ToString(passivemb); } //StringDecorator.Decorate(Convert.ToString("|k:" + passivegold + "|") + "<sprite=\"Point\" name=\"Gold\">");
                    else return "";
                }
            }
            public string mbhandyaml
            {
                get
                {
                    if (passivembhand > 0) { return Convert.ToString(passivembhand); } //StringDecorator.Decorate(Convert.ToString("|k:" + passivegold + "|") + "<sprite=\"Point\" name=\"Gold\">");
                    else return "";
                }
            }

            public string passivegoldyaml
            {
                get
                {
                    string res = "";
                    if (passivegold > 0)
                    {
                        try
                        {
                            res = LocalizeProperty(key: "passivegolddes", decorated: true, required: true).RuntimeFormat(FormatWrapper);
                        }
                        catch (Exception)
                        {
                            res = "<Error>";
                        }
                    }
                    return res;
                }
            }
            public string passivepoweryaml
            {
                get
                {
                    string res = "";
                    if (passivepower > 0)
                    {
                        try
                        {
                            res = LocalizeProperty(key: "passivepowerdes", decorated: true, required: true).RuntimeFormat(FormatWrapper);
                        }
                        catch (Exception)
                        {
                            res = "<Error>";
                        }
                    }
                    return res;
                }
            }
            public string passivembyaml
            {
                get
                {
                    string res = "";
                    if (passivemb > 0)
                    {
                        try
                        {
                            res = LocalizeProperty(key: "passivembdes", decorated: true, required: true).RuntimeFormat(FormatWrapper);
                        }
                        catch (Exception)
                        {
                            res = "<Error>";
                        }
                    }
                    return res;
                }
            }
            public string passivembhandyaml
            {
                get
                {
                    string res = "";
                    if (passivembhand > 0)
                    {
                        try
                        {
                            res = LocalizeProperty(key: "passivembhanddes", decorated: true, required: true).RuntimeFormat(FormatWrapper);
                        }
                        catch (Exception)
                        {
                            res = "<Error>";
                        }
                    }
                    return res;
                }
            }

            protected override void OnEnterBattle()
            {
                base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarting, new EventSequencedReactor<GameEventArgs>(this.OnBattleStarting));
                HandleBattleEvent(base.Battle.BattleEnded, OnBattleEnding);
                ReactBattleEvent<UnitEventArgs>(base.Owner.TurnStarting, new EventSequencedReactor<UnitEventArgs>(this.OnTurnStarting));
                ReactBattleEvent<CardMovingEventArgs>(base.Battle.CardMoved, new EventSequencedReactor<CardMovingEventArgs>(this.OnCardMoved));
                ReactBattleEvent<CardEventArgs>(base.Battle.CardDrawn, new EventSequencedReactor<CardEventArgs>(this.OnCardDrawn));
                ReactBattleEvent<CardsEventArgs>(base.Battle.CardsAddedToHand, new EventSequencedReactor<CardsEventArgs>(this.OnCardsAddedToHand));
            }

            IEnumerable<BattleAction> OnBattleStarting(GameEventArgs gameEventArgs)
            {
                if (passivegold > 0)
                {
                    base.NotifyActivating();
                    yield return new GainMoneyAction(passivegold, SpecialSourceType.None);
                }
                if (passivepower > 0)
                {
                    base.NotifyActivating();
                    yield return new GainPowerAction(passivepower);
                }
                yield break;
            }
            private IEnumerable<BattleAction> OnTurnStarting(UnitEventArgs args)
            {
                if (passivemb > 0)
                {
                    base.NotifyActivating();
                    yield return new ApplyStatusEffectAction<magicalburst>(base.Owner, new int?(passivemb), null, null, null, 0f, true);
                }
                yield break;
            }
            private void OnBattleEnding(GameEventArgs args)
            {
            }

            protected override void OnAdded(PlayerUnit player)
            {
                HandleGameRunEvent<CardsEventArgs>(GameRun.DeckCardsAdded, new GameEventHandler<CardsEventArgs>(this.OnDeckCardsAdded));
            }
            private IEnumerable<BattleAction> OnCardMoved(CardMovingEventArgs args)
            {
                if (args.DestinationZone == CardZone.Hand && passivembhand > 0)
                {
                    base.NotifyActivating();
                    yield return new ApplyStatusEffectAction<magicalburst>(base.Owner, new int?(passivembhand), null, null, null, 0f, true);
                }
                yield break;
            }
            private IEnumerable<BattleAction> OnCardDrawn(CardEventArgs args)
            {
                if (passivembhand > 0)
                {
                    base.NotifyActivating();
                    yield return new ApplyStatusEffectAction<magicalburst>(base.Owner, new int?(passivembhand), null, null, null, 0f, true);
                }
                yield break;
            }
            private IEnumerable<BattleAction> OnCardsAddedToHand(CardsEventArgs args)
            {
                if (passivembhand > 0)
                {
                    base.NotifyActivating();
                    yield return new ApplyStatusEffectAction<magicalburst>(base.Owner, new int?(passivembhand), null, null, null, 0f, true);
                }
                yield break;
            }
            private void OnDeckCardsAdded(CardsEventArgs args)
            {
                foreach (Card card in args.Cards)
                {
                    if (card is mimacard mimascard && mimascard.ispassive)
                    {
                        switch (card.Id)
                        {
                            case "cardpassivegold":
                                passivegold += card.Value1;
                                break;
                            case "cardpassivepower":
                                passivepower += card.Value1;
                                break;
                            case "cardpassivemb":
                                passivemb += card.Value1;
                                break;
                            case "cardpassivembhand":
                                passivembhand += card.Value1;
                                break;
                        }
                        GameRun.RemoveDeckCard(card, false);
                    }
                }
            }
        }
    }
}
