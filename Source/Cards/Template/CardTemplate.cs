using LBoL.Base;
using LBoLEntitySideloader.ExtraFunc;
using LBoL.ConfigData;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using lvalonmima.Config;
using lvalonmima.ImageLoader;
using lvalonmima.Localization;


namespace lvalonmima.Cards.Template;

public abstract class lvalonmimaCardTemplate : CardTemplate
{
	// Quest cards sit in a fixed, quest-ordered range at the end of this mod's index block:
	// the block base (Sideloader's index rounded down to its 10M block) + 9.5M + quest.
	protected static int QuestIndex(CardConfig config, int quest)
	{
		int block = LBoLEntitySideloader.ExtraFunc.CardIndexGenerator.GetUniqueIndex(config) / LBoLEntitySideloader.ExtraFunc.CardIndexGenerator.milx1 * LBoLEntitySideloader.ExtraFunc.CardIndexGenerator.milx1;
		return block + LBoLEntitySideloader.ExtraFunc.CardIndexGenerator.milx1 - 500000 + quest;
	}

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

	// Replaces the `config.Index = CardIndexGenerator.GetUniqueIndex(config); return config;`
	// tail repeated in every card Def.MakeConfig. Does not auto-copy Upgraded* fields
	// from their base counterparts — several cards intentionally differ there.
	public static CardConfig Finish(CardConfig config)
	{
		config.Index = CardIndexGenerator.GetUniqueIndex(config);
		return config;
	}
}
