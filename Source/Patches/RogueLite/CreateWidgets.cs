using System;
using System.Linq;
using HarmonyLib;
using LBoL.Presentation.I10N;
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.Dialogs;
using LBoL.Presentation.UI.ExtraWidgets;
using LBoL.Presentation.UI.Panels;
using LBoL.Presentation.UI.Widgets;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace lvalonmima.Source.Patches
{
	public static class CreateWidgets
	{
		private static GameObject buttonTemplate;
		[HarmonyPatch(typeof(MainMenuPanel), nameof(MainMenuPanel.Awake)), HarmonyPrefix]
		private static void LoadButtonTemplate(SettingPanel __instance)
		{
			if (!TryGetOptionTab(__instance, "Preference", out var tab2))
			{
				return;
			}
			Transform obj = tab2.transform.Find("RightPanel");
			buttonTemplate = obj.Find("ResetHint").gameObject;
		}

		private static bool TryGetChild(this Transform t, string childName, out Transform child)
		{
			child = t.Find(childName);
			if (null == (Object)(object)child)
			{
				BepinexPlugin.log.LogWarning("Could not find " + childName + " Transform, the config UI will not be initialised");
				return false;
			}
			return true;
		}

		private static bool TryGetOptionTab(this SettingPanel panel, string name, out CanvasGroup tab)
		{
			tab = panel.tabs.FirstOrDefault(t => t.name == name);
			if (null == (Object)(object)tab)
			{
				BepinexPlugin.log.LogWarning("Could not find " + name + " tab, the config UI will not be initialised");
				return false;
			}
			return true;
		}

		public static GameObject CreateButton(Transform panelParent, string internalName, string tooltipKey, UnityAction callback, bool interactable)
		{
			var button = Object.Instantiate(buttonTemplate, panelParent);
			button.name = internalName;
			var child = button.transform.Find("ResetHint");
			child.name = internalName;

			var widget = child.GetComponent<CommonButtonWidget>();
			// nuke those fucking scripted listeners
			widget.button.onClick = new Button.ButtonClickedEvent();
			widget.button.onClick.AddListener(callback);
			widget.button.interactable = interactable;

			var locTitleKey = internalName;
			var locDescKey = tooltipKey;

			SimpleTooltipSource.CreateWithGeneralKey(button, locTitleKey, locDescKey).WithPosition(TooltipDirection.Bottom, TooltipAlignment.Min);

			var optDesc = (child.Find("Text (TMP)") ?? child.Find("Layout/Text (TMP)")).GetComponent<LocalizedText>();
			optDesc.key = locTitleKey;

			return button;
		}

		private static GameObject CreateToggle(Transform parent, bool on, UnityAction<bool> callback)
		{
			PuzzleToggleWidget toggle = Object.Instantiate(UiManager.GetPanel<StartGamePanel>().puzzleToggleTemplate);
			toggle.SetEnd();
			toggle.Toggle.onValueChanged = new Toggle.ToggleEvent();
			toggle.Toggle.isOn = on;
			toggle.Toggle.onValueChanged.AddListener(callback);
			toggle.transform.SetParent(parent);

			return toggle.gameObject;
		}
		public static void ConfirmationPopup(string key, Action confirmAction)
		{
			UiManager
				.GetDialog<MessageDialog>()
				.Show(
					new MessageContent()
					{
						TextKey = key,
						Icon = MessageIcon.Warning,
						Buttons = DialogButtons.ConfirmCancel,
						OnConfirm = confirmAction,
					}
				);
		}
	}
}
