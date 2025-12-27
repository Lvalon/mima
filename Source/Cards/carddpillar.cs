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
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Cirno;
using LBoL.EntityLib.StatusEffects.Sakuya;

namespace lvalonmima.Cards
{
	public sealed class carddpillarDef : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig();
			config.Colors = new List<ManaColor>() { ManaColor.Blue, ManaColor.Black };
			config.Cost = new ManaGroup() { Any = 2, Blue = 1, Black = 1 };
			config.Rarity = Rarity.Uncommon;
			config.Type = CardType.Attack;
			config.TargetType = TargetType.AllEnemies;

			config.Damage = 9;

			config.GunName = GunNameID.GetGunFromId(14122);
			config.GunNameBurst = GunNameID.GetGunFromId(14123);

			config.RelativeKeyword = Keyword.Expel;
			config.UpgradedRelativeKeyword = Keyword.Expel;

			config.RelativeEffects = new List<string>() { nameof(Cold), nameof(TimeAuraSe) };
			config.UpgradedRelativeEffects = new List<string>() { nameof(Cold), nameof(TimeAuraSe) };

			config.RelativeCards = new List<string>() { nameof(cardpurediamond) };
			config.UpgradedRelativeCards = new List<string>() { nameof(cardpurediamond) };

			config.Value1 = 9;
			config.Value2 = 1;

			config.Illustrator = "二阶堂";

			config.Index = CardIndexGenerator.GetUniqueIndex(config);
			return config;
		}
	}

	[EntityLogic(typeof(carddpillarDef))]
	public sealed class carddpillar : lvalonmimaCard
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
		private IEnumerable<BattleAction> effect()
		{
			yield return AttackAction(UnitSelector.AllEnemies);
			if (Battle.BattleShouldEnd) { yield break; }
			foreach (Unit unit in Battle.AllAliveEnemies)
			{
				if (!unit.IsAlive || Battle.BattleShouldEnd) { yield break; }
				yield return DebuffAction<Cold>(unit, Value1);
			}
			if (Battle.BattleShouldEnd) { yield break; }
			yield return BuffAction<TimeAuraSe>(Value1, 0, 0, 0);
			if (Battle.BattleShouldEnd) { yield break; }
			yield return new AddCardsToDrawZoneAction(Library.CreateCards<cardpurediamond>(Value2, false), DrawZoneTarget.Random, AddCardsType.Normal);
		}
		protected override IEnumerable<BattleAction> OnExpel(DieEventArgs args)
		{
			expelling = true;
			try
			{
				NotifyActivating();
				if (Battle.AllAliveEnemies.Count() > 0)
				{
					foreach (BattleAction ba in effect()) yield return ba;
				}
				if (IsUpgraded)
				{
					yield return SacrificeAction(Value1);
					if (Battle.BattleShouldEnd) { yield break; }
					yield return DebuffAction<Cold>(Battle.Player, Value1);
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
				foreach (BattleAction ba in effect()) yield return ba;
			}
			finally
			{
				localplaying = false;
			}
		}
	}
}


