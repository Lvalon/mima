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

namespace lvalonmima.Cards
{
	public sealed class cardenterzoneDef : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig();
			config.Colors = new List<ManaColor>() { ManaColor.Green, ManaColor.Black };
			config.Cost = new ManaGroup() { Hybrid = 2, HybridColor = 8 };
			config.UpgradedCost = new ManaGroup() { Any = 1, Hybrid = 1, HybridColor = 8 };
			config.Rarity = Rarity.Common;
			config.Type = CardType.Skill;
			config.TargetType = TargetType.Self;

			config.Value1 = 1;
			config.Value2 = 2;
			config.UpgradedValue2 = 1;

			config.Keywords = Keyword.Exile;
			config.UpgradedKeywords = Keyword.Exile;
			config.RelativeKeyword = Keyword.TempMorph | Keyword.Overdraft;
			config.UpgradedRelativeKeyword = Keyword.TempMorph | Keyword.Overdraft;

			config.Illustrator = "camellia";

			config.Index = CardIndexGenerator.GetUniqueIndex(config);
			return config;
		}
	}

	[EntityLogic(typeof(cardenterzoneDef))]
	public sealed class cardenterzone : lvalonmimaCard
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			foreach (Card card in Battle.HandZone.Where(c => !c.IsXCost && c.Cost.Amount > 0 && c != this))
			{
				if (Battle.BattleShouldEnd) { yield break; }
				card.NotifyActivating();
				card.DecreaseTurnCost(ManaGroup.FromComponents(card.Cost.EnumerateComponents().SampleManyOrAll(Value1, GameRun.BattleRng)));
			}
			if (Battle.BattleShouldEnd) { yield break; }
			yield return new LockRandomTurnManaAction(Value2);
			yield break;
		}
	}
}


