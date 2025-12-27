using LBoL.ConfigData;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using lvalonmima.ImageLoader;
using lvalonmima.Localization;
using lvalonmima.Config;

namespace lvalonmima.Packs
{
	public class lvalonmimapacktemplate : PackTemplate
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

		public new PackConfig MakeConfig()
		{
			return GetDefaultPackConfig();
		}

		public static PackConfig GetDefaultPackConfig()
		{
			return lvalonmimaDefaultConfig.DefaultPackConfig();
		}
	}
}