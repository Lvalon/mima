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

public class ShopLabelUpdater : MonoBehaviour
{
	private float _timer;
	private const float Timeout = 3f;

	private void Update()
	{
		if (GameMaster.Instance?.CurrentProfile != null)
		{
			LiteShopButton.RefreshMainMenuButtonLabel();
			Destroy(this);
			return;
		}

		_timer += Time.deltaTime;
		if (_timer > Timeout)
			Destroy(this);
	}
}
