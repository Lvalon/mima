using LBoL.ConfigData;
using lvalonmima.SFX.Template;

namespace lvalonmima.SFX
{
	public sealed class mimadeathDef : lvalonmimaSFXTemplate
	{

		public override SfxConfig MakeConfig()
		{
			var config = GetCardDefaultConfig();

			config.Name = UniqueId;
			config.Folder = "";
			config.Path = "mimadeath.ogg";

			return config;
		}
	}

}
