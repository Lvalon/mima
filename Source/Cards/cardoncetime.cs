using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Cirno;

namespace lvalonmima.Cards
{
	public sealed class cardoncetimeDef : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig();
			config.Colors = new List<ManaColor>() { ManaColor.Blue, ManaColor.Colorless };
			config.Cost = new ManaGroup() { Any = 0 };
			config.Rarity = Rarity.Common;
			config.Type = CardType.Skill;
			config.TargetType = TargetType.All;

			config.Value1 = 1;
			config.Keywords = Keyword.Forbidden;
			config.UpgradedKeywords = Keyword.Forbidden;
			config.RelativeKeyword = Keyword.Purify;
			config.UpgradedRelativeKeyword = Keyword.Purify;
			config.RelativeEffects = new List<string>() { nameof(Cold) };
			config.UpgradedRelativeEffects = new List<string>() { nameof(Cold) };

			config.Illustrator = "Radal";

			config.Index = CardIndexGenerator.GetUniqueIndex(config);
			return config;
		}
	}

	[EntityLogic(typeof(cardoncetimeDef))]
	public sealed class cardoncetime : lvalonmimaCard
	{
		int togo = 0;
		protected override void EnterBattle2(BattleController battle)
		{
			ReactBattleEvent(Battle.BattleStarted, OnBattleStarted);
			ReactBattleEvent(Battle.Player.TurnStarted, OnTurnStarted);
		}

		private IEnumerable<BattleAction> OnTurnStarted(UnitEventArgs args)
		{
			if (Battle.BattleShouldEnd) { yield break; }
			if (Battle.Player.TurnCounter != 1)
			{
				yield break;
			}
			if (Battle.BattleMana.HasTrivial)
			{
				yield return ConvertManaAction.Purify(Battle.BattleMana, togo);
			}
		}

		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs args)
		{
			yield return new ExileCardAction(this);
			IEnumerable<Unit> mfs = IsUpgraded ? Battle.AllAliveUnits : Battle.AllAliveEnemies;
			foreach (Unit unit in mfs)
			{
				if (!unit.IsAlive || Battle.BattleShouldEnd) { continue; }
				yield return new ApplyStatusEffectAction<Cold>(unit, Value1, 0, 0, 0);
			}
			togo++;
		}
	}
}


