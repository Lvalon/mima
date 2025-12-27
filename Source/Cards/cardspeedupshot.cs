using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using lvalonmima.GunName;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Others;

namespace lvalonmima.Cards
{
	public sealed class cardspeedupshotDef : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig();
			config.Colors = new List<ManaColor>() { ManaColor.Black, ManaColor.Green };
			config.Cost = new ManaGroup() { Any = 0 };
			config.Rarity = Rarity.Common;
			config.Type = CardType.Attack;
			config.TargetType = TargetType.RandomEnemy;

			config.Keywords = Keyword.Forbidden | Keyword.Replenish;
			config.UpgradedKeywords = Keyword.Replenish;

			config.Damage = 2;

			config.GunName = GunNameID.GetGunFromId(7040);
			config.GunNameBurst = GunNameID.GetGunFromId(7041);

			config.RelativeEffects = new List<string>() { nameof(Poison) };
			config.UpgradedRelativeEffects = new List<string>() { nameof(Poison) };

			config.Value1 = 2;

			config.Value2 = 2;
			config.UpgradedValue2 = 3;

			config.Illustrator = "辻一穂";

			config.Index = CardIndexGenerator.GetUniqueIndex(config);
			return config;
		}
	}

	[EntityLogic(typeof(cardspeedupshotDef))]
	public sealed class cardspeedupshot : lvalonmimaCard
	{
		protected override void EnterBattle2(BattleController battle)
		{
			ReactBattleEvent(Battle.Player.TurnEnding, OnTurnEnding);
		}

		private IEnumerable<BattleAction> OnTurnEnding(UnitEventArgs args)
		{
			if (Zone != CardZone.Hand) { yield break; }
			NotifyActivating();
			for (int i = 0; i < Value2; i++)
			{
				Unit unit = Battle.RandomAliveEnemy;
				if (Battle.BattleShouldEnd || !unit.IsAlive) { yield break; }
				yield return AttackAction(unit);
				if (Battle.BattleShouldEnd || !unit.IsAlive) { yield break; }
				yield return DebuffAction<Poison>(unit, Value1, 0, 0, 0);
			}
			if (Battle.BattleShouldEnd) { yield break; }
			yield return new DiscardAction(this);
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield break;
		}
	}
}


