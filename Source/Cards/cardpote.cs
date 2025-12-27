using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core;
using System;
using lvalonmima.StatusEffects;
using LBoL.EntityLib.StatusEffects.Sakuya;
using LBoL.EntityLib.StatusEffects.Cirno;
using LBoL.EntityLib.StatusEffects.Others;
using LBoL.Core.StatusEffects;

namespace lvalonmima.Cards
{
	public sealed class cardpoteDef : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig();
			config.Colors = new List<ManaColor>() { ManaColor.White, ManaColor.Red, ManaColor.Green, ManaColor.Blue, ManaColor.Black };
			config.Cost = new ManaGroup() { White = 1, Red = 1, Green = 1, Blue = 1, Black = 1 };
			config.Rarity = Rarity.Rare;
			config.Type = CardType.Ability;
			config.TargetType = TargetType.All;

			config.Mana = new ManaGroup() { Philosophy = 2 };

			config.Value1 = 1;
			config.Value2 = 2;

			config.RelativeEffects = new List<string>() { nameof(TimeAuraSe), nameof(Cold), nameof(Charging), nameof(semburst), nameof(Poison) };
			config.UpgradedRelativeEffects = new List<string>() { nameof(TimeAuraSe), nameof(Cold), nameof(Charging), nameof(semburst), nameof(Poison) };

			config.Illustrator = "あとち";

			config.Index = CardIndexGenerator.GetUniqueIndex(config);
			return config;
		}
	}

	[EntityLogic(typeof(cardpoteDef))]
	public sealed class cardpote : lvalonmimaCard
	{
		public ManaGroup ManaHHHWU => new ManaGroup() { Hybrid = 3, HybridColor = 0 };
		public ManaGroup ManaUU => new ManaGroup() { Blue = 2 };
		public ManaGroup ManaHHBR => new ManaGroup() { Hybrid = 2, HybridColor = 7 };
		public ManaGroup ManaR => new ManaGroup() { Red = 1 };
		public ManaGroup ManaHHBG => new ManaGroup() { Hybrid = 2, HybridColor = 8 };
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			List<Type> list = new List<Type>() { typeof(sepotehhhwu), typeof(sepoteuu), typeof(sepotehhbr), typeof(sepoter), typeof(sepotehhbg) };
			foreach (Type setype in list)
			{
				yield return new ApplyStatusEffectAction(setype, Battle.Player, Value1, 0, 0);
			}
			if (Battle.BattleShouldEnd || !IsUpgraded) { yield break; }
			yield return new GainManaAction(Mana);
		}
	}
}


