using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.EntityLib.StatusEffects.ExtraTurn;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.IO.LowLevel.Unsafe;

namespace lvalonmima
{
    public class mimaextensions
    {
        public abstract class mimacard : Card
        {
        }
        public abstract class mimaexhibit : Exhibit
        {
        }
        public abstract class mimaexpartner : ExtraTurnPartner
        {
        }
        public abstract class mimase : StatusEffect
        {
            public bool isMBmod { get; set; }
            public int truecounter { get; set; }
        }
    }
}
