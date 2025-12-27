using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using lvalonmima.GunName;
using LBoL.Core.Battle;
using LBoL.Base.Extensions;
using LBoL.Core;
using System.Linq;
using LBoL.EntityLib.StatusEffects.Cirno;

namespace lvalonmima.Cards
{
	public sealed class cardicyroseDef : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig();
			config.Colors = new List<ManaColor>() { ManaColor.Blue, ManaColor.Black };
			config.Cost = new ManaGroup() { Blue = 1, Black = 1 };
			config.UpgradedCost = new ManaGroup() { Any = 1, Hybrid = 1, HybridColor = 4 };
			config.Rarity = Rarity.Uncommon;
			config.Type = CardType.Attack;
			config.TargetType = TargetType.SingleEnemy;

			config.Damage = 12;
			config.UpgradedDamage = 16;

			config.GunName = GunNameID.GetGunFromId(14040);
			config.GunNameBurst = GunNameID.GetGunFromId(14041);

			config.RelativeKeyword = Keyword.Expel;
			config.UpgradedRelativeKeyword = Keyword.Expel;

			config.RelativeEffects = new List<string>() { nameof(Cold) };
			config.UpgradedRelativeEffects = new List<string>() { nameof(Cold) };

			config.Value1 = 6;
			config.UpgradedValue1 = 8;
			config.Value2 = 1;

			config.Illustrator = "老邢";

			config.Index = CardIndexGenerator.GetUniqueIndex(config);
			return config;
		}
	}

	[EntityLogic(typeof(cardicyroseDef))]
	public sealed class cardicyrose : lvalonmimaCard
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
				int num = Battle.AllAliveEnemies.Where(e => e.HasStatusEffect<Cold>()).Count();
				GameRun.SetHpAndMaxHp(Battle.Player.Hp + Value2 * num, Battle.Player.MaxHp + Value2 * num, true);
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
				bool goon = false;
				if (selector.SelectedEnemy.IsAlive && selector.SelectedEnemy.HasStatusEffect<Cold>()) { goon = true; }
				yield return AttackAction(selector);
				if (goon)
				{
					yield return SacrificeAction(Value1);
					if (Battle.BattleShouldEnd || !selector.SelectedEnemy.IsAlive) { yield break; }
					yield return DebuffAction<Cold>(selector.SelectedEnemy, 1);
				}
			}
			finally
			{
				localplaying = false;
			}
		}
	}
}


