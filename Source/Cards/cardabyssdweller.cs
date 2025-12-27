using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using lvalonmima.GunName;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core;
using lvalonmima.StatusEffects;

namespace lvalonmima.Cards
{
	public sealed class cardabyssdwellerDef : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig();
			config.Colors = new List<ManaColor>() { ManaColor.Blue, ManaColor.Black };
			config.Cost = new ManaGroup() { Any = 1, Hybrid = 3, HybridColor = 4 };
			config.UpgradedCost = new ManaGroup() { Hybrid = 2, HybridColor = 4 };
			config.Rarity = Rarity.Uncommon;
			config.Type = CardType.Attack;
			config.TargetType = TargetType.SingleEnemy;

			config.Damage = 16;

			config.GunName = GunNameID.GetGunFromId(7080);
			config.GunNameBurst = GunNameID.GetGunFromId(7081);

			config.Keywords = Keyword.Exile | Keyword.Ethereal | Keyword.Accuracy;
			config.UpgradedKeywords = Keyword.Exile | Keyword.Ethereal | Keyword.Accuracy;

			config.RelativeKeyword = Keyword.Expel;
			config.UpgradedRelativeKeyword = Keyword.Expel;

			config.Value1 = 4;
			config.Value2 = 1;

			config.Illustrator = "わんこソラ";

			config.Index = CardIndexGenerator.GetUniqueIndex(config);
			return config;
		}
	}

	[EntityLogic(typeof(cardabyssdwellerDef))]
	public sealed class cardabyssdweller : lvalonmimaCard
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
				yield return SacrificeAction(Value1);
				if (Battle.BattleShouldEnd) { yield break; }
				yield return new ApplyStatusEffectAction<seabyssdweller>(Battle.Player, Value2);
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
				yield return AttackAction(selector);
			}
			finally
			{
				localplaying = false;
			}
		}
	}
}


