using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using lvalonmima.GunName;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core;
using System.Linq;
using LBoL.Core.StatusEffects;

namespace lvalonmima.Cards
{
	public sealed class cardflameonDef : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig();
			config.Colors = new List<ManaColor>() { ManaColor.Red };
			config.Cost = new ManaGroup() { Any = 1, Red = 1 };
			config.Rarity = Rarity.Common;
			config.Type = CardType.Attack;
			config.TargetType = TargetType.SingleEnemy;

			config.Damage = 7;

			config.GunName = GunNameID.GetGunFromId(12140);
			config.GunNameBurst = GunNameID.GetGunFromId(12141);

			config.Value1 = 4;
			config.Value2 = 4;
			config.UpgradedValue2 = 5;

			config.RelativeEffects = new List<string>() { nameof(Charging) };
			config.UpgradedRelativeEffects = new List<string>() { nameof(Charging), nameof(Vulnerable) };

			config.RelativeKeyword = Keyword.Expel;
			config.UpgradedRelativeKeyword = Keyword.Expel;

			config.Illustrator = "kazetuki";

			config.Index = CardIndexGenerator.GetUniqueIndex(config);
			return config;
		}
	}

	[EntityLogic(typeof(cardflameonDef))]
	public sealed class cardflameon : lvalonmimaCard
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
		protected override int BaseValue3 => 16;
		protected override int BaseUpgradedValue3 => 20;
		protected override IEnumerable<BattleAction> OnExpel(DieEventArgs args)
		{
			expelling = true;
			try
			{
				NotifyActivating();
				yield return SacrificeAction(Value2);
				yield return new GainPowerAction(Value3);
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
				yield return BuffAction<Charging>(Value1, 0, 0);
				if (IsUpgraded && selector.SelectedEnemy.IsAlive && Battle.AllAliveEnemies.Count() > 0)
				{
					yield return new ApplyStatusEffectAction<Vulnerable>(selector.SelectedEnemy, 0, 1, 0, 0);
				}
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


