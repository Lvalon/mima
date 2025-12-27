using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core;
using LBoL.EntityLib.StatusEffects.Cirno;

namespace lvalonmima.Cards
{
	public sealed class cardyukionnaDef : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig();
			config.Colors = new List<ManaColor>() { ManaColor.Blue, ManaColor.Black };
			config.Cost = new ManaGroup() { Blue = 1, Black = 1 };
			config.Rarity = Rarity.Uncommon;
			config.Type = CardType.Attack;
			config.TargetType = TargetType.RandomEnemy;

			config.Damage = 0;

			config.RelativeKeyword = Keyword.Expel;
			config.UpgradedRelativeKeyword = Keyword.Expel;

			config.RelativeEffects = new List<string>() { nameof(Cold) };
			config.UpgradedRelativeEffects = new List<string>() { nameof(Cold) };

			config.Value1 = 2;
			config.UpgradedValue1 = 3;
			config.Value2 = 1;

			config.Illustrator = "老邢";

			config.Index = CardIndexGenerator.GetUniqueIndex(config);
			return config;
		}
	}

	[EntityLogic(typeof(cardyukionnaDef))]
	public sealed class cardyukionna : lvalonmimaCard
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
				if (IsUpgraded)
				{
					GameRun.SetHpAndMaxHp(Battle.Player.Hp + Value2, Battle.Player.MaxHp + Value2, true);
				}
				if (Battle.BattleShouldEnd) { yield break; }
				yield return new DrawManyCardAction(Value1);
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
				yield return SacrificeAction(Value1);
				for (int i = 0; i < Value1; i++)
				{
					if (Battle.BattleShouldEnd) { yield break; }
					yield return DebuffAction<Cold>(Battle.RandomAliveEnemy, 1);
				}
			}
			finally
			{
				localplaying = false;
			}
		}
	}
}


