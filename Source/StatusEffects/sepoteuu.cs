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
using LBoL.EntityLib.StatusEffects.Cirno;
using LBoLEntitySideloader.Attributes;

namespace lvalonmima.StatusEffects
{
	public sealed class sepoteuuDef : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Positive;
			config.RelativeEffects = new List<string>() { nameof(Cold) };
			config.HasCount = true;
			return config;
		}
	}

	[EntityLogic(typeof(sepoteuuDef))]
	public sealed class sepoteuu : StatusEffect
	{
		public int Value1 => Owner == null ? 1 : Level;
		public ManaGroup Mana => new ManaGroup() { Blue = 2 };
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
				(int result, int remainder) tuple = Count.DivRem(Mana.Total);
				int item = tuple.result;
				int item2 = tuple.remainder;
				Count = item2;
				if (item != 0)
				{
					NotifyActivating();
					for (int i = 0; i < item * Value1; i++)
					{
						if (Battle.BattleShouldEnd) { yield break; }
						yield return new ApplyStatusEffectAction<Cold>(Battle.RandomAliveEnemy, 1, 0, 0, 0);
					}
				}
			}
		}
	}
}