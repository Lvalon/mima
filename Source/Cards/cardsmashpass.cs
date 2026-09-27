using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using lvalonmima.Common;
using lvalonmima.GunName;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core;
using System.Linq;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Units;
using lvalonmima.StatusEffects;

namespace lvalonmima.Cards;

public sealed class cardsmashpassDef : lvalonmimaCardTemplate
{
	public override CardConfig MakeConfig()
	{
		CardConfig config = GetCardDefaultConfig();
		config.Colors = [ManaColor.Green, ManaColor.Colorless];
		config.Cost = new ManaGroup() { Any = 2, Green = 2, Colorless = 1 };
		config.Rarity = Rarity.Rare;
		config.Type = CardType.Attack;
		config.TargetType = TargetType.SingleEnemy;

		config.Damage = 14;

		config.GunName = GunNameID.GetGunFromId(6048);
		config.GunNameBurst = GunNameID.GetGunFromId(6048);

		config.Mana = new ManaGroup() { Green = 1, Colorless = 1 };

		config.Value1 = 2;
		config.UpgradedValue1 = 3;
		config.Value2 = 1;
		config.UpgradedValue2 = 2;

		config.UpgradedKeywords = Keyword.FollowCard;

		config.RelativeEffects = config.UpgradedRelativeEffects = [nameof(seunder)];

		config.RelativeKeyword = config.UpgradedRelativeKeyword = Keyword.Expel | Keyword.FollowAttack;

		config.Illustrator = "くまばち";

		config.Index = CardIndexGenerator.GetUniqueIndex(config);
		return config;
	}
}

[EntityLogic(typeof(cardsmashpassDef))]
public sealed class cardsmashpass : lvalonmimaCard.trigger25card
{
	protected override IEnumerable<BattleAction> OnExpel(DieEventArgs args) => RunAsExpelling(OnExpelBody);

	private IEnumerable<BattleAction> OnExpelBody()
	{
		NotifyActivating();
		if (Battle.AllAliveEnemies.Any())
		{
			EnemyUnit tmp = Battle.RandomAliveEnemy;
			foreach (BattleAction ba in effect1(new UnitSelector(tmp))) yield return ba;
			if (Battle.AllAliveEnemies.Any())
			{
				if (!tmp.IsAlive)
				{
					tmp = Battle.RandomAliveEnemy;
				}
				foreach (BattleAction ba in effect2(new UnitSelector(tmp))) yield return ba;
			}
		}
		GameRun.GainMaxHpAndHp(Battle.Player, Value2);
	}
	private IEnumerable<BattleAction> effect1(UnitSelector selector)
	{
		if (Battle.BattleShouldEnd) yield break;
		yield return AttackAction(selector);
		if (Battle.BattleShouldEnd) yield break;
		yield return new GainManaAction(Mana);
	}
	private IEnumerable<BattleAction> effect2(UnitSelector selector)
	{
		if (Battle.BattleShouldEnd) yield break;
		yield return new FollowAttackAction(selector, Value1);
	}
	public override Interaction Precondition()
	{
		if (MimaHp.Below25)
			return null;
		return CreateChoicePair<cardsmashpass>();
	}
	protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition) => RunAsPlaying(() => ActionsBody(selector, precondition));

	private IEnumerable<BattleAction> ActionsBody(UnitSelector selector, Interaction precondition)
	{
		NotifyActivating();
		if (MimaHp.Below25)
		{
			foreach (BattleAction ba in effect1(selector)) yield return ba;
			foreach (BattleAction ba in effect2(selector)) yield return ba;
		}
		else
		{
			MiniSelectCardInteraction miniSelectCardInteraction = (MiniSelectCardInteraction)precondition;
			Card card = miniSelectCardInteraction?.SelectedCard;
			if (card != null && card.ChoiceCardIndicator == 1) // ExtraDescription1
			{
				foreach (BattleAction ba in effect1(selector)) yield return ba;
			}
			if (card != null && card.ChoiceCardIndicator == 2) // ExtraDescription2
			{
				foreach (BattleAction ba in effect2(selector)) yield return ba;
			}
		}
	}
}
