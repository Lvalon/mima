using LBoL.ConfigData;
using lvalonmima.SFX.Template;

namespace lvalonmima.SFX
{
	public sealed class mimametalpipeDef : lvalonmimaSFXTemplate
	{

		public override SfxConfig MakeConfig()
		{
			var config = GetCardDefaultConfig();

			config.Name = UniqueId;
			config.Folder = "";
			config.Path = "mimametalpipe.ogg";

			return config;
		}
	}

}
