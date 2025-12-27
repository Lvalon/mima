using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core;
using lvalonmima.StatusEffects;

namespace lvalonmima.Cards
{
	public sealed class cardsideloadDef : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig();
			config.Colors = new List<ManaColor>() { ManaColor.Green, ManaColor.Blue };
			config.Cost = new ManaGroup() { Hybrid = 3, HybridColor = 6 };
			config.Rarity = Rarity.Uncommon;
			config.Mana = new ManaGroup() { Blue = 1 };
			config.Type = CardType.Ability;
			config.TargetType = TargetType.Nobody;

			config.Value1 = 1;
			config.Value2 = 2;

			config.Illustrator = "こーろー";

			config.Index = CardIndexGenerator.GetUniqueIndex(config);
			return config;
		}
	}

	[EntityLogic(typeof(cardsideloadDef))]
	public sealed class cardsideload : lvalonmimaCard
	{
		public ManaGroup Mana2 => new ManaGroup() { Green = 1 };
		public int Value30 => 4;
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new ApplyStatusEffectAction<secardsideload>(Battle.Player, Value1, 0, 0, 0);
			yield return new ApplyStatusEffectAction<secardsideload2>(Battle.Player, Value1, 0, 0, 0);
			if (Battle.BattleShouldEnd || !IsUpgraded) { yield break; }
			yield return new DrawManyCardAction(Value2);
			yield return new GainManaAction(Mana2);
		}
	}
}


