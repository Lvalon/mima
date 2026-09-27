using System;
using System.Collections.Generic;
using LBoL.Core;
using LBoL.Presentation;
using LBoL.Presentation.InputSystemExtend;
using LBoL.Presentation.UI.ExtraWidgets;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace lvalonmima.Source.Patches;

// Tooltip content for a single watermark link (a category header or a purchased item).
internal readonly struct WatermarkItemTooltipData
{
	public readonly string Title;
	public readonly string Effect;

	public WatermarkItemTooltipData(string title, string effect)
	{
		Title = title ?? string.Empty;
		Effect = effect ?? string.Empty;
	}
}

// Drives the Challenger Mode watermark's hover tooltip and its battle transparency.
// This never participates in raycasting: the watermark text and its CanvasGroup keep
// raycastTarget/blocksRaycasts off, so units, status icons and intentions behind it stay
// fully clickable/hoverable. Instead we poll the pointer every frame and only show our
// own tooltip when nothing else claims the pointer and no selection UI is active.
internal sealed class WatermarkHoverController : MonoBehaviour
{
	private const float NormalAlpha = 1f;
	private const float BattleAlpha = 0.45f;
	private const float AlphaLerpSpeed = 4f;

	private TextMeshProUGUI _text;
	private RectTransform _anchorRect;
	private SimpleTooltipSource _tooltip;
	private Dictionary<string, WatermarkItemTooltipData> _itemEffects = new(StringComparer.Ordinal);
	private CanvasGroup _canvasGroup;
	private string _currentLinkId;
	private bool _tooltipVisible;
	private Transform _followParent;
	private Vector3 _followLocalPoint;

	private static readonly List<RaycastResult> RaycastResultsScratch = new();

	public void SetData(
		TextMeshProUGUI text,
		RectTransform anchorRect,
		SimpleTooltipSource tooltip,
		Dictionary<string, WatermarkItemTooltipData> itemEffects)
	{
		_text = text;
		_anchorRect = anchorRect;
		_tooltip = tooltip;
		_itemEffects = itemEffects ?? new Dictionary<string, WatermarkItemTooltipData>(StringComparer.Ordinal);
		_canvasGroup = text?.GetComponent<CanvasGroup>();
		ClearTooltip();
	}

	// The watermark is hosted on the root canvas (to draw above every panel) but positioned
	// relative to SystemBoard's version/hint lines, so track that point in world space.
	public void SetFollowTarget(Transform followParent, Vector3 followLocalPoint)
	{
		_followParent = followParent;
		_followLocalPoint = followLocalPoint;
		SyncToFollowTarget();
	}

	private void LateUpdate()
	{
		SyncToFollowTarget();
	}

	private void SyncToFollowTarget()
	{
		if (_followParent == null)
			return;

		transform.position = _followParent.TransformPoint(_followLocalPoint);

		// Match the scale the text would have had as a direct child of the follow parent.
		Transform host = transform.parent;
		if (host == null)
			return;
		Vector3 want = _followParent.lossyScale;
		Vector3 have = host.lossyScale;
		if (Mathf.Approximately(have.x, 0f) || Mathf.Approximately(have.y, 0f) || Mathf.Approximately(have.z, 0f))
			return;
		transform.localScale = new Vector3(want.x / have.x, want.y / have.y, want.z / have.z);
	}

	private void OnDisable()
	{
		ClearTooltip();
	}

	private void Update()
	{
		UpdateAlpha();

		if (_text == null || _tooltip == null || Mouse.current == null)
		{
			ClearTooltip();
			return;
		}

		if (Singleton<InputDeviceManager>.Instance.CurrentInputDevice != InputDeviceType.MouseAndKeyboard)
		{
			ClearTooltip();
			return;
		}

		if (!CanShowTooltip())
		{
			ClearTooltip();
			return;
		}

		Vector2 pointerPosition = Mouse.current.position.ReadValue();
		Camera uiCamera = CameraController.UiCamera;

		if (!RectTransformUtility.RectangleContainsScreenPoint(_text.rectTransform, pointerPosition, uiCamera))
		{
			ClearTooltip();
			return;
		}

		if (_text.textInfo == null || _text.textInfo.linkCount == 0)
		{
			ClearTooltip();
			return;
		}

		int linkIndex = TMP_TextUtilities.FindIntersectingLink(_text, pointerPosition, uiCamera);
		if (linkIndex == -1)
		{
			ClearTooltip();
			return;
		}

		// A selectable element (unit, status icon, intention, card, map node, button, …)
		// behind that spot always wins over the watermark's own tooltip.
		if (IsPointerOverSelectableUi(pointerPosition))
		{
			ClearTooltip();
			return;
		}

		string linkId = _text.textInfo.linkInfo[linkIndex].GetLinkID();
		if (string.IsNullOrEmpty(linkId))
		{
			ClearTooltip();
			return;
		}

		if (linkId == _currentLinkId)
			return;

		if (!_itemEffects.TryGetValue(linkId, out var data) || string.IsNullOrWhiteSpace(data.Effect))
		{
			ClearTooltip();
			return;
		}

		_currentLinkId = linkId;
		UpdateAnchorToLink(linkIndex);
		_tooltip.SetDirect(data.Title, data.Effect);
		ShowTooltip();
	}

	private void UpdateAlpha()
	{
		if (_canvasGroup == null)
			return;

		bool inBattle = GameMaster.Instance?.CurrentGameRun?.Battle != null;
		float targetAlpha = (inBattle && !_tooltipVisible) ? BattleAlpha : NormalAlpha;
		_canvasGroup.alpha = Mathf.MoveTowards(_canvasGroup.alpha, targetAlpha, AlphaLerpSpeed * Time.unscaledDeltaTime);
	}

	// The watermark's tooltip is active for the whole game run (map and battle alike);
	// it doesn't matter which panel or dialog happens to be open, because
	// IsPointerOverSelectableUi already yields to anything selectable at that screen point.
	private static bool CanShowTooltip()
	{
		try
		{
			return GameMaster.Instance?.CurrentGameRun != null;
		}
		catch (Exception e)
		{
			BepinexPlugin.log?.LogDebug(e);
			return false;
		}
	}

	// True when the element the pointer would actually interact with at that spot (the
	// topmost raycast hit other than the watermark itself) is selectable: it reacts to
	// hover, click or selection - a button, map node, card, unit, status icon, intention,
	// etc. Plain raycast blockers such as the map's or a panel's background don't count,
	// since the watermark draws above them and there's nothing to select there.
	private bool IsPointerOverSelectableUi(Vector2 pointerPosition)
	{
		if (EventSystem.current == null)
			return false;

		var pointerData = new PointerEventData(EventSystem.current) { position = pointerPosition };
		RaycastResultsScratch.Clear();
		EventSystem.current.RaycastAll(pointerData, RaycastResultsScratch);

		foreach (var result in RaycastResultsScratch)
		{
			GameObject hit = result.gameObject;
			if (hit == null || hit.transform.IsChildOf(transform))
				continue;

			// RaycastAll results are sorted front-to-back; only the topmost hit matters.
			return ExecuteEvents.GetEventHandler<IPointerEnterHandler>(hit) != null
				|| ExecuteEvents.GetEventHandler<IPointerClickHandler>(hit) != null
				|| ExecuteEvents.GetEventHandler<ISelectHandler>(hit) != null;
		}

		return false;
	}

	private void ShowTooltip()
	{
		if (_tooltipVisible)
			return;

		_tooltip.Hide();
		_tooltip.Show();
		_tooltipVisible = true;
	}

	private void ClearTooltip()
	{
		_currentLinkId = null;
		if (_tooltip == null || !_tooltipVisible)
			return;

		_tooltip.Hide();
		_tooltipVisible = false;
	}

	private void UpdateAnchorToLink(int linkIndex)
	{
		if (_anchorRect == null || _text == null || _text.textInfo == null)
			return;

		if (!TryGetLinkBounds(linkIndex, out var center, out var size))
			return;

		var paddedSize = size + new Vector2(6f, 4f);
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
