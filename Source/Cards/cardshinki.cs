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
using LBoL.Core.StatusEffects;

namespace lvalonmima.Cards
{
	public sealed class cardshinkiDef : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig();
			config.Colors = new List<ManaColor>() { ManaColor.White, ManaColor.Black, ManaColor.Red };
			config.Cost = new ManaGroup() { White = 1, Black = 3, Red = 1 };
			config.Rarity = Rarity.Rare;
			config.Type = CardType.Friend;
			config.TargetType = TargetType.Self;
			config.UpgradedKeywords = Keyword.Retain;

			config.RelativeKeyword = Keyword.TempMorph | Keyword.Replenish;
			config.UpgradedRelativeKeyword = Keyword.TempMorph | Keyword.Replenish;

			config.RelativeEffects = new List<string>() { nameof(seshinki), nameof(setranscendence), nameof(seabyss) };
			config.UpgradedRelativeEffects = new List<string>() { nameof(seshinki), nameof(setranscendence), nameof(seabyss) };

			config.Mana = new ManaGroup() { Any = 0 };

			config.Loyalty = 7;
			config.PassiveCost = 1;
			config.ActiveCost = -6;
			config.UltimateCost = -8;

			config.Value1 = 1;
			config.Value2 = 3;

			config.Illustrator = "hrusa";

			config.Index = CardIndexGenerator.GetUniqueIndex(config);
			return config;
		}
	}

	[EntityLogic(typeof(cardshinkiDef))]
	public sealed class cardshinki : lvalonmimaCard
	{
		public ManaGroup ManaW => new ManaGroup() { White = 1 };
		public ManaGroup ManaB => new ManaGroup() { Black = 1 };
		public ManaGroup ManaR => new ManaGroup() { Red = 1 };
		public ManaGroup Manas => ManaW + ManaB + ManaR;
		public int Value7 => 7;
		protected override void EnterBattle2(BattleController battle)
		{
			ReactBattleEvent(Battle.CardUsed, OnCardUsed);
		}

		private IEnumerable<BattleAction> OnCardUsed(CardUsingEventArgs args)
		{
			if (Battle.BattleShouldEnd || Zone != CardZone.Hand) { yield break; }
			yield return SacrificeAction(Value1);
			if (Battle.BattleShouldEnd) { yield break; }
			if (Battle.HandZone.Where(c => !c.IsUpgraded && c.CanUpgradeAndPositive).Count() > 0)
			{
				yield return new UpgradeCardAction(Battle.HandZone.Where(c => !c.IsUpgraded && c.CanUpgradeAndPositive).Sample(GameRun.BattleRng));
			}
			if (Battle.BattleShouldEnd) { yield break; }
			yield return BuffAction<TempFirepower>(Value1);
		}

		public override IEnumerable<BattleAction> OnTurnStartedInHand()
		{
			return GetPassiveActions();
		}

		public override IEnumerable<BattleAction> GetPassiveActions()
		{
			if (!Summoned || Battle.BattleShouldEnd)
			{
				yield break;
			}

			NotifyActivating();
			Loyalty += PassiveCost;
			for (int i = 0; i < Battle.FriendPassiveTimes; i++)
			{
				if (Battle.BattleShouldEnd)
				{
					break;
				}
				yield return PerformAction.Sfx("FairySupport");
				List<Card> list = Battle.EnumerateAllCardsButPlayingAreas().Where(c => c != this && c.Config.Colors.Intersect(Manas.EnumerateColors()).Any() && c.Zone != CardZone.Hand).ToList();
				if (IsUpgraded && list != null && list.Count > 0)
				{
					SelectCardInteraction interaction = new SelectCardInteraction(Value1, Value1, list)
					{
						Source = this
					};
					yield return new InteractionAction(interaction);
					Card card = interaction.SelectedCards.FirstOrDefault();
					if (card != null && card.Zone != CardZone.Hand)
					{
						yield return new MoveCardAction(card, CardZone.Hand);
					}
				}
			}
		}

		public override IEnumerable<BattleAction> SummonActions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			foreach (BattleAction item in base.SummonActions(selector, consumingMana, precondition))
			{
				yield return item;
			}
		}

		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			if (precondition == null || ((MiniSelectCardInteraction)precondition).SelectedCard.FriendToken == FriendToken.Active)
			{
				Loyalty += ActiveCost;
				if (Battle.BattleShouldEnd) { yield break; }
				yield return BuffAction<setranscendence>(Value1);
				if (Battle.BattleShouldEnd) { yield break; }
				yield return BuffAction<seabyss>(Value1);
				yield return SkillAnime;
			}
			else
			{
				Loyalty += UltimateCost;
				UltimateUsed = true;
				if (Battle.BattleShouldEnd) { yield break; }
				yield return BuffAction<seshinki>(Value2, 0, 0, Value7);
				yield return SkillAnime;
			}
		}
	}
}


