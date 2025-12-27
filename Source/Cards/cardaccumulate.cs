using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core;
using lvalonmima.StatusEffects;

namespace lvalonmima.Cards
{
	public sealed class cardaccumulateDef : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig();
			config.Colors = new List<ManaColor>() { ManaColor.White, ManaColor.Colorless, ManaColor.Green };
			config.Cost = new ManaGroup() { Colorless = 1, Hybrid = 1, HybridColor = 3 };
			config.UpgradedCost = new ManaGroup() { Hybrid = 1, HybridColor = 3 };
			config.Rarity = Rarity.Uncommon;
			config.Mana = new ManaGroup() { White = 1, Green = 1 };
			config.Type = CardType.Ability;
			config.TargetType = TargetType.Self;
			config.RelativeKeyword = Keyword.Overdraft | Keyword.Purify;
			config.UpgradedRelativeKeyword = Keyword.Overdraft | Keyword.Purify;
			config.RelativeEffects = new List<string>() { nameof(seunder) };
			config.UpgradedRelativeEffects = new List<string>() { nameof(seunder) };
			config.RelativeCards = new List<string>() { nameof(cardpurediamond) };
			config.UpgradedRelativeCards = new List<string>() { nameof(cardpurediamond) };

			config.Value1 = 1;
			config.Value2 = 10;
			config.UpgradedValue2 = 20;

			config.Illustrator = "カズハル／硝酸";

			config.Index = CardIndexGenerator.GetUniqueIndex(config);
			return config;
		}
	}

	[EntityLogic(typeof(cardaccumulateDef))]
	public sealed class cardaccumulate : lvalonmimaCard.trigger25card
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new ApplyStatusEffectAction<seaccumulate>(Battle.Player, Value1, 0, Value2, 0);
		}
	}
}


