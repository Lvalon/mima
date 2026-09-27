using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using lvalonmima.GunName;
using LBoL.Core.Battle;
using LBoL.Core;
using System.Linq;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Cirno;
using LBoL.EntityLib.StatusEffects.Sakuya;

namespace lvalonmima.Cards;

public sealed class carddpillarDef : lvalonmimaCardTemplate
{
	public override CardConfig MakeConfig()
	{
		CardConfig config = GetCardDefaultConfig();
		config.Colors = [ManaColor.Blue, ManaColor.Black];
		config.Cost = new ManaGroup() { Any = 2, Blue = 1, Black = 1 };
		config.Rarity = Rarity.Uncommon;
		config.Type = CardType.Attack;
		config.TargetType = TargetType.AllEnemies;

		config.Damage = 9;

		config.GunName = GunNameID.GetGunFromId(14122);
		config.GunNameBurst = GunNameID.GetGunFromId(14123);

		config.RelativeKeyword = config.UpgradedRelativeKeyword = Keyword.Expel;

		config.RelativeEffects = config.UpgradedRelativeEffects = [nameof(Cold), nameof(TimeAuraSe)];

		config.RelativeCards = config.UpgradedRelativeCards = [nameof(cardpurediamond)];

		config.Value1 = 9;
		config.Value2 = 1;

		config.Illustrator = "二阶堂";

		config.Index = CardIndexGenerator.GetUniqueIndex(config);
		return config;
	}
}

[EntityLogic(typeof(carddpillarDef))]
public sealed class carddpillar : lvalonmimaCard
{
	private IEnumerable<BattleAction> effect()
	{
		yield return AttackAction(UnitSelector.AllEnemies);
		if (Battle.BattleShouldEnd) yield break;
		foreach (Unit unit in Battle.AllAliveEnemies)
		{
			if (Battle.BattleShouldEnd) yield break;
			if (!unit.IsAlive) continue;
			yield return DebuffAction<Cold>(unit, Value1);
		}
		if (Battle.BattleShouldEnd) yield break;
		yield return BuffAction<TimeAuraSe>(Value1, 0, 0, 0);
		if (Battle.BattleShouldEnd) yield break;
		yield return AddPureDiamondsToDraw(Value2);
	}
	protected override IEnumerable<BattleAction> OnExpel(DieEventArgs args) => RunAsExpelling(OnExpelBody);

	private IEnumerable<BattleAction> OnExpelBody()
	{
		NotifyActivating();
		if (Battle.AllAliveEnemies.Any())
		{
			foreach (BattleAction ba in effect()) yield return ba;
		}
		if (IsUpgraded)
		{
			yield return SacrificeAction(Value1);
			if (Battle.BattleShouldEnd) yield break;
			yield return DebuffAction<Cold>(Battle.Player, Value1);
		}
	}
	protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition) => RunAsPlaying(() => ActionsBody(selector, consumingMana, precondition));

	private IEnumerable<BattleAction> ActionsBody(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
	{
		foreach (BattleAction ba in effect()) yield return ba;
	}
}
