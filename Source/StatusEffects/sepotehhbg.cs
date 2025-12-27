using System.Collections.Generic;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Core;
using System.Linq;
using LBoL.Core.Battle;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Others;
using LBoLEntitySideloader.Attributes;

namespace lvalonmima.StatusEffects
{
	public sealed class sepotehhbgDef : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Positive;
			config.HasCount = true;
			config.RelativeEffects = new List<string>() { nameof(Poison) };
			return config;
		}
	}

	[EntityLogic(typeof(sepotehhbgDef))]
	public sealed class sepotehhbg : StatusEffect
	{
		public int Value1 => Owner == null ? 1 : Level;
		public ManaGroup Mana => new ManaGroup() { Hybrid = 2, HybridColor = 8 };
		protected override void OnAdded(Unit unit)
		{
			ReactOwnerEvent(Battle.ManaConsumed, OnManaConsumed);
		}

		private IEnumerable<BattleAction> OnManaConsumed(ManaEventArgs args)
		{
			if (Battle.AllAliveEnemies.Count() > 0)
			{
				Count += args.Value.Green;
				Count += args.Value.Black;
				Count += args.Value.Philosophy;
				(int result, int remainder) tuple = Count.DivRem(Mana.Total);
				int item = tuple.result;
				int item2 = tuple.remainder;
				Count = item2;
				if (item != 0)
				{
					NotifyActivating();
					yield return BuffAction<Poison>(item * Value1, 0, 0);
				}
			}
		}
	}
}