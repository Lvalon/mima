using System.Collections.Generic;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using System.Linq;
using LBoL.Core.Battle;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoLEntitySideloader.Attributes;

namespace lvalonmima.StatusEffects
{
	public sealed class sepoterDef : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Positive;
			config.RelativeEffects = new List<string>() { nameof(semburst) };
			return config;
		}
	}

	[EntityLogic(typeof(sepoterDef))]
	public sealed class sepoter : StatusEffect
	{
		public int Value1 => Owner == null ? 1 : Level;
		public ManaGroup Mana => new ManaGroup() { Red = 1 };
		protected override void OnAdded(Unit unit)
		{
			ReactOwnerEvent(Battle.ManaConsumed, OnManaConsumed);
		}

		private IEnumerable<BattleAction> OnManaConsumed(ManaEventArgs args)
		{
			if (Battle.AllAliveEnemies.Count() > 0)
			{
				int goon = args.Value.Red + args.Value.Philosophy;
				if (goon != 0)
				{
					NotifyActivating();
					yield return BuffAction<semburst>(goon * Value1, 0, 0);
				}
			}
		}
	}
}