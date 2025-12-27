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
	public sealed class cardperlereinoDef : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig();
			config.Colors = new List<ManaColor>() { ManaColor.White, ManaColor.Blue, ManaColor.Black, ManaColor.Green };
			config.Cost = new ManaGroup() { White = 1, Green = 1, Black = 1, Blue = 1 };
			config.Rarity = Rarity.Rare;
			config.Type = CardType.Ability;
			config.TargetType = TargetType.Self;
			config.Value1 = 1;
			config.Value2 = 1000;
			config.RelativeEffects = new List<string>() { nameof(setranscendence) };
			config.UpgradedRelativeEffects = new List<string>() { nameof(setranscendence) };

			config.Illustrator = "camellia";

			config.Index = CardIndexGenerator.GetUniqueIndex(config);
			return config;
		}
	}

	[EntityLogic(typeof(cardperlereinoDef))]
	public sealed class cardperlereino : lvalonmimaCard
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new ApplyStatusEffectAction<seperlereino>(Battle.Player, Value1, 0, 0, 0);
			if (Battle.BattleShouldEnd) { yield break; }
			yield return new ApplyStatusEffectAction<setranscendence>(Battle.Player, Value1, 0, 0, 0);
			if (Battle.BattleShouldEnd) { yield break; }
			if (IsUpgraded)
			{
				if (Battle.BattleShouldEnd) { yield break; }
				yield return new ApplyStatusEffectAction<sefuckyou700>(Battle.Player, 0, 0, 0, 0);
			}
		}
	}
}


