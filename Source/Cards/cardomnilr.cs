using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using lvalonmima.GunName;
using LBoL.Core.Battle;
using LBoL.Core;
using LBoL.Core.StatusEffects;

namespace lvalonmima.Cards
{
	public sealed class cardomnilrDef : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig();
			config.Colors = new List<ManaColor>() { ManaColor.Red, ManaColor.Black };
			config.Cost = new ManaGroup() { Hybrid = 2, HybridColor = 7 };
			config.UpgradedCost = new ManaGroup() { Any = 1, Hybrid = 1, HybridColor = 7 };
			config.Rarity = Rarity.Uncommon;
			config.Type = CardType.Attack;
			config.TargetType = TargetType.RandomEnemy;
			config.IsPooled = false;
			config.HideMesuem = true;

			config.Damage = 4;
			config.UpgradedDamage = 6;

			config.GunName = GunNameID.GetGunFromId(4140);
			config.GunNameBurst = GunNameID.GetGunFromId(4140);

			config.Value1 = 1;
			config.Value2 = 4;
			config.UpgradedValue2 = 5;

			config.RelativeKeyword = Keyword.Expel;
			config.UpgradedRelativeKeyword = Keyword.Expel;

			config.RelativeEffects = new List<string>() { nameof(Charging) };
			config.UpgradedRelativeEffects = new List<string>() { nameof(Charging) };

			config.RelativeCards = new List<string>() { nameof(cardomniur), nameof(cardomniul) };
			config.UpgradedRelativeCards = new List<string>() { nameof(cardomniur) + "+", nameof(cardomniul) + "+" };

			config.Illustrator = "mefomefo";

			config.Index = CardIndexGenerator.GetUniqueIndex(config);
			return config;
		}
	}

	[EntityLogic(typeof(cardomnilrDef))]
	public sealed class cardomnilr : lvalonmimaCard
	{
		public int value3 => 3;
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
				GameRun.SetHpAndMaxHp(Battle.Player.Hp + Value1, Battle.Player.MaxHp + Value1, true);
				yield break;
			}
			finally { expelling = false; }
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			localplaying = true;
			try
			{
				yield return SacrificeAction(value3);
				if (Battle.BattleShouldEnd) { yield break; }
				yield return BuffAction<Charging>(value3, 0, 0);
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


