using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using lvalonmima.GunName;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.Battle.BattleActions;
using LBoL.Base.Extensions;
using LBoL.Core;
using System.Linq;
using lvalonmima.StatusEffects;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace lvalonmima.Cards
{
	public sealed class cardignitionDef : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig();
			config.Colors = new List<ManaColor>() { ManaColor.Red, ManaColor.Green };
			config.Cost = new ManaGroup() { Any = 1, Hybrid = 2, HybridColor = 9 };
			config.Rarity = Rarity.Uncommon;
			config.Type = CardType.Attack;
			config.TargetType = TargetType.AllEnemies;

			config.Damage = 0;

			config.GunName = GunNameID.GetGunFromId(23010);
			config.GunNameBurst = GunNameID.GetGunFromId(23011);

			config.Value1 = 1;
			config.UpgradedValue1 = 2;
			config.Value2 = 3;

			config.Keywords = Keyword.FollowCard;
			config.UpgradedKeywords = Keyword.FollowCard;

			config.RelativeEffects = new List<string>() { nameof(Vulnerable), nameof(sesideload), nameof(semburst) };
			config.UpgradedRelativeEffects = new List<string>() { nameof(Vulnerable), nameof(sesideload), nameof(semburst) };

			config.RelativeKeyword = Keyword.Expel;
			config.UpgradedRelativeKeyword = Keyword.Expel;

			config.Illustrator = "菓しおり";

			config.Index = CardIndexGenerator.GetUniqueIndex(config);
			return config;
		}
	}

	[EntityLogic(typeof(cardignitionDef))]
	public sealed class cardignition : lvalonmimaCard
	{
		bool localplaying = false;
		bool expelling = false;
		public override bool playing
		{
			get
			{
				return localplaying || expelling;
			}
		}
		public override int AdditionalDamage => Battle?.Player.TryGetStatusEffect(out semburst se) == true ? se.Count : 0;
		public int svalue => 6;
		protected override IEnumerable<BattleAction> OnExpel(DieEventArgs args)
		{
			expelling = true;
			try
			{
				if (!Battle.Player.TryGetStatusEffect(out Charging se) || se.Level < svalue || Battle.HandZone.Where(c => c != this).Count() == 0)
				{
					yield break;
				}
				NotifyActivating();
				List<cardignition> list = Library.CreateCards<cardignition>(2, IsUpgraded).ToList();
				cardignition cardignition = list[0];
				cardignition cardignition2 = list[1];
				cardignition.ChoiceCardIndicator = 1;
				cardignition2.ChoiceCardIndicator = 2;
				cardignition.SetBattle(Battle);
				cardignition.Keywords = Keyword.None;
				cardignition2.SetBattle(Battle);
				cardignition2.Keywords = Keyword.None;
				MiniSelectCardInteraction interaction = new MiniSelectCardInteraction(list, false, false, false);
				yield return new InteractionAction(interaction);
				Card card = interaction?.SelectedCard;
				if (card != null && card.ChoiceCardIndicator == 2) // ExtraDescription2
				{
					if (se.Level == svalue)
					{
						yield return new RemoveStatusEffectAction(se);
					}
					else
					{
						se.Level -= svalue;
					}
					SelectCardInteraction interaction2 = new SelectCardInteraction(1, 1, Battle.HandZone)
					{
						Source = this
					};
					yield return new InteractionAction(interaction2);
					IReadOnlyList<Card> cards = interaction2.SelectedCards;
					if (cards.Count > 0)
					{
						foreach (Card card2 in cards)
						{
							GameRun.SetHpAndMaxHp(Battle.Player.Hp + card2.ConfigCost.Amount, Battle.Player.MaxHp + card2.ConfigCost.Amount, true);
							yield return new ExileCardAction(card2);
						}
					}
				}
				yield break;
			}
			finally
			{
				expelling = false;
			}
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			localplaying = true;
			try
			{
				foreach (Unit unit in Battle.AllAliveEnemies)
				{
					if (!unit.IsAlive || Battle.BattleShouldEnd) { continue; }
					yield return DebuffAction<Vulnerable>(unit, 0, Value1);
				}
				if (Battle.BattleShouldEnd) { yield break; }
				yield return BuffAction<Charging>(Value2);
				if (IsUpgraded && Battle.AllAliveEnemies.Count() > 0)
				{
					yield return BuffAction<TempFirepower>(Value2);
				}
				if (Battle.BattleShouldEnd) { yield break; }
				yield return BuffAction<semburst>(Value2);
				if (Battle.BattleShouldEnd) { yield break; }
				yield return AttackAction(selector);
			}
			finally
			{
				localplaying = false;
			}
		}
	}
}


