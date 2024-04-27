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
using LBoL.Core.Randoms;
using static lvalonmima.SE.transcendeddef;
using static lvalonmima.SE.magicalburstdef;
using static lvalonmima.NotImages.Starter.cardchannellingdef;
using LBoL.Core.Stations;
using LBoL.Core.Cards;
using LBoL.EntityLib.Cards.Neutral.NoColor;
using LBoL.Presentation.UI.Panels;
using static lvalonmima.NotImages.Starter.cardmountaindef;
using static lvalonmima.NotImages.Starter.cardreminidef;
using static lvalonmima.NotImages.Starter.carderosiondef;
using HarmonyLib;
using static lvalonmima.playermima;

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
            public bool haspassivegold = false;
            public mimapassives() : base()
            {
            }

            protected override string LocalizeProperty(string key, bool decorated = false, bool required = true)
            {
                return TypeFactory<Exhibit>.LocalizeProperty(Id, key, decorated, required);
            }

            public string theplayername
            {
                get
                {
                    if (GameRun == null) { return "The Player";  }
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
                        catch (Exception ex)
                        {
                            res = "<Error>";
                        }
                    }
                    return res;
                }
            }

            //[HarmonyPatch(typeof(GameRunController), nameof(GameRunController.Create))]
            //class GameRunController_Create_Patch
            //{
            //    static void Postfix(GameRunController __result)
            //    {
            //        var player = __result.Player;

            //        Exhibit exhibit = player.GetExhibit<mimapassives>();
            //        if (exhibit != null && exhibit is mimapassives mimapassive)
            //        {
            //            mimapassive.rng = new RandomGen(__result.RootRng.NextULong());
            //        }
            //    }
            //}

            protected override void OnEnterBattle()
            {
                base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarting, new EventSequencedReactor<GameEventArgs>(this.OnBattleStarting));
                HandleBattleEvent(base.Battle.BattleEnded, OnBattleEnding);
            }

            private void OnBattleEnding(GameEventArgs args)
            {
            }

            IEnumerable<BattleAction> OnBattleStarting(GameEventArgs gameEventArgs)
            {
                if (passivegold > 0)
                {
                    yield return new GainMoneyAction(passivegold, SpecialSourceType.None);
                    base.NotifyActivating();
                }
            }
            protected override void OnAdded(PlayerUnit player)
            {
            }
        }
    }
}
