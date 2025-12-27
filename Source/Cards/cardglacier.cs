using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using LBoL.Core.Battle;
using LBoL.Core;
using LBoL.EntityLib.StatusEffects.Cirno;

namespace lvalonmima.Cards
{
	public sealed class cardglacierDef : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig();
			config.Colors = new List<ManaColor>() { ManaColor.Blue };
			config.Cost = new ManaGroup() { Any = 1 };
			config.UpgradedCost = new ManaGroup() { Any = 0 };
			config.Rarity = Rarity.Uncommon;
			config.Type = CardType.Attack;
			config.TargetType = TargetType.SingleEnemy;

			config.Damage = 0;

			config.Keywords = Keyword.Echo | Keyword.Retain;
			config.UpgradedKeywords = Keyword.EternalEcho | Keyword.Retain;

			config.RelativeKeyword = Keyword.Expel;
			config.UpgradedRelativeKeyword = Keyword.Expel;

			config.RelativeEffects = new List<string>() { nameof(Cold) };
			config.UpgradedRelativeEffects = new List<string>() { nameof(Cold) };

			config.Value1 = 1;
			config.UpgradedValue1 = 2;

			config.Illustrator = "五七七";

			config.Index = CardIndexGenerator.GetUniqueIndex(config);
			return config;
		}
	}

	[EntityLogic(typeof(cardglacierDef))]
	public sealed class cardglacier : lvalonmimaCard
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
				for (int i = 0; i < Value1; i++)
				{
					if (Battle.BattleShouldEnd) { break; }
					yield return DebuffAction<Cold>(Battle.Player, 1);
				}
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
				yield return DebuffAction<Cold>(selector.SelectedEnemy, 1);
			}
			finally
			{
				localplaying = false;
			}
		}
	}
}


