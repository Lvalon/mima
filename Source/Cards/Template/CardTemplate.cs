using LBoL.Base;
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

		public CardConfig GetCardDefaultConfig(bool quest = false)
		{
			CardConfig config = lvalonmimaDefaultConfig.CardDefaultConfig();
			if (quest)
			{
				config.Owner = null;
				config.IsPooled = false;
				config.Cost = new ManaGroup() { Any = 0 };
				config.Type = CardType.Ability;
				config.TargetType = TargetType.Nobody;
				config.IsUpgradable = false;
			}
			return config;
		}
	}


}


