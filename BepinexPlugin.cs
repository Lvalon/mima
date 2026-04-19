using BepInEx;
using HarmonyLib;
using LBoL.Base;
using LBoL.Core;
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
using LBoL.Core.Battle;
using System;
using Random = System.Random;
using LBoL.Presentation.Units;
using LBoL.Presentation.UI.Panels;
using System.Collections;
using LBoL.Presentation.UI.ExtraWidgets;
using lvalonmima.Source.Patches;
using LBoLEntitySideloader.CustomHandlers;
using BepInEx.Configuration;
using lvalonmima.Config;


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

		public static ConfigEntry<double> mimaCardMult;

		public static CustomConfigEntry<double> mimaCardMultEntry = new CustomConfigEntry<double>(
			value: 0.1,
			section: "1. Content Modifications 內容設定",
			key: "Mima card weight multiplier when playing as another character 遊玩其他角色時的魅魔卡牌比重倍率",
			description: "Weight multiplier of Mima cards appearing when playing as another character (normal weight is 1). 以其他角色遊玩時魅魔卡牌的比重倍率（正常為 1）。");

		private static readonly Harmony harmony = lvalonmima.PInfo.harmony;

		internal static BepInEx.Logging.ManualLogSource log;

		internal static TemplateSequenceTable sequenceTable = new TemplateSequenceTable();

		internal static IResourceSource embeddedSource = new EmbeddedSource(Assembly.GetExecutingAssembly());

		// add this for audio loading
		internal static DirectorySource directorySource = new DirectorySource(lvalonmima.PInfo.GUID, "");


		private void Awake()
		{
			log = Logger;

			mimaCardMult = Config.Bind(mimaCardMultEntry.Section, mimaCardMultEntry.Key, mimaCardMultEntry.Value, mimaCardMultEntry.Description);

			// very important. Without this the entry point MonoBehaviour gets destroyed
			DontDestroyOnLoad(gameObject);
			gameObject.hideFlags = HideFlags.HideAndDontSave;

			CardIndexGenerator.PromiseClearIndexSet();
			EntityManager.RegisterSelf();
			new LiteProfileSaveData().RegisterSelf(PInfo.GUID);
			// new CustomSaveData().RegisterSelf(PInfo.GUID);

			harmony.PatchAll();

			CHandlerManager.RegisterGameEventHandler(
				gr => gr.StationEntering,
				ShopModHandlers.StationEntering
				);
			CHandlerManager.RegisterGameEventHandler(
				gr => gr.StationEntered,
				ShopModHandlers.StationEntered
				);
			CHandlerManager.RegisterGameEventHandler(
				gr => gr.StationEntered,
				ShopModHandlers.StationEnteredBlitz,
				GameEventPriority.Highest
				);
			CHandlerManager.RegisterGameEventHandler(
				gr => gr.DeckCardsAdded,
				ShopModHandlers.DeckCardsAdded
				);
			ShopModHandlers.addreactors();

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
		public void Reload(BepInEx.PluginInfo scriptEngineInfo, bool hardReload = false)
		{
			MiniTracker.DestroySelf();
		}
		public static bool u50 = false;
		public static bool u25 = false;
		public static bool u10 = false;
		private void Update()
		{
			var gamerun = GameMaster.Instance?.CurrentGameRun;
			if (gamerun == null)
			{
				ResetFlags();
				return;
			}

			HandleDeckCleanup(GameMaster.Instance);

			var battle = gamerun.Battle;
			if (battle == null)
			{
				ResetFlags();
				return;
			}

			var player = battle.Player;
			int hp = player.Hp;

			if (player.GetExhibit<exmimab>() != null && hp == 0)
			{
				Random r = new Random();
				if (r.Next(1, player.MaxHp * 100) <= player.GetExhibit<exmimab>().Counter)
				{
					UnitView target = GameDirector.GetUnit(player);

					Color color = Color.HSVToRGB(
					UnityEngine.Random.value,
					UnityEngine.Random.Range(0.8f, 1f),
					UnityEngine.Random.Range(0.85f, 1f)
					);

					Camera cam = Camera.main;
					float scale = cam != null ? cam.orthographicSize : 5f;

					float radiusBias = Mathf.Pow(UnityEngine.Random.value, 0.25f);
					Vector2 direction = UnityEngine.Random.insideUnitCircle.normalized;

					Vector2 chaosOffset2D =
						direction
						* radiusBias
						* scale
						* player.GetExhibit<exmimab>().Counter / player.MaxHp;

					PopupHud.Instance.PopupFromScene(
						int.MinValue,
						color,
						target.transform.position + (Vector3)chaosOffset2D
					);

					PopupHud.Instance.StartCoroutine(
				FollowupPopups(
						target,
						color,
						target.transform.position,
						(Vector3)chaosOffset2D
					));
				}
			}

			int hp50 = toolbox.hpfrompercent(player, 50);
			int hp25 = toolbox.hpfrompercent(player, 25);
			int hp10 = toolbox.hpfrompercent(player, 10);

			UpdateHighlight(battle, hp < hp50, ref u50, typeof(sehl50), typeof(exhl50));
			UpdateHighlight(battle, hp < hp25, ref u25, typeof(sehl25), typeof(exhl25));
			UpdateHighlight(battle, hp < hp10, ref u10, typeof(sehl10), typeof(exhl10));
		}
		private void ResetFlags()
		{
			u50 = false;
			u25 = false;
			u10 = false;
		}

		private void HandleDeckCleanup(GameMaster gamerun2)
		{
			var gamerun = GameMaster.Instance?.CurrentGameRun;
			if (gamerun == null)
				return;
			if (!gamerun.Packs.Contains(nameof(packtrumpDef)[..^3]))
				return;

			RemoveDuplicates(gamerun2, nameof(cardmimaexa));
			RemoveDuplicates(gamerun2, nameof(cardmimaexb));

			if (gamerun.Player.MaxHp >= 12)
				return;

			if (gamerun.Player.Id != modUniqueID)
				return;

			if (gamerun.Player.HasExhibit<exmimaa>() &&
				!HasCard(gamerun2, nameof(cardmimaexa)))
			{
				gamerun.AddDeckCard(Library.CreateCard<cardmimaexa>());
			}

			if (gamerun.Player.HasExhibit<exmimab>() &&
				!HasCard(gamerun2, nameof(cardmimaexb)))
			{
				gamerun.AddDeckCard(Library.CreateCard<cardmimaexb>());
			}
		}
		private void RemoveDuplicates(GameMaster gamerun2, string cardId)
		{
			var gamerun = GameMaster.Instance?.CurrentGameRun;
			if (gamerun == null)
				return;
			bool found = false;

			for (int i = gamerun.BaseDeck.Count - 1; i >= 0; i--)
			{
				if (gamerun.BaseDeck[i].Id != cardId)
					continue;

				if (!found)
				{
					found = true;
				}
				else
				{
					gamerun.RemoveDeckCard(gamerun.BaseDeck[i]);
				}
			}
		}

		private bool HasCard(GameMaster gamerun2, string cardId)
		{
			var gamerun = GameMaster.Instance?.CurrentGameRun;
			if (gamerun == null)
				return false;
			for (int i = 0; i < gamerun.BaseDeck.Count; i++)
			{
				if (gamerun.BaseDeck[i].Id == cardId)
					return true;
			}
			return false;
		}
		private void UpdateHighlight(
			BattleController battle,
			bool shouldBeActive,
			ref bool flag,
			Type t1,
			Type t2
		)
		{
			if (flag == shouldBeActive)
				return;

			flag = shouldBeActive;

			foreach (var unit in battle.AllAliveUnits)
			{
				foreach (var status in unit.StatusEffects)
				{
					if ((status.GetType() == t1 || status.GetType() == t2) &&
						status.Highlight != shouldBeActive)
					{
						status.Highlight = shouldBeActive;
					}
				}
			}
		}

		private static IEnumerator FollowupPopups(
		UnitView target,
		Color color,
		Vector3 basePosition,
		Vector2 initialOffset
		)
		{
			yield return new WaitForSeconds(-0.6f);
			SpawnFollowup(target, color, basePosition, initialOffset * 0.6f, 2f);

			yield return new WaitForSeconds(0.6f);
			SpawnFollowup(target, color, basePosition, initialOffset * 0.25f, 2f);
		}

		private static void SpawnFollowup(
			UnitView target,
			Color color,
			Vector3 basePosition,
			Vector2 offset,
			float scale
		)
		{
			// PopupHud.Instance.PopupFromScene(
			// 	int.MinValue,
			// 	color,
			// 	basePosition + (Vector3)offset
			// );
			DamagePopup obj = Instantiate(
		PopupHud.Instance.damagePopup,
		PopupHud.Instance.transform
	);

			obj.transform.localPosition =
				CameraController.ScenePositionToLocalPositionInRectTransform(
					basePosition + (Vector3)offset,
					(RectTransform)PopupHud.Instance.transform
				);

			obj.tmp.text = toolbox.gibberish();
			obj.tmp.color = color;

			obj.transform.localScale = Vector3.one * scale;

			obj.rb.linearVelocity = new Vector2(0, 0);
			obj.rb.gravityScale = 0;

			obj.gameObject.SetActive(value: true);

			Destroy(obj.gameObject, 0.1f);
		}
	}
}
