using LBoL.ConfigData;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using lvalonmima.Config;
using lvalonmima.ImageLoader;
using lvalonmima.Localization;


namespace lvalonmima.Cards.Template
{
	public abstract class lvalonmimaCardTemplate : CardTemplate
	{
		public override IdContainer GetId()
		{
			return lvalonmimaDefaultConfig.DefaultID(this);
		}

		public override CardImages LoadCardImages()
		{
			return lvalonmimaImageLoader.LoadCardImages(this);
		}

		public override LocalizationOption LoadLocalization()
		{
			return lvalonmimaLocalization.CardsBatchLoc.AddEntity(this);
		}

		public CardConfig GetCardDefaultConfig()
		{
			return lvalonmimaDefaultConfig.CardDefaultConfig();
		}
	}


}


