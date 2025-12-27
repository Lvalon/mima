using System.Collections.Generic;
using LBoL.ConfigData;
using lvalonmima.Cards;
using lvalonmima.Packs;

namespace lvalonmima.Source.Packs
{
	public sealed class packtrumpDef : lvalonmimapacktemplate
	{
		public new PackConfig MakeConfig()
		{
			PackConfig config = GetDefaultPackConfig();
			config.Id = GetId();
			config.CardList = new List<string>() { nameof(cardmimaexa), nameof(cardmimaexb) };
			return config;
		}
	}
}

