using BepInEx;
using HarmonyLib;
using LBoL.Base;
using LBoL.Core;
using System.Linq;
using LBoL.Core.StatusEffects;
using LBoL.EntityLib.EnemyUnits.Character;
using LBoL.Presentation;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Resource;
using lvalonmima.Cards;
using lvalonmima.Cards.Template;
using lvalonmima.Exhibits;
using lvalonmima.StatusEffects;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using lvalonmima.Source.Packs;


namespace lvalonmima
{
	[BepInPlugin(lvalonmima.PInfo.GUID, lvalonmima.PInfo.Name, lvalonmima.PInfo.version)]
	[BepInDependency(LBoLEntitySideloader.PluginInfo.GUID, BepInDependency.DependencyFlags.HardDependency)]
	//[BepInDependency(AddWatermark.API.GUID, BepInDependency.DependencyFlags.SoftDependency)]
	[BepInProcess("LBoL.exe")]
	public class BepinexPlugin : BaseUnityPlugin
	{
		//The Unique mod ID of the mod.
		//If defined, this is also the ID used by the Act 1 boss.
		//WARNING: It is mandatory to rename it to avoid issues.
		public static string modUniqueID = "lvalonmima";
		//Name of the character.
		//This is also the prefix that is used before every .png file in DirResources. 
		public static string playerName = "Mima";
		//Whether to us an ingame or custom model.
		//InGame: Will load the character model of the ingame character.
		//Custom: Will load DirResource/lvalonmimamodel.png 
		public static bool useInGameModel = false;
		//If InGame is selected, this is the model that will be loaded. 
		//Check LBoL.EntityLib.EnemyUnits.Character or using LBoL.EntityLib.PlayerUnits for a list of all the characters available. 
		public static string modelName = nameof(Youmu);
		//Some in-game model needs to be flipped (most notably elites).
		public static bool modelIsFlipped = true;
		//The character's off-color.
		//Used to separate cards in the card collection and put the off-color cards at the end.
		public static List<ManaColor> offColors = new List<ManaColor>() { ManaColor.White, ManaColor.Blue, ManaColor.Red };
		public static ManaGroup offColorsMana = new ManaGroup() { White = 1, Blue = 1, Red = 1 };

		private static readonly Harmony harmony = lvalonmima.PInfo.harmony;

		internal static BepInEx.Logging.ManualLogSource log;

		internal static TemplateSequenceTable sequenceTable = new TemplateSequenceTable();

		internal static IResourceSource embeddedSource = new EmbeddedSource(Assembly.GetExecutingAssembly());

		// add this for audio loading
		internal static DirectorySource directorySource = new DirectorySource(lvalonmima.PInfo.GUID, "");


		private void Awake()
		{
			log = Logger;

			// very important. Without this the entry point MonoBehaviour gets destroyed
			DontDestroyOnLoad(gameObject);
			gameObject.hideFlags = HideFlags.HideAndDontSave;

			CardIndexGenerator.PromiseClearIndexSet();
			EntityManager.RegisterSelf();

			harmony.PatchAll();

			//if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(AddWatermark.API.GUID))
			//    WatermarkWrapper.ActivateWatermark();

			//Func<Sprite> getSprite = () => ResourceLoader.LoadSprite("BossIcon.png", directorySource);
			//EnemyUnitTemplate.AddBossNodeIcon(nameof(lvalonmima.Enemies.lvalonmima), getSprite);
		}

		private void OnDestroy()
		{
			if (harmony != null)
				harmony.UnpatchSelf();
		}
		public static bool u50 = false;
		public static bool u25 = false;
		public static bool u10 = false;
		private void Update()
		{
			var gamerun = GameMaster.Instance?.CurrentGameRun;
			if (gamerun == null)
			{
				u50 = false;
				u25 = false;
				u10 = false;
				return;
			}
			if (gamerun.Packs.Contains(nameof(packtrumpDef)[..^3]))
			{
				while (gamerun.BaseDeck.Count(c => c.Id == nameof(cardmimaexa)) > 1)
				{
					gamerun.RemoveDeckCard(gamerun.BaseDeck.FirstOrDefault(c => c.Id == nameof(cardmimaexa)));
				}
				while (gamerun.BaseDeck.Count(c => c.Id == nameof(cardmimaexb)) > 1)
				{
					gamerun.RemoveDeckCard(gamerun.BaseDeck.FirstOrDefault(c => c.Id == nameof(cardmimaexb)));
				}
				if (gamerun.Player.MaxHp < 12)
				{
					if (gamerun.Player.Id == modUniqueID && gamerun.Player.HasExhibit<exmimaa>() && gamerun.BaseDeck.Count(c => c.Id == nameof(cardmimaexa)) < 1)
					{
						gamerun.AddDeckCard(Library.CreateCard<cardmimaexa>());
					}
					if (gamerun.Player.Id == modUniqueID && gamerun.Player.HasExhibit<exmimab>() && gamerun.BaseDeck.Count(c => c.Id == nameof(cardmimaexb)) < 1)
					{
						gamerun.AddDeckCard(Library.CreateCard<cardmimaexb>());
					}
				}
			}
			var battle = gamerun.Battle;
			if (battle != null)
			{
				if (battle.Player.Hp < toolbox.hpfrompercent(battle.Player, 50))
				{
					u50 = true;
					IEnumerable<StatusEffect> se50 = battle.AllAliveUnits.SelectMany(u => u.StatusEffects).Where(s => (s is sehl50 || s is exhl50) && s.Highlight == false);
					if (se50.Count() > 0)
					{
						foreach (StatusEffect se in se50)
						{
							se.Highlight = true;
						}
					}
				}
				else
				{
					u50 = false;
					IEnumerable<StatusEffect> se50 = battle.AllAliveUnits.SelectMany(u => u.StatusEffects).Where(s => (s is sehl50 || s is exhl50) && s.Highlight == true);
					if (se50.Count() > 0)
					{
						foreach (StatusEffect se in se50)
						{
							se.Highlight = false;
						}
					}
				}

				if (battle.Player.Hp < toolbox.hpfrompercent(battle.Player, 25))
				{
					u25 = true;
					IEnumerable<StatusEffect> se25 = battle.AllAliveUnits.SelectMany(u => u.StatusEffects).Where(s => (s is sehl25 || s is exhl25) && s.Highlight == false);
					if (se25.Count() > 0)
					{
						foreach (StatusEffect se in se25)
						{
							se.Highlight = true;
						}
					}
				}
				else
				{
					u25 = false;
					IEnumerable<StatusEffect> se25 = battle.AllAliveUnits.SelectMany(u => u.StatusEffects).Where(s => (s is sehl25 || s is exhl25) && s.Highlight == true);
					if (se25.Count() > 0)
					{
						foreach (StatusEffect se in se25)
						{
							se.Highlight = false;
						}
					}
				}

				if (battle.Player.Hp < toolbox.hpfrompercent(battle.Player, 10))
				{
					u10 = true;
					IEnumerable<StatusEffect> se10 = battle.AllAliveUnits.SelectMany(u => u.StatusEffects).Where(s => (s is sehl10 || s is exhl10) && s.Highlight == false);
					if (se10.Count() > 0)
					{
						foreach (StatusEffect se in se10)
						{
							se.Highlight = true;
						}
					}
				}
				else
				{
					u10 = false;
					IEnumerable<StatusEffect> se10 = battle.AllAliveUnits.SelectMany(u => u.StatusEffects).Where(s => (s is sehl10 || s is exhl10) && s.Highlight == true);
					if (se10.Count() > 0)
					{
						foreach (StatusEffect se in se10)
						{
							se.Highlight = false;
						}
					}
				}
			}
			else
			{
				u50 = false;
				u25 = false;
				u10 = false;
			}
		}
	}
}
