using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using lvalonmima.Common;
using lvalonmima.GunName;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core;
using lvalonmima.StatusEffects;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.StatusEffects;

namespace lvalonmima.Cards;

public sealed class cardimplodemagicDef : lvalonmimaCardTemplate
{
	public override CardConfig MakeConfig()
	{
		CardConfig config = GetCardDefaultConfig();
		config.Colors = [ManaColor.Red, ManaColor.Green];
		config.Cost = new ManaGroup() { Red = 1, Green = 1 };
		config.Rarity = Rarity.Common;
		config.Type = CardType.Attack;
		config.TargetType = TargetType.AllEnemies;

		config.Damage = 8;

		config.GunName = GunNameID.GetGunFromId(4522);
		config.GunNameBurst = GunNameID.GetGunFromId(4521);

		config.Value1 = 8;
		config.Value2 = 1;

		config.RelativeEffects = config.UpgradedRelativeEffects = [nameof(semburst), nameof(sesideload)];

		config.Keywords = config.UpgradedKeywords = Keyword.FollowCard;

		config.UpgradedRelativeKeyword = Keyword.Expel;

		config.Illustrator = "hachi (8bit canvas)";

		config.Index = CardIndexGenerator.GetUniqueIndex(config);
		return config;
	}
}

[EntityLogic(typeof(cardimplodemagicDef))]
public sealed class cardimplodemagic : lvalonmimaCard
{
	public int svalue => 1;
	bool go = false;
	public override int AdditionalDamage
	{
		get
		{
			if (Battle != null && go && IsUpgraded && Battle.Player.TryGetStatusEffect(out semburst se))
				return se.Count;
			return 0;
		}
	}
	public override DamageInfo Damage
	{
		get
		{
			if (Battle != null && go)
				return DamageInfo.Attack(RawDamage, true);
			return DamageInfo.Attack(RawDamage, IsAccuracy);
		}
	}
	protected override IEnumerable<BattleAction> OnExpel(DieEventArgs args) => RunAsExpelling(OnExpelBody);

	private IEnumerable<BattleAction> OnExpelBody()
	{
		if (IsUpgraded)
		{
			NotifyActivating();
			GameRun.GainMaxHpAndHp(Battle.Player, Value2);
		}
		yield break;
	}

	public override Interaction Precondition()
	{
		if (!Battle.Player.TryGetStatusEffect(out Charging se) || se.Level < svalue)
			return null;

		return CreateChoicePair<cardimplodemagic>();
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
		yield return BuffAction<semburst>(Value1, 0, 0);
		if (Battle.BattleShouldEnd) yield break;
		yield return AttackAction(selector);
	}
}
