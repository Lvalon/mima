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
using LBoL.Core.Battle.Interactions;
using LBoLEntitySideloader.CustomKeywords;

namespace lvalonmima.Cards
{
	public sealed class cardgraspDef : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig();
			config.Colors = new List<ManaColor>() { ManaColor.Blue, ManaColor.Black, ManaColor.Colorless };
			config.Cost = new ManaGroup() { Blue = 1, Black = 1, Colorless = 1 };
			config.UpgradedCost = new ManaGroup() { Colorless = 1, Hybrid = 1, HybridColor = 4 };
			config.Rarity = Rarity.Uncommon;
			config.Type = CardType.Skill;
			config.TargetType = TargetType.Self;
			config.Keywords = Keyword.Exile;
			config.UpgradedKeywords = Keyword.Exile;
			config.RelativeKeyword = Keyword.Purified;
			config.UpgradedRelativeKeyword = Keyword.Purified;

			config.RelativeEffects = new List<string>() { nameof(seused), nameof(seunder) };
			config.UpgradedRelativeEffects = new List<string>() { nameof(seused), nameof(seunder) };

			config.Value1 = 6;
			config.UpgradedValue1 = 8;
			config.Value2 = 1;

			config.Illustrator = "核燃黑猫";

			config.Index = CardIndexGenerator.GetUniqueIndex(config);
			return config;
		}
	}

	[EntityLogic(typeof(cardgraspDef))]
	public sealed class cardgrasp : lvalonmimaCard.trigger10card
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return SacrificeAction(Value1);
			List<Card> list = new List<Card>();
			if (BepinexPlugin.u10)
			{
				SelectCardInteraction interaction = new SelectCardInteraction(0, Value2, Battle.DrawZoneToShow, SelectedCardHandling.DoNothing)
				{
					Source = this
				};
				yield return new InteractionAction(interaction, false);
				IReadOnlyList<Card> selectedCards = interaction.SelectedCards;

				if (selectedCards != null)
				{
					IReadOnlyList<Card> cards = selectedCards;
					if (cards.Count > 0)
					{
						foreach (Card card in cards)
						{
							if (Battle.BattleShouldEnd) { yield break; }
							list.Add(card);
						}
					}
				}
				if (Battle.BattleShouldEnd) { yield break; }
				if (Battle.DiscardZone.Count > 0)
				{
					SelectCardInteraction interaction2 = new SelectCardInteraction(0, Value2, Battle.DiscardZone)
					{
						Source = this
					};
					yield return new InteractionAction(interaction2);
					IReadOnlyList<Card> cards = interaction2.SelectedCards;
					if (cards.Count > 0)
					{
						foreach (Card card in cards)
						{
							if (Battle.BattleShouldEnd) { yield break; }
							list.Add(card);
						}
					}
				}
			}
			else
			{
				if (Battle.DrawZone.Count > 0)
				{
					list.Add(Battle.DrawZone.First());
				}
				if (Battle.DiscardZone.Count > 0)
				{
					list.Add(Battle.DiscardZone.Last());
				}
			}

			if (list != null && list.Count > 0)
			{
				foreach (Card card in list)
				{
					if (Battle.BattleShouldEnd) { yield break; }
					if (card.HasCustomKeyword(nameof(seused)))
					{
						card.RemoveCustomKeyword(lvalonmimakeyword.Used);
					}
					yield return new MoveCardAction(card, CardZone.Hand);
				}
				foreach (Card card in list)
				{
					if (Battle.BattleShouldEnd) { yield break; }
					if (!card.IsPurified && !card.IsXCost)
					{
						card.NotifyChanged();
						card.IsPurified = true;
					}
				}
			}
		}
	}
}


