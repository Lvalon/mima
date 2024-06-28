using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.EntityLib.Cards;
using LBoL.EntityLib.Exhibits;
using LBoL.EntityLib.StatusEffects.ExtraTurn;
using System.Collections.Generic;
using lvalonmima.NotImages.Passive.Uncommon;
using lvalonmima.NotImages.Passive.Rare;
using lvalonmima.NotImages.Blitz.Rare;
using lvalonmima.SE.mburstmodifiers;
using lvalonmima.NotImages.Monster.Rare;

namespace lvalonmima
{
    public class mimaextensions
    {
        public abstract class mimacard : Card
        {
            static mimacard()
            {
            }
            public abstract class passivecard : mimacard
            {
            }
            public abstract class blitzcard : mimacard
            {
                static blitzcard()
                {
                    List<Card> list = new List<Card>
                    {
                        Library.CreateCard<blitzeburstdef.blitzeburst>()
                    };
                    blitzlist = list;
                }
                public static readonly List<Card> blitzlist;
                public abstract class bmcard : blitzcard
                {
                    static bmcard()
                    {
                        List<Card> list = new List<Card>
                        {
                            Library.CreateCard<blitzeburstdef.blitzeburst>()
                        };
                        bmlist = list;
                    }
                    public static readonly List<Card> bmlist;
                }
            }
            public abstract class monstercard : mimacard
            {
            }
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
            static mimase()
            {
                List<string> list = new List<string>
                {
                    nameof(accumulationdef.accumulation),
                    nameof(concentratedburstdef.concentratedburst),
                    nameof(everlastingmagicdef.everlastingmagic),
                    nameof(fastburstdef.fastburst),
                    nameof(retributiondef.retribution),
                    nameof(splitburstdef.splitburst)
                };
                MBmods = list;
            }
            public static readonly List<string> MBmods;
            public int truecounter { get; set; }
        }
    }
}
