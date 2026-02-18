using LBoLEntitySideloader.Attributes;
using LBoL.Core.StatusEffects;
using UnityEngine;
using LBoL.ConfigData;
using System.Collections.Generic;

namespace lvalonmima.StatusEffects
{
	public sealed class sesideloadDef : lvalonmimaStatusEffectTemplate
	{
		public override Sprite LoadSprite() => null;
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.RelativeEffects = new List<string>() { nameof(Charging) };
			return config;
		}
	}

	[EntityLogic(typeof(sesideloadDef))]
	public sealed class sesideload : StatusEffect
	{
	}
	public sealed class seusedDef : lvalonmimaStatusEffectTemplate
	{
		public override Sprite LoadSprite() => null;
	}
	[EntityLogic(typeof(seusedDef))]
	public sealed class seused : StatusEffect
	{
	}
	public sealed class selinkedDef : lvalonmimaStatusEffectTemplate
	{
		public override Sprite LoadSprite() => null;
	}

	[EntityLogic(typeof(selinkedDef))]
	public sealed class selinked : StatusEffect
	{
	}
	public sealed class seunderDef : lvalonmimaStatusEffectTemplate
	{
		public override Sprite LoadSprite() => null;
	}

	[EntityLogic(typeof(seunderDef))]
	public sealed class seunder : StatusEffect
	{
	}
	public sealed class sequestDef : lvalonmimaStatusEffectTemplate
	{
		public override Sprite LoadSprite() => null;
	}

	[EntityLogic(typeof(sequestDef))]
	public sealed class sequest : StatusEffect
	{
	}
}

