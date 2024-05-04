using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.EntityLib.Cards;
using LBoL.EntityLib.Exhibits;
using LBoL.EntityLib.StatusEffects.ExtraTurn;
using System;
using System.Collections.Generic;
using lvalonmima.NotImages.Passive.Uncommon;

namespace lvalonmima
{
    public class mimaextensions
    {
        public abstract class mimacard : Card
        {
            public bool ispassive { get; set; } = false;
            public bool ismonster { get; set; } = false;
            public bool isblitz { get; set; } = false;
            public bool isbm { get; set; } = false;
        }
        public abstract class mimaoptioncard : OptionCard
        {
        }
        public abstract class mimaexhibit : Exhibit
        {
        }
        public abstract class mimasexhibit : ShiningExhibit
        {
        }
        public abstract class mimaexpartner : ExtraTurnPartner
        {
        }
        public abstract class mimase : StatusEffect
        {
            public bool isMBmod { get; set; } = false;
            public int truecounter { get; set; }
        }
    }
}
