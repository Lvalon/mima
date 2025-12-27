using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core;
using System.Linq;
using lvalonmima.StatusEffects;

namespace lvalonmima.Cards
{
	public sealed class cardtwelveDef : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig();
			config.Colors = new List<ManaColor>() { ManaColor.Colorless };
			config.Cost = new ManaGroup() { Any = 3, Colorless = 1 };
			config.UpgradedCost = new ManaGroup() { Any = 2, Colorless = 1 };
			config.Rarity = Rarity.Uncommon;
			config.Type = CardType.Skill;
			config.TargetType = TargetType.Self;
			config.Keywords = Keyword.Exile;
			config.UpgradedKeywords = Keyword.Exile | Keyword.Retain;
			config.RelativeKeyword = Keyword.Purified;
			config.UpgradedRelativeKeyword = Keyword.Purified;

			config.RelativeEffects = new List<string>() { nameof(seunder) };
			config.UpgradedRelativeEffects = new List<string>() { nameof(seunder) };

			config.Value1 = 1;

			config.Illustrator = "海源";

			config.Index = CardIndexGenerator.GetUniqueIndex(config);
			return config;
		}
	}

	[EntityLogic(typeof(cardtwelveDef))]
	public sealed class cardtwelve : lvalonmimaCard.trigger50card
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			foreach (Card card in Battle.HandZone.Where(c => c != this && !c.IsPurified && !c.IsXCost).ToList())
			{
				card.NotifyChanged();
				card.IsPurified = true;
			}
			if (BepinexPlugin.u50)
			{
				yield return new ForceKillAction(Battle.Player, Battle.Player);
			}
		}
	}
}


