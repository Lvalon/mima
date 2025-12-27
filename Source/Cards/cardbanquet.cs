using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core;
using System.Linq;
using lvalonmima.StatusEffects;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Others;
using LBoL.EntityLib.StatusEffects.Koishi;

namespace lvalonmima.Cards
{
	public sealed class cardbanquetDef : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig();
			config.Colors = new List<ManaColor>() { ManaColor.Red, ManaColor.Green };
			config.Cost = new ManaGroup() { Any = 3, Red = 1, Green = 1 };
			config.Rarity = Rarity.Uncommon;
			config.Type = CardType.Attack;
			config.TargetType = TargetType.SingleEnemy;

			config.Damage = 0;

			config.Keywords = Keyword.FollowCard | Keyword.Exile;
			config.UpgradedKeywords = Keyword.FollowCard | Keyword.Exile;

			config.RelativeKeyword = Keyword.Expel;
			config.UpgradedRelativeKeyword = Keyword.Expel;

			config.RelativeEffects = new List<string>() { nameof(MoodPeace), nameof(GuangxueMicai), nameof(Poison), nameof(Burst) };
			config.UpgradedRelativeEffects = new List<string>() { nameof(MoodPeace), nameof(GuangxueMicai), nameof(Poison), nameof(Burst) };

			config.Value1 = 1;
			config.Value2 = 10;

			config.Illustrator = "ささ吉";

			config.Index = CardIndexGenerator.GetUniqueIndex(config);
			return config;
		}
	}

	[EntityLogic(typeof(cardbanquetDef))]
	public sealed class cardbanquet : lvalonmimaCard
	{
		bool localplaying = false;
		bool expelling = false;
		public override bool playing
		{
			get
			{
				return localplaying || expelling;
			}
		}
		protected override IEnumerable<BattleAction> OnExpel(DieEventArgs args)
		{
			expelling = true;
			try
			{
				if (Battle.BattleShouldEnd) { yield break; }
				NotifyActivating();
				if (Battle.Player.TryGetStatusEffect(out GuangxueMicai se))
				{
					yield return new RemoveStatusEffectAction(se);
				}
				if (Battle.BattleShouldEnd) { yield break; }
				Mood mood = (Mood)Battle.Player.StatusEffects.FirstOrDefault(se => se is MoodPeace);
				if (mood != null)
				{
					yield return new MoodChangeAction(Battle.Player, mood, null);
				}
				if (Battle.BattleShouldEnd) { yield break; }
				yield return BuffAction<Burst>(1);
			}
			finally
			{
				expelling = false;
			}
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			localplaying = true;
			try
			{
				yield return BuffAction<MoodPeace>();
				if (Battle.BattleShouldEnd) { yield break; }
				yield return BuffAction<GuangxueMicai>(0, Value1, 0, 0);
				if (Battle.BattleShouldEnd) { yield break; }
				yield return new ApplyStatusEffectAction<senopeace>(Battle.Player, 0, 0, 0, 0);
				if (Battle.BattleShouldEnd) { yield break; }
				if (!IsUpgraded)
				{
					if (selector.SelectedEnemy.IsAlive)
					{
						yield return DebuffAction<Poison>(selector.SelectedEnemy, Value2, 0, 0, 0);
					}
				}
				else
				{
					foreach (Unit unit in Battle.AllAliveEnemies)
					{
						if (!unit.IsAlive || Battle.BattleShouldEnd) { yield break; }
						yield return DebuffAction<Poison>(unit, Value2, 0, 0, 0);
					}
				}
				foreach (EnemyUnit item2 in Battle.AllAliveEnemies.Where(enemy => enemy.HasStatusEffect<Poison>()).ToList())
				{
					if (Battle.BattleShouldEnd || !item2.IsAlive) { yield break; }
					foreach (BattleAction item3 in item2.GetStatusEffect<Poison>().TakeEffect())
					{
						yield return item3;
					}
				}
			}
			finally
			{
				localplaying = false;
			}
		}
	}
}


