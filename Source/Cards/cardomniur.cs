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

namespace lvalonmima.Cards;

public sealed class cardomniurDef : lvalonmimaCardTemplate
{
	public override CardConfig MakeConfig()
	{
		CardConfig config = GetCardDefaultConfig();
		config.Colors = [ManaColor.Black, ManaColor.Colorless];
		config.Cost = new ManaGroup() { Black = 2, Colorless = 1 };
		config.UpgradedCost = new ManaGroup() { Any = 1, Black = 1, Colorless = 1 };
		config.Rarity = Rarity.Uncommon;
		config.Type = CardType.Attack;
		config.TargetType = TargetType.RandomEnemy;
		config.FindInBattle = false;

		config.Damage = 4;
		config.UpgradedDamage = 6;

		config.GunName = GunNameID.GetGunFromId(4141); //4142 4531
		config.GunNameBurst = GunNameID.GetGunFromId(4141);

		config.Value1 = 2;
		config.UpgradedValue1 = 3;
		config.Value2 = 4;
		config.UpgradedValue2 = 5;

		config.RelativeKeyword = config.UpgradedRelativeKeyword = Keyword.Purify | Keyword.Expel;

		config.RelativeCards = [nameof(cardomniul), nameof(cardomnilr)];
		config.UpgradedRelativeCards = [nameof(cardomniul) + "+", nameof(cardomnilr) + "+"];

		config.Illustrator = "mefomefo";

		config.Index = CardIndexGenerator.GetUniqueIndex(config);
		return config;
	}
}

[EntityLogic(typeof(cardomniurDef))]
public sealed class cardomniur : lvalonmimaCard
{
	protected override IEnumerable<BattleAction> OnExpel(DieEventArgs args) => RunAsExpelling(OnExpelBody);

	private IEnumerable<BattleAction> OnExpelBody()
	{
		NotifyActivating();
		Card deckCardByInstanceId = GameRun.GetDeckCardByInstanceId(InstanceId);
		bool hasself = deckCardByInstanceId != null;
		foreach (BattleAction ba in RemoveSelf()) yield return ba;
		if (hasself)
			yield return new AddCardsToDeckAction(Library.CreateCard<cardomniul>(IsUpgraded));
	}
	protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition) => RunAsPlaying(() => ActionsBody(selector, consumingMana, precondition));

	private IEnumerable<BattleAction> ActionsBody(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
	{
		yield return SacrificeAction(Value1);
		if (Battle.BattleShouldEnd) yield break;
		if (Battle.BattleMana.HasTrivial)
			yield return ConvertManaAction.Purify(Battle.BattleMana, Value1);
		for (int i = 0; i < Value2; i++)
		{
			if (Battle.BattleShouldEnd) yield break;
			yield return AttackAction(UnitSelector.RandomEnemy, i == 0 ? GunName : GunNameID.GetGunFromId(4531));
		}
	}
}
