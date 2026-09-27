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

namespace lvalonmima.Cards;

public sealed class cardintoxicatedDef : lvalonmimaCardTemplate
{
	public override CardConfig MakeConfig()
	{
		CardConfig config = GetCardDefaultConfig();
		config.Colors = [ManaColor.White, ManaColor.Green];
		config.Cost = new ManaGroup() { Any = 1, White = 1, Green = 1 };
		config.Rarity = Rarity.Uncommon;
		config.Type = CardType.Attack;
		config.TargetType = TargetType.AllEnemies;

		config.Damage = 10;

		config.GunName = GunNameID.GetGunFromId(7000);
		config.GunNameBurst = GunNameID.GetGunFromId(7001);

		config.Keywords = config.UpgradedKeywords = Keyword.FollowCard;

		config.RelativeKeyword = config.UpgradedRelativeKeyword = Keyword.Expel;

		config.RelativeEffects = config.UpgradedRelativeEffects = [nameof(Poison)];

		config.Value1 = 1;

		config.Illustrator = "ヘッツァ";

		config.Index = CardIndexGenerator.GetUniqueIndex(config);
		return config;
	}
}

[EntityLogic(typeof(cardintoxicatedDef))]
public sealed class cardintoxicated : lvalonmimaCard
{
	protected override IEnumerable<BattleAction> OnExpel(DieEventArgs args) => RunAsExpelling(OnExpelBody);

	private IEnumerable<BattleAction> OnExpelBody()
	{
		NotifyActivating();
		List<Card> list = [.. GameRun.BaseDeck.Where(card2 => card2.CanUpgradeAndPositive)];
		if (list.Count <= 0)
			yield break;

		Card card = list.Sample(GameRun.GameRunEventRng);
		GameRun.UpgradeDeckCard(card);
		foreach (Card item in Battle.EnumerateAllCards())
		{
			if (item.InstanceId == card.InstanceId)
			{
				if (item.CanUpgrade && Battle.AllAliveEnemies.Any())
				{
					yield return new UpgradeCardAction(item);
				}
			}
		}
	}
	protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition) => RunAsPlaying(() => ActionsBody(selector, consumingMana, precondition));

	private IEnumerable<BattleAction> ActionsBody(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
	{
		List<EnemyUnit> units = [.. Battle.AllAliveEnemies.Where(e => e.HasStatusEffect<Poison>())];
		int count = units.Count();
		if (count > 0)
		{
			yield return new DamageAction(Battle.Player, units, Damage, GunName);
			yield return new ApplyStatusEffectAction<Poison>(Battle.Player, count * Value1);
			IEnumerable<Card> cards = Battle.HandZone.Where(c => !c.IsUpgraded && c.CanUpgradeAndPositive).SampleManyOrAll(count * Value1, GameRun.BattleRng);
			if (cards.Any())
			{
				yield return new UpgradeCardsAction(cards);
			}
		}
		if (IsUpgraded)
		{
			foreach (EnemyUnit item2 in Battle.AllAliveEnemies.Where(enemy => enemy.HasStatusEffect<Poison>()))
			{
				if (Battle.BattleShouldEnd || !item2.IsAlive) yield break;
				foreach (BattleAction item3 in item2.GetStatusEffect<Poison>().TakeEffect())
				{
					yield return item3;
				}
			}
		}
	}
}
