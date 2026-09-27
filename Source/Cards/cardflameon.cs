using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using lvalonmima.GunName;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core;
using System.Linq;
using LBoL.Core.StatusEffects;

namespace lvalonmima.Cards;

public sealed class cardflameonDef : lvalonmimaCardTemplate
{
	public override CardConfig MakeConfig()
	{
		CardConfig config = GetCardDefaultConfig();
		config.Colors = [ManaColor.Red];
		config.Cost = new ManaGroup() { Any = 1, Red = 1 };
		config.Rarity = Rarity.Common;
		config.Type = CardType.Attack;
		config.TargetType = TargetType.SingleEnemy;

		config.Damage = 4;

		config.GunName = GunNameID.GetGunFromId(12140);
		config.GunNameBurst = GunNameID.GetGunFromId(12141);

		config.Value1 = 4;
		config.Value2 = 4;
		config.UpgradedValue2 = 5;

		config.RelativeEffects = [nameof(Charging)];
		config.UpgradedRelativeEffects = [.. config.RelativeEffects, nameof(Vulnerable)];

		config.RelativeKeyword = config.UpgradedRelativeKeyword = Keyword.Expel;

		config.Illustrator = "kazetuki";

		config.Index = CardIndexGenerator.GetUniqueIndex(config);
		return config;
	}
}

[EntityLogic(typeof(cardflameonDef))]
public sealed class cardflameon : lvalonmimaCard
{
	protected override int BaseValue3 => 16;
	protected override int BaseUpgradedValue3 => 20;
	protected override IEnumerable<BattleAction> OnExpel(DieEventArgs args) => RunAsExpelling(OnExpelBody);

	private IEnumerable<BattleAction> OnExpelBody()
	{
		NotifyActivating();
		yield return SacrificeAction(Value2);
		yield return new GainPowerAction(Value3);
	}
	protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition) => RunAsPlaying(() => ActionsBody(selector, consumingMana, precondition));

	private IEnumerable<BattleAction> ActionsBody(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
	{
		yield return BuffAction<Charging>(Value1, 0, 0);
		if (IsUpgraded && selector.SelectedEnemy.IsAlive && Battle.AllAliveEnemies.Any())
			yield return new ApplyStatusEffectAction<Vulnerable>(selector.SelectedEnemy, 0, 1, 0, 0);
		if (Battle.BattleShouldEnd) yield break;
		yield return AttackAction(selector);
	}
}
