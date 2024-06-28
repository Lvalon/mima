using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;
using LBoL.Presentation;
using LBoLEntitySideloader.CustomHandlers;
using System.Collections.Generic;
using System;
using System.Collections;
using System.Linq;
using lvalonmima.NotImages.Blitz.Rare;

namespace lvalonmima
{
    public class CustomHandlers
    {
        private static IEnumerator GainExhibits(GameRunController gameRun, HashSet<Type> exhibits, bool triggerVisual = false, VisualSourceData exhibitSource = null)
        {
            foreach (Type et in exhibits)
            {
                Exhibit ex = Library.CreateExhibit(et);
                ex.GameRun = gameRun;

                yield return gameRun.GainExhibitRunner(ex, triggerVisual, exhibitSource);
            }

            gameRun.ExhibitPool.RemoveAll(e => exhibits.Contains(e));
        }
        public static void OnDeckCardsAdding(CardsEventArgs args)
        {
            var gamerun = GameMaster.Instance.CurrentGameRun;
            int num = args.Cards.Count((Card card) => card is mimaextensions.mimacard mimascard && mimascard is mimaextensions.mimacard.passivecard);
            PlayerUnit player = gamerun.Player;
            bool hasexhibit = player.HasExhibit<NotRelics.mimapassivesdef.mimapassives>();
            if (num > 0 && hasexhibit == false)
            {
                GameMaster.Instance.StartCoroutine(GainExhibits(
                         gameRun: gamerun,
                         exhibits: new HashSet<Type>() { typeof(NotRelics.mimapassivesdef.mimapassives) },
                         triggerVisual: true,
                         exhibitSource: new VisualSourceData()
                         {
                             SourceType = VisualSourceType.Debug,
                             Source = null
                         }));
            }
        }
        public static void OnDeckCardsAdded(CardsEventArgs args)
        {
            var gamerun = GameMaster.Instance.CurrentGameRun;
            BattleController battle = gamerun.Battle;
            var rmvlist = new List<Card>();
            foreach (Card card in args.Cards)
            {
                //blitz cards
                if (card is mimaextensions.mimacard mimascard && mimascard is mimaextensions.mimacard.blitzcard)
                {
                    //queue delet
                    //rmvlist.Add(card);
                    switch (card.Id)
                    {
                        //elemental burst
                        case nameof(blitzeburstdef.blitzeburst):
                            if (gamerun.Battle != null)
                            {
                                blitzeburstdef.blitzeburst.onconfirm(card);
                            }
                            break;
                    }
                }
            }
            //monster card anti duplicate
            var firstmonster = new List<Card>();
            foreach (Card card in args.Cards)
            {
                if (card is mimaextensions.mimacard mimascard && mimascard is mimaextensions.mimacard.monstercard)
                {
                    //check if there isnt a monster of the same id in lib before, 0 means isnt, else is
                    int equal = 0;
                    foreach (Card libcard in gamerun.BaseDeck)
                    {
                        if (libcard.Id == card.Id)
                            equal++;
                    }
                    foreach (Card card2 in args.Cards)
                    {
                        if (card2.Id == card.Id)
                            equal--;
                    }
                    if (equal != 0)
                    {
                        foreach (Card libcard in gamerun.BaseDeck)
                        {
                            bool isargscard = false;
                            foreach (Card card2 in args.Cards)
                            {
                                if (libcard == card2)
                                {
                                    isargscard = true;
                                }
                            }
                            if (!isargscard)
                            {
                                bool alrexist = false;
                                foreach (Card first in firstmonster)
                                {
                                    if (libcard == first)
                                    {
                                        alrexist = true;
                                    }
                                }
                                if (!alrexist)
                                {
                                    firstmonster.Add(libcard);
                                }
                            }
                        }
                    }
                    else
                    {
                        bool alrexist = false;
                        foreach (Card first in firstmonster)
                        {
                            if (first.Id == card.Id)
                            {
                                alrexist = true;
                            }
                        }
                        if (!alrexist)
                        {
                            firstmonster.Add(card);
                        }
                    }
                    //remove cards that arent first monster
                    bool infirst = false;
                    foreach (Card first in firstmonster)
                    {
                        if (card == first)
                        {
                            infirst = true;
                        }
                    }
                    if (!infirst)
                    {
                        foreach (Card first in firstmonster)
                        {
                            if (card.Id == first.Id)
                            {
                                bool isinlib = false;
                                foreach (Card libcard in gamerun.BaseDeck) {
                                    if (libcard == first) {
                                        isinlib = true;
                                    }
                                }
                                if (isinlib) {
                                    first.Upgrade();
                                }
                            }
                        }
                        rmvlist.Add(card);
                    }
                }
            }
            //in case blitz outside battle
            foreach (Card card in gamerun.BaseDeck)
            {
                if (card is mimaextensions.mimacard mimascard && mimascard is mimaextensions.mimacard.blitzcard)
                {
                    rmvlist.Add(card);
                }
            }
            //remove rmvlist cards
            foreach (Card card in rmvlist)
            {
                gamerun.RemoveDeckCard(card);

            }
        }
        public static void addreactors()
        {
            CHandlerManager.RegisterBattleEventHandler(
                (BattleController b) => b.BattleStarting,
                addbattlereactor,
                null,
                GameEventPriority.Highest
            );
        }
        static void addbattlereactor(GameEventArgs args)
        {
            var unit = GameMaster.Instance.CurrentGameRun.Battle.Player;
            battlereactor(unit);

        }
        static void battlereactor(Unit unit)
        {
            var gamerun = GameMaster.Instance.CurrentGameRun;
            //monster anti dupe
            IEnumerable<BattleAction> battlefieldyeet(CardsEventArgs args)
            {
                var rez = new List<BattleAction>
                {
                };
                var rmvlist = new List<Card>();
                var firstmonster = new List<Card>();
                foreach (Card card in args.Cards)
                {
                    if (card is mimaextensions.mimacard mimascard && mimascard is mimaextensions.mimacard.monstercard)
                    {
                        //check if there isnt a monster of the same id in battlefield before, 0 means isnt, else is
                        int equal = 0;
                        foreach (Card battlecard in gamerun.Battle.EnumerateAllCards())
                        {
                            if (battlecard.Id == card.Id)
                                equal++;
                        }
                        foreach (Card card2 in args.Cards)
                        {
                            if (card2.Id == card.Id)
                                equal--;
                        }
                        if (equal != 0)
                        {
                            foreach (Card battlecard in gamerun.Battle.EnumerateAllCards())
                            {
                                bool isargscard = false;
                                foreach (Card card2 in args.Cards)
                                {
                                    if (battlecard == card2)
                                    {
                                        isargscard = true;
                                    }
                                }
                                if (!isargscard)
                                {
                                    bool alrexist = false;
                                    foreach (Card first in firstmonster)
                                    {
                                        if (first.Id == battlecard.Id)
                                        {
                                            alrexist = true;
                                        }
                                    }
                                    if (!alrexist)
                                    {
                                        firstmonster.Add(battlecard);
                                    }
                                }
                            }
                        }
                        else
                        {
                            bool alrexist = false;
                            foreach (Card first in firstmonster)
                            {
                                if (first.Id == card.Id)
                                {
                                    alrexist = true;
                                }
                            }
                            if (!alrexist)
                            {
                                firstmonster.Add(card);
                            }
                        }
                        //remove cards that arent first monster
                        bool infirst = false;
                        foreach (Card first in firstmonster)
                        {
                            if (card == first)
                            {
                                infirst = true;
                            }
                        }
                        if (!infirst)
                        {
                            foreach (Card first in firstmonster)
                            {
                                if (card.Id == first.Id)
                                {
                                    bool isinlib = false;
                                    foreach (Card libcard in gamerun.BaseDeck) {
                                        if (first == libcard) {
                                            isinlib = true;
                                        }
                                    }
                                    if (!isinlib) {
                                        rez.Add(new UpgradeCardAction(first));
                                    }
                                }
                            }
                            rmvlist.Add(card);
                        }
                    }
                }
                foreach (Card card2 in gamerun.Battle.EnumerateAllCards())
                {
                    if (card2 is mimaextensions.mimacard mimascard && mimascard is mimaextensions.mimacard.blitzcard)
                    {
                        rez.Add(new RemoveCardAction(card2));
                    }
                }
                foreach (Card card in rmvlist)
                {
                    rez.Add(new RemoveCardAction(card));
                }
                return rez;
            }
            IEnumerable<BattleAction> battlefieldyeet2(CardsAddingToDrawZoneEventArgs args)
            {
                var rez = new List<BattleAction>
                {
                };
                var rmvlist = new List<Card>();
                var firstmonster = new List<Card>();
                foreach (Card card in args.Cards)
                {
                    if (card is mimaextensions.mimacard mimascard && mimascard is mimaextensions.mimacard.monstercard)
                    {
                        //check if there isnt a monster of the same id in battlefield before, 0 means isnt, else is
                        int equal = 0;
                        foreach (Card battlecard in gamerun.Battle.EnumerateAllCards())
                        {
                            if (battlecard.Id == card.Id)
                                equal++;
                        }
                        foreach (Card card2 in args.Cards)
                        {
                            if (card2.Id == card.Id)
                                equal--;
                        }
                        if (equal != 0)
                        {
                            foreach (Card battlecard in gamerun.Battle.EnumerateAllCards())
                            {
                                bool isargscard = false;
                                foreach (Card card2 in args.Cards)
                                {
                                    if (battlecard == card2)
                                    {
                                        isargscard = true;
                                    }
                                }
                                if (!isargscard)
                                {
                                    bool alrexist = false;
                                    foreach (Card first in firstmonster)
                                    {
                                        if (first.Id == battlecard.Id)
                                        {
                                            alrexist = true;
                                        }
                                    }
                                    if (!alrexist)
                                    {
                                        firstmonster.Add(battlecard);
                                    }
                                }
                            }
                        }
                        else
                        {
                            bool alrexist = false;
                            foreach (Card first in firstmonster)
                            {
                                if (first.Id == card.Id)
                                {
                                    alrexist = true;
                                }
                            }
                            if (!alrexist)
                            {
                                firstmonster.Add(card);
                            }
                        }
                        //remove cards that arent first monster
                        bool infirst = false;
                        foreach (Card first in firstmonster)
                        {
                            if (card == first)
                            {
                                infirst = true;
                            }
                        }
                        if (!infirst)
                        {
                            foreach (Card first in firstmonster)
                            {
                                if (card.Id == first.Id)
                                {
                                    bool isindrawpile = false;
                                    foreach (Card drawcard in gamerun.Battle.DrawZone) {
                                        if (first == drawcard) {
                                            isindrawpile = true;
                                        }
                                    }
                                    if (isindrawpile) {
                                        rez.Add(new UpgradeCardAction(first));
                                    }
                                }
                            }
                            rmvlist.Add(card);
                        }
                    }
                }
                foreach (Card card2 in gamerun.Battle.EnumerateAllCards())
                {
                    if (card2 is mimaextensions.mimacard mimascard && mimascard is mimaextensions.mimacard.blitzcard)
                    {
                        rez.Add(new RemoveCardAction(card2));
                    }
                }
                foreach (Card card in rmvlist)
                {
                    rez.Add(new RemoveCardAction(card));
                }
                return rez;
            }
            List<BattleAction> rmvbbm(GameEventArgs args)
            {
                var rez = new List<BattleAction>
                {
                };
                var rmvlist = new List<Card>();
                foreach (Card card in gamerun.Battle.EnumerateAllCards())
                {
                    if (card is mimaextensions.mimacard mimacard && (mimacard is mimaextensions.mimacard.blitzcard))
                    {
                        rmvlist.Add(card);
                    }
                }
                foreach (Card card in rmvlist)
                {
                    gamerun.RemoveDeckCard(card);
                }
                return rez;
            }

            unit.ReactBattleEvent(gamerun.Battle.CardsAddedToDiscard, (CardsEventArgs args) =>
            {
                return battlefieldyeet(args);
            });
            unit.ReactBattleEvent(gamerun.Battle.CardsAddedToExile, (CardsEventArgs args) =>
            {
                return battlefieldyeet(args);
            });
            unit.ReactBattleEvent(gamerun.Battle.CardsAddedToHand, (CardsEventArgs args) =>
            {
                return battlefieldyeet(args);
            });
            unit.ReactBattleEvent(gamerun.Battle.CardsAddedToDiscard, (CardsEventArgs args) =>
            {
                return battlefieldyeet(args);
            });
            unit.ReactBattleEvent(gamerun.Battle.CardsAddedToDrawZone, (CardsAddingToDrawZoneEventArgs args) =>
            {
                return battlefieldyeet2(args);
            });
            unit.ReactBattleEvent(gamerun.Battle.BattleEnded, (GameEventArgs args) =>
            {
                return rmvbbm(args);
            });
        }
    }
}