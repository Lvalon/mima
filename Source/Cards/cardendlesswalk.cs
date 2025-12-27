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

namespace lvalonmima.Cards
{
	public sealed class cardendlesswalkDef : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig();
			config.Colors = new List<ManaColor>() { ManaColor.Green, ManaColor.Colorless };
			config.Cost = new ManaGroup() { Colorless = 1, Green = 1 };
			config.UpgradedCost = new ManaGroup() { Any = 1 };
			config.Rarity = Rarity.Uncommon;
			config.Type = CardType.Skill;
			config.TargetType = TargetType.Nobody;
			config.Keywords = Keyword.Echo;
			config.UpgradedKeywords = Keyword.Echo;
			config.RelativeKeyword = Keyword.Purified | Keyword.Overdraft;
			config.UpgradedRelativeKeyword = Keyword.Purified | Keyword.Overdraft;

			config.Value1 = 1;

			config.Illustrator = "donjuan";

			config.Index = CardIndexGenerator.GetUniqueIndex(config);
			return config;
		}
	}

	[EntityLogic(typeof(cardendlesswalkDef))]
	public sealed class cardendlesswalk : lvalonmimaCard
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			DrawManyCardAction drawAction = new DrawManyCardAction(Value1);
			yield return drawAction;
			IReadOnlyList<Card> drawnCards = drawAction.DrawnCards;
			if (drawnCards != null && drawnCards.Count > 0)
			{

				foreach (Card card in drawnCards.Where(c => !c.IsXCost))
				{
					card.NotifyChanged();
					card.IsPurified = true;
				}
			}
			if (Battle.BattleShouldEnd) { yield break; }
			yield return new LockRandomTurnManaAction(Value1);
		}
	}
}


