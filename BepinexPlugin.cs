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
using LBoLEntitySideloader.PersistentValues;
using static lvalonmima.NotRelics.mimapassivesdef;
using static lvalonmima.NotRelics.mimaadef;
using static lvalonmima.mimaextensions;
using static lvalonmima.NotRelics.mimabdef;

namespace lvalonmima
{
    [BepInPlugin(lvalonmima.PInfo.GUID, lvalonmima.PInfo.Name, lvalonmima.PInfo.version)]
    [BepInDependency(LBoLEntitySideloader.PluginInfo.GUID, BepInDependency.DependencyFlags.HardDependency)]
    //[BepInDependency(AddWatermark.API.GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInProcess("LBoL.exe")]
    public class BepinexPlugin : BaseUnityPlugin
    {

        private static readonly Harmony harmony = lvalonmima.PInfo.harmony;

        internal static BepInEx.Logging.ManualLogSource log;

        internal static TemplateSequenceTable sequenceTable = new TemplateSequenceTable();

        internal static IResourceSource embeddedSource = new EmbeddedSource(Assembly.GetExecutingAssembly());

        // add this for audio loading
        internal static DirectorySource directorySource = new DirectorySource(lvalonmima.PInfo.GUID, "");

        static KeyboardShortcut TestTab = new KeyboardShortcut(KeyCode.Tab);

        public readonly Dictionary<string, Toggle> _characterFilterList = new Dictionary<string, Toggle>();
        public IEnumerable<Type> FilterCardByCharacter()
        {
            List<Type> list = new List<Type>();
            string[] array = { "Mima" };
            foreach (var (item, cardConfig) in Library.EnumerateCardTypes())
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

            (new mimaplayerdata()).RegisterSelf(LBoLEntitySideloader.PluginInfo.GUID);
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
        //the funny starts here
        private void Update()
        {
            var gamerun = GameMaster.Instance?.CurrentGameRun;
            if (gamerun != null)
            {
                //HandleGameRunEvent<CardsEventArgs>(gamerun.DeckCardsAdding, new GameEventHandler<CardsEventArgs>(OnDeckCardsAdding));
                if (TestTab.IsDown())
                {
                    var player = gamerun.Player;
                    if (player.HasExhibit<mimab>() && gamerun.Money >= 10 && gamerun.CurrentStation.Level != 0 || gamerun.CurrentStation.Stage.Id != "BambooForest")
                    {
                        GameMaster.Instance.StartCoroutine(tabberbase());
                    }
                }
            }
        }
        [HarmonyPatch(typeof(GameRunController), nameof(GameRunController.Create))]
        class GameRunController_Create_Patch
        {
            static void OnDeckCardsAdding(CardsEventArgs args)
            {
                var gamerun = GameMaster.Instance?.CurrentGameRun;
                int num = args.Cards.Count((Card card) => card is mimacard mimascard && mimascard.ispassive == true);
                var player = gamerun.Player;
                bool hasexhibit = player.HasExhibit<mimapassives>();
                if (num > 0 && hasexhibit == false)
                {
                    GameMaster.Instance.StartCoroutine(GainExhibits(
                             gameRun: gamerun,
                             exhibits: new HashSet<Type>() { typeof(mimapassives) },
                             triggerVisual: true,
                             exhibitSource: new VisualSourceData()
                             {
                                 SourceType = VisualSourceType.Debug,
                                 Source = null
                             }));
                }
            }
            static void Postfix(GameRunController __result)
            {
                Instance.HandleGameRunEvent<CardsEventArgs>(__result.DeckCardsAdding, new GameEventHandler<CardsEventArgs>(OnDeckCardsAdding));
            }
        }
        static IEnumerator GainExhibits(GameRunController gameRun, HashSet<Type> exhibits, bool triggerVisual = false, VisualSourceData exhibitSource = null)
        {
            foreach (var et in exhibits)
            {
                var ex = Library.CreateExhibit(et);
                ex.GameRun = gameRun;

                yield return gameRun.GainExhibitRunner(ex, triggerVisual, exhibitSource);
            }

            gameRun.ExhibitPool.RemoveAll(e => exhibits.Contains(e));
        }

        //MIMAA REMOVE BASE MANA
        class CoroutineExtender : IEnumerable
        {
            public IEnumerator target_enumerator;
            public List<IEnumerator> preItems = new List<IEnumerator>();
            public List<IEnumerator> postItems = new List<IEnumerator>();
            public List<IEnumerator> midItems = new List<IEnumerator>();


            public CoroutineExtender() { }

            public CoroutineExtender(IEnumerator target_enumerator) { this.target_enumerator = target_enumerator; }

            public IEnumerator GetEnumerator()
            {
                foreach (var e in preItems) yield return e;
                int i = 0;
                while (target_enumerator.MoveNext())
                {
                    yield return target_enumerator.Current;
                    i++;
                }
                foreach (var e in postItems) yield return e;
            }
        }

        [HarmonyPatch(typeof(Exhibit), "TriggerGain")]
        [HarmonyDebug]
        class Exhibit_Patch
        {
            static void Postfix(ref IEnumerator __result)
            {
                var extendedRez = new CoroutineExtender(__result);

                extendedRez.postItems.Add(mimaArmrf());

                __result = extendedRez.GetEnumerator();
            }

            static IEnumerator mimaArmrf()
            {
                if ((GameMaster.Instance != null) && (GameMaster.Instance.CurrentGameRun != null))
                {
                    var run = GameMaster.Instance.CurrentGameRun;
                    Exhibit exhibit = run.Player.GetExhibit<mimaa>();
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
                var player = gameRun.Player;
                Exhibit exhibit = player.GetExhibit<mimapassives>();
                if (exhibit != null && exhibit is mimapassives mimapassive)
                {
                    mimapassive.passivegold = passivegold;
                    mimapassive.passivepower = passivepower;
                    mimapassive.passivemb = passivemb;
                    mimapassive.passivembhand = passivembhand;
                }
            }

            public override void Save(GameRunController gameRun)
            {
                var player = gameRun.Player;
                Exhibit exhibit = player.GetExhibit<mimapassives>();
                if (exhibit != null && exhibit is mimapassives mimapassive)
                {
                    passivegold = mimapassive.passivegold;
                    passivepower = mimapassive.passivepower;
                    passivemb = mimapassive.passivemb;
                    passivembhand = mimapassive.passivembhand;
                }
            }
            public int passivegold;
            public int passivepower;
            public int passivemb;
            public int passivembhand;
        }

        private IEnumerator tabberbase()
        {
            var gamerun = GameMaster.Instance?.CurrentGameRun;
            if (gamerun != null)
            {
                Exhibit exhibit = gamerun.Player.GetExhibit<mimab>();
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
            //GameMaster.Instance.StartCoroutine(tabber());
            yield break;
        }
    }
}
