using System.Collections.Generic;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Core;
using System.Linq;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoLEntitySideloader.Attributes;

namespace lvalonmima.StatusEffects
{
	public sealed class secardsideloadDef : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Positive;
			config.HasCount = true;
			return config;
		}
	}

	[EntityLogic(typeof(secardsideloadDef))]
	public sealed class secardsideload : StatusEffect
	{
		public int Value1 => 6;
		public int Value2 => Owner == null ? 1 : Level;
		public ManaGroup Mana => new ManaGroup() { Blue = 1 };
		protected override void OnAdded(Unit unit)
		{
			ReactOwnerEvent(Battle.ManaConsumed, OnManaConsumed);
		}

		private IEnumerable<BattleAction> OnManaConsumed(ManaEventArgs args)
		{
			if (Battle.AllAliveEnemies.Count() > 0)
			{
				Count += args.Value.Blue;
				Count += args.Value.Philosophy;
				(int result, int remainder) tuple = Count.DivRem(Value1);
				int item = tuple.result;
				int item2 = tuple.remainder;
				Count = item2;
				if (item != 0)
				{
					NotifyActivating();
					yield return new DrawManyCardAction(item * Value2);
				}
			}
		}
	}
}