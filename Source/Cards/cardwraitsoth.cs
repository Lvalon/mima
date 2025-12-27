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
	public sealed class cardwraitsothDef : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig();
			config.Colors = new List<ManaColor>() { ManaColor.Red, ManaColor.Colorless };
			config.Cost = new ManaGroup() { Any = 1, Red = 2, Colorless = 1 };
			config.UpgradedCost = new ManaGroup() { Red = 1, Colorless = 1 };
			config.Rarity = Rarity.Rare;
			config.Mana = new ManaGroup() { Any = 1 };
			config.Type = CardType.Ability;
			config.TargetType = TargetType.Self;
			config.RelativeKeyword = Keyword.Purified;
			config.UpgradedRelativeKeyword = Keyword.Purified;
			config.RelativeEffects = new List<string>() { nameof(semburst), nameof(seunder) };
			config.UpgradedRelativeEffects = new List<string>() { nameof(semburst), nameof(seunder) };

			config.Value1 = 1;
			config.Value2 = 10;

			config.Illustrator = "camellia";

			config.Index = CardIndexGenerator.GetUniqueIndex(config);
			return config;
		}
	}

	[EntityLogic(typeof(cardwraitsothDef))]
	public sealed class cardwraitsoth : lvalonmimaCard.trigger25card
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new ApplyStatusEffectAction<semburst>(Battle.Player, Value2, 0, 0, 0);
			if (Battle.BattleShouldEnd) { yield break; }
			yield return new ApplyStatusEffectAction<sewraitsoth>(Battle.Player, Value1, 0, 0, 0);
		}
	}
}


