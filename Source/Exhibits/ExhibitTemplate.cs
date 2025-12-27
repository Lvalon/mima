using LBoL.ConfigData;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using lvalonmima.Config;
using lvalonmima.ImageLoader;
using lvalonmima.Localization;

namespace lvalonmima.Exhibits
{
	public class lvalonmimaExhibitTemplate : ExhibitTemplate
	{
		public override IdContainer GetId()
		{
			return lvalonmimaDefaultConfig.DefaultID(this);
		}

		public override LocalizationOption LoadLocalization()
		{
			return lvalonmimaLocalization.ExhibitsBatchLoc.AddEntity(this);
		}

		public override ExhibitSprites LoadSprite()
		{
			return lvalonmimaImageLoader.LoadExhibitSprite(exhibit: this);
		}

		public override ExhibitConfig MakeConfig()
		{
			return GetDefaultExhibitConfig();
		}

		public ExhibitConfig GetDefaultExhibitConfig()
		{
			return lvalonmimaDefaultConfig.DefaultExhibitConfig();
		}

	}
}