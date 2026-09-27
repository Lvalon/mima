using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using LBoL.Core.Battle;
using LBoL.Core;
using LBoL.EntityLib.StatusEffects.Cirno;

namespace lvalonmima.Cards;

public sealed class cardglacierDef : lvalonmimaCardTemplate
{
	public override CardConfig MakeConfig()
	{
		CardConfig config = GetCardDefaultConfig();
		config.Colors = [ManaColor.Blue];
		config.Cost = new ManaGroup() { Any = 1 };
		config.UpgradedCost = new ManaGroup() { Any = 0 };
		config.Rarity = Rarity.Uncommon;
		config.Type = CardType.Attack;
		config.TargetType = TargetType.SingleEnemy;

		config.Damage = 0;

		config.Keywords = Keyword.Echo | Keyword.Retain;
		config.UpgradedKeywords = Keyword.EternalEcho | Keyword.Retain;

		config.RelativeKeyword = config.UpgradedRelativeKeyword = Keyword.Expel;

		config.RelativeEffects = config.UpgradedRelativeEffects = [nameof(Cold)];

		config.Value1 = 1;
		config.UpgradedValue1 = 2;

		config.Illustrator = "五七七";

		config.Index = CardIndexGenerator.GetUniqueIndex(config);
		return config;
	}
}

[EntityLogic(typeof(cardglacierDef))]
public sealed class cardglacier : lvalonmimaCard
{
	protected override IEnumerable<BattleAction> OnExpel(DieEventArgs args) => RunAsExpelling(OnExpelBody);

	private IEnumerable<BattleAction> OnExpelBody()
	{
		NotifyActivating();
		for (int i = 0; i < Value1; i++)
		{
			if (Battle.BattleShouldEnd) break;
			yield return DebuffAction<Cold>(Battle.Player, 1);
		}
	}
	protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition) => RunAsPlaying(() => ActionsBody(selector, consumingMana, precondition));

	private IEnumerable<BattleAction> ActionsBody(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
	{
		yield return DebuffAction<Cold>(selector.SelectedEnemy, 1);
	}
}
