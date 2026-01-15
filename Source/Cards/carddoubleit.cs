using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using lvalonmima.GunName;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Base.Extensions;
using LBoL.Core;
using System.Linq;
using LBoL.Core.Units;

namespace lvalonmima.Cards
{
	public sealed class carddoubleitDef : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig();
			config.Colors = new List<ManaColor>() { ManaColor.Green, ManaColor.Black };
			config.Cost = new ManaGroup() { Black = 2, Green = 1 };
			config.Rarity = Rarity.Uncommon;
			config.Type = CardType.Attack;
			config.TargetType = TargetType.SingleEnemy;

			config.GunName = GunNameID.GetGunFromId(4534);
			config.GunNameBurst = GunNameID.GetGunFromId(4534);

			config.Damage = 0;
			config.Keywords = Keyword.FollowCard;
			config.UpgradedKeywords = Keyword.FollowCard;

			config.RelativeKeyword = Keyword.Expel;
			config.UpgradedRelativeKeyword = Keyword.Expel;

			config.Value1 = 4;
			config.Value2 = 2;

			config.Illustrator = "白影";

			config.Index = CardIndexGenerator.GetUniqueIndex(config);
			return config;
		}
	}

	[EntityLogic(typeof(carddoubleitDef))]
	public sealed class carddoubleit : lvalonmimaCard
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
		int mult = 1;
		int goes = 0;
		List<EnemyUnit> a1 = new List<EnemyUnit>();
		List<EnemyUnit> a2 = new List<EnemyUnit>();
		List<EnemyUnit> full = new List<EnemyUnit>();
		EnemyUnit enemy;
		protected override IEnumerable<BattleAction> OnExpel(DieEventArgs args)
		{
			expelling = true;
			try
			{
				NotifyActivating();
				int up = toolbox.Round(goes * 1.0 / 3);
				if (Battle.Player.Hp + up > 0)
				{
					GameRun.SetHpAndMaxHp(Battle.Player.Hp + up, Battle.Player.MaxHp + up, true);
				}
				yield break;
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
				yield return SacrificeAction(Value2);
				if (Battle.BattleShouldEnd) { yield break; }
				a1 = new List<EnemyUnit>();
				a2 = new List<EnemyUnit>();
				goes = 0;
				mult = 1;
				full = Battle.AllAliveEnemies.ToList();
				enemy = selector.SelectedEnemy;
				if (enemy.IsAlive)
				{
					a1.Add(enemy);
					yield return DamageAction.LoseLife(enemy, mult * Value1);
				}

				if (IsUpgraded)
				{
					while ((a2.Count() < full.Count() || mult * Value1 >= int.MaxValue / 2) && Battle.AllAliveEnemies.Any(e => !a2.Contains(e)) && Battle.AllAliveEnemies.Count() > 0)
					{
						full = Battle.AllAliveEnemies.ToList();
						enemy = Battle.AllAliveEnemies.Where(e => !a2.Contains(e)).Sample(GameRun.BattleRng);
						if (a1.Contains(enemy))
						{
							a2.Add(enemy);
						}
						else
						{
							a1.Add(enemy);
						}
						mult *= 2;
						goes++;
						yield return DamageAction.LoseLife(enemy, mult * Value1, GunName);
						a1 = a1.Where(e => full.Contains(e)).ToList();
						a2 = a2.Where(e => full.Contains(e)).ToList();
					}
				}
				else
				{
					while ((a1.Count() < full.Count() || mult * Value1 >= int.MaxValue / 2) && Battle.AllAliveEnemies.Any(e => !a1.Contains(e)) && Battle.AllAliveEnemies.Count() > 0)
					{
						full = Battle.AllAliveEnemies.ToList();
						enemy = Battle.AllAliveEnemies.Where(e => !a1.Contains(e)).Sample(GameRun.BattleRng);
						a1.Add(enemy);
						mult *= 2;
						goes++;
						yield return DamageAction.LoseLife(enemy, mult * Value1, GunName);
						a1 = a1.Where(e => full.Contains(e)).ToList();
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


