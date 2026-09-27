using System;
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.Dialogs;

namespace lvalonmima.Source.Patches;

public static class CreateWidgets
{
	// LoadButtonTemplate/CreateButton/CreateToggle were removed: LoadButtonTemplate
	// patched MainMenuPanel.Awake but took a mismatched `SettingPanel __instance`
	// parameter, and this class carried no class-level [HarmonyPatch] attribute, so
	// Harmony's PatchAll (which requires one — confirmed via decompile of
	// PatchClassProcessor/allowUnannotatedType) skipped the whole class. The patch
	// never ran, buttonTemplate stayed null forever, and CreateButton/CreateToggle
	// (which depend on it) were unreachable dead code.
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
