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
            if (args.Cards.Count((Card card) => card is mimaextensions.mimacard mimascard && mimascard is mimaextensions.mimacard.passivecard) > 0 && !gamerun.Player.HasExhibit<NotRelics.mimapassivesdef.mimapassives>())
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
            //blitz
            foreach (Card card in args.Cards.Where(c => c is mimaextensions.mimacard.blitzcard))
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
            //monster card anti duplicate
            var firstmonster = new List<Card>();

            //add non args library monster card to firstmosnter
            foreach (Card libcard in gamerun.BaseDeck.Where(c => !args.Cards.Any(ac => ac == c) && c is mimaextensions.mimacard.monstercard))
            {
                //add to firstmonster if there isnt already one
                if (!firstmonster.Any(fcard => fcard.Id == libcard.Id))
                {
                    firstmonster.Add(libcard);
                }
            }

            //add args monster card to firstmonster if tehre isnt already one
            foreach (Card argc in args.Cards.Where(c => c is mimaextensions.mimacard.monstercard))
            {
                if (!firstmonster.Any(fcard => fcard.Id == argc.Id))
                {
                    firstmonster.Add(argc);
                }
            }

            //upgrade first one, remove others
            foreach (Card libcard in gamerun.BaseDeck.Where(c => c is mimaextensions.mimacard.monstercard))
            {
                foreach (Card respectivefirst in firstmonster.Where(fcard => fcard.Id == libcard.Id && fcard != libcard))
                {
                    respectivefirst.Upgrade();
                }
                //if lib card isnt in firstmonster list
                if (!firstmonster.Any(c => c == libcard))
                {
                    rmvlist.Add(libcard);
                }
            }
            //in case blitz outside battle
            foreach (Card card in gamerun.BaseDeck.Where(c => c is mimaextensions.mimacard.blitzcard))
            {
                rmvlist.Add(card);
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

                //add non args battlefield monster card to firstmosnter
                foreach (Card battlecard in gamerun.Battle.EnumerateAllCards().Where(c => !args.Cards.Any(ac => ac == c) && c is mimaextensions.mimacard.monstercard))
                {
                    //add to firstmonster if there isnt already one
                    if (!firstmonster.Any(fcard => fcard.Id == battlecard.Id))
                    {
                        firstmonster.Add(battlecard);
                    }
                }

                //add args monster card to firstmonster if tehre isnt already one
                foreach (Card argc in args.Cards.Where(c => c is mimaextensions.mimacard.monstercard))
                {
                    if (!firstmonster.Any(fcard => fcard.Id == argc.Id))
                    {
                        firstmonster.Add(argc);
                    }
                }

                //upgrade first one, remove others
                foreach (Card battlecard in gamerun.Battle.EnumerateAllCards().Where(c => c is mimaextensions.mimacard.monstercard))
                {
                    foreach (Card respectivefirst in firstmonster.Where(fcard => fcard.Id == battlecard.Id && fcard != battlecard))
                    {
                        rez.Add(new UpgradeCardAction(respectivefirst));
                        //respectivefirst.Upgrade();
                    }
                    //if lib card isnt in firstmonster list
                    if (!firstmonster.Any(c => c == battlecard))
                    {
                        rmvlist.Add(battlecard);
                    }
                }

                foreach (Card card2 in gamerun.Battle.EnumerateAllCards().Where(c => c is mimaextensions.mimacard.blitzcard))
                {
                    rez.Add(new RemoveCardAction(card2));
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
                //add non args battlefield monster card to firstmosnter
                foreach (Card battlecard in gamerun.Battle.EnumerateAllCards().Where(c => !args.Cards.Any(ac => ac == c) && c is mimaextensions.mimacard.monstercard))
                {
                    //add to firstmonster if there isnt already one
                    if (!firstmonster.Any(fcard => fcard.Id == battlecard.Id))
                    {
                        firstmonster.Add(battlecard);
                    }
                }

                //add args monster card to firstmonster if tehre isnt already one
                foreach (Card argc in args.Cards.Where(c => c is mimaextensions.mimacard.monstercard))
                {
                    if (!firstmonster.Any(fcard => fcard.Id == argc.Id))
                    {
                        firstmonster.Add(argc);
                    }
                }

                //upgrade first one, remove others
                foreach (Card battlecard in gamerun.Battle.EnumerateAllCards().Where(c => c is mimaextensions.mimacard.monstercard))
                {
                    foreach (Card respectivefirst in firstmonster.Where(fcard => fcard.Id == battlecard.Id && fcard != battlecard))
                    {
                        rez.Add(new UpgradeCardAction(respectivefirst));
                        //respectivefirst.Upgrade();
                    }
                    //if lib card isnt in firstmonster list
                    if (!firstmonster.Any(c => c == battlecard))
                    {
                        rmvlist.Add(battlecard);
                    }
                }

                foreach (Card card2 in gamerun.Battle.EnumerateAllCards().Where(c => c is mimaextensions.mimacard.blitzcard))
                {
                    rez.Add(new RemoveCardAction(card2));
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
                foreach (Card card in gamerun.Battle.EnumerateAllCards().Where(c => c is mimaextensions.mimacard.blitzcard))
                {
                    rmvlist.Add(card);
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