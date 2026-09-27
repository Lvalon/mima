using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using lvalonmima.GunName;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core;
using lvalonmima.StatusEffects;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.StatusEffects;

namespace lvalonmima.Cards;

public sealed class cardsuperimpactDef : lvalonmimaCardTemplate
{
	public override CardConfig MakeConfig()
	{
		CardConfig config = GetCardDefaultConfig();
		config.Colors = [ManaColor.Red, ManaColor.Black];
		config.Cost = new ManaGroup() { Hybrid = 2, HybridColor = 7 };
		config.Rarity = Rarity.Common;
		config.Type = CardType.Attack;
		config.TargetType = TargetType.SingleEnemy;

		config.Damage = 14;
		config.UpgradedDamage = 18;

		config.GunName = GunNameID.GetGunFromId(12140);
		config.GunNameBurst = GunNameID.GetGunFromId(12141);

		config.Value1 = 5;
		config.Value2 = 14;
		config.UpgradedValue2 = 18;

		config.RelativeEffects = config.UpgradedRelativeEffects = [nameof(sesideload)];

		config.RelativeKeyword = config.UpgradedRelativeKeyword = Keyword.Expel;

		config.Illustrator = "カズハル／硝酸";

		config.Index = CardIndexGenerator.GetUniqueIndex(config);
		return config;
	}
}

[EntityLogic(typeof(cardsuperimpactDef))]
public sealed class cardsuperimpact : lvalonmimaCard
{
	public int Value10 => 10;
	public int svalue => 2;
	bool go = false;
	protected override IEnumerable<BattleAction> OnExpel(DieEventArgs args) => RunAsExpelling(OnExpelBody);

	private IEnumerable<BattleAction> OnExpelBody()
	{
		if (go)
		{
			NotifyActivating();
			yield return SacrificeAction(Value10);
			yield return new GainPowerAction(Value2);
		}
	}

	public override Interaction Precondition()
	{
		if (!Battle.Player.TryGetStatusEffect(out Charging se) || se.Level < svalue)
			return null;

		return CreateChoicePair<cardsuperimpact>();
	}
	protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition) => RunAsPlaying(() => ActionsBody(selector, precondition));

	private IEnumerable<BattleAction> ActionsBody(UnitSelector selector, Interaction precondition)
	{
		go = false;
		MiniSelectCardInteraction miniSelectCardInteraction = (MiniSelectCardInteraction)precondition;
		Card card = miniSelectCardInteraction?.SelectedCard;
		if (card != null)
		{
			if (card.ChoiceCardIndicator == 2) // ExtraDescription2
			{
				go = true;
				if (Battle.Player.TryGetStatusEffect(out Charging se))
				{
					foreach (BattleAction action in SpendCharging(se, svalue)) yield return action;
				}
			}
		}
		yield return AttackAction(selector);
		if (Battle.BattleShouldEnd)
			yield break;
		yield return BuffAction<Charging>(svalue);
		if (go)
			yield return SacrificeAction(Value1);
	}
}
