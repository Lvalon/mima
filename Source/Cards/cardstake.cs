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
using System;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.StatusEffects;

namespace lvalonmima.Cards;

public sealed class cardstakeDef : lvalonmimaCardTemplate
{
	public override CardConfig MakeConfig()
	{
		CardConfig config = GetCardDefaultConfig();
		config.Colors = [ManaColor.Green, ManaColor.Red];
		config.Cost = new ManaGroup() { Green = 1, Red = 1 };
		config.Rarity = Rarity.Uncommon;
		config.Type = CardType.Skill;
		config.TargetType = TargetType.Nobody;
		config.UpgradedKeywords = Keyword.Echo;
		config.RelativeKeyword = config.UpgradedRelativeKeyword = Keyword.Overdraft;
		config.Value1 = 1;

		config.Illustrator = "ぱじ";

		config.Index = CardIndexGenerator.GetUniqueIndex(config);
		return config;
	}
}

[EntityLogic(typeof(cardstakeDef))]
public sealed class cardstake : lvalonmimaCard
{
	public override Interaction Precondition()
	{
		if (Battle.HandZone.Count > 1 || (IsUpgraded && (Battle.ExileZone.Count > 0 || Battle.DiscardZone.Count > 0)))
			return new SelectCardInteraction(Value1, Value1, IsUpgraded ? Battle.EnumerateAllCardsButExile().Where(c => c != this) : Battle.HandZone.Where(c => c != this));
		return null;
	}
	protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
	{
		IEnumerable<ManaColor> cardcolor = [];
		if (precondition != null)
		{
			IReadOnlyList<Card> selectedCards = ((SelectCardInteraction)precondition).SelectedCards;
			Card card = selectedCards?.FirstOrDefault();
			if (card != null)
			{
				cardcolor = card.Config.Colors;
				yield return new ExileCardAction(card);
				if (card.ConfigCost.Amount > 0)
				{
					if (Battle.BattleShouldEnd) yield break;
					yield return new LockRandomTurnManaAction(card.ConfigCost.Amount);
					if (Battle.BattleShouldEnd) yield break;
					yield return new ApplyStatusEffectAction<Charging>(Battle.Player, card.ConfigCost.Amount, 0, 0, 0);
				}
			}
		}
		List<Card> pass = [.. Battle.EnumerateAllCards().Where(c => c.Zone != CardZone.Hand)
		.Where(c => c != this && (
			c.Config.Colors.Intersect(cardcolor).Any()
			|| c.Config.Colors.OrderBy(x => x).SequenceEqual(cardcolor.OrderBy(x => x))
			|| (c.Config.Colors.Contains(ManaColor.Colorless) && !cardcolor.Any())
			|| (!c.Config.Colors.Any() && cardcolor.Contains(ManaColor.Colorless))))];

		if (pass.Count > 0 && precondition != null)
		{
			SelectCardInteraction interaction = new(Value1, Value1, pass)
			{
				Source = this
			};
			yield return new InteractionAction(interaction);
			Card selected = interaction.SelectedCards.FirstOrDefault();
			if (selected != null)
				yield return new MoveCardAction(selected, CardZone.Hand);
		}
	}
}
