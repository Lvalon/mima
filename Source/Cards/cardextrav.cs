using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core;

namespace lvalonmima.Cards
{
	public sealed class cardextravDef : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig();
			config.Colors = new List<ManaColor>() { ManaColor.Blue, ManaColor.Black };
			config.Cost = new ManaGroup() { Any = 1, Hybrid = 1, HybridColor = 4 };
			config.Rarity = Rarity.Common;
			config.Type = CardType.Skill;
			config.TargetType = TargetType.Self;

			config.UpgradedKeywords = Keyword.Retain | Keyword.Replenish;

			config.Value1 = 4;

			config.Illustrator = "Radal";

			config.Index = CardIndexGenerator.GetUniqueIndex(config);
			return config;
		}
	}

	[EntityLogic(typeof(cardextravDef))]
	public sealed class cardextrav : lvalonmimaCard
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return SacrificeAction(Value1);
			if (Battle.BattleShouldEnd) { yield break; }
			yield return new DrawManyCardAction(Value1);
		}
	}
}


