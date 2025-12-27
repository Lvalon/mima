using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using lvalonmima.Cards.Template;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.Battle.BattleActions;
using LBoL.Base.Extensions;
using LBoL.Core;
using System.Linq;
using lvalonmima.StatusEffects;
using LBoL.Core.StatusEffects;
using LBoL.Core.Battle.Interactions;

namespace lvalonmima.Cards
{
	public sealed class cardburstwaveDef : lvalonmimaCardTemplate
	{
		public override CardConfig MakeConfig()
		{
			CardConfig config = GetCardDefaultConfig();
			config.Colors = new List<ManaColor>() { ManaColor.Red, ManaColor.Green };
			config.Cost = new ManaGroup() { Any = 1, Hybrid = 1, HybridColor = 9 };
			config.Rarity = Rarity.Uncommon;
			config.Type = CardType.Ability;
			config.TargetType = TargetType.Self;
			config.RelativeEffects = new List<string>() { nameof(semburst), nameof(Charging) };
			config.UpgradedRelativeEffects = new List<string>() { nameof(semburst), nameof(Charging) };

			config.Value1 = 1;

			config.Illustrator = "EMON-YU";

			config.Index = CardIndexGenerator.GetUniqueIndex(config);
			return config;
		}
	}

	[EntityLogic(typeof(cardburstwaveDef))]
	public sealed class cardburstwave : lvalonmimaCard
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new ApplyStatusEffectAction<seburstwave>(Battle.Player, Value1, 0, 0, 0);
			if (Battle.AllAliveEnemies.Count() > 0 && IsUpgraded && Battle.HandZone.Where(c => c != this).Count() > 0)
			{
				SelectCardInteraction interaction = new SelectCardInteraction(0, Value1, Battle.HandZone.Where(c => c != this), SelectedCardHandling.DoNothing)
				{
					Source = this
				};
				yield return new InteractionAction(interaction, false);
				IReadOnlyList<Card> selectedCards = interaction.SelectedCards;

				if (selectedCards != null && selectedCards.Count > 0)
				{
					if (Battle.BattleShouldEnd) { yield break; }
					yield return new ExileManyCardAction(selectedCards);
					if (Battle.BattleShouldEnd) { yield break; }
					yield return new ApplyStatusEffectAction<semburst>(Battle.Player, selectedCards.Count, 0, 0, 0);
					if (Battle.BattleShouldEnd) { yield break; }
					yield return new ApplyStatusEffectAction<Charging>(Battle.Player, selectedCards.Count, 0, 0, 0);
				}
			}
		}
	}
}

