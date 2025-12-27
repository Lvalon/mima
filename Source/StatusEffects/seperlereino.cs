using System.Collections.Generic;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core.StatusEffects;
using LBoLEntitySideloader.Attributes;

namespace lvalonmima.StatusEffects
{
	public sealed class seperlereinoDef : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Positive;
			config.RelativeEffects = new List<string>() { nameof(setranscendence) };
			return config;
		}
	}

	[EntityLogic(typeof(seperlereinoDef))]
	public sealed class seperlereino : StatusEffect
	{
	}
}