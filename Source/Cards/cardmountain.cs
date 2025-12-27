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
	public sealed class cardmountainDef : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig();
			config.Colors = new List<ManaColor>() { ManaColor.Black, ManaColor.Green };
			config.Cost = new ManaGroup() { Any = 1, Hybrid = 1, HybridColor = 8 };
			config.UpgradedCost = new ManaGroup() { Any = 2 };
			config.Rarity = Rarity.Common;
			config.Type = CardType.Skill;
			config.TargetType = TargetType.Self;
			config.UpgradedKeywords = Keyword.Replenish;
			config.RelativeCards = new List<string>() { nameof(cardpurediamond) };
			config.UpgradedRelativeCards = new List<string>() { nameof(cardpurediamond) };

			config.Value1 = 2;

			config.Illustrator = "Radal";

			config.Index = CardIndexGenerator.GetUniqueIndex(config);
			return config;
		}
	}

	[EntityLogic(typeof(cardmountainDef))]
	public sealed class cardmountain : lvalonmimaCard
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new AddCardsToDrawZoneAction(Library.CreateCards<cardpurediamond>(Value1, false), DrawZoneTarget.Random, AddCardsType.Normal);
		}
	}
}


