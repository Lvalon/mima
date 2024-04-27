using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using LBoLEntitySideloader.Entities;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Adventures;
using LBoL.Core.Attributes;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActionRecord;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
using LBoL.Core.Dialogs;
using LBoL.Core.GapOptions;
using LBoL.Core.Helpers;
using LBoL.Core.Intentions;
using LBoL.Core.JadeBoxes;
using LBoL.Core.PlatformHandlers;
using LBoL.Core.Randoms;
using LBoL.Core.SaveData;
using LBoL.Core.Stations;
using LBoL.Core.Stats;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.Adventures;
using LBoL.EntityLib.Adventures.Common;
using LBoL.EntityLib.Adventures.FirstPlace;
using LBoL.EntityLib.Adventures.Shared12;
using LBoL.EntityLib.Adventures.Shared23;
using LBoL.EntityLib.Adventures.Stage1;
using LBoL.EntityLib.Adventures.Stage2;
using LBoL.EntityLib.Adventures.Stage3;
using LBoL.EntityLib.Cards.Character.Cirno;
using LBoL.EntityLib.Cards.Character.Koishi;
using LBoL.EntityLib.Cards.Character.Marisa;
using LBoL.EntityLib.Cards.Character.Reimu;
using LBoL.EntityLib.Cards.Character.Sakuya;
using LBoL.EntityLib.Cards.Neutral;
using LBoL.EntityLib.Cards.Neutral.Black;
using LBoL.EntityLib.Cards.Neutral.Blue;
using LBoL.EntityLib.Cards.Neutral.Green;
using LBoL.EntityLib.Cards.Neutral.MultiColor;
using LBoL.EntityLib.Cards.Neutral.NoColor;
using LBoL.EntityLib.Cards.Neutral.Red;
using LBoL.EntityLib.Cards.Neutral.TwoColor;
using LBoL.EntityLib.Cards.Neutral.White;
using LBoL.EntityLib.Cards.Adventure;
using LBoL.EntityLib.Cards.Enemy;
using LBoL.EntityLib.Cards.Misfortune;
using LBoL.EntityLib.Cards.Tool;
using LBoL.EntityLib.Dolls;
using LBoL.EntityLib.EnemyUnits.Character;
using LBoL.EntityLib.EnemyUnits.Character.DreamServants;
using LBoL.EntityLib.EnemyUnits.Lore;
using LBoL.EntityLib.EnemyUnits.Normal;
using LBoL.EntityLib.EnemyUnits.Normal.Bats;
using LBoL.EntityLib.EnemyUnits.Normal.Drones;
using LBoL.EntityLib.EnemyUnits.Normal.Guihuos;
using LBoL.EntityLib.EnemyUnits.Normal.Maoyus;
using LBoL.EntityLib.EnemyUnits.Normal.Ravens;
using LBoL.EntityLib.EnemyUnits.Opponent;
using LBoL.EntityLib.Exhibits;
using LBoL.EntityLib.Exhibits.Adventure;
using LBoL.EntityLib.Exhibits.Common;
using LBoL.EntityLib.Exhibits.Mythic;
using LBoL.EntityLib.Exhibits.Seija;
using LBoL.EntityLib.Exhibits.Shining;
using LBoL.EntityLib.JadeBoxes;
using LBoL.EntityLib.Mixins;
using LBoL.EntityLib.PlayerUnits;
using LBoL.EntityLib.Stages;
using LBoL.EntityLib.Stages.NormalStages;
using LBoL.EntityLib.StatusEffects.Basic;
using LBoL.EntityLib.StatusEffects.Cirno;
using LBoL.EntityLib.StatusEffects.Enemy;
using LBoL.EntityLib.StatusEffects.Enemy.SeijaItems;
using LBoL.EntityLib.StatusEffects.Marisa;
using LBoL.EntityLib.StatusEffects.Neutral;
using LBoL.EntityLib.StatusEffects.Neutral.Black;
using LBoL.EntityLib.StatusEffects.Neutral.Blue;
using LBoL.EntityLib.StatusEffects.Neutral.Green;
using LBoL.EntityLib.StatusEffects.Neutral.Red;
using LBoL.EntityLib.StatusEffects.Neutral.TwoColor;
using LBoL.EntityLib.StatusEffects.Neutral.White;
using LBoL.EntityLib.StatusEffects.Others;
using LBoL.EntityLib.StatusEffects.Reimu;
using LBoL.EntityLib.StatusEffects.Sakuya;
using LBoL.EntityLib.UltimateSkills;
using LBoL.Presentation;
using static LBoL.Presentation.GameMaster;
using LBoL.Presentation.Animations;
using LBoL.Presentation.Bullet;
using LBoL.Presentation.Effect;
using LBoL.Presentation.I10N;
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.Dialogs;
using LBoL.Presentation.UI.ExtraWidgets;
using LBoL.Presentation.UI.Panels;
using LBoL.Presentation.UI.Transitions;
using LBoL.Presentation.UI.Widgets;
using LBoL.Presentation.Units;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Resource;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using UnityEngine;
using Untitled;
using Untitled.ConfigDataBuilder;
using Untitled.ConfigDataBuilder.Base;
using Debug = UnityEngine.Debug;
using UnityEngine.UI;
using static lvalonmima.NotRelics.mimabdef.mimab;
using LBoLEntitySideloader.PersistentValues;
using static lvalonmima.playermima;
using static lvalonmima.NotRelics.mimapassivesdef;
using static lvalonmima.NotRelics.mimaadef;
using LBoLEntitySideloader.ExtraFunc;

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
        }

        private void OnDestroy()
        {
            if (harmony != null)
                harmony.UnpatchSelf();
        }

        //the funny starts here
        private void Update()
        {
            if (TestTab.IsDown())
            {
                if (GameMaster.Instance?.CurrentGameRun != null)
                {
                    GameMaster.Instance.StartCoroutine(tabber());
                }
                else
                {
                    log.LogInfo("run needs to be started");
                }
            }
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
                }
            }

            public override void Save(GameRunController gameRun)
            {
                var player = gameRun.Player;
                Exhibit exhibit = player.GetExhibit<mimapassives>();
                if (exhibit != null && exhibit is mimapassives mimapassive)
                {
                    passivegold = mimapassive.passivegold;
                }
            }

            public int passivegold;
        }

        private IEnumerator tabber()
        {
            //initcardpool();
            //ShowCardsPayload payload = new ShowCardsPayload
            //{
            //    Name = "Game.Deck".Localize(true),
            //    Description = "Cards.Show".Localize(true),
            //    Cards = extradeck,
            //    InteractionType = InteractionType.None,
            //    CardZone = ShowCardZone.None
            //};
            //UiManager.GetPanel<ShowCardsPanel>().Show(payload);
            yield break;
        }
    }
}
