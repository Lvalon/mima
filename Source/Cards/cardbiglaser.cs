using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using lvalonmima.GunName;
using LBoL.Core.Battle;
using LBoL.Core;
using System.Linq;
using LBoL.EntityLib.StatusEffects.Others;

namespace lvalonmima.Cards
{
	public sealed class cardbiglaserDef : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig();
			config.Colors = new List<ManaColor>() { ManaColor.Green, ManaColor.Black };
			config.Cost = new ManaGroup() { Any = 1, Green = 1, Black = 1 };
			config.UpgradedCost = new ManaGroup() { Any = 1, Hybrid = 1, HybridColor = 8 };
			config.Rarity = Rarity.Common;
			config.Type = CardType.Attack;
			config.TargetType = TargetType.AllEnemies;

			config.Damage = 9;

			config.GunName = GunNameID.GetGunFromId(25000);
			config.GunNameBurst = GunNameID.GetGunFromId(25000);

			config.Keywords = Keyword.Accuracy | Keyword.FollowCard;
			config.UpgradedKeywords = Keyword.Accuracy | Keyword.FollowCard;

			config.RelativeKeyword = Keyword.Expel;
			config.UpgradedRelativeKeyword = Keyword.Expel;

			config.Value1 = 3;

			config.Illustrator = "白河久遠";

			config.Index = CardIndexGenerator.GetUniqueIndex(config);
			return config;
		}
	}

	[EntityLogic(typeof(cardbiglaserDef))]
	public sealed class cardbiglaser : lvalonmimaCard
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
		protected override void EnterBattle2(BattleController battle)
		{
			ReactBattleEvent(Battle.Player.DamageDealt, OnPlayerDamageDealt);
		}

		public IEnumerable<BattleAction> OnPlayerDamageDealt(DamageEventArgs args)
		{
			if (Battle.AllAliveEnemies.Count() > 0 && args.ActionSource == this)
			{
				DamageInfo damageInfo = args.DamageInfo;
				if (damageInfo.Damage > 0f)
				{
					if (args.Target.IsAlive)
					{
						yield return DebuffAction<Poison>(args.Target, (int)damageInfo.Damage, 0, 0, 0);
					}
				}
			}
		}
		private IEnumerable<BattleAction> effect()
		{
			yield return SacrificeAction(Value1);
			if (Battle.BattleShouldEnd) { yield break; }
			yield return AttackAction(UnitSelector.AllEnemies);
		}
		protected override IEnumerable<BattleAction> OnExpel(DieEventArgs args)
		{
			expelling = true;
			try
			{
				NotifyActivating();
				foreach (BattleAction ba in effect()) yield return ba;
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
				foreach (BattleAction ba in effect()) yield return ba;
			}
			finally
			{
				localplaying = false;
			}
		}
	}
}


