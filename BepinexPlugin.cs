using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using LBoLEntitySideloader.Entities;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.Core.Stations;
using LBoL.Presentation;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Resource;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using LBoL.Presentation.UI;
using LBoLEntitySideloader.PersistentValues;

namespace lvalonmima
{
    [BepInPlugin(PInfo.GUID, PInfo.Name, PInfo.version)]
    [BepInDependency(LBoLEntitySideloader.PluginInfo.GUID, BepInDependency.DependencyFlags.HardDependency)]
    //[BepInDependency(AddWatermark.API.GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInProcess("LBoL.exe")]
    public class BepinexPlugin : BaseUnityPlugin
    {

        private static readonly Harmony harmony = PInfo.harmony;

        internal static BepInEx.Logging.ManualLogSource log;

        internal static TemplateSequenceTable sequenceTable = new TemplateSequenceTable();

        internal static IResourceSource embeddedSource = new EmbeddedSource(Assembly.GetExecutingAssembly());

        // add this for audio loading
        internal static DirectorySource directorySource = new DirectorySource(PInfo.GUID, "");
        private static KeyboardShortcut TestTab = new KeyboardShortcut(KeyCode.Tab);

        public readonly Dictionary<string, Toggle> _characterFilterList = new Dictionary<string, Toggle>();
        public IEnumerable<Type> FilterCardByCharacter()
        {
            List<Type> list = new List<Type>();
            string[] array = { "Mima" };
            foreach ((Type item, LBoL.ConfigData.CardConfig cardConfig) in Library.EnumerateCardTypes())
            {
                if (array.Length == 0)
                {
                    list.Add(item);
                }

                if (array.Contains(cardConfig.Owner))
                {
                    list.Add(item);
                }

                if (array.Contains("Neutral") && string.IsNullOrWhiteSpace(cardConfig.Owner))
                {
                    list.Add(item);
                }
            }
            return list;
        }

        public static List<Card> extradeck = new List<Card>();
        public bool initcardpool()
        {
            extradeck = new List<Card>();
            IEnumerable<Type> enumerable = FilterCardByCharacter();
            foreach (Type cardType in enumerable)
            {
                Card card = Library.CreateCard(cardType);
                extradeck.Add(card);
            }
            return true;
        }

        internal static BatchLocalization cardbatchloc = new BatchLocalization(directorySource, typeof(CardTemplate), "card");
        internal static BatchLocalization sebatchloc = new BatchLocalization(directorySource, typeof(StatusEffectTemplate), "SE");
        internal static BatchLocalization exbatchloc = new BatchLocalization(directorySource, typeof(ExhibitTemplate), "ex");
        internal static BatchLocalization ultbatchloc = new BatchLocalization(directorySource, typeof(UltimateSkillTemplate), "ult");
        internal static BatchLocalization playerbatchloc = new BatchLocalization(directorySource, typeof(PlayerUnitTemplate), "player");
        internal static BatchLocalization modelbatchloc = new BatchLocalization(directorySource, typeof(UnitModelTemplate), "model");

        public static BepinexPlugin Instance;
        private void Awake()
        {
            log = Logger;

            // very important. Without this the entry point MonoBehaviour gets destroyed
            DontDestroyOnLoad(gameObject);
            gameObject.hideFlags = HideFlags.HideAndDontSave;

            EntityManager.RegisterSelf();

            harmony.PatchAll();

            //if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(AddWatermark.API.GUID))
            //   WatermarkWrapper.ActivateWatermark();

            new mimaplayerdata().RegisterSelf(LBoLEntitySideloader.PluginInfo.GUID);
            Instance = this;
        }

        private void OnDestroy()
        {
            if (harmony != null)
                harmony.UnpatchSelf();
        }

        public class GameEventHandlerHolder
        {
            public readonly List<Action> _removeHandlerFunctions = new List<Action>();

            public void HandleEvent<T>(GameEvent<T> e, GameEventHandler<T> action, GameEventPriority priority) where T : GameEventArgs
            {
                e.AddHandler(action, priority);
                _removeHandlerFunctions.Add(delegate
                {
                    e.RemoveHandler(action, priority);
                });
            }

            public void ClearEventHandlers()
            {
                foreach (Action removeHandlerFunction in _removeHandlerFunctions)
                {
                    removeHandlerFunction();
                }
            }
        }
        public GameEventPriority DefaultEventPriority => (GameEventPriority)1;
        public readonly GameEventHandlerHolder _gameRunHandlerHolder = new GameEventHandlerHolder();
        public void HandleGameRunEvent<TEventArgs>(GameEvent<TEventArgs> @event, GameEventHandler<TEventArgs> handler, GameEventPriority priority) where TEventArgs : GameEventArgs
        {
            _gameRunHandlerHolder.HandleEvent(@event, handler, priority);
        }

        public void HandleGameRunEvent<TEventArgs>(GameEvent<TEventArgs> @event, GameEventHandler<TEventArgs> handler) where TEventArgs : GameEventArgs
        {
            HandleGameRunEvent(@event, handler, DefaultEventPriority);
        }
        private void Update()
        {
            GameRunController gamerun = GameMaster.Instance?.CurrentGameRun;
            
            if (gamerun != null)
            {
                LBoL.Core.Units.PlayerUnit player = gamerun.Player;
                if (TestTab.IsDown())
                {
                    if (player.HasExhibit<NotRelics.mimabdef.mimab>() && gamerun.Money >= 10 && gamerun.CurrentStation.Level != 0 || gamerun.CurrentStation.Stage.Id != "BambooForest")
                    {
                        GameMaster.Instance.StartCoroutine(tabberbase());
                    }
                }
            }
        }
        [HarmonyPatch(typeof(UiManager), nameof(UiManager.EnterGameRun))]
        private class UiManager_EnterGameRun_Patch
        {
            private static void OnDeckCardsAdding(CardsEventArgs args)
            {
                log.LogDebug("deck card add detected");
                GameRunController gamerun = GameMaster.Instance?.CurrentGameRun;
                int num = args.Cards.Count((Card card) => card is mimaextensions.mimacard mimascard && mimascard.ispassive == true);
                LBoL.Core.Units.PlayerUnit player = gamerun.Player;
                bool hasexhibit = player.HasExhibit<NotRelics.mimapassivesdef.mimapassives>();
                log.LogDebug("does player have passive exhibit? " + hasexhibit);
                if (num > 0 && hasexhibit == false)
                {
                    log.LogDebug("passive exhibit not found, adding exhibit");
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

            private static void Postfix(UiManager __instance)
            {
                Instance.HandleGameRunEvent<CardsEventArgs>(GameMaster.Instance?.CurrentGameRun.DeckCardsAdding, new GameEventHandler<CardsEventArgs>(OnDeckCardsAdding));
            }
        }

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

        //MIMAA REMOVE BASE MANA
        private class CoroutineExtender : IEnumerable
        {
            public IEnumerator target_enumerator;
            public List<IEnumerator> preItems = new List<IEnumerator>();
            public List<IEnumerator> postItems = new List<IEnumerator>();
            public List<IEnumerator> midItems = new List<IEnumerator>();


            public CoroutineExtender() { }

            public CoroutineExtender(IEnumerator target_enumerator) { this.target_enumerator = target_enumerator; }

            public IEnumerator GetEnumerator()
            {
                foreach (IEnumerator e in preItems) yield return e;
                int i = 0;
                while (target_enumerator.MoveNext())
                {
                    yield return target_enumerator.Current;
                    i++;
                }
                foreach (IEnumerator e in postItems) yield return e;
            }
        }

        [HarmonyPatch(typeof(Exhibit), "TriggerGain")]
        [HarmonyDebug]
        private class Exhibit_Patch
        {
            private static void Postfix(ref IEnumerator __result)
            {
                CoroutineExtender extendedRez = new CoroutineExtender(__result);

                extendedRez.postItems.Add(mimaArmrf());

                __result = extendedRez.GetEnumerator();
            }

            private static IEnumerator mimaArmrf()
            {
                if ((GameMaster.Instance != null) && (GameMaster.Instance.CurrentGameRun != null))
                {
                    GameRunController run = GameMaster.Instance.CurrentGameRun;
                    Exhibit exhibit = run.Player.GetExhibit<NotRelics.mimaadef.mimaa>();
                    if (run.CurrentStation != null && run.CurrentStation.Type == StationType.Boss && exhibit != null && run.BaseMana.Colorless > 0)
                    {
                        run.LoseBaseMana(ManaGroup.Colorlesses(1), false);
                        yield break;
                    }
                }
                yield break;
            }
        }

        //PERSISTENT VALUES
        public class mimaplayerdata : CustomGameRunSaveData
        {
            public override void Restore(GameRunController gameRun)
            {
                log.LogDebug("bepinex restoring");
                LBoL.Core.Units.PlayerUnit player = gameRun.Player;
                Exhibit exhibit = player.GetExhibit<NotRelics.mimapassivesdef.mimapassives>();

                if (exhibit != null && exhibit is NotRelics.mimapassivesdef.mimapassives mimapassive)
                {
                    log.LogDebug("restoring passives");
                    mimapassive.haspassive = haspassive;
                    mimapassive.passivegold = passivegold;
                    mimapassive.passivepower = passivepower;
                    mimapassive.passivemb = passivemb;
                    mimapassive.passivembhand = passivembhand;
                    mimapassive.passiveimplosion = passiveimplosion;
                    mimapassive.passiveretribution = passiveretribution;
                    mimapassive.passiveeverlast = passiveeverlast;
                }
                else { haspassive = false; }
            }

            public override void Save(GameRunController gameRun)
            {
                log.LogDebug("bepinex saving");
                LBoL.Core.Units.PlayerUnit player = gameRun.Player;
                Exhibit exhibit = player.GetExhibit<NotRelics.mimapassivesdef.mimapassives>();
                
                if (exhibit != null && exhibit is NotRelics.mimapassivesdef.mimapassives mimapassive)
                {
                    log.LogDebug("saving passives");
                    haspassive = mimapassive.haspassive;
                    passivegold = mimapassive.passivegold;
                    passivepower = mimapassive.passivepower;
                    passivemb = mimapassive.passivemb;
                    passivembhand = mimapassive.passivembhand;
                    passiveimplosion = mimapassive.passiveimplosion;
                    passiveretribution = mimapassive.passiveretribution;
                    passiveeverlast = mimapassive.passiveeverlast;
                }
                else { haspassive = false; }
            }
            public bool haspassive;
            public int passivegold;
            public int passivepower;
            public int passivemb;
            public int passivembhand;
            public int passiveimplosion;
            public int passiveretribution;
            public int passiveeverlast;
        }

        private IEnumerator tabberbase()
        {
            GameRunController gamerun = GameMaster.Instance?.CurrentGameRun;
            if (gamerun != null)
            {
                Exhibit exhibit = gamerun.Player.GetExhibit<NotRelics.mimabdef.mimab>();
                if (exhibit != null)
                {
                    if (gamerun.Battle == null)
                    {
                        GameMaster.Instance.StartCoroutine(shopplugin.shopCoroutinePassive());
                    }
                    else
                    {
                        GameMaster.Instance.StartCoroutine(shopplugin.shopCoroutineBlitz());
                    }
                }
            }
            yield break;
        }
    }
}
