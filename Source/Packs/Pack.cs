using LBoL.ConfigData;
using lvalonmima.Cards;
using lvalonmima.Packs;

namespace lvalonmima.Source.Packs;

public sealed class packtrumpDef : lvalonmimapacktemplate
{
	public override PackConfig MakeConfig()
	{
		PackConfig config = GetDefaultPackConfig();
		config.Id = GetId();
		config.CardList = [nameof(cardmimaexa), nameof(cardmimaexb)];
		return config;
	}
}
