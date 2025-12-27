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
	public sealed class cardentertainmentDef : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig();
			config.Colors = new List<ManaColor>() { ManaColor.Black, ManaColor.Green, ManaColor.White };
			config.Cost = new ManaGroup() { Black = 1, Green = 1, White = 1 };
			config.Rarity = Rarity.Rare;
			config.Type = CardType.Skill;
			config.TargetType = TargetType.Self;
			config.Keywords = Keyword.Exile;
			config.UpgradedKeywords = Keyword.Exile | Keyword.Initial;

			config.Value1 = 10;
			config.Value2 = 1;
			config.Mana = new ManaGroup() { Black = 2, Green = 2, White = 2 };
			config.UpgradedMana = new ManaGroup() { Black = 2, Green = 2, White = 2, Philosophy = 1, Colorless = 1 };

			config.RelativeCards = new List<string>() { nameof(cardpurediamond) };
			config.UpgradedRelativeCards = new List<string>() { nameof(cardpurediamond) };

			config.Illustrator = "ツバネ";

			config.Index = CardIndexGenerator.GetUniqueIndex(config);
			return config;
		}
	}

	[EntityLogic(typeof(cardentertainmentDef))]
	public sealed class cardentertainment : lvalonmimaCard
	{
		public int Value10 => 10;
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return SacrificeAction(Value1);
			yield return new GainMoneyAction(Value10);
			if (IsUpgraded)
			{
				yield return new GainPowerAction(Value10);
			}
			if (Battle.BattleShouldEnd) { yield break; }
			yield return new GainManaAction(Mana);
			if (Battle.BattleShouldEnd) { yield break; }
			yield return new AddCardsToDrawZoneAction(Library.CreateCards<cardpurediamond>(Value2, false), DrawZoneTarget.Random, AddCardsType.Normal);
		}
	}
}


