using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Cards;
using LBoLEntitySideloader.Resource;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace lvalonmima
{
    public abstract class toolbox
    {

        public static GlobalLocalization LocalizationCard(DirectorySource dirsorc)
        {
            var loc = new GlobalLocalization(dirsorc);
            loc.LocalizationFiles.AddLocaleFile(Locale.En, "cardEn.yaml");
            return loc;
        }
        public static GlobalLocalization LocalizationStatus(DirectorySource dirsorc)
        {
            var loc = new GlobalLocalization(dirsorc);
            loc.LocalizationFiles.AddLocaleFile(Locale.En, "SEEn.yaml");
            return loc;
        }
        public static GlobalLocalization LocalizationExhibit(DirectorySource dirsorc)
        {
            var loc = new GlobalLocalization(dirsorc);
            loc.LocalizationFiles.AddLocaleFile(Locale.En, "exEn.yaml");
            return loc;
        }
        public static GlobalLocalization LocalizationUlt(DirectorySource dirsorc)
        {
            var loc = new GlobalLocalization(dirsorc);
            loc.LocalizationFiles.AddLocaleFile(Locale.En, "bombEn.yaml");
            return loc;
        }
        public static GlobalLocalization LocalizationPlayer(DirectorySource dirsorc)
        {
            var loc = new GlobalLocalization(dirsorc);
            loc.LocalizationFiles.AddLocaleFile(Locale.En, "playerEn.yaml");
            return loc;
        }
    }
}