using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core;
using lvalonmima.StatusEffects;
using LBoL.Core.StatusEffects;

namespace lvalonmima.Cards
{
	public sealed class cardextratickDef : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig();
			config.Colors = new List<ManaColor>() { ManaColor.Green, ManaColor.Colorless };
			config.Cost = new ManaGroup() { Any = 1, Green = 2, Colorless = 2 };
			config.Rarity = Rarity.Rare;
			config.Type = CardType.Ability;
			config.TargetType = TargetType.Self;
			config.RelativeKeyword = Keyword.NaturalTurn | Keyword.FollowAttack | Keyword.Purified;
			config.UpgradedRelativeKeyword = Keyword.NaturalTurn | Keyword.FollowAttack | Keyword.Purified;
			config.RelativeEffects = new List<string>() { nameof(ExtraTurn), nameof(seunder) };
			config.UpgradedRelativeEffects = new List<string>() { nameof(ExtraTurn), nameof(seunder) };

			config.Value1 = 1;
			config.Value2 = 1;
			config.UpgradedValue2 = 2;

			config.Illustrator = "turtle-kun";

			config.Index = CardIndexGenerator.GetUniqueIndex(config);
			return config;
		}
	}

	[EntityLogic(typeof(cardextratickDef))]
	public sealed class cardextratick : lvalonmimaCard.trigger50card
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new ApplyStatusEffectAction<seextratick>(Battle.Player, Value1, 0, Value2, 0);
		}
	}
}


