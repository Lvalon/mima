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
using lvalonmima.StatusEffects;
using LBoL.Core.Battle.Interactions;
using LBoLEntitySideloader.CustomKeywords;

namespace lvalonmima.Cards
{
	public sealed class cardstarliegeDef : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig();
			config.Colors = new List<ManaColor>() { ManaColor.White, ManaColor.Red };
			config.Cost = new ManaGroup() { Any = 2, Hybrid = 1, HybridColor = 2 };
			config.Rarity = Rarity.Uncommon;
			config.Type = CardType.Skill;
			config.TargetType = TargetType.Nobody;
			config.Keywords = Keyword.Exile;
			config.RelativeKeyword = Keyword.Retain;
			config.UpgradedRelativeKeyword = Keyword.Retain;
			config.Value1 = 1;
			config.RelativeEffects = new List<string>() { nameof(selinked) };
			config.UpgradedRelativeEffects = new List<string>() { nameof(selinked) };
			config.RelativeCards = new List<string>() { nameof(cardpurediamond) };
			config.UpgradedRelativeCards = new List<string>() { nameof(cardpurediamond) };

			config.Illustrator = "神楽坂いろは";

			config.Index = CardIndexGenerator.GetUniqueIndex(config);
			return config;
		}
	}

	[EntityLogic(typeof(cardstarliegeDef))]
	public sealed class cardstarliege : lvalonmimaCard
	{
		protected override void EnterBattle2(BattleController battle)
		{
			ReactBattleEvent(Battle.CardExiled, OnCardExiled);
		}

		private IEnumerable<BattleAction> OnCardExiled(CardEventArgs args)
		{
			if (args.Card.HasCustomKeyword(nameof(selinked)))
			{
				foreach (Card card in Battle.EnumerateAllCards().ToList().Where(c => c.HasCustomKeyword(nameof(selinked))
				&& c != args.Card
				&& (c.Zone == CardZone.Hand || c.Zone == CardZone.Discard || c.Zone == CardZone.Draw)))
				{
					yield return new ExileCardAction(card);
				}
			}
		}
		public override Interaction Precondition()
		{
			if (Battle.HandZone.Count > 1)
			{
				return new SelectCardInteraction(Value1, Value1, Battle.HandZone.Where(c => c != this));
			}
			return null;
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			if (precondition != null)
			{
				IReadOnlyList<Card> selectedCards = ((SelectCardInteraction)precondition).SelectedCards;
				if (selectedCards != null)
				{
					foreach (Card card in Battle.EnumerateAllCards().Where(c => c.HasCustomKeyword(nameof(selinked))).ToList())
					{
						card.RemoveCustomKeyword(lvalonmimakeyword.Linked);
					}
					selectedCards.FirstOrDefault().AddCustomKeyword(lvalonmimakeyword.Linked);
					Card tmp = selectedCards.FirstOrDefault();
					if (!tmp.IsRetain)
					{
						tmp.NotifyChanged();
						tmp.IsRetain = true;
					}
				}
			}
			if (Battle.ExileZone.Count > 0)
			{
				SelectCardInteraction interaction = new SelectCardInteraction(Value1, Value1, Battle.ExileZone)
				{
					Source = this
				};
				yield return new InteractionAction(interaction);
				if (interaction.SelectedCards.NotEmpty())
				{
					if (Battle.EnumerateAllCards().Any(c => c.HasCustomKeyword(nameof(selinked))))
					{
						interaction.SelectedCards.FirstOrDefault().AddCustomKeyword(lvalonmimakeyword.Linked);
					}
					yield return new MoveCardAction(interaction.SelectedCards.FirstOrDefault(), CardZone.Hand);
				}
			}
			yield return new AddCardsToDrawZoneAction(Library.CreateCards<cardpurediamond>(Value1, false), DrawZoneTarget.Random, AddCardsType.Normal);
		}
	}
}


