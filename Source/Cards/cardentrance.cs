using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core;
using lvalonmima.StatusEffects;
using LBoL.EntityLib.StatusEffects.Sakuya;
using LBoL.EntityLib.StatusEffects.Cirno;
using LBoL.EntityLib.StatusEffects.Others;
using LBoL.Core.Units;

namespace lvalonmima.Cards
{
	public sealed class cardentranceDef : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig();
			config.Colors = new List<ManaColor>() { ManaColor.Red, ManaColor.Green, ManaColor.Blue, ManaColor.White, ManaColor.Black };
			config.Cost = new ManaGroup() { Red = 1, Green = 1, Blue = 1, White = 1, Black = 1 };
			config.Rarity = Rarity.Uncommon;
			config.Mana = new ManaGroup() { Any = 0 };
			config.Type = CardType.Ability;
			config.TargetType = TargetType.All;
			config.RelativeEffects = new List<string>() { nameof(TimeAuraSe), nameof(Cold), nameof(semburst), nameof(Poison) };
			config.UpgradedRelativeEffects = new List<string>() { nameof(TimeAuraSe), nameof(Cold), nameof(semburst), nameof(Poison) };
			config.RelativeCards = new List<string>() { nameof(cardpurediamond) };
			config.UpgradedRelativeCards = new List<string>() { nameof(cardpurediamond) };

			config.Value1 = 1;
			config.Value2 = 5;

			config.UpgradedKeywords = Keyword.Initial;

			config.Illustrator = "shimadoriru";

			config.Index = CardIndexGenerator.GetUniqueIndex(config);
			return config;
		}
	}

	[EntityLogic(typeof(cardentranceDef))]
	public sealed class cardentrance : lvalonmimaCard.trigger25card
	{
		public override bool Triggered => IsForceCost;

		public override bool IsForceCost
		{
			get
			{
				if (Battle != null)
				{
					return Battle.BattleCardUsageHistory.Count == 0;
				}
				return false;
			}
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new AddCardsToDrawZoneAction(Library.CreateCards<cardpurediamond>(Value1, false), DrawZoneTarget.Random, AddCardsType.Normal);
			if (Battle.BattleShouldEnd) { yield break; }
			yield return BuffAction<TimeAuraSe>(Value2, 0, 0, 0);
			if (Battle.BattleShouldEnd) { yield break; }
			yield return BuffAction<semburst>(Value2, 0, 0, 0);
			foreach (Unit unit in Battle.AllAliveUnits)
			{
				if (!unit.IsAlive || Battle.BattleShouldEnd) { continue; }
				yield return new ApplyStatusEffectAction<Cold>(unit, Value1, 0, 0, 0);
			}
			foreach (Unit unit in Battle.AllAliveUnits)
			{
				if (!unit.IsAlive || Battle.BattleShouldEnd) { continue; }
				yield return new ApplyStatusEffectAction<Poison>(unit, Value2, 0, 0, 0);
			}
		}
	}
}


