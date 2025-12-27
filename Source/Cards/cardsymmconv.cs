using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using lvalonmima.GunName;
using LBoL.Core.Battle;
using LBoL.Core;

namespace lvalonmima.Cards
{
	public sealed class cardsymmconvDef : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig();
			config.Colors = new List<ManaColor>() { ManaColor.Green };
			config.Cost = new ManaGroup() { Any = 1 };
			config.UpgradedCost = new ManaGroup() { Any = 0 };
			config.Rarity = Rarity.Common;
			config.Type = CardType.Attack;
			config.TargetType = TargetType.SingleEnemy;

			config.Damage = 3;

			config.GunName = GunNameID.GetGunFromId(12190);
			config.GunNameBurst = GunNameID.GetGunFromId(12191);

			config.Keywords = Keyword.Replenish;
			config.UpgradedKeywords = Keyword.Replenish;

			config.RelativeKeyword = Keyword.Grow | Keyword.Expel;
			config.UpgradedRelativeKeyword = Keyword.Grow | Keyword.Expel;

			config.Value1 = 2;
			config.Value2 = 2;

			config.Illustrator = "门番神玉";

			config.Index = CardIndexGenerator.GetUniqueIndex(config);
			return config;
		}
	}

	[EntityLogic(typeof(cardsymmconvDef))]
	public sealed class cardsymmconv : lvalonmimaCard
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
		public int battleatk => Value1 + GrowCount * Value2;
		protected override IEnumerable<BattleAction> OnExpel(DieEventArgs args)
		{
			expelling = true;
			try
			{
				if (PlayCount == 0) { yield break; }
				NotifyActivating();
				yield return SacrificeAction(PlayCount);
			}
			finally { expelling = false; }
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			localplaying = true;
			try
			{
				for (int i = 0; i < battleatk; i++)
				{
					if (!selector.SelectedEnemy.IsAlive) { break; }
					yield return AttackAction(selector, i == 0 ? GunName : "Instant");
				}
			}
			finally { localplaying = false; }
		}
	}
}


