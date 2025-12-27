using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core;
using LBoL.Core.StatusEffects;

namespace lvalonmima.Cards
{
	public sealed class cardsharppeakDef : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig();
			config.Colors = new List<ManaColor>() { ManaColor.Red, ManaColor.Black };
			config.Cost = new ManaGroup() { Black = 1, Red = 1 };
			config.Rarity = Rarity.Uncommon;
			config.Type = CardType.Skill;
			config.TargetType = TargetType.Self;
			config.IsXCost = true;

			config.Mana = new ManaGroup() { Any = 1 };

			config.Value1 = 1;
			config.Value2 = 1;
			config.UpgradedValue2 = 2;
			config.Keywords = Keyword.Exile | Keyword.Ethereal;
			config.UpgradedKeywords = Keyword.Exile | Keyword.Ethereal;
			config.RelativeKeyword = Keyword.XCost | Keyword.Synergy | Keyword.Overdraft;
			config.UpgradedRelativeKeyword = Keyword.XCost | Keyword.Synergy | Keyword.Overdraft;
			config.RelativeEffects = new List<string>() { nameof(Charging) };
			config.UpgradedRelativeEffects = new List<string>() { nameof(Charging) };

			config.Illustrator = "ta";

			config.Index = CardIndexGenerator.GetUniqueIndex(config);
			return config;
		}
	}

	[EntityLogic(typeof(cardsharppeakDef))]
	public sealed class cardsharppeak : lvalonmimaCard
	{
		public override ManaGroup GetXCostFromPooled(ManaGroup pooledMana)
		{
			return pooledMana;
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			for (int i = 0; i < SynergyAmount(consumingMana, ManaColor.Any, 1); i++)
			{
				if (Battle.BattleShouldEnd) { yield break; }
				yield return SacrificeAction(Value1);
				yield return new LockRandomTurnManaAction(Value1);
				//if (Battle.BattleShouldEnd) { yield break; }
				//yield return new ApplyStatusEffectAction<TempFirepower>(Battle.Player, Value2, 0, 0, 0);
				if (Battle.BattleShouldEnd) { yield break; }
				yield return new ApplyStatusEffectAction<Charging>(Battle.Player, Value2, 0, 0, 0);
			}
		}
	}
}


