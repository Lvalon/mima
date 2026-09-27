using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using lvalonmima.GunName;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.Battle.BattleActions;
using LBoL.Base.Extensions;
using LBoL.Core;
using System.Linq;

namespace lvalonmima.Cards;

public sealed class cardphantasmlaserDef : lvalonmimaCardTemplate
{
	public override CardConfig MakeConfig()
	{
		CardConfig config = GetCardDefaultConfig();
		config.Colors = [ManaColor.Green, ManaColor.Black];
		config.Cost = new ManaGroup() { Hybrid = 1, HybridColor = 8 };
		config.Rarity = Rarity.Common;
		config.Type = CardType.Attack;
		config.TargetType = TargetType.AllEnemies;

		config.Damage = 3;
		config.UpgradedDamage = 5;

		config.GunName = GunNameID.GetGunFromId(12010);
		config.GunNameBurst = GunNameID.GetGunFromId(12011);

		config.Keywords = Keyword.Accuracy | Keyword.Retain;
		config.UpgradedKeywords = config.Keywords | Keyword.Replenish;

		config.RelativeKeyword = config.UpgradedRelativeKeyword = Keyword.Expel;

		config.Value1 = 1;

		config.Illustrator = "JPF";

		config.Index = CardIndexGenerator.GetUniqueIndex(config);
		return config;
	}
}

[EntityLogic(typeof(cardphantasmlaserDef))]
public sealed class cardphantasmlaser : lvalonmimaCard
{
	protected override IEnumerable<BattleAction> OnExpel(DieEventArgs args) => RunAsExpelling(OnExpelBody);

	private IEnumerable<BattleAction> OnExpelBody()
	{
		NotifyActivating();
		List<Card> list = [.. GameRun.BaseDeck.Where(card2 => card2.CanUpgradeAndPositive)];
		if (list.Count <= 0)
			yield break;

		Card card = list.Sample(GameRun.GameRunEventRng);
		GameRun.UpgradeDeckCard(card);
		foreach (Card item in Battle.EnumerateAllCards())
		{
			if (item.InstanceId == card.InstanceId)
			{
				if (item.CanUpgrade && !Battle.BattleShouldEnd)
				{
					yield return new UpgradeCardAction(item);
				}
			}
		}
	}
	protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition) => RunAsPlaying(() => ActionsBody(selector, consumingMana, precondition));

	private IEnumerable<BattleAction> ActionsBody(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
	{
		yield return SacrificeAction(Value1);
		if (Battle.BattleShouldEnd) yield break;
		yield return AttackAction(selector);
	}
}
