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

namespace lvalonmima.Cards;

public sealed class cardbigbangDef : lvalonmimaCardTemplate
{
	public override CardConfig MakeConfig()
	{
		CardConfig config = GetCardDefaultConfig();
		config.Colors = [ManaColor.Green, ManaColor.Colorless];
		config.Cost = new ManaGroup() { Any = 2, Green = 1, Colorless = 1 };
		config.Rarity = Rarity.Uncommon;
		config.Type = CardType.Attack;
		config.TargetType = TargetType.AllEnemies;

		config.Damage = 18;
		config.UpgradedDamage = 24;

		config.GunName = GunNameID.GetGunFromId(23071);
		config.GunNameBurst = GunNameID.GetGunFromId(23072);

		config.Keywords = Keyword.Accuracy | Keyword.FollowCard;
		config.UpgradedKeywords = config.Keywords | Keyword.Replenish;

		config.RelativeKeyword = config.UpgradedRelativeKeyword = Keyword.Expel | Keyword.FollowAttack | Keyword.Overdraft;

		config.RelativeEffects = config.UpgradedRelativeEffects = [nameof(setranscendence), nameof(seabyss), nameof(seunder)];

		config.Value1 = 1;
		config.Value2 = 1;

		config.Illustrator = "mifuru";

		config.Index = CardIndexGenerator.GetUniqueIndex(config);
		return config;
	}
}

[EntityLogic(typeof(cardbigbangDef))]
public sealed class cardbigbang : lvalonmimaCard.trigger50card
{
	protected override void EnterBattle2(BattleController battle)
	{
		ReactBattleEvent(Battle.Player.StatusEffectAdded, OnStatusEffectAdded);
	}

	private IEnumerable<BattleAction> OnStatusEffectAdded(StatusEffectApplyEventArgs args)
	{
		if ((args.Effect is setranscendence || args.Effect is seabyss) && Zone == CardZone.Discard)
			yield return new MoveCardToDrawZoneAction(this, DrawZoneTarget.Random);
	}
	protected override IEnumerable<BattleAction> OnExpel(DieEventArgs args) => RunAsExpelling(OnExpelBody);

	private IEnumerable<BattleAction> OnExpelBody()
	{
		NotifyActivating();
		if (IsUpgraded)
			GameRun.SetHpAndMaxHp(Battle.Player.Hp + Value1, Battle.Player.MaxHp + Value1, true);
		if (Battle.BattleShouldEnd) yield break;
		yield return new LockRandomTurnManaAction(Value2);
	}
	protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition) => RunAsPlaying(() => ActionsBody(selector, consumingMana, precondition));

	private IEnumerable<BattleAction> ActionsBody(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
	{
		bool goon = BepinexPlugin.u50;
		yield return AttackAction(selector);
		if (Battle.BattleShouldEnd) yield break;
		if (goon)
			yield return new FollowAttackAction(UnitSelector.RandomEnemy, Value2);
	}
}
