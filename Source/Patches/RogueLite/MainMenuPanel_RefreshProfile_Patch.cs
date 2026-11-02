using HarmonyLib;
using LBoL.Presentation.UI.Panels;

namespace lvalonmima.Source.Patches;

[HarmonyPatch(typeof(MainMenuPanel), nameof(MainMenuPanel.RefreshProfile))]
public static class MainMenuPanel_RefreshProfile_Patch
{
	public static void Postfix()
	{
		LiteShopButton.RefreshMainMenuButtonLabel();
	}
}
