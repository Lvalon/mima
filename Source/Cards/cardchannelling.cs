using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using LBoL.Core.Battle;
using LBoL.Core;
using LBoL.EntityLib.StatusEffects.Sakuya;

namespace lvalonmima.Cards
{
	public sealed class cardchannellingDef : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig();
			config.Colors = new List<ManaColor>() { ManaColor.Blue, ManaColor.White };
			config.Cost = new ManaGroup() { Any = 2, Hybrid = 1, HybridColor = 0 };
			config.Rarity = Rarity.Common;
			config.Type = CardType.Skill;
			config.TargetType = TargetType.Self;
			config.RelativeEffects = new List<string>() { nameof(TimeAuraSe) };
			config.UpgradedRelativeEffects = new List<string>() { nameof(TimeAuraSe) };

			config.Value1 = 3;
			config.UpgradedValue1 = 5;
			config.Value2 = 8;
			config.UpgradedValue2 = 10;

			config.Illustrator = "camellia";

			config.Index = CardIndexGenerator.GetUniqueIndex(config);
			return config;
		}
	}

	[EntityLogic(typeof(cardchannellingDef))]
	public sealed class cardchannelling : lvalonmimaCard
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			if (Battle.BattleShouldEnd) { yield break; }
			yield return SacrificeAction(Value1);
			if (Battle.BattleShouldEnd) { yield break; }
			yield return BuffAction<TimeAuraSe>(Value2, 0, 0, 0);
		}
	}
}


