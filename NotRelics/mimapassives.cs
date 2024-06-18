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
using System.Linq;
using LBoL.Base.Extensions;
using LBoL.Core.StatusEffects;

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
                Order: 20, //for dmg multiplier
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
            //passive stuff
            public bool haspassive = false;
            public int passivegold = 0;
            public int passivepower = 0;
            public int passivemb = 0;
            public int passivembhand = 0;
            public int passiveupgrade = 0;

            public int passiveimplosion = 0;
            public int passiveretribution = 0;
            public int passiveeverlast = 0;
            public int passivecharge = 0; public int chargeleftinternal = 4;
            //crit stuff below
            public float critdmginternal = 100;
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
            public string upgradeyaml => passiveupgrade > 0 ? Convert.ToString(passiveupgrade) : "";
            public string chargeyaml => passivecharge > 0 ? Convert.ToString(passivecharge) : "";

            public string implosionyaml => passiveimplosion > 0 ? Convert.ToString(passiveimplosion) : "";
            public string retributionyaml => passiveretribution > 0 ? Convert.ToString(passiveretribution) : "";
            public string everlastyaml => passiveeverlast > 0 ? (passiveeverlast > 2 ? "100" : Convert.ToString(passiveeverlast * 50)) : "";
            //
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
            public string passiveupgradeyaml
            {
                get
                {
                    string res = "";
                    if (passiveupgrade > 0)
                    {
                        try
                        {
                            res = LocalizeProperty(key: "passiveupgradedes", decorated: true, required: true).RuntimeFormat(FormatWrapper);
                        }
                        catch (Exception)
                        {
                            res = "<Error>";
                        }
                    }
                    return res;
                }
            }
            public string passivechargeyaml
            {
                get
                {
                    string res = "";
                    if (passivecharge > 0)
                    {
                        try
                        {
                            res = LocalizeProperty(key: "passivechargedes", decorated: true, required: true).RuntimeFormat(FormatWrapper);
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
            //some more public getters for passives
            public int chargeleft
            {
                get
                {
                    return chargeleftinternal;
                }
            }

            protected override void OnEnterBattle()
            {
                ReactBattleEvent(Battle.BattleStarting, new EventSequencedReactor<GameEventArgs>(OnBattleStarting));
                HandleBattleEvent(Battle.BattleEnded, OnBattleEnding);
                ReactBattleEvent(Owner.TurnStarting, new EventSequencedReactor<UnitEventArgs>(OnTurnStarting));
                ReactBattleEvent(Battle.CardMoved, new EventSequencedReactor<CardMovingEventArgs>(OnCardMoved));
                ReactBattleEvent(Battle.CardUsed, new EventSequencedReactor<CardUsingEventArgs>(OnCardUsed));
                ReactBattleEvent(Battle.CardDrawn, new EventSequencedReactor<CardEventArgs>(OnCardDrawn));
                ReactBattleEvent(Battle.CardsAddedToHand, new EventSequencedReactor<CardsEventArgs>(OnCardsAddedToHand));
                //ReactBattleEvent(Owner.DamageDealing, new EventSequencedReactor<DamageDealingEventArgs>(OnDamageDealing));
                //HandleBattleEvent(Owner.DamageDealing, OnDamageDealing);
                ReactBattleEvent(Battle.Player.TurnStarted, new EventSequencedReactor<UnitEventArgs>(OnPlayerTurnStarted));
            }
            protected override void OnAdded(PlayerUnit player)
            {
                HandleGameRunEvent(GameRun.DeckCardsAdded, new GameEventHandler<CardsEventArgs>(OnDeckCardsAdded));
            }
            //one time start of battle eff

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
                if (passiveimplosion > 0)
                {
                    NotifyActivating();
                    yield return new ApplyStatusEffectAction<SE.mburstmodifiers.implosiondef.implosion>(Owner, new int?(passiveimplosion), null, null, null, 0f, true);
                }
                if (passiveretribution > 0)
                {
                    NotifyActivating();
                    yield return new ApplyStatusEffectAction<SE.mburstmodifiers.retributiondef.retribution>(Owner, new int?(passiveretribution), null, null, null, 0f, true);
                }
                if (passiveeverlast > 0)
                {
                    NotifyActivating();
                    yield return new ApplyStatusEffectAction<SE.mburstmodifiers.everlastingmagicdef.everlastingmagic>(Owner, new int?(passiveeverlast), null, null, null, 0f, true);
                }
                yield break;
            }
            private IEnumerable<BattleAction> OnPlayerTurnStarted(UnitEventArgs args)
            {
                if (Battle.Player.TurnCounter == 1 && passiveupgrade > 0)
                {
                    NotifyActivating();
                    for (int i = 1; i <= passiveupgrade; i++)
                    {
                        Card[] cards = (from c in Battle.HandZone
                                        where c.CanUpgrade
                                        select c).ToList().SampleManyOrAll(passiveupgrade, GameRun.BattleRng);
                        yield return new UpgradeCardsAction(cards);
                    }
                }
                yield break;
            }
            //per turn
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
                chargeleftinternal = 4;
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
            //passive charge
            private IEnumerable<BattleAction> OnCardUsed(CardUsingEventArgs args)
            {
                if (Battle.BattleShouldEnd)
                {
                    yield break;
                }
                if (passivecharge > 0)
                {
                    chargeleftinternal--;
                    if (chargeleftinternal == 0)
                    {
                        NotifyActivating();
                        chargeleftinternal += 4;
                        yield return new ApplyStatusEffectAction<Charging>(Owner, new int?(passivecharge), null, null, null, 0f, true);
                    }
                }
                yield break;
            }
            //register passives
            private void OnDeckCardsAdded(CardsEventArgs args)
            {
                foreach (Card card in args.Cards)
                {
                    if (card is mimaextensions.mimacard mimascard && mimaextensions.mimacard.passivecards.Contains(mimascard.Id))
                    {
                        haspassive = true;
                        switch (card.Id)
                        {
                            //start of uncommons
                            case nameof(NotImages.Passive.Uncommon.passivegolddef.passivegold):
                                passivegold += card.Value1;
                                break;
                            case nameof(NotImages.Passive.Uncommon.passivepowerdef.passivepower):
                                passivepower += card.Value1;
                                break;
                            case nameof(NotImages.Passive.Uncommon.passivembdef.passivemb):
                                passivemb += card.Value1;
                                break;
                            case nameof(NotImages.Passive.Uncommon.passivembhanddef.passivembhand):
                                passivembhand += card.Value1;
                                break;
                            case nameof(NotImages.Passive.Uncommon.passiveupgradedef.passiveupgrade):
                                passiveupgrade += card.Value1;
                                break;
                            case nameof(NotImages.Passive.Uncommon.passivechargedef.passivecharge):
                                passivecharge += card.Value1;
                                break;
                            //start of rares
                            case nameof(NotImages.Passive.Rare.passivealgophobiadef.passivealgophobia):
                                passiveimplosion += card.Value1;
                                break;
                            case nameof(NotImages.Passive.Rare.passiverpolaritydef.passiverpolarity):
                                passiveretribution += card.Value1;
                                break;
                            case nameof(NotImages.Passive.Rare.passivewraitsothdef.passivewraitsoth):
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
