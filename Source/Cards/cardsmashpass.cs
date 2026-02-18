using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using lvalonmima.GunName;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core;
using System.Linq;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Units;
using lvalonmima.StatusEffects;

namespace lvalonmima.Cards
{
	public sealed class cardsmashpassDef : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig();
			config.Colors = new List<ManaColor>() { ManaColor.Green, ManaColor.Colorless };
			config.Cost = new ManaGroup() { Any = 2, Green = 2, Colorless = 1 };
			config.Rarity = Rarity.Rare;
			config.Type = CardType.Attack;
			config.TargetType = TargetType.SingleEnemy;

			config.Damage = 14;

			config.GunName = GunNameID.GetGunFromId(6048);
			config.GunNameBurst = GunNameID.GetGunFromId(6048);

			config.Mana = new ManaGroup() { Green = 1, Colorless = 1 };

			config.Value1 = 2;
			config.UpgradedValue1 = 3;
			config.Value2 = 1;
			config.UpgradedValue2 = 2;

			config.UpgradedKeywords = Keyword.FollowCard;

			config.RelativeEffects = new List<string>() { nameof(seunder) };
			config.UpgradedRelativeEffects = new List<string>() { nameof(seunder) };

			config.RelativeKeyword = Keyword.Expel | Keyword.FollowAttack;
			config.UpgradedRelativeKeyword = Keyword.Expel | Keyword.FollowAttack;

			config.Illustrator = "くまばち";

			config.Index = CardIndexGenerator.GetUniqueIndex(config);
			return config;
		}
	}

	[EntityLogic(typeof(cardsmashpassDef))]
	public sealed class cardsmashpass : lvalonmimaCard.trigger25card
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
		protected override IEnumerable<BattleAction> OnExpel(DieEventArgs args)
		{
			expelling = true;
			try
			{
				NotifyActivating();
				EnemyUnit tmp = Battle.RandomAliveEnemy;
				foreach (BattleAction ba in effect1(new UnitSelector(tmp))) yield return ba;
				if (Battle.AllAliveEnemies.Count() > 0)
				{
					if (!tmp.IsAlive)
					{
						tmp = Battle.RandomAliveEnemy;
					}
					foreach (BattleAction ba in effect2(new UnitSelector(tmp))) yield return ba;
				}
				GameRun.SetHpAndMaxHp(Battle.Player.Hp + Value2, Battle.Player.MaxHp + Value2, true);
			}
			finally
			{
				expelling = false;
			}
		}
		private IEnumerable<BattleAction> effect1(UnitSelector selector)
		{
			if (Battle.BattleShouldEnd) { yield break; }
			yield return AttackAction(selector);
			if (Battle.BattleShouldEnd) { yield break; }
			yield return new GainManaAction(Mana);
		}
		private IEnumerable<BattleAction> effect2(UnitSelector selector)
		{
			if (Battle.BattleShouldEnd) { yield break; }
			yield return new FollowAttackAction(selector, Value1);
		}
		public override Interaction Precondition()
		{
			if (BepinexPlugin.u25)
			{
				return null;
			}
			List<cardsmashpass> list = Library.CreateCards<cardsmashpass>(2, IsUpgraded).ToList();
			cardsmashpass cardsmashpass = list[0];
			cardsmashpass cardsmashpass2 = list[1];
			cardsmashpass.ChoiceCardIndicator = 1;
			cardsmashpass2.ChoiceCardIndicator = 2;
			cardsmashpass.SetBattle(Battle);
			cardsmashpass.Keywords = Keyword.None;
			cardsmashpass2.SetBattle(Battle);
			cardsmashpass2.Keywords = Keyword.None;
			return new MiniSelectCardInteraction(list, false, false, false);
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			localplaying = true;
			try
			{
				if (BepinexPlugin.u25)
				{
					foreach (BattleAction ba in effect1(selector)) yield return ba;
					foreach (BattleAction ba in effect2(selector)) yield return ba;
				}
				else
				{
					MiniSelectCardInteraction miniSelectCardInteraction = (MiniSelectCardInteraction)precondition;
					Card card = (miniSelectCardInteraction != null) ? miniSelectCardInteraction.SelectedCard : null;
					if (card != null && card.ChoiceCardIndicator == 1) // ExtraDescription1
					{
						foreach (BattleAction ba in effect1(selector)) yield return ba;
					}
					if (card != null && card.ChoiceCardIndicator == 2) // ExtraDescription2
					{
						foreach (BattleAction ba in effect2(selector)) yield return ba;
					}
				}

				NotifyActivating();
				EnemyUnit tmp = Battle.RandomAliveEnemy;
				foreach (BattleAction ba in effect1(new UnitSelector(tmp))) yield return ba;
				if (Battle.AllAliveEnemies.Count() > 0)
				{
					if (!tmp.IsAlive)
					{
						tmp = Battle.RandomAliveEnemy;
					}
					foreach (BattleAction ba in effect2(new UnitSelector(tmp))) yield return ba;
				}
				GameRun.SetHpAndMaxHp(Battle.Player.Hp + Value2, Battle.Player.MaxHp + Value2, true);
			}
			finally
			{
				localplaying = false;
			}
		}
	}
}


