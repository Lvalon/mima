using System.Collections.Generic;
using System.Linq;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Sakuya;
using LBoLEntitySideloader.Attributes;

namespace lvalonmima.StatusEffects
{
	public sealed class sepotehhhwuDef : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Positive;
			config.HasCount = true;
			config.RelativeEffects = new List<string>() { nameof(TimeAuraSe) };
			return config;
		}
	}

	[EntityLogic(typeof(sepotehhhwuDef))]
	public sealed class sepotehhhwu : StatusEffect
	{
		public int Value1 => Owner == null ? 1 : Level;
		public int Value2 => Owner == null ? 2 : Level * 2;
		public ManaGroup Mana => new ManaGroup() { Hybrid = 3, HybridColor = 0 };
		protected override void OnAdded(Unit unit)
		{
			ReactOwnerEvent(Battle.ManaConsumed, OnManaConsumed);
		}

		private IEnumerable<BattleAction> OnManaConsumed(ManaEventArgs args)
		{
			if (Battle.AllAliveEnemies.Count() > 0)
			{
				Count += args.Value.Blue;
				Count += args.Value.White;
				Count += args.Value.Philosophy;
				(int result, int remainder) tuple = Count.DivRem(Mana.Total);
				int item = tuple.result;
				int item2 = tuple.remainder;
				Count = item2;
				if (item != 0)
				{
					NotifyActivating();
					IEnumerable<Card> cards = Battle.HandZone.Where(c => !c.IsUpgraded && c.CanUpgradeAndPositive).SampleManyOrAll(item * Value1, GameRun.BattleRng);
					if (cards.Count() > 0)
					{
						yield return new UpgradeCardsAction(cards);
					}
					if (Battle.BattleShouldEnd) { yield break; }
					yield return BuffAction<TimeAuraSe>(item * Value2, 0, 0);
				}
			}
		}
	}
}