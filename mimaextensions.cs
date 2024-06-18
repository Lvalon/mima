using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.EntityLib.Cards;
using LBoL.EntityLib.Exhibits;
using LBoL.EntityLib.StatusEffects.ExtraTurn;
using System.Collections.Generic;
using lvalonmima.NotImages.Passive.Uncommon;
using lvalonmima.NotImages.Passive.Rare;
using lvalonmima.SE.mburstmodifiers;

namespace lvalonmima
{
    public class mimaextensions
    {
        public abstract class mimacard : Card
        {
            static mimacard()
            {
                List<string> passivelist = new List<string>
                {
                    //uncommons start here
                    nameof(passivechargedef.passivecharge),
                    nameof(passivegolddef.passivegold),
                    nameof(passivembdef.passivemb),
                    nameof(passivembhanddef.passivembhand),
                    nameof(passivepowerdef.passivepower),
                    nameof(passiveupgradedef.passiveupgrade),
                    //rares start here
                    nameof(passivealgophobiadef.passivealgophobia),
                    nameof(passiverpolaritydef.passiverpolarity),
                    nameof(passivewraitsothdef.passivewraitsoth)
                };
                passivecards = passivelist;
                List<string> monsterlist = new List<string>
                {
                };
                monstercards = monsterlist;
                List<string> blitzlist = new List<string>
                {
                };
                monstercards = blitzlist;
                List<string> bmlist = new List<string>
                {
                };
                monstercards = bmlist;
            }
            public static readonly List<string> passivecards;
            public static readonly List<string> monstercards;
            public static readonly List<string> blitzcards;
            public static readonly List<string> bmcards;
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
