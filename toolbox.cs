﻿using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoLEntitySideloader.Resource;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;
using static lvalonmima.SE.transcendeddef;
using static UnityEngine.UI.GridLayoutGroup;

namespace lvalonmima
{
    public abstract class toolbox
    {
        //CARD IMG

        //var imgs = new CardImages(embeddedSource);
        //imgs.AutoLoad(this, extension: ".png");
        //    return imgs;
        public static GlobalLocalization loccard()
        {
            var loc = new GlobalLocalization(BepinexPlugin.embeddedSource);
            loc.LocalizationFiles.AddLocaleFile(Locale.En, "cardEn.yaml");
            return loc;
        }
        public static GlobalLocalization locse()
        {
            var loc = new GlobalLocalization(BepinexPlugin.embeddedSource);
            loc.LocalizationFiles.AddLocaleFile(Locale.En, "SEEn.yaml");
            return loc;
        }
        public static GlobalLocalization locex()
        {
            var loc = new GlobalLocalization(BepinexPlugin.embeddedSource);
            loc.LocalizationFiles.AddLocaleFile(Locale.En, "exEn.yaml");
            return loc;
        }
        public static GlobalLocalization locbomb()
        {
            var loc = new GlobalLocalization(BepinexPlugin.embeddedSource);
            loc.LocalizationFiles.AddLocaleFile(Locale.En, "bombEn.yaml");
            return loc;
        }
        public static GlobalLocalization locplayer()
        {
            var loc = new GlobalLocalization(BepinexPlugin.embeddedSource);
            loc.LocalizationFiles.AddLocaleFile(Locale.En, "playerEn.yaml");
            return loc;
        }
        public static GlobalLocalization locmodel()
        {
            var loc = new GlobalLocalization(BepinexPlugin.embeddedSource);
            loc.LocalizationFiles.AddLocaleFile(Locale.En, "modelEn.yaml");
            return loc;
        }
        public static GlobalLocalization locult()
        {
            var loc = new GlobalLocalization(BepinexPlugin.embeddedSource);
            loc.LocalizationFiles.AddLocaleFile(Locale.En, "ultEn.yaml");
            return loc;
        }
        //ONREVIVE

        //private int oghp;
        //private int ogmax;
        //private int ogdmg;

        //protected override void OnEnterBattle(BattleController battle)
        //{
        //    base.HandleBattleEvent<DamageEventArgs>(base.Battle.Player.DamageTaking, new GameEventHandler<DamageEventArgs>(this.OnPlayerDamageTaking));
        //    base.HandleBattleEvent<DamageEventArgs>(base.Battle.Player.DamageReceived, new GameEventHandler<DamageEventArgs>(this.OnPlayerDamageReceived));
        //}

        //private void OnPlayerDamageTaking(DamageEventArgs args)
        //{
        //    oghp = Owner.Hp;
        //    ogmax = Owner.MaxHp;
        //    ogdmg = args.DamageInfo.Damage.RoundToInt();
        //}

        //private void OnPlayerDamageReceived(DamageEventArgs args)
        //{
        //    if (Owner.Hp == Owner.MaxHp && ogdmg > 0 && ((oghp - ogdmg != Owner.Hp - ogdmg) || ogdmg >= ogmax))
        //    {
        //        base.NotifyActivating();
        //    }
        //}

        //private void OnPlayerDamageTaking(DamageEventArgs args)
        //{
        //    oghp = base.Battle.Player.Hp;
        //    ogmax = base.Battle.Player.MaxHp;
        //    ogdmg = args.DamageInfo.Damage.RoundToInt();
        //}
        //private void OnPlayerDamageReceived(DamageEventArgs args)
        //{
        //    if (base.Battle.Player.Hp == base.Battle.Player.MaxHp && ogdmg > 0 && ((oghp - ogdmg != base.Battle.Player.Hp - ogdmg) || ogdmg >= ogmax))
        //    {
        //        base.NotifyActivating();
        //    }
        //}
    }
}