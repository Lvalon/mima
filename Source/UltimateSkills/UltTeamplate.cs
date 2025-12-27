using LBoL.ConfigData;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using UnityEngine;
using lvalonmima.ImageLoader;
using lvalonmima.Localization;
using lvalonmima.Config;

namespace lvalonmima.lvalonmimaUlt
{
	public class lvalonmimaUltTemplate : UltimateSkillTemplate
	{
		public override IdContainer GetId()
		{
			return lvalonmimaDefaultConfig.DefaultID(this);
		}

		public override LocalizationOption LoadLocalization()
		{
			return lvalonmimaLocalization.UltimateSkillsBatchLoc.AddEntity(this);
		}

		public override Sprite LoadSprite()
		{
			return lvalonmimaImageLoader.LoadUltLoader(ult: this);
		}

		public override UltimateSkillConfig MakeConfig()
		{
			throw new System.NotImplementedException();
		}

		public UltimateSkillConfig GetDefaulUltConfig()
		{
			return lvalonmimaDefaultConfig.DefaultUltConfig();
		}
	}
}