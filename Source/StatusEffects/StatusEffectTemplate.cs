using LBoL.ConfigData;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using UnityEngine;
using lvalonmima.ImageLoader;
using lvalonmima.Localization;
using lvalonmima.Config;

namespace lvalonmima.StatusEffects
{
	public class lvalonmimaStatusEffectTemplate : StatusEffectTemplate
	{
		public override IdContainer GetId()
		{
			return lvalonmimaDefaultConfig.DefaultID(this);
		}

		public override LocalizationOption LoadLocalization()
		{
			return lvalonmimaLocalization.StatusEffectsBatchLoc.AddEntity(this);
		}

		public override Sprite LoadSprite()
		{
			return lvalonmimaImageLoader.LoadStatusEffectLoader(status: this);
		}

		public override StatusEffectConfig MakeConfig()
		{
			return GetDefaultStatusEffectConfig();
		}

		public static StatusEffectConfig GetDefaultStatusEffectConfig()
		{
			return lvalonmimaDefaultConfig.DefaultStatusEffectConfig();
		}
	}
}