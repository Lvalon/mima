using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using lvalonmima.GunName;
using LBoL.Core.Battle;
using LBoL.Core;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Cirno;
using lvalonmima.StatusEffects;

namespace lvalonmima.Cards
{
	public sealed class cardmachinegunDef : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig();
			config.Colors = new List<ManaColor>() { ManaColor.Blue, ManaColor.Colorless };
			config.Cost = new ManaGroup() { Any = 2, Colorless = 1, Blue = 1 };
			config.UpgradedCost = new ManaGroup() { Any = 1, Colorless = 1, Blue = 1 };
			config.Rarity = Rarity.Uncommon;
			config.Type = CardType.Attack;
			config.TargetType = TargetType.RandomEnemy;

			config.Damage = 5;

			config.GunName = GunNameID.GetGunFromId(14080);
			config.GunNameBurst = GunNameID.GetGunFromId(14081);

			config.UpgradedRelativeKeyword = Keyword.Expel;

			config.RelativeEffects = new List<string>() { nameof(Cold), nameof(seunder) };
			config.UpgradedRelativeEffects = new List<string>() { nameof(Cold), nameof(seunder) };

			config.Value1 = 6;
			config.Value2 = 2;

			config.Illustrator = "酒醉的蝴蝶";

			config.Index = CardIndexGenerator.GetUniqueIndex(config);
			return config;
		}
	}

	[EntityLogic(typeof(cardmachinegunDef))]
	public sealed class cardmachinegun : lvalonmimaCard.trigger50card
	{
		public int Value4 => 1;
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
				if (IsUpgraded)
				{
					NotifyActivating();
					GameRun.SetHpAndMaxHp(Battle.Player.Hp + Value4, Battle.Player.MaxHp + Value4, true);
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
				if (BepinexPlugin.u50)
				{
					foreach (Unit unit in Battle.AllAliveEnemies)
					{
						if (!unit.IsAlive || Battle.BattleShouldEnd) { continue; }
						yield return DebuffAction<Cold>(unit, 1);
					}
				}
				else
				{
					if (Battle.BattleShouldEnd) { yield break; }
					yield return DebuffAction<Cold>(Battle.RandomAliveEnemy, 1);
				}
				for (int i = 0; i < Value1; i++)
				{
					if (Battle.BattleShouldEnd) { break; }
					Unit unit = Battle.RandomAliveEnemy;
					DamageInfo tmp = Damage;
					if (unit.HasStatusEffect<Cold>())
					{
						tmp.Damage *= Value2;
					}
					yield return AttackAction(unit, tmp, GunName);
				}
			}
			finally
			{
				localplaying = false;
			}
		}
	}
}


