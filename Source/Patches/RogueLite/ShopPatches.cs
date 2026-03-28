using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Security.Cryptography;
using System.Text;
using Cysharp.Threading.Tasks;
using HarmonyLib;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.SaveData;
using LBoL.Core.Units;
using LBoL.Presentation;
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.ExtraWidgets;
using LBoL.Presentation.UI.Panels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using YamlDotNet.Serialization;
using static LBoL.Presentation.GameMaster;
using lvalonmima.Exhibits;

namespace lvalonmima.Source.Patches
{
	// [HarmonyPatch(typeof(BattleController), nameof(BattleController.Flow), MethodType.Enumerator)]
	// public static class InvertFlow_Patch
	// {
	// 	internal static int PreloadedForRound;
	// 	static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
	// 	{
	// 		var playerFlow = AccessTools.Method(typeof(BattleController), nameof(BattleController.PlayerTurnFlow));
	// 		var enemyFlow = AccessTools.Method(typeof(BattleController), nameof(BattleController.EnemyTurnFlow));
	// 		var maybePlayerFlow = AccessTools.Method(typeof(InvertFlow_Patch), nameof(MaybePlayerFlow));
	// 		var maybeEnemyFlow = AccessTools.Method(typeof(InvertFlow_Patch), nameof(MaybeEnemyFlow));

	// 		foreach (var instr in instructions)
	// 		{
	// 			if (instr.Calls(playerFlow))
	// 			{
	// 				yield return new CodeInstruction(OpCodes.Call, maybePlayerFlow);
	// 				continue;
	// 			}

	// 			if (instr.Calls(enemyFlow))
	// 			{
	// 				yield return new CodeInstruction(OpCodes.Call, maybeEnemyFlow);
	// 				continue;
	// 			}

	// 			yield return instr;
	// 		}
	// 	}

	// 	private static IEnumerator MaybePlayerFlow(BattleController __instance)
	// 	{
	// 		if (ShouldInvertFlow())
	// 			return __instance.EnemyTurnFlow();

	// 		return __instance.PlayerTurnFlow();
	// 	}

	// 	private static IEnumerator MaybeEnemyFlow(BattleController __instance)
	// 	{
	// 		if (ShouldInvertFlow())
	// 		{
	// 			PreloadIntentions(__instance);
	// 			return __instance.PlayerTurnFlow();
	// 		}

	// 		return __instance.EnemyTurnFlow();
	// 	}

	// 	private static bool ShouldInvertFlow()
	// 	{
	// 		var shop = MiniTracker.Instance?.CustomGrSaveData?.GetShopForCurrentProfile();
	// 		if (shop == null || !shop.ChallengerModeEnabled)
	// 			return false;

	// 		return shop.GetItem("difficulty.reverse")?.CurrentTier <= 0;
	// 	}

	// 	private static void PreloadIntentions(BattleController __instance)
	// 	{
	// 		foreach (EnemyUnit enemy in __instance.AllAliveEnemies)
	// 		{
	// 			enemy.UpdateTurnMoves();
	// 		}
	// 		// record that we preloaded for round (current + 1)
	// 		try
	// 		{
	// 			PreloadedForRound = __instance.RoundCounter + 1;
	// 		}
	// 		catch
	// 		{
	// 			PreloadedForRound = 0;
	// 		}
	// 	}
	// }

	// [HarmonyPatch(typeof(BattleController), nameof(BattleController.StartRound))]
	// public static class BattleController_StartRound_Patch
	// {
	// 	static bool Prefix(BattleController __instance)
	// 	{
	// 		if (InvertFlow_Patch.PreloadedForRound != 0 && InvertFlow_Patch.PreloadedForRound == __instance.RoundCounter)
	// 		{
	// 			try
	// 			{
	// 				__instance.RoundStartDecreaseDurations();
	// 			}
	// 			finally
	// 			{
	// 				InvertFlow_Patch.PreloadedForRound = 0;
	// 			}
	// 			return false;
	// 		}
	// 		return true;
	// 	}
	// }
	[HarmonyPatch(typeof(BattleController), nameof(BattleController.PlayerTurnFlow))]
	public static class BattleController_PlayerTurnFlow_Patch
	{
		static bool Prefix(BattleController __instance, ref IEnumerator __result)
		{
			try
			{
				if (MiniTracker.Instance?.CustomGrSaveData?.GetShopForCurrentProfile()?.ChallengerModeEnabled == true
				&& MiniTracker.Instance?.CustomGrSaveData?.GetShopForCurrentProfile()?.GetItem("difficulty.reverse")?.CurrentTier == 0
				&& __instance.Player?.TurnCounter == 0
				&& __instance.RoundCounter == 1)
				{
					__result = Enumerable.Empty<object>().GetEnumerator();
					return false;
				}
			}
			catch (Exception)
			{
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(SystemBoard), nameof(SystemBoard.OnEnterGameRun))]
	public static class SystemBoard_OnEnterGameRun_Patch
	{
		private const string WatermarkName = "challengerModeWatermark";
		private const string WatermarkRaycastName = "challengerModeWatermarkRaycast";
		private const string WatermarkAnchorName = "challengerModeWatermarkAnchor";
		private const float WatermarkSpacing = 50f;
		private static bool _watermarkDisabled;

		public static void Postfix(SystemBoard __instance)
		{
			try
			{
				_watermarkDisabled = false;
				UpdateChallengerModeWatermark(__instance);
			}
			catch (Exception)
			{
			}
		}

		public static void DisableWatermarkAll()
		{
			_watermarkDisabled = true;
			try
			{
				foreach (var board in UnityEngine.Object.FindObjectsByType<SystemBoard>(FindObjectsSortMode.None))
				{
					if (board != null)
						SetWatermarkActive(board, false, null);
				}
			}
			catch (Exception)
			{
			}
		}

		private static void UpdateChallengerModeWatermark(SystemBoard board)
		{
			if (_watermarkDisabled)
			{
				SetWatermarkActive(board, false, null);
				return;
			}

			var shop = MiniTracker.Instance?.CustomGrSaveData?.GetShopForCurrentProfile();
			if (shop == null || !shop.ChallengerModeEnabled)
			{
				SetWatermarkActive(board, false, null);
				return;
			}
			string text = BuildChallengerModeWatermarkText(shop);
			var itemEffects = BuildChallengerModeWatermarkItemEffects(shop);
			SetWatermarkActive(board, true, text, itemEffects);
		}

		private static string BuildChallengerModeWatermarkText(LiteShop shop)
		{
			string header = GetLocalizedText($"{LocalisationKeys.ShopPrefix}{LocalisationKeys.BattlePrefix}ChallengerModeWatermark.HoverHint");
			header = $"<b>|e:{header}|</b>";

			var sb = new StringBuilder(header);
			var categoryOrder = new[]
			{
				LocalisationKeys.DifficultyPrefix,
				LocalisationKeys.InitPrefix,
				LocalisationKeys.DiscountPrefix,
				LocalisationKeys.FeaturePrefix,
				LocalisationKeys.BattlePrefix,
				LocalisationKeys.AlterPrefix,
			};

			foreach (var prefix in categoryOrder)
			{
				var items = shop.Items.Values
					.Where(item => item.CurrentTier > 0 && item.Id.StartsWith(prefix, StringComparison.Ordinal))
					.ToList();
				if (items.Count == 0)
					continue;

				string categoryKey = $"{LocalisationKeys.ShopPrefix}{prefix[..^1]}";
				string categoryName = GetLocalizedText(categoryKey);
				string categoryLinkId = prefix[..^1];
				// Make category header a link so the watermark tooltip system can show category descriptions
				sb.Append('\n').Append($"<link=\"{categoryLinkId}\"><b>{categoryName}</b></link>");

				foreach (var item in items)
				{
					string nameKey = $"{LocalisationKeys.ShopPrefix}{item.Id}";
					string name = GetLocalizedText(nameKey);
					if (name == nameKey)
						name = item.Id;
					string coloredName = LocalisationKeys.ColorizeTierName(name, item.CurrentTier, item.MaxTier);
					string linkedName = $"<link=\"{item.Id}\">{coloredName}</link>";
					string line = $"<b>{linkedName}</b>";
					if (item.MaxTier > 1)
						line = $"{line} {item.CurrentTier}";

					sb.Append('\n').Append("  ").Append(line);
				}
			}

			return StringDecorator.Decorate(sb.ToString());
		}

		private readonly struct WatermarkItemTooltipData
		{
			public readonly string Title;
			public readonly string Effect;

			public WatermarkItemTooltipData(string title, string effect)
			{
				Title = title ?? string.Empty;
				Effect = effect ?? string.Empty;
			}
		}

		private static Dictionary<string, WatermarkItemTooltipData> BuildChallengerModeWatermarkItemEffects(LiteShop shop)
		{
			var itemEffects = new Dictionary<string, WatermarkItemTooltipData>(StringComparer.Ordinal);
			var categoryOrder = new[]
			{
				LocalisationKeys.DifficultyPrefix,
				LocalisationKeys.InitPrefix,
				LocalisationKeys.DiscountPrefix,
				LocalisationKeys.FeaturePrefix,
				LocalisationKeys.BattlePrefix,
				LocalisationKeys.AlterPrefix,
			};

			foreach (var prefix in categoryOrder)
			{
				var items = shop.Items.Values
						.Where(item => item.CurrentTier > 0 && item.Id.StartsWith(prefix, StringComparison.Ordinal))
						.ToList();
				if (items.Count == 0)
					continue;

				// Add a tooltip entry for the category itself (keyed by the prefix without trailing dot)
				string categoryId = prefix[..^1];
				string categoryKey = $"{LocalisationKeys.ShopPrefix}{categoryId}";
				string categoryName = GetLocalizedText(categoryKey);
				string categoryEffect = LocalisationKeys.GetShopItemDescription(categoryId, false);
				if (!string.IsNullOrWhiteSpace(categoryEffect))
				{
					string decoratedTitle = StringDecorator.Decorate(categoryName);
					string decoratedEffect = StringDecorator.Decorate(categoryEffect);
					itemEffects[categoryId] = new WatermarkItemTooltipData(decoratedTitle, decoratedEffect);
				}

				foreach (var item in items)
				{
					string nameKey = $"{LocalisationKeys.ShopPrefix}{item.Id}";
					string name = GetLocalizedText(nameKey);
					if (name == nameKey)
						name = item.Id;
					string effect = LocalisationKeys.GetShopItemDescription(item.Id, false);
					if (string.IsNullOrWhiteSpace(effect))
						continue;

					string decoratedTitle = StringDecorator.Decorate(name);
					string decoratedEffect = StringDecorator.Decorate(effect);
					itemEffects[item.Id] = new WatermarkItemTooltipData(decoratedTitle, decoratedEffect);
				}
			}

			return itemEffects;
		}

		private static void SetWatermarkActive(SystemBoard board, bool isActive, string text, Dictionary<string, WatermarkItemTooltipData> itemEffects = null)
		{
			if (board?.gameVersion == null)
				return;

			Transform parent = board.gameVersion.gameObject.transform.parent;
			if (parent == null)
				return;

			GameObject watermark = null;
			float? baseY = null;
			int activeCount = 0;

			foreach (Transform item in parent)
			{
				var child = item;
				if (child.gameObject.activeSelf && child.name != "Hint" && child.name != WatermarkName)
				{
					activeCount++;
					if (!baseY.HasValue)
						baseY = child.localPosition.y;
				}
				if (child.name == WatermarkName)
					watermark = child.gameObject;
			}

			if (!isActive)
			{
				watermark?.SetActive(false);
				return;
			}

			if (watermark == null)
			{
				watermark = UnityEngine.Object.Instantiate(board.gameVersion.gameObject, parent);
				watermark.name = WatermarkName;
			}

			var tmp = watermark.GetComponent<TextMeshProUGUI>();
			if (tmp != null)
			{
				tmp.text = text ?? string.Empty;
				tmp.alignment = TextAlignmentOptions.TopRight;
				tmp.textWrappingMode = TextWrappingModes.NoWrap;
				tmp.ForceMeshUpdate();
				var rectTransform = tmp.rectTransform;
				rectTransform.pivot = new Vector2(1f, 1f);
				rectTransform.anchorMin = new Vector2(1f, 1f);
				rectTransform.anchorMax = new Vector2(1f, 1f);
				rectTransform.sizeDelta = new Vector2(tmp.preferredWidth, tmp.preferredHeight);
				ApplyWatermarkVisibility(tmp);

				GameObject raycastTarget = GetOrCreateWatermarkRaycastTarget(tmp);
				RectTransform tooltipAnchor = GetOrCreateWatermarkTooltipAnchor(tmp);
				var tooltip = tooltipAnchor.GetComponent<SimpleTooltipSource>();
				if (tooltip == null)
					tooltip = SimpleTooltipSource.CreateDirect(tooltipAnchor.gameObject, string.Empty).WithPosition(TooltipDirection.Bottom, TooltipAlignment.Max);
				var itemTooltip = raycastTarget.GetComponent<WatermarkItemTooltip>();
				if (itemTooltip == null)
					itemTooltip = raycastTarget.AddComponent<WatermarkItemTooltip>();
				itemTooltip.SetData(tmp, tooltip, tooltipAnchor, itemEffects ?? new Dictionary<string, WatermarkItemTooltipData>(StringComparer.Ordinal));
			}

			if (baseY.HasValue)
				watermark.transform.localPosition = new Vector3(
					watermark.transform.localPosition.x,
					baseY.Value - (activeCount * WatermarkSpacing),
					0f
				);

			watermark.transform.SetAsLastSibling();
			watermark.SetActive(true);
		}

		private static GameObject GetOrCreateWatermarkRaycastTarget(TextMeshProUGUI text)
		{
			Transform parent = text.transform;
			var existing = parent.Find(WatermarkRaycastName) as RectTransform;
			RectTransform raycastRect = existing;
			if (raycastRect == null)
			{
				var raycastObject = new GameObject(WatermarkRaycastName, typeof(RectTransform));
				raycastRect = raycastObject.GetComponent<RectTransform>();
				raycastRect.SetParent(parent, false);
			}

			var image = raycastRect.GetComponent<Image>() ?? raycastRect.gameObject.AddComponent<Image>();
			image.color = new Color(1f, 1f, 1f, 0f);
			image.raycastTarget = true;

			UpdateWatermarkRaycastRect(text, raycastRect);
			raycastRect.SetAsLastSibling();
			return raycastRect.gameObject;
		}

		private static void UpdateWatermarkRaycastRect(TextMeshProUGUI text, RectTransform raycastRect)
		{
			raycastRect.anchorMin = Vector2.zero;
			raycastRect.anchorMax = Vector2.one;
			raycastRect.pivot = new Vector2(0.5f, 0.5f);
			raycastRect.anchoredPosition = Vector2.zero;
			raycastRect.localScale = Vector3.one;
			raycastRect.sizeDelta = Vector2.zero;
		}

		private static RectTransform GetOrCreateWatermarkTooltipAnchor(TextMeshProUGUI text)
		{
			Transform parent = text.transform;
			var existing = parent.Find(WatermarkAnchorName) as RectTransform;
			RectTransform anchorRect = existing;
			if (anchorRect == null)
			{
				var anchorObject = new GameObject(WatermarkAnchorName, typeof(RectTransform));
				anchorRect = anchorObject.GetComponent<RectTransform>();
				anchorRect.SetParent(parent, false);
			}

			anchorRect.anchorMin = new Vector2(1f, 1f);
			anchorRect.anchorMax = new Vector2(1f, 1f);
			anchorRect.pivot = new Vector2(0.5f, 0.5f);
			anchorRect.anchoredPosition = Vector2.zero;
			anchorRect.localScale = Vector3.one;
			anchorRect.sizeDelta = new Vector2(1f, 1f);
			anchorRect.SetAsLastSibling();
			return anchorRect;
		}

		private sealed class WatermarkItemTooltip : MonoBehaviour, IPointerMoveHandler, IPointerExitHandler
		{
			private TextMeshProUGUI _text;
			private SimpleTooltipSource _tooltip;
			private RectTransform _anchorRect;
			private Dictionary<string, WatermarkItemTooltipData> _itemEffects = new Dictionary<string, WatermarkItemTooltipData>(StringComparer.Ordinal);
			private string _currentItemId;
			private bool _tooltipVisible;
			private bool _suppressPointerEvents;

			public void SetData(TextMeshProUGUI text, SimpleTooltipSource tooltip, RectTransform anchorRect, Dictionary<string, WatermarkItemTooltipData> itemEffects)
			{
				_text = text;
				_tooltip = tooltip;
				_anchorRect = anchorRect;
				_itemEffects = itemEffects ?? new Dictionary<string, WatermarkItemTooltipData>(StringComparer.Ordinal);
				ClearTooltip();
			}

			public void OnPointerMove(PointerEventData eventData)
			{
				if (_text == null || _tooltip == null)
					return;
				if (EventSystem.current == null || _text.textInfo == null || _text.textInfo.linkCount == 0)
				{
					ClearTooltip();
					return;
				}

				int linkIndex = TMP_TextUtilities.FindIntersectingLink(_text, eventData.position, eventData.enterEventCamera);
				if (linkIndex == -1)
				{
					ClearTooltip();
					return;
				}

				string itemId = _text.textInfo.linkInfo[linkIndex].GetLinkID();
				if (string.IsNullOrEmpty(itemId) || itemId == _currentItemId)
					return;

				if (!_itemEffects.TryGetValue(itemId, out var data) || string.IsNullOrWhiteSpace(data.Effect))
				{
					ClearTooltip();
					return;
				}

				_currentItemId = itemId;
				UpdateAnchorToLink(linkIndex);
				_tooltip.SetDirect(data.Title, data.Effect);
				TriggerTooltipEnter(eventData);
			}

			public void OnPointerExit(PointerEventData eventData)
			{
				if (_suppressPointerEvents)
					return;

				ClearTooltip();
			}

			private void ClearTooltip()
			{
				_currentItemId = null;
				if (_tooltip == null)
					return;

				_tooltip.SetDirect(string.Empty, string.Empty);
				TriggerTooltipExit();
			}

			private void TriggerTooltipEnter(PointerEventData eventData)
			{
				if (_tooltipVisible || EventSystem.current == null)
					return;

				_suppressPointerEvents = true;
				ExecuteEvents.Execute(
					_tooltip.gameObject,
					eventData,
					ExecuteEvents.pointerEnterHandler
				);
				_suppressPointerEvents = false;
				_tooltipVisible = true;
			}

			private void TriggerTooltipExit()
			{
				if (!_tooltipVisible || EventSystem.current == null)
					return;

				_suppressPointerEvents = true;
				ExecuteEvents.Execute(
					_tooltip.gameObject,
					new PointerEventData(EventSystem.current),
					ExecuteEvents.pointerExitHandler
				);
				_suppressPointerEvents = false;
				_tooltipVisible = false;
			}

			private void UpdateAnchorToLink(int linkIndex)
			{
				if (_anchorRect == null || _text == null || _text.textInfo == null)
					return;

				if (!TryGetLinkBounds(linkIndex, out var center, out var size))
					return;

				var paddedSize = size + new Vector2(6f, 4f);
				_anchorRect.anchorMin = new Vector2(1f, 1f);
				_anchorRect.anchorMax = new Vector2(1f, 1f);
				_anchorRect.pivot = new Vector2(0.5f, 0.5f);
				_anchorRect.anchoredPosition = center;
				_anchorRect.sizeDelta = paddedSize;
			}

			private bool TryGetLinkBounds(int linkIndex, out Vector2 center, out Vector2 size)
			{
				center = Vector2.zero;
				size = Vector2.zero;
				if (_text.textInfo.linkInfo == null || linkIndex < 0 || linkIndex >= _text.textInfo.linkCount)
					return false;

				TMP_LinkInfo linkInfo = _text.textInfo.linkInfo[linkIndex];
				int start = linkInfo.linkTextfirstCharacterIndex;
				int length = linkInfo.linkTextLength;
				var chars = _text.textInfo.characterInfo;
				if (start < 0 || start >= chars.Length || length <= 0)
					return false;

				float minX = float.PositiveInfinity;
				float minY = float.PositiveInfinity;
				float maxX = float.NegativeInfinity;
				float maxY = float.NegativeInfinity;
				bool hasVisible = false;
				int end = Math.Min(start + length, chars.Length);
				for (int i = start; i < end; i++)
				{
					var ch = chars[i];
					if (!ch.isVisible)
						continue;

					hasVisible = true;
					minX = Math.Min(minX, ch.bottomLeft.x);
					maxX = Math.Max(maxX, ch.topRight.x);
					minY = Math.Min(minY, ch.descender);
					maxY = Math.Max(maxY, ch.ascender);
				}

				if (!hasVisible)
					return false;

				size = new Vector2(maxX - minX, maxY - minY);
				center = new Vector2(minX + size.x * 0.5f, minY + size.y * 0.5f);
				return true;
			}
		}

		private static void ApplyWatermarkVisibility(TextMeshProUGUI text)
		{
			text.color = Color.white;
			var outline = text.GetComponent<Outline>();
			if (outline != null)
				UnityEngine.Object.Destroy(outline);

			text.outlineColor = Color.black;
			text.outlineWidth = 0.2f;
		}

		// Public helper to toggle watermark visibility for all SystemBoard instances.
		public static void SetWatermarkVisibleForAll(bool visible)
		{
			try
			{
				foreach (var board in UnityEngine.Object.FindObjectsByType<SystemBoard>(FindObjectsSortMode.None))
				{
					if (board == null)
						continue;
					if (!visible)
						SetWatermarkActive(board, false, null);
					else
						UpdateChallengerModeWatermark(board);
				}
			}
			catch (Exception)
			{
			}
		}

		private static string GetLocalizedText(string key)
		{
			var locale = LBoL.Core.Localization.CurrentLocale;
			if (LocalisationKeys.LocTable.TryGetValue((locale, key), out var text))
				return text;
			if (LocalisationKeys.LocTable.TryGetValue((Locale.En, key), out var fallback))
				return fallback;
			return key;
		}
	}

	[HarmonyPatch]
	public static class ShopPanel_WatermarkPatches
	{
		[HarmonyPatch(typeof(ShopPanel), nameof(ShopPanel.OnShowing))]
		[HarmonyPostfix]
		public static void OnShopPanelShowing(ShopPanel __instance)
		{
			// Hide watermark while any shop panel is showing
			SystemBoard_OnEnterGameRun_Patch.SetWatermarkVisibleForAll(false);
		}

		[HarmonyPatch(typeof(ShopPanel), nameof(ShopPanel.OnHiding))]
		[HarmonyPostfix]
		public static void OnShopPanelHiding(ShopPanel __instance)
		{
			// Ensure watermark restored when shop starts hiding
			SystemBoard_OnEnterGameRun_Patch.SetWatermarkVisibleForAll(true);
		}

		[HarmonyPatch(typeof(ShopPanel), nameof(ShopPanel.OnHided))]
		[HarmonyPostfix]
		public static void OnShopPanelHided(ShopPanel __instance)
		{
			// Restore watermark after shop fully hidden
			SystemBoard_OnEnterGameRun_Patch.SetWatermarkVisibleForAll(true);
		}
	}
	public static class ShopSaveLoader
	{
		private const string SaveFileName = "lvalonmimaShopSave.txt";
		private static bool _loadFailed;
		private static bool _pendingRestoreQuestHydration;
		public static bool IsGameRunRestoreInProgress { get; private set; }

		private static readonly byte[] EncryptionKey = new byte[]
		{
			0x4C, 0x76, 0x61, 0x6C, 0x6F, 0x6E, 0x6D, 0x69,
			0x6D, 0x61, 0x53, 0x68, 0x6F, 0x70, 0x4B, 0x65,
			0x79, 0x32, 0x30, 0x32, 0x34, 0x53, 0x65, 0x63,
			0x72, 0x65, 0x74, 0x44, 0x61, 0x74, 0x61, 0x21
		};

		private static readonly byte[] EncryptionIV = new byte[]
		{
			0x21, 0x61, 0x74, 0x61, 0x44, 0x74, 0x65, 0x72,
			0x63, 0x65, 0x53, 0x34, 0x32, 0x30, 0x32, 0x79
		};

		private static byte[] Encrypt(byte[] data)
		{
			using Aes aes = Aes.Create();
			aes.Key = EncryptionKey;
			aes.IV = EncryptionIV;
			aes.Mode = CipherMode.CBC;
			aes.Padding = PaddingMode.PKCS7;

			using ICryptoTransform encryptor = aes.CreateEncryptor();
			using MemoryStream ms = new MemoryStream();
			using CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
			cs.Write(data, 0, data.Length);
			cs.FlushFinalBlock();
			return ms.ToArray();
		}

		private static byte[] Decrypt(byte[] encryptedData)
		{
			using Aes aes = Aes.Create();
			aes.Key = EncryptionKey;
			aes.IV = EncryptionIV;
			aes.Mode = CipherMode.CBC;
			aes.Padding = PaddingMode.PKCS7;

			using ICryptoTransform decryptor = aes.CreateDecryptor();
			using MemoryStream ms = new MemoryStream(encryptedData);
			using CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
			using MemoryStream resultStream = new MemoryStream();
			cs.CopyTo(resultStream);
			return resultStream.ToArray();
		}

		private static string GetSaveFilePath()
		{
			return Path.Combine(GameMaster.PlatformHandler.GetSaveDataFolder(), SaveFileName);
		}

		public static bool ConsumePendingRestoreQuestHydration()
		{
			if (!_pendingRestoreQuestHydration)
			{
				return false;
			}

			_pendingRestoreQuestHydration = false;
			return true;
		}

		public static void SetGameRunRestoreInProgress(bool inProgress)
		{
			IsGameRunRestoreInProgress = inProgress;
		}

		public static bool GetGameRunRestoreInProgress()
		{
			return IsGameRunRestoreInProgress;
		}

		public static void Save()
		{
			var filePath = GetSaveFilePath();
			if (_loadFailed && File.Exists(filePath))
			{
				return;
			}

			var customData = MiniTracker.Instance.CustomGrSaveData;
			var csd = customData;
			var shop = customData?.GetShopForCurrentProfile();
			Dictionary<string, int> originalQuestProgress = null;
			Dictionary<string, string> originalQuestRequirements = null;
			HashSet<string> originalCompletedQuestCards = null;
			Dictionary<string, int> originalQuestModifiers = null;
			try
			{
				if (shop != null)
				{
					originalQuestProgress = shop.QuestProgress != null
						? new Dictionary<string, int>(shop.QuestProgress, StringComparer.Ordinal)
						: new Dictionary<string, int>(StringComparer.Ordinal);

					originalQuestRequirements = shop.QuestRequirements != null
						? new Dictionary<string, string>(shop.QuestRequirements, StringComparer.Ordinal)
						: new Dictionary<string, string>(StringComparer.Ordinal);

					originalCompletedQuestCards = shop.QuestCompletedCards != null
						? new HashSet<string>(shop.QuestCompletedCards.Where(id => !string.IsNullOrEmpty(id)), StringComparer.Ordinal)
						: new HashSet<string>(StringComparer.Ordinal);

					originalQuestModifiers = shop.QuestModifiers != null
						? new Dictionary<string, int>(shop.QuestModifiers, StringComparer.Ordinal)
						: new Dictionary<string, int>(StringComparer.Ordinal);

					Dictionary<string, int> persistedProgress = originalQuestProgress
						.Where(kvp => !string.IsNullOrEmpty(kvp.Key))
						.ToDictionary(kvp => kvp.Key, kvp => kvp.Value, StringComparer.Ordinal);

					Dictionary<string, string> persistedRequirements = originalQuestRequirements
						.Where(kvp => !string.IsNullOrEmpty(kvp.Key)
							&& !string.IsNullOrEmpty(kvp.Value)
							&& persistedProgress.ContainsKey(kvp.Key))
						.ToDictionary(kvp => kvp.Key, kvp => kvp.Value, StringComparer.Ordinal);

					HashSet<string> persistedCompleted = new HashSet<string>(
						originalCompletedQuestCards.Where(id => !string.IsNullOrEmpty(id)),
						StringComparer.Ordinal);

					Dictionary<string, int> persistedModifiers = originalQuestModifiers
						.Where(kvp => !string.IsNullOrEmpty(kvp.Key))
						.ToDictionary(kvp => kvp.Key, kvp => kvp.Value, StringComparer.Ordinal);

					shop.QuestProgress = persistedProgress;
					shop.QuestRequirements = persistedRequirements;
					shop.QuestCompletedCards = persistedCompleted;
					shop.QuestModifiers = persistedModifiers;

				}

				// csd.Save(); // Note: csd.Save() adds BluePoints from current run, usually not desired if just saving shop state from menu.
				// However, if we are just persisting the current state (which includes purchased items), we just serialize csd.

				using StringWriter stringWriter = new StringWriter { NewLine = "\n" };
				var seBuilder = new SerializerBuilder().DisableAliases();
				csd.TypeConverters().Do((tc) => { seBuilder = seBuilder.WithTypeConverter(tc); });

				seBuilder.Build().Serialize(stringWriter, csd);
				var data = SaveDataHelper.EncodeYaml(stringWriter.ToString(), false);
				var encryptedData = Encrypt(data);

				WriteSaveData(SaveFileName, encryptedData);
			}
			catch (Exception)
			{
			}
			finally
			{
				if (shop != null)
				{
					shop.QuestProgress = originalQuestProgress ?? new Dictionary<string, int>(StringComparer.Ordinal);
					shop.QuestRequirements = originalQuestRequirements ?? new Dictionary<string, string>(StringComparer.Ordinal);
					shop.QuestCompletedCards = originalCompletedQuestCards ?? new HashSet<string>(StringComparer.Ordinal);
					shop.QuestModifiers = originalQuestModifiers ?? new Dictionary<string, int>(StringComparer.Ordinal);
				}
			}
		}

		public static void Load(string location)
		{
			_pendingRestoreQuestHydration = string.Equals(location, "GameRunController.Restore", StringComparison.Ordinal);
			var customData = MiniTracker.Instance.CustomGrSaveData ?? MiniTracker.LoadedFromDiskCustomGrSaveData;
			if (customData == null)
			{
				_pendingRestoreQuestHydration = false;
				return;
			}
			if (MiniTracker.Instance.CustomGrSaveData == null)
			{
				MiniTracker.Instance.SetActive(customData);
			}
			var csd = customData;

			var filePath = GetSaveFilePath();

			if (!File.Exists(filePath))
			{
				_pendingRestoreQuestHydration = false;
				return;
			}

			try
			{
				var deBuilder = new DeserializerBuilder().IgnoreUnmatchedProperties();
				csd.TypeConverters().Do((tc) => { deBuilder = deBuilder.WithTypeConverter(tc); });

				var encryptedData = File.ReadAllBytes(filePath);
				var decryptedData = Decrypt(encryptedData);
				var decodedYaml = SaveDataHelper.DecodeYaml(decryptedData);
				object csdObject = deBuilder.Build().Deserialize(decodedYaml, csd.GetType());

				var loadedData = (LiteProfileSaveData)csdObject;
				if (loadedData?.Saves != null)
				{
					foreach (var key in loadedData.Saves.Keys.ToList())
					{
						loadedData.Saves[key] = LiteShop.ReconcileWithDefaults(loadedData.Saves[key]);
					}
				}

				loadedData.Restore();
				_loadFailed = false;
			}
			catch (Exception)
			{
				_loadFailed = true;
				_pendingRestoreQuestHydration = false;
			}
		}
	}

	[HarmonyPatch(typeof(MainMenuPanel), nameof(MainMenuPanel.Awake))]
	class MainMenuPanel_Awake_Patch
	{
		static void Postfix()
		{
			ShopSaveLoader.Load("MainMenuPanel.Awake");
			LiteShopButton.RefreshMainMenuButtonLabel();
		}
	}

	[HarmonyPatch(typeof(GameRunController), nameof(GameRunController.Save))]
	[HarmonyPriority(Priority.VeryLow)]
	class GameRunController_Save_Patch
	{

		static void Postfix()
		{
			// 2do maybe. Optimize memory usage by not storing container values statically.
			var customData = MiniTracker.Instance.CustomGrSaveData;

			customData.Save(0, false);

			ShopSaveLoader.Save();
		}
	}


	[HarmonyPatch(typeof(GameRunController), nameof(GameRunController.Restore))]
	[HarmonyPriority(Priority.VeryHigh)]
	class GameRunController_Restore_Patch
	{
		static void Prefix()
		{
			ShopSaveLoader.SetGameRunRestoreInProgress(true);
		}

		static void Postfix()
		{
			try
			{
				ShopSaveLoader.Load("GameRunController.Restore");
				exquesting.ProcessDeferredRestoreHydration();
			}
			finally
			{
				ShopSaveLoader.SetGameRunRestoreInProgress(false);
			}
		}
	}

	[HarmonyPatch(typeof(GameMaster))]
	[HarmonyPatch(nameof(CoAbandonGameRun))]
	public static class CoAbandonGameRun_Postfix
	{
		static void Postfix(GameStatisticData data)
		{
			var customData = MiniTracker.Instance.CustomGrSaveData;
			try
			{
				// Save logic from LiteProfileSaveData (add BluePoints)
				customData.Save(0, false);

				// Serialize to disk
				ShopSaveLoader.Save();
			}
			catch (Exception)
			{
			}
		}
	}

	[HarmonyPatch(typeof(GameMaster), nameof(EndGameStatistics)), HarmonyPriority(Priority.VeryLow)]
	public static class GameMaster_EndGameStatistics_Patch
	{
		private const string BluePointPrefix = "BluePoint.";

		static void Postfix(ref GameStatisticData __result, GameRunController gameRun, GameResultType resultType)
		{
			SystemBoard_OnEnterGameRun_Patch.DisableWatermarkAll();
			if (__result == null)
				return;
			var shop = MiniTracker.Instance?.CustomGrSaveData?.GetShopForCurrentProfile();
			if (shop == null || !shop.ChallengerModeEnabled)
				return;

			var ids = new HashSet<string>(StringComparer.Ordinal);
			foreach (var keyTuple in LocalisationKeys.LocTable.Keys)
			{
				var key = keyTuple.Item2;
				if (!key.StartsWith(BluePointPrefix, StringComparison.Ordinal))
					continue;

				var remainder = key[BluePointPrefix.Length..];
				var root = remainder.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries)[0];
				var fullId = BluePointPrefix + root;
				ids.Add(fullId);
			}

			if (ids.Count == 0)
				return;

			__result.ScoreDatas ??= new List<ScoreData>();

			float diffMult = gameRun.Difficulty switch
			{
				GameDifficulty.Easy => 0.75f,
				GameDifficulty.Normal => 1f,
				GameDifficulty.Hard => 1.25f,
				GameDifficulty.Lunatic => 1.5f,
				_ => 0f,
			};

			float toAdd = 0;

			GameRunSaveData gameRunSaveData = Singleton<GameMaster>.Instance.GameRunSaveData;

			foreach (var fullId in ids)
			{
				if (__result.ScoreDatas.Exists(sd => sd.Id == fullId))
					continue;

				var idRoot = fullId[BluePointPrefix.Length..];
				int delta = idRoot switch
				{
					// known specific adjustments
					"hurryact1level4" => -10,
					"hurryact1" => -1,
					"hurryact2" => 100,
					"hurryact3" => 500,
					"hurryact3win" => 1000,
					"hurryact4" => 1500,
					"hurryact4win" => 2500,
					_ => 0
				};

				// stage check
				int indexSTAGE = -1;
				int levelSTATION = -1;
				if (shop?.BPProgress != null)
				{
					shop.BPProgress.TryGetValue("stage", out indexSTAGE);
					shop.BPProgress.TryGetValue("level", out levelSTATION);
				}

				bool skipReward = false;
				switch (idRoot)
				{
					case "hurryact1level4":
						if (indexSTAGE > 0 || levelSTATION > 4)
							skipReward = true;
						break;
					case "hurryact1":
						if (indexSTAGE > 0 || levelSTATION <= 4)
							skipReward = true;
						break;
					case "hurryact2":
						if (indexSTAGE != 1)
							skipReward = true;
						break;
					case "hurryact3":
						if (indexSTAGE != 2 || resultType != GameResultType.Failure)
							skipReward = true;
						break;
					case "hurryact3win":
						if (indexSTAGE != 2 || resultType != GameResultType.NormalEnd)
							skipReward = true;
						break;
					case "hurryact4":
						if (indexSTAGE != 3 || (resultType != GameResultType.TrueEndFail && resultType != GameResultType.Failure))
							skipReward = true;
						break;
					case "hurryact4win":
						if (indexSTAGE != 3 || resultType != GameResultType.TrueEnd)
							skipReward = true;
						break;
					default:
						break;
				}

				if (skipReward)
					continue;


				EnsureLocalizationKey(fullId + ".Name");
				EnsureLocalizationKey(fullId + ".Description");

				// panel showing, mult is handled elsewhere
				__result.ScoreDatas.Add(new ScoreData
				{
					Id = fullId,
					TotalBluePoint = delta
				});
				toAdd += delta;
			}
			__result.BluePoint += (int)(toAdd * diffMult);

			shop.BPProgress = new Dictionary<string, int>();
			MiniTracker.Instance.CustomGrSaveData.Save(__result.BluePoint);
			ShopSaveLoader.Save();  //save progress on the spot
		}

		private static void EnsureLocalizationKey(string key)
		{
			var locale = LBoL.Core.Localization.CurrentLocale;
			if (!TryAddLocalizationKey(locale, key))
				TryAddLocalizationKey(Locale.En, key);
		}

		private static bool TryAddLocalizationKey(Locale locale, string key)
		{
			if (!LocalisationKeys.LocTable.TryGetValue((locale, key), out var value))
				return false;

			var table = LBoL.Core.Localization.LocalizationTable;
			if (!table.ContainsKey(key))
				table.Add(key, value);

			return true;
		}
	}

	[HarmonyPatch(typeof(GameResultPanel), nameof(GameResultPanel.CustomLocalizationAsync))]
	public static class GameResultPanel_CustomLocalizationAsync_Patch
	{
		private const string BluePointPrefix = "BluePoint.";

		static void Postfix(GameResultPanel __instance, ref UniTask __result)
		{
			__result = __result.ContinueWith(() => AddCustomScoreEntries(__instance));
		}

		private static void AddCustomScoreEntries(GameResultPanel panel)
		{
			if (panel?._stringTable == null)
				return;

			var ids = new HashSet<string>(StringComparer.Ordinal);
			foreach (var keyTuple in LocalisationKeys.LocTable.Keys)
			{
				var key = keyTuple.Item2;
				if (!key.StartsWith(BluePointPrefix, StringComparison.Ordinal))
					continue;

				var remainder = key[BluePointPrefix.Length..];
				var root = remainder.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries)[0];
				var fullId = BluePointPrefix + root;
				ids.Add(fullId);
			}

			foreach (var fullId in ids)
			{
				panel._stringTable[fullId] = new GameResultPanel.StringTableEntry
				{
					Name = GetLoc(fullId + ".Name"),
					Description = GetLoc(fullId + ".Description")
				};
			}

			// reset quest run-state snapshots
			var shop = MiniTracker.Instance?.CustomGrSaveData?.GetShopForCurrentProfile();
			if (shop != null)
			{
				shop.QuestProgress = new Dictionary<string, int>(StringComparer.Ordinal);
				shop.QuestRequirements = new Dictionary<string, string>(StringComparer.Ordinal);
				shop.QuestCompletedCards = new HashSet<string>(StringComparer.Ordinal);
				shop.QuestModifiers = new Dictionary<string, int>(StringComparer.Ordinal);
			}

			MiniTracker.Instance.CustomGrSaveData.Save(0, false);
			ShopSaveLoader.Save();
		}

		private static string GetLoc(string key)
		{
			var locale = LBoL.Core.Localization.CurrentLocale;
			if (LocalisationKeys.LocTable.TryGetValue((locale, key), out var value))
				return value;
			if (LocalisationKeys.LocTable.TryGetValue((Locale.En, key), out var fallback))
				return fallback;
			return "<" + key + ">";
		}
	}

	[HarmonyPatch(typeof(GameMaster), nameof(GameMaster.AppendGameRunHistory))]
	public static class GameMaster_AppendGameRunHistory_Patch
	{
		static void Postfix(GameRunRecordSaveData record)
		{
			try
			{
				if (record == null || string.IsNullOrEmpty(record.SaveTimestamp))
					return;

				var shop = MiniTracker.Instance?.CustomGrSaveData?.GetShopForCurrentProfile();
				if (shop == null || !shop.ChallengerModeEnabled)
					return;


				shop.RunModifiersByTimestamp ??= new Dictionary<string, List<(string, int)>>();
				var modifiers = shop.Items?.Values
						.Where(item => item != null && item.CurrentTier > 0)
						.Select(item => (item.Id, item.CurrentTier))
						.ToList() ?? new List<(string, int)>();

				// Always record that Challenger Mode was active for this run when the shop indicates so.
				// If no modifiers were purchased, store an empty list so the history UI can still
				// indicate "Challenger Mode Active" and show the "None" tooltip.
				modifiers.Sort((left, right) => string.CompareOrdinal(left.Id, right.Id));
				shop.RunModifiersByTimestamp[record.SaveTimestamp] = modifiers;

				shop.RunCompletedQuestsByTimestamp ??= new Dictionary<string, List<string>>(StringComparer.Ordinal);
				List<string> completedQuests = shop.CurrentRunCompletedQuests != null
					? shop.CurrentRunCompletedQuests.Where(id => !string.IsNullOrEmpty(id)).ToList()
					: new List<string>();
				shop.RunCompletedQuestsByTimestamp[record.SaveTimestamp] = completedQuests;

				ShopModHandlers.ResetCurrentRunRewardedQuestCompletions(shop);
				MiniTracker.Instance?.CustomGrSaveData?.Save(0, false);
				ShopSaveLoader.Save();
			}
			catch (Exception)
			{
			}
		}
	}

	[HarmonyPatch(typeof(HistoryPanel), nameof(HistoryPanel.Awake))]
	public static class HistoryPanel_Awake_Patch
	{
		static void Postfix(HistoryPanel __instance)
		{
			try
			{
				HistoryPanelChallengerHistory.EnsureChallengerHistoryUi(__instance);
			}
			catch (Exception)
			{
			}
		}
	}

	[HarmonyPatch(typeof(HistoryPanel), nameof(HistoryPanel.SetRecord))]
	public static class HistoryPanel_SetRecord_Patch
	{
		static void Postfix(HistoryPanel __instance, GameRunRecordSaveData record)
		{
			try
			{
				HistoryPanelChallengerHistory.UpdateChallengerHistoryUi(__instance, record);
			}
			catch (Exception)
			{
			}
		}
	}

	internal static class HistoryPanelChallengerHistory
	{
		private sealed class ChallengerHistoryUi
		{
			public TextMeshProUGUI Label;
			public SimpleTooltipSource Tooltip;
			public TextMeshProUGUI QuestLabel;
			public SimpleTooltipSource QuestTooltip;
		}

		private const string ChallengerHistoryLabelName = "challengerModeHistoryLabel";
		private const string ChallengerQuestHistoryLabelName = "challengerModeQuestHistoryLabel";
		private static readonly Dictionary<HistoryPanel, ChallengerHistoryUi> ChallengerHistoryUiMap = new Dictionary<HistoryPanel, ChallengerHistoryUi>();

		public static void EnsureChallengerHistoryUi(HistoryPanel panel)
		{
			GetOrCreateUi(panel);
		}

		private static ChallengerHistoryUi GetOrCreateUi(HistoryPanel panel)
		{
			if (panel == null || panel.packImage == null)
				return null;
			if (ChallengerHistoryUiMap.TryGetValue(panel, out var ui) && ui?.Label != null && ui.QuestLabel != null)
				return ui;

			Transform parent = panel.packImage.transform.parent;
			if (parent == null)
				return null;

			TextMeshProUGUI label = null;
			var existing = parent.Find(ChallengerHistoryLabelName) as RectTransform;
			if (existing != null)
				label = existing.GetComponent<TextMeshProUGUI>();

			if (label == null)
			{
				var go = new GameObject(ChallengerHistoryLabelName, typeof(RectTransform), typeof(TextMeshProUGUI));
				go.transform.SetParent(parent, false);
				label = go.GetComponent<TextMeshProUGUI>();

				var packRect = panel.packImage.rectTransform;
				var labelRect = label.rectTransform;
				labelRect.anchorMin = packRect.anchorMin;
				labelRect.anchorMax = packRect.anchorMax;
				labelRect.pivot = packRect.pivot;
				labelRect.anchoredPosition = packRect.anchoredPosition + new Vector2(0f, packRect.sizeDelta.y + 12f);
				labelRect.sizeDelta = new Vector2(Mathf.Max(200f, packRect.sizeDelta.x * 2f), packRect.sizeDelta.y);

				if (panel.seedText != null)
				{
					label.font = panel.seedText.font;
					label.fontSize = panel.seedText.fontSize;
					label.color = panel.seedText.color;
				}

				label.alignment = TextAlignmentOptions.Center;
				label.textWrappingMode = TextWrappingModes.NoWrap;
				label.raycastTarget = true;
				label.text = string.Empty;
				label.gameObject.SetActive(false);
				label.transform.SetAsLastSibling();

			}

			TextMeshProUGUI questLabel = null;
			var questExisting = parent.Find(ChallengerQuestHistoryLabelName) as RectTransform;
			if (questExisting != null)
				questLabel = questExisting.GetComponent<TextMeshProUGUI>();

			if (questLabel == null)
			{
				var go = new GameObject(ChallengerQuestHistoryLabelName, typeof(RectTransform), typeof(TextMeshProUGUI));
				go.transform.SetParent(parent, false);
				questLabel = go.GetComponent<TextMeshProUGUI>();

				var packRect = panel.packImage.rectTransform;
				var labelRect = questLabel.rectTransform;
				labelRect.anchorMin = packRect.anchorMin;
				labelRect.anchorMax = packRect.anchorMax;
				labelRect.pivot = packRect.pivot;
				labelRect.anchoredPosition = packRect.anchoredPosition + new Vector2(0f, packRect.sizeDelta.y + 32f);
				labelRect.sizeDelta = new Vector2(Mathf.Max(200f, packRect.sizeDelta.x * 2f), packRect.sizeDelta.y);

				if (panel.seedText != null)
				{
					questLabel.font = panel.seedText.font;
					questLabel.fontSize = panel.seedText.fontSize;
					questLabel.color = panel.seedText.color;
				}

				questLabel.alignment = TextAlignmentOptions.Center;
				questLabel.textWrappingMode = TextWrappingModes.NoWrap;
				questLabel.raycastTarget = true;
				questLabel.text = string.Empty;
				questLabel.gameObject.SetActive(false);
				questLabel.transform.SetAsLastSibling();
			}

			var tooltip = label.GetComponent<SimpleTooltipSource>() ?? SimpleTooltipSource.CreateDirect(label.gameObject, string.Empty, string.Empty)
				.WithPosition(TooltipDirection.Top, TooltipAlignment.Center);
			var questTooltip = questLabel.GetComponent<SimpleTooltipSource>() ?? SimpleTooltipSource.CreateDirect(questLabel.gameObject, string.Empty, string.Empty)
				.WithPosition(TooltipDirection.Top, TooltipAlignment.Center);

			ui = new ChallengerHistoryUi
			{
				Label = label,
				Tooltip = tooltip,
				QuestLabel = questLabel,
				QuestTooltip = questTooltip
			};
			ChallengerHistoryUiMap[panel] = ui;

			return ui;
		}

		public static void UpdateChallengerHistoryUi(HistoryPanel panel, GameRunRecordSaveData record)
		{
			var ui = GetOrCreateUi(panel);
			if (ui?.Label == null || ui.QuestLabel == null)
				return;


			ui.Label.gameObject.SetActive(false);
			ui.Tooltip?.SetDirect(string.Empty, string.Empty);
			ui.QuestLabel.gameObject.SetActive(false);
			ui.QuestTooltip?.SetDirect(string.Empty, string.Empty);

			if (record == null || string.IsNullOrEmpty(record.SaveTimestamp))
				return;


			var shop = MiniTracker.Instance?.CustomGrSaveData?.GetShopForCurrentProfile();
			if (shop == null)
				return;


			List<(string, int)> modifiers = null;
			bool hasModifierData = shop.RunModifiersByTimestamp != null
				&& shop.RunModifiersByTimestamp.TryGetValue(record.SaveTimestamp, out modifiers);

			if (hasModifierData)
			{
				string labelText = GetShopLocalizedText($"{LocalisationKeys.ShopPrefix}ChallengerModeHistory.Active");
				labelText = StringDecorator.Decorate($"<b>{labelText}</b>");
				ui.Label.text = labelText;

				ui.Label.ForceMeshUpdate();
				var labelRect = ui.Label.rectTransform;
				float padW = 8f;
				float padH = 4f;
				float prefW = ui.Label.preferredWidth;
				float prefH = ui.Label.preferredHeight;
				labelRect.sizeDelta = new Vector2(Mathf.Max(32f, prefW + padW), Mathf.Max(16f, prefH + padH));
				if (panel?.packImage != null)
				{
					var packRect = panel.packImage.rectTransform;
					labelRect.anchoredPosition = packRect.anchoredPosition + new Vector2(0f, packRect.sizeDelta.y * 0.5f + labelRect.sizeDelta.y * 0.5f + 6f);
				}
				labelRect.SetAsLastSibling();
				ui.Label.gameObject.SetActive(true);

				string title = GetShopLocalizedText($"{LocalisationKeys.ShopPrefix}ChallengerModeHistory.Title");
				string body;
				if (modifiers == null || modifiers.Count == 0)
				{
					body = StringDecorator.Decorate("|r" + GetShopLocalizedText($"{LocalisationKeys.ShopPrefix}Loadout.None") + "|");
				}
				else
				{
					body = BuildChallengerHistoryTooltip(shop, modifiers);
				}
				ui.Tooltip?.SetDirect(title, body);
			}

			List<string> completedQuests = null;
			if (shop.RunCompletedQuestsByTimestamp != null
				&& shop.RunCompletedQuestsByTimestamp.TryGetValue(record.SaveTimestamp, out var history)
				&& history != null)
			{
				completedQuests = history.Where(id => !string.IsNullOrEmpty(id)).ToList();
			}

			if (completedQuests == null || completedQuests.Count == 0)
				return;


			string questLabelText = GetShopLocalizedText($"{LocalisationKeys.ShopPrefix}QuestHistory.Active");
			questLabelText = StringDecorator.Decorate($"<b>{questLabelText}</b>");
			ui.QuestLabel.text = questLabelText;
			ui.QuestLabel.ForceMeshUpdate();

			var questLabelRect = ui.QuestLabel.rectTransform;
			float questPadW = 8f;
			float questPadH = 4f;
			float questPrefW = ui.QuestLabel.preferredWidth;
			float questPrefH = ui.QuestLabel.preferredHeight;
			questLabelRect.sizeDelta = new Vector2(Mathf.Max(32f, questPrefW + questPadW), Mathf.Max(16f, questPrefH + questPadH));

			if (panel?.packImage != null)
			{
				var packRect = panel.packImage.rectTransform;
				float baseY = packRect.anchoredPosition.y + packRect.sizeDelta.y * 0.5f + questLabelRect.sizeDelta.y * 0.5f + 6f;
				if (ui.Label.gameObject.activeSelf)
				{
					var labelRect = ui.Label.rectTransform;
					baseY = labelRect.anchoredPosition.y + labelRect.sizeDelta.y * 0.5f + questLabelRect.sizeDelta.y * 0.5f + 6f;
				}
				questLabelRect.anchoredPosition = new Vector2(packRect.anchoredPosition.x, baseY);
			}

			questLabelRect.SetAsLastSibling();
			ui.QuestLabel.gameObject.SetActive(true);

			string questTitle = GetShopLocalizedText($"{LocalisationKeys.ShopPrefix}QuestHistory.Title");
			string questBody = BuildQuestHistoryTooltip(completedQuests);
			ui.QuestTooltip?.SetDirect(questTitle, questBody);
		}

		private static string BuildQuestHistoryTooltip(List<string> completedQuests)
		{
			if (completedQuests == null || completedQuests.Count == 0)
				return string.Empty;

			var lines = new List<string>(completedQuests.Count);
			foreach (string questCardId in completedQuests)
			{
				if (string.IsNullOrEmpty(questCardId))
					continue;

				lines.Add(GetQuestName(questCardId));
			}

			return string.Join("\n", lines);
		}

		private static string GetQuestName(string questCardId)
		{
			if (string.IsNullOrEmpty(questCardId))
				return string.Empty;

			Card card = Library.TryCreateCard(questCardId, false);
			if (card == null)
				return questCardId;

			string localizedName = TypeFactory<Card>.LocalizeProperty(card.Id, "Name", true, true);
			if (string.IsNullOrEmpty(localizedName))
				localizedName = card.Id;

			var rarity = card.Config?.Rarity ?? Rarity.Common;
			return rarity switch
			{
				Rarity.Rare => StringDecorator.Decorate("|" + localizedName + "|"),
				Rarity.Uncommon => StringDecorator.Decorate("|s:" + localizedName + "|"),
				_ => localizedName,
			};
		}

		private static string BuildChallengerHistoryTooltip(LiteShop shop, List<(string, int)> modifiers)
		{
			var modifierMap = new Dictionary<string, int>(StringComparer.Ordinal);
			foreach (var entry in modifiers)
			{
				if (!string.IsNullOrEmpty(entry.Item1))
					modifierMap[entry.Item1] = entry.Item2;
			}

			var sb = new StringBuilder();
			var categoryOrder = new[]
			{
				LocalisationKeys.DifficultyPrefix,
				LocalisationKeys.InitPrefix,
				LocalisationKeys.DiscountPrefix,
				LocalisationKeys.FeaturePrefix,
				LocalisationKeys.BattlePrefix,
				LocalisationKeys.AlterPrefix,
			};

			foreach (var prefix in categoryOrder)
			{
				var items = shop.Items?.Values
					.Where(item => item != null && item.Id.StartsWith(prefix, StringComparison.Ordinal))
					.Select(item => item.Id)
					.Where(modifierMap.ContainsKey)
					.ToList();
				if (items == null || items.Count == 0)
					continue;

				string categoryKey = $"{LocalisationKeys.ShopPrefix}{prefix[..^1]}";
				string categoryName = GetShopLocalizedText(categoryKey);
				sb.Append(categoryName).Append('\n');

				foreach (var itemId in items)
				{
					int tier = modifierMap[itemId];
					int maxTier = shop.Items != null && shop.Items.TryGetValue(itemId, out var item) ? item.MaxTier : 0;
					string nameKey = $"{LocalisationKeys.ShopPrefix}{itemId}";
					string name = GetShopLocalizedText(nameKey);
					if (name == nameKey)
						name = itemId;
					string coloredName = LocalisationKeys.ColorizeTierName(name, tier, maxTier);
					string line = maxTier > 1 ? $"  {coloredName} {tier}" : $"  {coloredName}";
					sb.Append(line).Append('\n');
				}

				sb.Append('\n');
			}

			return sb.ToString().TrimEnd();
		}

		private static string GetShopLocalizedText(string key)
		{
			var locale = LBoL.Core.Localization.CurrentLocale;
			if (LocalisationKeys.LocTable.TryGetValue((locale, key), out var text))
				return text;
			if (LocalisationKeys.LocTable.TryGetValue((Locale.En, key), out var fallback))
				return fallback;
			return key;
		}
	}

	// custom save file deletion isn't really necessary
	// [HarmonyPatch(typeof(GameMaster), nameof(GameMaster.TryDeleteSaveData))]
	// class GameMaster_Patch
	// {
	// 	static void Prefix(string filename)
	// 	{
	// 		var index = GameMaster.Instance.CurrentSaveIndex;
	// 		if (index == null)
	// 			return;
	// 		if (GameMaster.GetGameRunFileName(index.Value) != filename)
	// 			return;

	// 		var customData = MiniTracker.Instance.CustomGrSaveData;
	// 		var csd = customData;
	// 		var fileName = "lvalonmimaShopSave.txt";
	// 		var filePath = Path.Combine(GameMaster.PlatformHandler.GetSaveDataFolder(), fileName);
	// 		if (!File.Exists(filePath))
	// 			return;


	// 		try
	// 		{
	// 			csd.OnGamerunEnded();

	// 			if (csd.DeleteFileOnGamerunEnd)
	// 				File.Delete(filePath);
	// 		}
	// 		catch (Exception)
	// 		{
	// 		}
	// 	}
	// }
}

