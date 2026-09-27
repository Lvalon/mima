using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Cirno;

namespace lvalonmima.Cards;

public sealed class cardyukionnaDef : lvalonmimaCardTemplate
{
	public override CardConfig MakeConfig()
	{
		CardConfig config = GetCardDefaultConfig();
		config.Colors = [ManaColor.Blue, ManaColor.Black];
		config.Cost = new ManaGroup() { Blue = 1, Black = 1 };
		config.Rarity = Rarity.Uncommon;
		config.Type = CardType.Attack;
		config.TargetType = TargetType.RandomEnemy;

		config.Damage = 0;

		config.RelativeKeyword = config.UpgradedRelativeKeyword = Keyword.Expel;

		config.RelativeEffects = config.UpgradedRelativeEffects = [nameof(Cold)];

		config.Value1 = 2;
		config.UpgradedValue1 = 3;
		config.Value2 = 1;

		config.Illustrator = "老邢";

		config.Index = CardIndexGenerator.GetUniqueIndex(config);
		return config;
	}
}

[EntityLogic(typeof(cardyukionnaDef))]
public sealed class cardyukionna : lvalonmimaCard
{
	protected override IEnumerable<BattleAction> OnExpel(DieEventArgs args) => RunAsExpelling(OnExpelBody);

	private IEnumerable<BattleAction> OnExpelBody()
	{
		NotifyActivating();
		if (IsUpgraded)
			GameRun.SetHpAndMaxHp(Battle.Player.Hp + Value2, Battle.Player.MaxHp + Value2, true);
		if (Battle.BattleShouldEnd) yield break;
		yield return new DrawManyCardAction(Value1);
	}
	protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition) => RunAsPlaying(() => ActionsBody(selector, consumingMana, precondition));

	private IEnumerable<BattleAction> ActionsBody(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
	{
		yield return SacrificeAction(Value1);
		for (int i = 0; i < Value1; i++)
		{
			if (Battle.BattleShouldEnd) yield break;
			EnemyUnit target = Battle.RandomAliveEnemy;
			if (target == null) yield break;
			yield return DebuffAction<Cold>(target, 1);
		}
	}
}
