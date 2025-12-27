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

namespace lvalonmima.Cards
{
	public sealed class carddragonslayDef : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig();
			config.Colors = new List<ManaColor>() { ManaColor.Black, ManaColor.Green };
			config.Cost = new ManaGroup() { Black = 12, Green = 12 };
			config.IsXCost = true;
			config.Rarity = Rarity.Rare;
			config.Type = CardType.Attack;
			config.TargetType = TargetType.SingleEnemy;
			config.FindInBattle = false;

			config.Damage = 0;

			config.GunName = GunNameID.GetGunFromId(15210);
			config.GunNameBurst = GunNameID.GetGunFromId(15210);

			config.Value1 = 6;
			config.Value2 = 2;

			config.Keywords = Keyword.Retain;
			config.UpgradedKeywords = Keyword.Retain | Keyword.FollowCard;

			config.RelativeKeyword = Keyword.Purified;
			config.UpgradedRelativeKeyword = Keyword.Purified;

			config.RelativeEffects = new List<string>() { nameof(seused) };
			config.UpgradedRelativeEffects = new List<string>() { nameof(seused) };

			config.Illustrator = "れらりん/神依レラ";

			config.Index = CardIndexGenerator.GetUniqueIndex(config);
			return config;
		}
	}

	[EntityLogic(typeof(carddragonslayDef))]
	public sealed class carddragonslay : lvalonmimaCard
	{
		public int Value6 => 36;
		int selfdmg = 0;
		public int self => Battle == null ? Value1 : selfdmg;
		int dmg = 0;
		public int damage => Battle == null ? Value6 : dmg;
		protected override void EnterBattle2(BattleController battle)
		{
			selfdmg = Value1;
			dmg = Value6;
			ReactBattleEvent(Battle.CardPlayed, OnCardPlayed);
			ReactBattleEvent(Battle.CardUsed, OnCardPlayed);
		}
		private IEnumerable<BattleAction> OnCardPlayed(CardUsingEventArgs args)
		{
			if (args.Card == this && (Zone == CardZone.Draw || Zone == CardZone.Discard || Zone == CardZone.Exile))
			{
				yield return new MoveCardAction(this, CardZone.Hand);
			}
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return SacrificeAction(self);
			if (selector.SelectedEnemy != null && selector.SelectedEnemy.IsAlive && Battle.AllAliveEnemies.Count() > 0)
			{
				if (selector.SelectedEnemy.Hp < damage)
				{
					yield return new ForceKillAction(Battle.Player, selector.SelectedEnemy);
				}
				else
				{
					yield return DamageAction.LoseLife(selector.SelectedEnemy, damage, GunName);
				}
			}

			if (Battle.HandZone.Where(c => c != this).Count() > 2 && Battle.AllAliveEnemies.Count() > 0)
			{
				SelectCardInteraction interaction = new SelectCardInteraction(Value2, Value2, Battle.HandZone.Where(c => c != this), SelectedCardHandling.DoNothing)
				{
					Source = this
				};
				yield return new InteractionAction(interaction, false);
				IReadOnlyList<Card> selectedCards = interaction.SelectedCards;

				if (selectedCards != null)
				{
					yield return new ExileManyCardAction(selectedCards);
					if (Battle.BattleShouldEnd) { yield break; }
					if (selfdmg < int.MaxValue / 2)
					{
						selfdmg *= Value2;
					}
					if (dmg < int.MaxValue / 2)
					{
						dmg *= Value2;
					}
				}
			}
			else
			{
				bool qualifies = Battle.HandZone.Where(c => c != this).Count() == 2;
				yield return new ExileManyCardAction(Battle.HandZone.Where(c => c != this));
				if (Battle.BattleShouldEnd) { yield break; }
				if (qualifies || IsUpgraded)
				{
					if (selfdmg < int.MaxValue / 2)
					{
						selfdmg *= Value2;
					}
					if (dmg < int.MaxValue / 2)
					{
						dmg *= Value2;
					}
				}
			}
			yield return new MoveCardAction(this, CardZone.Hand);
		}
	}
}


