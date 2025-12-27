using System.Collections.Generic;
using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;

namespace lvalonmima.StatusEffects
{
	public sealed class sewraitsothDef : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Positive;
			config.Keywords = Keyword.Purified;
			config.RelativeEffects = new List<string>() { nameof(semburst) };
			return config;
		}
	}

	[EntityLogic(typeof(sewraitsothDef))]
	public sealed class sewraitsoth : sehl25
	{
	}
}