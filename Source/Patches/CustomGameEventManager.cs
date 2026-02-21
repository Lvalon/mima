using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using DG.Tweening;
using HarmonyLib;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActionRecord;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;
using LBoL.Presentation;
using LBoL.Presentation.UI.ExtraWidgets;
using LBoL.Presentation.UI.Panels;
using LBoL.Presentation.Units;
using lvalonmima.Cards;
using lvalonmima.Exhibits;
using lvalonmima.GunName;
using lvalonmima.lvalonmimaUlt;
using lvalonmima.Source.Patches;
using TMPro;
using UnityEngine;

namespace lvalonmima.Patches
{
	[HarmonyPatch]
	class CustomGameEventManager
	{
		[HarmonyPatch(typeof(GameRunController), "InternalGainPower")]
		private class GameRunController_InternalGainPower_Patch
		{

			private static void Prefix(GameRunController __instance, ref int power)
			{
				power *= 1 + (GameMaster.Instance?.CurrentGameRun?.Battle?.ExileZone.Count(c => c.Id == nameof(cardstone4)) ?? 0);
			}
		}
		[HarmonyPatch(typeof(PopupHud), nameof(PopupHud.PopupFromScene))]
		public static class PopupHud_PopupFromScene_Patch
		{
			static bool Prefix(
				PopupHud __instance,
				int popValue,
				Color color,
				Vector3 worldPosition
			)
			{
				if (popValue > int.MinValue)
					return true;

				string text = toolbox.gibberish();

				string coloredText = "";
				foreach (char c in text)
				{
					Color charColor = RandomBrightColor();
					string hex = ColorUtility.ToHtmlStringRGB(charColor);
					coloredText += $"<color=#{hex}>{c}</color>";
				}

				DamagePopup obj = Object.Instantiate(
					__instance.damagePopup,
					__instance.transform
				);

				obj.transform.localPosition =
					CameraController.ScenePositionToLocalPositionInRectTransform(
						worldPosition,
						(RectTransform)__instance.transform
					);

				obj.tmp.text = coloredText;

				// manual obj.Show() exec
				obj.rb.linearVelocity = new Vector2(0, 0);
				obj.rb.gravityScale = 0;

				obj.gameObject.SetActive(value: true);
				float scale = 1f + GameMaster.Instance?.CurrentGameRun?.Battle?.Player?.GetExhibit<exmimab>()?.Counter / GameMaster.Instance?.CurrentGameRun?.Battle?.Player?.MaxHp ?? 1f;

				obj.transform.localScale = Vector3.one * scale;

				//obj.transform.DOScale(new Vector3(1f, 1f, 1f), 1f).From(Vector3.one).SetEase(Ease.InSine);

				Object.Destroy(obj.gameObject, 0.2f);

				// obj.Show();

				return false;
			}

			// Helper: generate a bright random color
			private static Color RandomBrightColor()
			{
				return Color.HSVToRGB(
					Random.value,
					Random.Range(0.8f, 1f),
					Random.Range(0.85f, 1f)
				);
			}
		}



		public static GameEvent<DeadEventArgs> PreDeadEvent { get; set; }
		public static GameEvent<DeadEventArgs> PostDeadEvent { get; set; }
		[HarmonyPatch(typeof(ActionResolver), nameof(ActionResolver.InternalResolve))]
		public static class ActionResolver_InternalResolve_Prefix
		{
			private const int NewMaxDepth = 666666;

			static bool Prefix(
				ActionResolver __instance,
				PhaseRecord parentPhase,
				BattleAction action,
				int depth,
				string recordName,
				ref IEnumerator<object> __result
			)
			{
				if (depth <= NewMaxDepth)
					return true;
				Debug.LogError($"ActionResolver stack depth over {NewMaxDepth}, reactions regarded");

				__result = tmp();
				return false;
			}

			private static IEnumerator<object> tmp()
			{
				yield break;
			}
		}

		[HarmonyPatch(typeof(UnitView), nameof(UnitView.SpellDeclare))]
		public static class UnitView_SpellDeclare_Patch
		{
			public static bool Prefix(string spellName, ref IEnumerator __result)
			{
				var battle = GameMaster.Instance?.CurrentGameRun?.Battle;
				var checks = new List<(bool, bool)>
				{
					(
						battle?.PlayArea.Any(c => c.Id == nameof(cardtwilightspark)) == true,
						spellName == GunNameID.GetGunFromId(12180) || spellName == GunNameID.GetGunFromId(12181)
					),
				};
				if (checks.Any(pair => pair.Item1 && pair.Item2))
				{
					__result = Enumerable.Empty<object>().GetEnumerator();
					return false;
				}
				return true;
			}
		}
		[HarmonyPatch(typeof(SpellPanel), nameof(SpellPanel.CallSpellDeclare))]
		public static class SpellPanel_CallSpellDeclare_Patch
		{
			public static void Postfix(SpellPanel __instance)
			{
				var battle = GameMaster.Instance?.CurrentGameRun?.Battle;
				string spellName = __instance.spellName.text;
				string ultmimaaName = Library.CreateUs<ultmimaa>().Title;
				string ultmimabName = Library.CreateUs<ultmimab>().Title;
				if ((!spellName.Contains(ultmimaaName) && !spellName.Contains(ultmimabName)) || battle?.Player.Id != nameof(lvalonmima))
				{
					return;
				}
				if (__instance.portraitShadow != null)
				{
					__instance.portraitShadow.gameObject.SetActive(false);
				}

				StopCoroutineHelper(__instance, "CoInternalSpellDeclare");
				__instance.StartCoroutine(SpellDeclareUpperRight(__instance));
			}

			private static void StopCoroutineHelper(SpellPanel panel, string coroutineName)
			{
				panel.StopCoroutine(coroutineName);
			}

			private static IEnumerator SpellDeclareUpperRight(SpellPanel __instance)
			{
				__instance.spellName.gameObject.SetActive(value: true);
				__instance.spellName.GetComponent<RectTransform>().DOAnchorPos(Vector2.zero, 0.6f).From(new Vector2(1000f, 0f));
				__instance.spellName.DOFade(1f, 0.2f).From(0f);

				__instance.root.gameObject.SetActive(value: true);
				__instance.root.DOKill();
				__instance.root.GetComponent<CanvasGroup>().DOFade(1f, 0f);
				__instance.portrait.DOKill();
				__instance.portraitShadow.DOKill();

				Vector2 vector = new Vector2(__instance.portrait.transform.localPosition.x, __instance.portrait.transform.localPosition.y);

				__instance.portrait.GetComponent<RectTransform>().DOAnchorPos(new Vector2(-500f, 500f) + vector, 1.5f).From(new Vector2(2500f, 1000f))
					.SetEase(Ease.OutCubic);
				__instance.portrait.GetComponent<RectTransform>().DORotate(Vector3.zero, 1.5f).From(new Vector3(0f, 0f, 15f))
					.SetEase(Ease.OutCubic);

				__instance.portraitShadow.GetComponent<RectTransform>().DOAnchorPos(new Vector2(-700f, 300f) + vector, 1.5f).From(new Vector2(2500f, 1000f))
					.SetEase(Ease.OutCubic);
				__instance.portraitShadow.GetComponent<RectTransform>().DORotate(Vector3.zero, 1.5f).From(new Vector3(0f, 0f, 15f))
					.SetEase(Ease.OutCubic);

				__instance.background.GetComponent<RectTransform>().DOAnchorPos(new Vector2(1920f, 0f), 8f).From(new Vector2(-1920f, 0f))
					.SetLoops(-1, LoopType.Restart)
					.SetEase(Ease.Linear);
				__instance.speedLine1.GetComponent<RectTransform>().DOAnchorPos(new Vector2(5120f, 0f), 0.5f).From(Vector2.zero)
					.SetLoops(-1, LoopType.Restart)
					.SetEase(Ease.Linear);
				__instance.speedLine2.GetComponent<RectTransform>().DOAnchorPos(new Vector2(-4096f, 0f), 0.5f).From(Vector2.zero)
					.SetLoops(-1, LoopType.Restart)
					.SetEase(Ease.Linear);
				__instance.trail.GetComponent<RectTransform>().DOAnchorPos(new Vector2(0f, 5000f), 0.5f).From(new Vector2(0f, -1000f));

				__instance.root.GetComponent<CanvasGroup>().DOFade(0f, 0.4f).From(1f)
					.SetDelay(1.5f);
				__instance.particleRoot.Play();

				yield return new WaitForSeconds(2f);

				__instance.background.GetComponent<RectTransform>().DOKill();
				__instance.speedLine1.GetComponent<RectTransform>().DOKill();
				__instance.speedLine2.GetComponent<RectTransform>().DOKill();
				__instance.root.gameObject.SetActive(value: false);

				__instance.spellName.gameObject.SetActive(value: false);
			}
		}
	}
}