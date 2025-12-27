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
	public sealed class cardindomitableDef : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig();
			config.Colors = new List<ManaColor>() { ManaColor.Black, ManaColor.Green, ManaColor.Colorless };
			config.Cost = new ManaGroup() { Black = 2, Green = 2, Colorless = 1 };
			config.UpgradedCost = new ManaGroup() { Black = 1, Green = 1, Colorless = 1 };
			config.Rarity = Rarity.Rare;
			config.Type = CardType.Skill;
			config.TargetType = TargetType.Self;
			config.Keywords = Keyword.Exile | Keyword.Retain;
			config.UpgradedKeywords = Keyword.Exile | Keyword.Retain;

			config.RelativeEffects = new List<string>() { nameof(seunder) };
			config.UpgradedRelativeEffects = new List<string>() { nameof(seunder) };

			config.Value1 = 1;

			config.Illustrator = "拒绝神绮99次";

			config.Index = CardIndexGenerator.GetUniqueIndex(config);
			return config;
		}
	}

	[EntityLogic(typeof(cardindomitableDef))]
	public sealed class cardindomitable : lvalonmimaCard.trigger25card
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new ApplyStatusEffectAction<seindomitable>(Battle.Player, 0, 0, 0, 0);
			if (BepinexPlugin.u25)
			{
				if (Battle.BattleShouldEnd) { yield break; }
				yield return new ForceKillAction(Battle.Player, Battle.Player);
			}
		}
	}
}


