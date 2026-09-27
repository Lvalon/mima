using LBoL.ConfigData;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using lvalonmima.ImageLoader;
using lvalonmima.Localization;
using lvalonmima.Config;

namespace lvalonmima.Packs;

// Checklist G11: fix applied, but flagged as risky without live-game
// verification. PackTemplate.MakeConfig() is non-virtual and implicitly
// implements IConfigProvider<PackConfig>; Sideloader's registration calls
// through that interface, which always binds to PackTemplate's own
// MakeConfig() (confirmed via decompile of LBoL-Entity-Sideloader.dll's
// IConfigProvider<C>/PackTemplate), no matter how many `new`-hiding
// overrides sit beneath it -- so packtrumpDef's real CardList never
// registered; the trump pack got vanilla's empty default instead.
// Re-declaring IConfigProvider<PackConfig> here and making MakeConfig
// `new virtual` re-anchors the interface dispatch at this class, so a
// subclass's `override` (see packtrumpDef in Pack.cs) is what Sideloader
// actually calls. If this breaks pack registration in-game, revert: drop
// `IConfigProvider<PackConfig>` here and change MakeConfig back to
// `public new PackConfig MakeConfig()` (non-virtual), and change
// packtrumpDef's `override` back to `new`.
public class lvalonmimapacktemplate : PackTemplate, IConfigProvider<PackConfig>
{
	public override IdContainer GetId()
	{
		return lvalonmimaDefaultConfig.DefaultID(this);
	}

	public override LocalizationOption LoadLocalization()
	{
		return lvalonmimaLocalization.PacksBatchLoc.AddEntity(this);
	}

	public override PackIcons LoadPackIcon()
	{
		return lvalonmimaImageLoader.LoadPackIconLoader(this);
	}

	public new virtual PackConfig MakeConfig()
	{
		return GetDefaultPackConfig();
	}

	public static PackConfig GetDefaultPackConfig()
	{
		return lvalonmimaDefaultConfig.DefaultPackConfig();
	}
}
