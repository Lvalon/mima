using System.Linq;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoLEntitySideloader.Attributes;
using LBoLEntitySideloader.CustomKeywords;
using lvalonmima.Cards;
using lvalonmima.Cards.Template;

namespace lvalonmima.StatusEffects
{
	public sealed class secreativeDef : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Special;
			return config;
		}
	}

	[EntityLogic(typeof(secreativeDef))]
	public sealed class secreative : StatusEffect
	{
		public override bool ForceNotShowDownText => true;
		protected override void OnAdded(Unit unit)
		{
			HandleOwnerEvent(Battle.CardPlayed, OnCardPlayed);
			HandleOwnerEvent(Battle.CardUsed, OnCardPlayed);
			HandleOwnerEvent(Battle.Player.TurnEnded, OnRoundEnded, GameEventPriority.Lowest);
			//HandleOwnerEvent(Battle.RoundEnded, OnRoundEnded, GameEventPriority.Lowest);
			HandleOwnerEvent(Battle.CardsAddedToHand, OnCardAdded);
			HandleOwnerEvent(Battle.CardsAddedToDiscard, OnCardAdded);
			HandleOwnerEvent(Battle.CardsAddedToDrawZone, OnCardAddedDraw);
			HandleOwnerEvent(Battle.CardsAddedToExile, OnCardAdded);
		}

		private void OnCardAddedDraw(CardsAddingToDrawZoneEventArgs args)
		{
			foreach (Card card in args.Cards)
			{
				if (card.HasCustomKeyword(nameof(seused)))
				{
					card.RemoveCustomKeyword(lvalonmimakeyword.Used);
				}
			}
		}

		private void OnCardAdded(CardsEventArgs args)
		{
			foreach (Card card in args.Cards)
			{
				if (card.HasCustomKeyword(nameof(seused)))
				{
					card.RemoveCustomKeyword(lvalonmimakeyword.Used);
				}
			}
		}

		private void OnCardPlayed(CardUsingEventArgs args)
		{
			if (!args.Card.HasCustomKeyword(nameof(seused)))
			{
				if (args.Card.Id != nameof(carddragonslay) && args.Card.CardType != CardType.Friend)
				{
					args.Card.AddCustomKeyword(lvalonmimakeyword.Used);
				}
			}
		}
		public override bool ShouldPreventCardUsage(Card card)
		{
			return card.HasCustomKeyword(nameof(seused));
		}
		public override string PreventCardUsageMessage
		{
			get
			{
				return TypeFactory<StatusEffect>.LocalizeProperty(Id, "seerror", true, true).RuntimeFormat(FormatWrapper);
			}
		}
		private void OnRoundEnded(GameEventArgs args)
		{
			foreach (Card card in Battle.EnumerateAllCards().Where(c => c.HasCustomKeyword(nameof(seused))))
			{
				card.RemoveCustomKeyword(lvalonmimakeyword.Used);
			}
		}
	}
}