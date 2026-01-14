using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core;

namespace lvalonmima.Cards
{
	public sealed class cardwheresleepDef : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig();
			config.Colors = new List<ManaColor>() { ManaColor.Black, ManaColor.Colorless };
			config.Cost = new ManaGroup() { Any = 0 };
			config.Rarity = Rarity.Common;
			config.Type = CardType.Skill;
			config.TargetType = TargetType.Self;

			config.Value1 = 2;
			config.UpgradedValue1 = 4;
			config.Value2 = 1;
			//config.UpgradedValue2 = 2;
			config.Keywords = Keyword.Forbidden;
			config.UpgradedKeywords = Keyword.Forbidden;

			config.RelativeCards = new List<string>() { nameof(cardpurediamond) };
			config.UpgradedRelativeCards = new List<string>() { nameof(cardpurediamond) };

			config.Illustrator = "Radal";

			config.Index = CardIndexGenerator.GetUniqueIndex(config);
			return config;
		}
	}

	[EntityLogic(typeof(cardwheresleepDef))]
	public sealed class cardwheresleep : lvalonmimaCard
	{
		protected override void EnterBattle2(BattleController battle)
		{
			ReactBattleEvent(Battle.BattleStarted, OnBattleStarted);
		}

		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs args)
		{
			yield return new ExileCardAction(this);
			if (Battle.BattleShouldEnd) { yield break; }
			yield return SacrificeAction(Value1);
			if (Battle.BattleShouldEnd) { yield break; }
			yield return new AddCardsToDrawZoneAction(Library.CreateCards<cardpurediamond>(Value2, false), DrawZoneTarget.Random, AddCardsType.Normal);
		}
	}
}


