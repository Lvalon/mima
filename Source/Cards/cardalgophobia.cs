using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core;
using lvalonmima.StatusEffects;
using LBoL.EntityLib.StatusEffects.Others;

namespace lvalonmima.Cards
{
	public sealed class cardalgophobiaDef : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig();
			config.Colors = new List<ManaColor>() { ManaColor.Green, ManaColor.Black };
			config.Cost = new ManaGroup() { Black = 1, Green = 1, Hybrid = 1, HybridColor = 8 };
			config.Rarity = Rarity.Rare;
			config.Type = CardType.Ability;
			config.TargetType = TargetType.Self;
			config.Value1 = 1;
			config.UpgradedRelativeEffects = new List<string>() { nameof(Poison) };

			config.Illustrator = "camellia";

			config.Index = CardIndexGenerator.GetUniqueIndex(config);
			return config;
		}
	}

	[EntityLogic(typeof(cardalgophobiaDef))]
	public sealed class cardalgophobia : lvalonmimaCard
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new ApplyStatusEffectAction<sealgophobia>(Battle.Player, Value1, 0, 0, 0);
			if (IsUpgraded)
			{
				if (Battle.BattleShouldEnd) { yield break; }
				yield return new ApplyStatusEffectAction<sealgophobia2>(Battle.Player, Value1, 0, 0, 0);
			}
		}
	}
}


