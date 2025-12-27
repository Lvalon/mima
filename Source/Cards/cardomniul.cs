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
using LBoL.Core.StatusEffects;

namespace lvalonmima.Cards
{
	public sealed class cardomniulDef : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig();
			config.Colors = new List<ManaColor>() { ManaColor.Red, ManaColor.Colorless };
			config.Cost = new ManaGroup() { Red = 1, Colorless = 2 };
			config.UpgradedCost = new ManaGroup() { Any = 2, Colorless = 1 };
			config.Rarity = Rarity.Uncommon;
			config.Type = CardType.Attack;
			config.TargetType = TargetType.RandomEnemy;
			config.IsPooled = false;
			config.HideMesuem = true;

			config.Damage = 4;
			config.UpgradedDamage = 6;

			config.GunName = GunNameID.GetGunFromId(4142); // 4531
			config.GunNameBurst = GunNameID.GetGunFromId(4142);

			config.Value1 = 2;
			config.UpgradedValue1 = 3;
			config.Value2 = 4;
			config.UpgradedValue2 = 5;

			config.RelativeKeyword = Keyword.Purify | Keyword.Expel;
			config.UpgradedRelativeKeyword = Keyword.Purify | Keyword.Expel;

			config.RelativeEffects = new List<string>() { nameof(Charging) };
			config.UpgradedRelativeEffects = new List<string>() { nameof(Charging) };

			config.RelativeCards = new List<string>() { nameof(cardomniur), nameof(cardomnilr) };
			config.UpgradedRelativeCards = new List<string>() { nameof(cardomniur) + "+", nameof(cardomnilr) + "+" };

			config.Illustrator = "mefomefo";

			config.Index = CardIndexGenerator.GetUniqueIndex(config);
			return config;
		}
	}

	[EntityLogic(typeof(cardomniulDef))]
	public sealed class cardomniul : lvalonmimaCard
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
				Card deckCardByInstanceId = GameRun.GetDeckCardByInstanceId(InstanceId);
				bool hasself = deckCardByInstanceId != null;
				foreach (BattleAction ba in RemoveSelf()) yield return ba;
				if (hasself)
				{
					yield return new AddCardsToDeckAction(Library.CreateCard<cardomnilr>(IsUpgraded));
				}
			}
			finally { expelling = false; }
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			localplaying = true;
			try
			{
				yield return BuffAction<Charging>(Value1, 0, 0);
				if (Battle.BattleShouldEnd) { yield break; }
				if (Battle.BattleMana.HasTrivial)
				{
					yield return ConvertManaAction.Purify(Battle.BattleMana, Value1);
				}
				for (int i = 0; i < Value2; i++)
				{
					if (Battle.BattleShouldEnd) { yield break; }
					yield return AttackAction(UnitSelector.RandomEnemy, i == 0 ? GunName : GunNameID.GetGunFromId(4531));
				}
			}
			finally { localplaying = false; }
		}
	}
}


