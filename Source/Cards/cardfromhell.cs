using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core;
using lvalonmima.StatusEffects;
using LBoL.Core.StatusEffects;

namespace lvalonmima.Cards
{
	public sealed class cardfromhellDef : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig();
			config.Colors = new List<ManaColor>() { ManaColor.Red, ManaColor.Colorless };
			config.Cost = new ManaGroup() { Red = 1, Colorless = 1 };
			config.UpgradedCost = new ManaGroup() { Red = 1, Any = 1 };
			config.Rarity = Rarity.Uncommon;
			config.Type = CardType.Skill;
			config.TargetType = TargetType.Nobody;
			config.UpgradedKeywords = Keyword.Replenish;
			config.RelativeKeyword = Keyword.Purify;
			config.UpgradedRelativeKeyword = Keyword.Purify;
			config.Value1 = 1;
			config.Value2 = 2;
			config.RelativeEffects = new List<string>() { nameof(semburst), nameof(Charging), nameof(seunder) };
			config.UpgradedRelativeEffects = new List<string>() { nameof(semburst), nameof(Charging), nameof(seunder) };
			config.RelativeCards = new List<string>() { nameof(cardpurediamond) };
			config.UpgradedRelativeCards = new List<string>() { nameof(cardpurediamond) };

			config.Value1 = 1;

			config.Illustrator = "彩峰";

			config.Index = CardIndexGenerator.GetUniqueIndex(config);
			return config;
		}
	}

	[EntityLogic(typeof(cardfromhellDef))]
	public sealed class cardfromhell : lvalonmimaCard.trigger25card
	{
		public int Value10 => 10;
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			if (BepinexPlugin.u25)
			{
				NotifyActivating();
				foreach (BattleAction ba in eff(selector, consumingMana, precondition)) yield return ba;
			}
			foreach (BattleAction ba in eff(selector, consumingMana, precondition)) yield return ba;
		}
		private IEnumerable<BattleAction> eff(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			if (Battle.BattleShouldEnd) { yield break; }
			yield return new AddCardsToDrawZoneAction(Library.CreateCards<cardpurediamond>(Value1, false), DrawZoneTarget.Random, AddCardsType.Normal);
			if (Battle.BattleShouldEnd) { yield break; }
			yield return new ApplyStatusEffectAction<semburst>(Battle.Player, Value10, 0, 0, 0);
			if (Battle.BattleMana.HasTrivial)
			{
				yield return ConvertManaAction.Purify(Battle.BattleMana, Value2);
			}
			if (Battle.BattleShouldEnd) { yield break; }
			yield return new ApplyStatusEffectAction<Charging>(Battle.Player, Value2, 0, 0, 0);
		}
	}
}


