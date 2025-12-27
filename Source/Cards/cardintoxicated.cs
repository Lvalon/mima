using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using lvalonmima.GunName;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.Battle.BattleActions;
using LBoL.Base.Extensions;
using LBoL.Core;
using System.Linq;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Others;

namespace lvalonmima.Cards
{
	public sealed class cardintoxicatedDef : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig();
			config.Colors = new List<ManaColor>() { ManaColor.White, ManaColor.Green };
			config.Cost = new ManaGroup() { Any = 1, White = 1, Green = 1 };
			config.Rarity = Rarity.Uncommon;
			config.Type = CardType.Attack;
			config.TargetType = TargetType.AllEnemies;

			config.Damage = 10;

			config.GunName = GunNameID.GetGunFromId(7000);
			config.GunNameBurst = GunNameID.GetGunFromId(7001);

			config.Keywords = Keyword.FollowCard;
			config.UpgradedKeywords = Keyword.FollowCard;

			config.RelativeKeyword = Keyword.Expel;
			config.UpgradedRelativeKeyword = Keyword.Expel;

			config.RelativeEffects = new List<string>() { nameof(Poison) };
			config.UpgradedRelativeEffects = new List<string>() { nameof(Poison) };

			config.Value1 = 1;

			config.Illustrator = "ヘッツァ";

			config.Index = CardIndexGenerator.GetUniqueIndex(config);
			return config;
		}
	}

	[EntityLogic(typeof(cardintoxicatedDef))]
	public sealed class cardintoxicated : lvalonmimaCard
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
				NotifyActivating();
				List<Card> list = GameRun.BaseDeck.Where(card2 => card2.CanUpgradeAndPositive).ToList();
				if (list.Count <= 0)
				{
					yield break;
				}

				Card card = list.Sample(GameRun.GameRunEventRng);
				GameRun.UpgradeDeckCard(card);
				foreach (Card item in Battle.EnumerateAllCards())
				{
					if (item.InstanceId == card.InstanceId)
					{
						if (item.CanUpgrade && Battle.AllAliveEnemies.Count() > 0)
						{
							yield return new UpgradeCardAction(item);
						}
					}
				}
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
				List<EnemyUnit> units = Battle.AllAliveEnemies.Where(e => e.HasStatusEffect<Poison>()).ToList();
				int count = units.Count();
				if (count > 0)
				{
					yield return new DamageAction(Battle.Player, units, Damage, GunName);
					yield return new ApplyStatusEffectAction<Poison>(Battle.Player, count * Value1);
					IEnumerable<Card> cards = Battle.HandZone.Where(c => !c.IsUpgraded && c.CanUpgradeAndPositive).SampleManyOrAll(count * Value1, GameRun.BattleRng);
					if (cards.Count() > 0)
					{
						yield return new UpgradeCardsAction(cards);
					}
				}
				if (IsUpgraded)
				{
					foreach (EnemyUnit item2 in Battle.AllAliveEnemies.Where(enemy => enemy.HasStatusEffect<Poison>()))
					{
						if (Battle.BattleShouldEnd || !item2.IsAlive) { yield break; }
						foreach (BattleAction item3 in item2.GetStatusEffect<Poison>().TakeEffect())
						{
							yield return item3;
						}
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


