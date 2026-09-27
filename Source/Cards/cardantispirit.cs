using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.Battle.BattleActions;
using LBoL.Base.Extensions;
using LBoL.Core;
using System.Linq;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Others;

namespace lvalonmima.Cards;

public sealed class cardantispiritDef : lvalonmimaCardTemplate
{
	public override CardConfig MakeConfig()
	{
		CardConfig config = GetCardDefaultConfig();
		config.Colors = [ManaColor.Green, ManaColor.White];
		config.Cost = new ManaGroup() { White = 1, Green = 1 };
		config.UpgradedCost = new ManaGroup() { Any = 1, Hybrid = 1, HybridColor = 3 };
		config.Rarity = Rarity.Uncommon;
		config.Type = CardType.Attack;
		config.TargetType = TargetType.SingleEnemy;

		config.Damage = 0;

		config.UpgradedKeywords = Keyword.FollowCard;

		config.RelativeKeyword = config.UpgradedRelativeKeyword = Keyword.FollowAttack | Keyword.Expel;

		config.RelativeEffects = config.UpgradedRelativeEffects = [nameof(Poison)];

		config.Value1 = 2;
		config.Value2 = 3;

		config.Illustrator = "takatsuki nato";

		config.Index = CardIndexGenerator.GetUniqueIndex(config);
		return config;
	}
}

[EntityLogic(typeof(cardantispiritDef))]
public sealed class cardantispirit : lvalonmimaCard
{
	EnemyUnit enemy;
	protected override IEnumerable<BattleAction> OnExpel(DieEventArgs args) => RunAsExpelling(OnExpelBody);

	private IEnumerable<BattleAction> OnExpelBody()
	{
		NotifyActivating();
		if (Battle.ExileZone.Count > 0)
		{
			SelectCardInteraction interaction = new(1, 1, Battle.ExileZone)
			{
				Source = this
			};
			yield return new InteractionAction(interaction);
			IReadOnlyList<Card> cards = interaction.SelectedCards;
			if (cards.Count > 0)
			{
				foreach (Card card in cards)
				{
					GameRun.SetHpAndMaxHp(Battle.Player.Hp + card.ConfigCost.Amount, Battle.Player.MaxHp + card.ConfigCost.Amount, true);
					yield return new RemoveCardAction(card);
				}
			}
		}
	}
	private IEnumerable<BattleAction> effect(EnemyUnit unit)
	{
		if (!unit.IsAlive && Battle.AllAliveEnemies.Any())
		{
			unit = Battle.RandomAliveEnemy;
			enemy = unit;
		}
		yield return DebuffAction<Poison>(unit, Value1, 0, 0, 0);
		if (Battle.BattleShouldEnd) yield break;
		IEnumerable<Card> cards = Battle.HandZone.Where(c => !c.IsUpgraded && c.CanUpgradeAndPositive);
		if (cards != null && cards.Any())
			yield return new UpgradeCardAction(cards.Sample(GameRun.BattleRng));
		if (unit.IsAlive)
		{
			UnitSelector selector = new(unit);
			yield return new FollowAttackAction(selector);
		}
	}
	protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition) => RunAsPlaying(() => ActionsBody(selector, consumingMana, precondition));

	private IEnumerable<BattleAction> ActionsBody(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
	{
		enemy = selector.SelectedEnemy;
		if (!enemy.IsAlive)
			enemy = Battle.RandomAliveEnemy;

		for (int i = 0; i < Value2; i++)
		{
			if (Battle.BattleShouldEnd) yield break;
			if (enemy == null || !enemy.IsAlive || i != 0)
			{
				if (!Battle.AllAliveEnemies.Any(e => e != enemy))
				{
					break;
				}
				enemy = Battle.AllAliveEnemies.Where(e => e != enemy).Sample(GameRun.BattleRng);
				if (enemy == null)
				{
					break;
				}
			}
			foreach (BattleAction ba in effect(enemy)) yield return ba;
		}
	}
}
