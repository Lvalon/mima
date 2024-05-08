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
using LBoL.Core.Cards;

namespace lvalonmima.NotRelics
{
    public sealed class mimapassivesdef : ExhibitTemplate
    {
        public override IdContainer GetId()
        {
            return nameof(mimapassives);
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
                Order: 10,
                IsDebug: false,
                IsPooled: false,
                IsSentinel: false,
                Revealable: false,
                Appearance: AppearanceType.Nowhere,
                Owner: "Mima",
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
            public bool haspassive = false;
            public int passivegold = 0;
            public int passivepower = 0;
            public int passivemb = 0;
            public int passivembhand = 0;
            public int passiveimplosion = 0;
            public int passiveretribution = 0;
            public int passiveeverlast = 0;
            public mimapassives() : base()
            {
            }

            protected override string LocalizeProperty(string key, bool decorated = false, bool required = true)
            {
                return TypeFactory<Exhibit>.LocalizeProperty(Id, key, decorated, required);
            }
            //playername for future use
            public string theplayername => GameRun == null ? "The Player" : "<color=" + GameRun.Player.Config.NarrativeColor + ">" + GameRun.Player.Name + "</color>";

            //START OF PASSIVES
            public string goldyaml => passivegold > 0 ? Convert.ToString(passivegold) : "";
            public string poweryaml => passivepower > 0 ? Convert.ToString(passivepower) : "";
            public string mbyaml => passivemb > 0 ? Convert.ToString(passivemb) : "";
            public string mbhandyaml => passivembhand > 0 ? Convert.ToString(passivembhand) : "";
            public string implosionyaml => passiveimplosion > 0 ? Convert.ToString(passiveimplosion) : "";
            public string retributionyaml => passiveretribution > 0 ? Convert.ToString(passiveretribution) : "";
            public string everlastyaml => passiveeverlast > 0 ? (passiveeverlast > 2 ? "100" : Convert.ToString(passiveeverlast*50)) : "";

            public string spaceyaml
            {
                get
                {
                    string res = "";
                    if (haspassive)
                    {
                        try
                        {
                            res = LocalizeProperty(key: "spacedes", decorated: true, required: true).RuntimeFormat(FormatWrapper);
                        }
                        catch (Exception)
                        {
                            res = "<Error>";
                        }
                    }
                    return res;
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
            public string passiveimplosionyaml
            {
                get
                {
                    string res = "";
                    if (passiveimplosion > 0)
                    {
                        try
                        {
                            res = LocalizeProperty(key: "passiveimplosiondes", decorated: true, required: true).RuntimeFormat(FormatWrapper);
                        }
                        catch (Exception)
                        {
                            res = "<Error>";
                        }
                    }
                    return res;
                }
            }
            public string passiveretributionyaml
            {
                get
                {
                    string res = "";
                    if (passiveretribution > 0)
                    {
                        try
                        {
                            res = LocalizeProperty(key: "passiveretributiondes", decorated: true, required: true).RuntimeFormat(FormatWrapper);
                        }
                        catch (Exception)
                        {
                            res = "<Error>";
                        }
                    }
                    return res;
                }
            }
            public string passiveeverlastyaml
            {
                get
                {
                    string res = "";
                    if (passiveeverlast > 0)
                    {
                        try
                        {
                            res = LocalizeProperty(key: "passiveeverlastdes", decorated: true, required: true).RuntimeFormat(FormatWrapper);
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
                ReactBattleEvent(Battle.BattleStarting, new EventSequencedReactor<GameEventArgs>(OnBattleStarting));
                HandleBattleEvent(Battle.BattleEnded, OnBattleEnding);
                ReactBattleEvent<UnitEventArgs>(Owner.TurnStarting, new EventSequencedReactor<UnitEventArgs>(OnTurnStarting));
                ReactBattleEvent<CardMovingEventArgs>(Battle.CardMoved, new EventSequencedReactor<CardMovingEventArgs>(OnCardMoved));
                ReactBattleEvent<CardEventArgs>(Battle.CardDrawn, new EventSequencedReactor<CardEventArgs>(OnCardDrawn));
                ReactBattleEvent<CardsEventArgs>(Battle.CardsAddedToHand, new EventSequencedReactor<CardsEventArgs>(OnCardsAddedToHand));
            }

            private IEnumerable<BattleAction> OnBattleStarting(GameEventArgs gameEventArgs)
            {
                if (passivegold > 0)
                {
                    NotifyActivating();
                    yield return new GainMoneyAction(passivegold, SpecialSourceType.None);
                }
                if (passivepower > 0)
                {
                    NotifyActivating();
                    yield return new GainPowerAction(passivepower);
                }
                if (passiveimplosion > 0) {
                    NotifyActivating();
                    yield return new ApplyStatusEffectAction<SE.mburstmodifiers.implosiondef.implosion>(Owner, new int?(passiveimplosion), null, null, null, 0f, true);
                }
                if (passiveretribution > 0) {
                    NotifyActivating();
                    yield return new ApplyStatusEffectAction<SE.mburstmodifiers.retributiondef.retribution>(Owner, new int?(passiveretribution), null, null, null, 0f, true);
                }
                if (passiveeverlast > 0) {
                    NotifyActivating();
                    yield return new ApplyStatusEffectAction<SE.mburstmodifiers.everlastingmagicdef.everlastingmagic>(Owner, new int?(passiveeverlast), null, null, null, 0f, true);
                }
                yield break;
            }
            private IEnumerable<BattleAction> OnTurnStarting(UnitEventArgs args)
            {
                if (passivemb > 0)
                {
                    NotifyActivating();
                    yield return new ApplyStatusEffectAction<SE.magicalburstdef.magicalburst>(Owner, new int?(passivemb), null, null, null, 0f, true);
                }
                yield break;
            }
            private void OnBattleEnding(GameEventArgs args)
            {
            }

            protected override void OnAdded(PlayerUnit player)
            {
                HandleGameRunEvent<CardsEventArgs>(GameRun.DeckCardsAdded, new GameEventHandler<CardsEventArgs>(OnDeckCardsAdded));
            }
            private IEnumerable<BattleAction> OnCardMoved(CardMovingEventArgs args)
            {
                if (args.DestinationZone == CardZone.Hand && passivembhand > 0)
                {
                    NotifyActivating();
                    yield return new ApplyStatusEffectAction<SE.magicalburstdef.magicalburst>(Owner, new int?(passivembhand), null, null, null, 0f, true);
                }
                yield break;
            }
            private IEnumerable<BattleAction> OnCardDrawn(CardEventArgs args)
            {
                if (passivembhand > 0)
                {
                    NotifyActivating();
                    yield return new ApplyStatusEffectAction<SE.magicalburstdef.magicalburst>(Owner, new int?(passivembhand), null, null, null, 0f, true);
                }
                yield break;
            }
            private IEnumerable<BattleAction> OnCardsAddedToHand(CardsEventArgs args)
            {
                if (passivembhand > 0)
                {
                    NotifyActivating();
                    yield return new ApplyStatusEffectAction<SE.magicalburstdef.magicalburst>(Owner, new int?(passivembhand), null, null, null, 0f, true);
                }
                yield break;
            }
            private void OnDeckCardsAdded(CardsEventArgs args)
            {
                foreach (Card card in args.Cards)
                {
                    if (card is mimaextensions.mimacard mimascard && mimascard.ispassive)
                    {
                        haspassive = true;
                        switch (card.Id)
                        {
                            //start of uncommons
                            case nameof(NotImages.Passive.Uncommon.cardpassivegolddef.cardpassivegold):
                                passivegold += card.Value1;
                                break;
                            case nameof(NotImages.Passive.Uncommon.cardpassivepowerdef.cardpassivepower):
                                passivepower += card.Value1;
                                break;
                            case nameof(NotImages.Passive.Uncommon.cardpassivembdef.cardpassivemb):
                                passivemb += card.Value1;
                                break;
                            case nameof(NotImages.Passive.Uncommon.cardpassivembhanddef.cardpassivembhand):
                                passivembhand += card.Value1;
                                break;
                            //start of rares
                            case nameof(NotImages.Passive.Rare.cardpassivealgophobiadef.cardpassivealgophobia):
                                passiveimplosion += card.Value1;
                                break;
                            case nameof(NotImages.Passive.Rare.cardpassiverpolaritydef.cardpassiverpolarity):
                                passiveretribution += card.Value1;
                                break;
                            case nameof(NotImages.Passive.Rare.cardpassivewraitsothdef.cardpassivewraitsoth):
                                passiveeverlast += card.Value1;
                                break;
                        }
                        GameRun.RemoveDeckCard(card, false);
                    }
                }
            }
        }
    }
}
