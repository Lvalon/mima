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
using LBoL.Core.Battle.Interactions;
using lvalonmima.StatusEffects;

namespace lvalonmima.Cards
{
	public sealed class cardbirdqDef : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig();
			config.Colors = new List<ManaColor>() { ManaColor.Black, ManaColor.Colorless };
			config.Cost = new ManaGroup() { Black = 1, Colorless = 1 };
			config.UpgradedCost = new ManaGroup() { Black = 1, Any = 1 };
			config.Rarity = Rarity.Uncommon;
			config.Type = CardType.Skill;
			config.TargetType = TargetType.Nobody;
			config.Keywords = Keyword.Ethereal;
			config.UpgradedKeywords = Keyword.Replenish;
			config.RelativeKeyword = Keyword.Purified;
			config.UpgradedRelativeKeyword = Keyword.Purified;
			config.RelativeEffects = new List<string>() { nameof(seunder) };
			config.UpgradedRelativeEffects = new List<string>() { nameof(seunder) };
			config.RelativeCards = new List<string>() { nameof(cardpurediamond) };
			config.UpgradedRelativeCards = new List<string>() { nameof(cardpurediamond) };

			config.Value1 = 1;

			config.Illustrator = "keinelove";

			config.Index = CardIndexGenerator.GetUniqueIndex(config);
			return config;
		}
	}

	[EntityLogic(typeof(cardbirdqDef))]
	public sealed class cardbirdq : lvalonmimaCard.trigger50card
	{
		public override Interaction Precondition()
		{
			if (Battle.HandZone.Count(c => !c.IsPurified && !c.IsXCost && c != this) > 1)
			{
				return new SelectCardInteraction(0, Value1, Battle.HandZone.Where(c => !c.IsPurified && !c.IsXCost && c != this));
			}
			return null;
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			int extra = BepinexPlugin.u50 ? 1 : 0;
			if (precondition != null)
			{
				IReadOnlyList<Card> selectedCards = ((SelectCardInteraction)precondition).SelectedCards;
				if (selectedCards != null)
				{
					if (selectedCards.NotEmpty() && !selectedCards.FirstOrDefault().IsPurified && !selectedCards.FirstOrDefault().IsXCost)
					{
						selectedCards.FirstOrDefault().NotifyChanged();
						selectedCards.FirstOrDefault().IsPurified = true;
					}
				}
			}
			if (Battle.BattleShouldEnd) { yield break; }
			yield return new AddCardsToDrawZoneAction(Library.CreateCards<cardpurediamond>(Value1 + extra, false), DrawZoneTarget.Random, AddCardsType.Normal);
		}
	}
}


