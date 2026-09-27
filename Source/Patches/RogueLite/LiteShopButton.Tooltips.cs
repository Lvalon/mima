using System;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LBoL.Presentation.UI.Panels;
using LBoL.Presentation.I10N;
using LBoL.Core;
using System.Collections.Generic;
using LBoL.Presentation.UI;
using LBoL.Base.Extensions;
using System.Text;
using Object = UnityEngine.Object;
using LBoL.Presentation.InputSystemExtend;
using LBoL.Presentation;
using LBoL.Presentation.UI.ExtraWidgets;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using YamlDotNet.RepresentationModel;
using lvalonmima.Localization;

namespace lvalonmima.Source.Patches;

public partial class LiteShopButton
{

	private readonly struct LoadoutTooltipData
	{
		public readonly string Title;
		public readonly string Effect;

		public LoadoutTooltipData(string title, string effect)
		{
			Title = title ?? string.Empty;
			Effect = effect ?? string.Empty;
		}
	}

	private static void AttachLoadoutTooltip(GameObject textContainer, Dictionary<string, LoadoutTooltipData> tooltipMap)
	{
		try
		{
			if (textContainer == null || tooltipMap == null || tooltipMap.Count == 0)
				return;

			var textComp = textContainer.GetComponent<TextMeshProUGUI>();
			if (textComp == null)
				return;

			// Raycast target
			var raycastObj = new GameObject("LoadoutRaycast", typeof(RectTransform));
			raycastObj.transform.SetParent(textContainer.transform, false);
			var raycastRect = raycastObj.GetComponent<RectTransform>();
			raycastRect.anchorMin = Vector2.zero;
			raycastRect.anchorMax = Vector2.one;
			raycastRect.pivot = new Vector2(0.5f, 0.5f);
			raycastRect.anchoredPosition = Vector2.zero;
			raycastRect.localScale = Vector3.one;
			raycastRect.sizeDelta = Vector2.zero;
			var image = raycastObj.AddComponent<Image>();
			image.color = new Color(1f, 1f, 1f, 0f);
			image.raycastTarget = true;

			// Tooltip anchor: attach under the root canvas so positioning matches watermark behavior
			var anchorObj = new GameObject("LoadoutTooltipAnchor", typeof(RectTransform));
			Canvas canvas = textComp.canvas ?? textContainer.GetComponentInParent<Canvas>();
			Transform anchorParent = canvas != null ? canvas.transform : textContainer.transform;
			anchorObj.transform.SetParent(anchorParent, false);
			var anchorRect = anchorObj.GetComponent<RectTransform>();
			anchorRect.anchorMin = new Vector2(1f, 1f);
			anchorRect.anchorMax = new Vector2(1f, 1f);
			anchorRect.pivot = new Vector2(0.5f, 0.5f);
			anchorRect.anchoredPosition = Vector2.zero;
			anchorRect.localScale = Vector3.one;
			anchorRect.sizeDelta = new Vector2(1f, 1f);

			var tooltip = SimpleTooltipSource.CreateDirect(anchorObj, string.Empty).WithPosition(TooltipDirection.Bottom, TooltipAlignment.Min);

			var handler = raycastObj.AddComponent<LoadoutItemTooltip>();
			handler.SetData(textComp, tooltip, anchorRect, tooltipMap);
		}
		catch (Exception e)
		{
			BepinexPlugin.log?.LogDebug(e);
		}
	}

	private sealed class LoadoutItemTooltip : MonoBehaviour, IPointerMoveHandler, IPointerExitHandler
	{
		private TextMeshProUGUI _text;
		private SimpleTooltipSource _tooltip;
		private RectTransform _anchorRect;
		private Dictionary<string, LoadoutTooltipData> _map = new(StringComparer.Ordinal);
		private string _currentId;
		private bool _tooltipVisible;
		private bool _suppressPointerEvents;

		public void SetData(TextMeshProUGUI text, SimpleTooltipSource tooltip, RectTransform anchorRect, Dictionary<string, LoadoutTooltipData> map)
		{
			_text = text;
			_tooltip = tooltip;
			_anchorRect = anchorRect;
			_map = map ?? new Dictionary<string, LoadoutTooltipData>(StringComparer.Ordinal);
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
			if (string.IsNullOrEmpty(itemId) || itemId == _currentId)
				return;

			if (!_map.TryGetValue(itemId, out var data) || string.IsNullOrWhiteSpace(data.Effect))
			{
				ClearTooltip();
				return;
			}

			_currentId = itemId;
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
			_currentId = null;
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
			ExecuteEvents.Execute<IPointerEnterHandler>(
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
			ExecuteEvents.Execute<IPointerExitHandler>(
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
			// Use world-space placement so the tooltip anchor sits directly under the hovered link
			_anchorRect.anchorMin = new Vector2(0.5f, 0.5f);
			_anchorRect.anchorMax = new Vector2(0.5f, 0.5f);
			// pivot set to left so tooltip aligns to the start of link
			_anchorRect.pivot = new Vector2(0f, 0.5f);
			_anchorRect.sizeDelta = paddedSize;
			// Transform the link-local center into world space and set the anchor's world position
			var leftX = center.x - (size.x * 0.5f);
			var worldLeft = _text.transform.TransformPoint(new Vector3(leftX, center.y, 0f));
			var canvas = _text.canvas;
			if (canvas != null)
			{
				var canvasRect = canvas.GetComponent<RectTransform>();
				_anchorRect.anchoredPosition = canvasRect.InverseTransformPoint(worldLeft);
			}
			else
			{
				_anchorRect.position = worldLeft;
			}
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

		private void OnDisable()
		{
			// Ensure the tooltip is hidden if this handler is disabled (e.g., panel closed with Escape)
			try
			{
				ClearTooltip();
			}
			catch (Exception)
			{
				// Swallow to avoid noise during teardown
			}
		}
	}
}
