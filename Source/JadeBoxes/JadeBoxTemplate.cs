using LBoL.ConfigData;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using lvalonmima.Config;
using lvalonmima.Localization;

namespace lvalonmima.JadeBoxes
{
	public class lvalonmimajadeboxtemplate : JadeBoxTemplate
	{
		public override IdContainer GetId()
		{
			return lvalonmimaDefaultConfig.DefaultID(this);
		}

		public override LocalizationOption LoadLocalization()
		{
			return lvalonmimaLocalization.JadeBoxBatchLoc.AddEntity(this);
		}

		public override JadeBoxConfig MakeConfig()
		{
			return GetDefaultJadeBoxConfig();
		}

		public JadeBoxConfig GetDefaultJadeBoxConfig()
		{
			return lvalonmimaDefaultConfig.DefaultJadeBoxConfig();
		}

	}
}