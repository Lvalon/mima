using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using lvalonmima.GunName;
using LBoL.Core.Battle;
using LBoL.Core;
using LBoL.Core.StatusEffects;

namespace lvalonmima.Cards;

public sealed class cardomnilrDef : lvalonmimaCardTemplate
{
	public override CardConfig MakeConfig()
	{
		CardConfig config = GetCardDefaultConfig();
		config.Colors = [ManaColor.Red, ManaColor.Black];
		config.Cost = new ManaGroup() { Hybrid = 2, HybridColor = 7 };
		config.UpgradedCost = new ManaGroup() { Any = 1, Hybrid = 1, HybridColor = 7 };
		config.Rarity = Rarity.Uncommon;
		config.Type = CardType.Attack;
		config.TargetType = TargetType.RandomEnemy;
		config.IsPooled = false;
		config.HideMesuem = true;

		config.Damage = 4;
		config.UpgradedDamage = 6;

		config.GunName = GunNameID.GetGunFromId(4140);
		config.GunNameBurst = GunNameID.GetGunFromId(4140);

		config.Value1 = 1;
		config.Value2 = 4;
		config.UpgradedValue2 = 5;

		config.RelativeKeyword = config.UpgradedRelativeKeyword = Keyword.Expel;

		config.RelativeEffects = config.UpgradedRelativeEffects = [nameof(Charging)];

		config.RelativeCards = [nameof(cardomniur), nameof(cardomniul)];
		config.UpgradedRelativeCards = [nameof(cardomniur) + "+", nameof(cardomniul) + "+"];

		config.Illustrator = "mefomefo";

		config.Index = CardIndexGenerator.GetUniqueIndex(config);
		return config;
	}
}

[EntityLogic(typeof(cardomnilrDef))]
public sealed class cardomnilr : lvalonmimaCard
{
	public int value3 => 3;
	protected override IEnumerable<BattleAction> OnExpel(DieEventArgs args) => RunAsExpelling(OnExpelBody);

	private IEnumerable<BattleAction> OnExpelBody()
	{
		NotifyActivating();
		GameRun.SetHpAndMaxHp(Battle.Player.Hp + Value1, Battle.Player.MaxHp + Value1, true);
		yield break;
	}
	protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition) => RunAsPlaying(() => ActionsBody(selector, consumingMana, precondition));

	private IEnumerable<BattleAction> ActionsBody(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
	{
		yield return SacrificeAction(value3);
		if (Battle.BattleShouldEnd) yield break;
		yield return BuffAction<Charging>(value3, 0, 0);
		for (int i = 0; i < Value2; i++)
		{
			if (Battle.BattleShouldEnd) yield break;
			yield return AttackAction(UnitSelector.RandomEnemy, i == 0 ? GunName : GunNameID.GetGunFromId(4531));
		}
	}
}
