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
using LBoL.Base.Extensions;
using LBoL.Core;
using System.Linq;
using lvalonmima.StatusEffects;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace lvalonmima.Cards;

public sealed class cardignitionDef : lvalonmimaCardTemplate
{
	public override CardConfig MakeConfig()
	{
		CardConfig config = GetCardDefaultConfig();
		config.Colors = [ManaColor.Red, ManaColor.Green];
		config.Cost = new ManaGroup() { Any = 1, Hybrid = 2, HybridColor = 9 };
		config.Rarity = Rarity.Uncommon;
		config.Type = CardType.Attack;
		config.TargetType = TargetType.AllEnemies;

		config.Damage = 0;

		config.GunName = GunNameID.GetGunFromId(23010);
		config.GunNameBurst = GunNameID.GetGunFromId(23011);

		config.Value1 = 1;
		config.UpgradedValue1 = 2;
		config.Value2 = 3;

		config.Keywords = config.UpgradedKeywords = Keyword.FollowCard;

		config.RelativeEffects = config.UpgradedRelativeEffects = [nameof(Vulnerable), nameof(sesideload), nameof(semburst)];

		config.RelativeKeyword = config.UpgradedRelativeKeyword = Keyword.Expel;

		config.Illustrator = "菓しおり";

		config.Index = CardIndexGenerator.GetUniqueIndex(config);
		return config;
	}
}

[EntityLogic(typeof(cardignitionDef))]
public sealed class cardignition : lvalonmimaCard
{
	public override int AdditionalDamage => Battle?.Player.TryGetStatusEffect(out semburst se) == true ? se.Count : 0;
	public int svalue => 6;
	protected override IEnumerable<BattleAction> OnExpel(DieEventArgs args) => RunAsExpelling(OnExpelBody);

	private IEnumerable<BattleAction> OnExpelBody()
	{
		if (!Battle.Player.TryGetStatusEffect(out Charging se) || se.Level < svalue || Battle.HandZone.Count(c => c != this) == 0)
			yield break;
		NotifyActivating();
		MiniSelectCardInteraction interaction = CreateChoicePair<cardignition>();
		yield return new InteractionAction(interaction);
		Card card = interaction?.SelectedCard;
		if (card != null && card.ChoiceCardIndicator == 2) // ExtraDescription2
		{
			foreach (BattleAction action in SpendCharging(se, svalue)) yield return action;
			SelectCardInteraction interaction2 = new(1, 1, Battle.HandZone)
			{
				Source = this
			};
			yield return new InteractionAction(interaction2);
			IReadOnlyList<Card> cards = interaction2.SelectedCards;
			if (cards.Count > 0)
			{
				foreach (Card card2 in cards)
				{
					GameRun.GainMaxHpAndHp(Battle.Player, card2.ConfigCost.Amount);
					yield return new ExileCardAction(card2);
				}
			}
		}
		yield break;
	}
	protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition) => RunAsPlaying(() => ActionsBody(selector));

	private IEnumerable<BattleAction> ActionsBody(UnitSelector selector)
	{
		foreach (Unit unit in Battle.AllAliveEnemies)
		{
			if (!unit.IsAlive || Battle.BattleShouldEnd) continue;
			yield return DebuffAction<Vulnerable>(unit, 0, Value1);
		}
		if (Battle.BattleShouldEnd) yield break;
		yield return BuffAction<Charging>(Value2);
		if (IsUpgraded && Battle.AllAliveEnemies.Any())
			yield return BuffAction<TempFirepower>(Value2);
		if (Battle.BattleShouldEnd) yield break;
		yield return BuffAction<semburst>(Value2);
		if (Battle.BattleShouldEnd) yield break;
		yield return AttackAction(selector);
	}
}
