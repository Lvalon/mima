using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoLEntitySideloader.Resource;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using static lvalonmima.SE.transmigrateddef;
using static UnityEngine.UI.GridLayoutGroup;

namespace lvalonmima
{
    public abstract class toolbox
    {

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
        //ONREVIVE

        //private int oghp;
        //private int ogmax;
        //private int ogdmg;

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
    }
}