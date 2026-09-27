using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using lvalonmima.GunName;
using LBoL.Core.Battle;
using LBoL.Core;

namespace lvalonmima.Cards;

public sealed class cardsymmconvDef : lvalonmimaCardTemplate
{
	public override CardConfig MakeConfig()
	{
		CardConfig config = GetCardDefaultConfig();
		config.Colors = [ManaColor.Green];
		config.Cost = new ManaGroup() { Any = 1 };
		config.UpgradedCost = new ManaGroup() { Any = 0 };
		config.Rarity = Rarity.Common;
		config.Type = CardType.Attack;
		config.TargetType = TargetType.SingleEnemy;

		config.Damage = 3;

		config.GunName = GunNameID.GetGunFromId(12190);
		config.GunNameBurst = GunNameID.GetGunFromId(12191);

		config.Keywords = config.UpgradedKeywords = Keyword.Replenish;

		config.RelativeKeyword = config.UpgradedRelativeKeyword = Keyword.Grow | Keyword.Expel;

		config.Value1 = 2;
		config.Value2 = 2;

		config.Illustrator = "门番神玉";

		config.Index = CardIndexGenerator.GetUniqueIndex(config);
		return config;
	}
}

[EntityLogic(typeof(cardsymmconvDef))]
public sealed class cardsymmconv : lvalonmimaCard
{
	public int battleatk => Value1 + GrowCount * Value2;
	protected override IEnumerable<BattleAction> OnExpel(DieEventArgs args) => RunAsExpelling(OnExpelBody);

	private IEnumerable<BattleAction> OnExpelBody()
	{
		if (PlayCount == 0) yield break;
		NotifyActivating();
		yield return SacrificeAction(PlayCount);
	}
	protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition) => RunAsPlaying(() => ActionsBody(selector));

	private IEnumerable<BattleAction> ActionsBody(UnitSelector selector)
	{
		for (int i = 0; i < battleatk; i++)
		{
			if (!selector.SelectedEnemy.IsAlive) break;
			yield return AttackAction(selector, i == 0 ? GunName : "Instant");
		}
	}
}
