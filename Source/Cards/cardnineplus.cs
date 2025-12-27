using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.Battle.BattleActions;
using LBoL.Base.Extensions;
using LBoL.Core;
using System.Linq;
using System;
using lvalonmima.StatusEffects;
using LBoL.Core.StatusEffects;

namespace lvalonmima.Cards
{
	public sealed class cardnineplusDef : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig();
			config.Colors = new List<ManaColor>() { ManaColor.Blue, ManaColor.Red };
			config.Cost = new ManaGroup() { Hybrid = 3, HybridColor = 5 };
			config.Rarity = Rarity.Uncommon;
			config.Type = CardType.Skill;
			config.TargetType = TargetType.Nobody;
			config.Keywords = Keyword.Exile;
			config.UpgradedKeywords = Keyword.Exile;
			config.RelativeEffects = new List<string>() { nameof(semburst) };
			config.UpgradedRelativeEffects = new List<string>() { nameof(semburst), nameof(Charging) };

			config.Value1 = 1;
			config.Value2 = 9;

			config.Illustrator = "r0g0b0";

			config.Index = CardIndexGenerator.GetUniqueIndex(config);
			return config;
		}
	}

	[EntityLogic(typeof(cardnineplusDef))]
	public sealed class cardnineplus : lvalonmimaCard
	{
		public int Value10 => 10;
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new ApplyStatusEffectAction<semburst>(Battle.Player, Value2, 0, 0, 0);
			DrawManyCardAction drawAction = new DrawManyCardAction(Value10);
			yield return drawAction;
			if (Battle.BattleShouldEnd) { yield break; }
			IReadOnlyList<Card> drawnCards = drawAction.DrawnCards;
			int num = drawnCards.Count((Card card) => card.Config.Colors.Contains(ManaColor.Red));
			if (num > 0)
			{
				yield return new ApplyStatusEffectAction<Charging>(Battle.Player, Value1 * num, 0, 0, 0, 0);
			}
		}
	}
}


