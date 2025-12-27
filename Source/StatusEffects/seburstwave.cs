using System.Collections.Generic;
using System.Linq;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoLEntitySideloader.Attributes;

namespace lvalonmima.StatusEffects
{
	public sealed class seburstwaveDef : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Positive;
			config.RelativeEffects = new List<string>() { nameof(Charging) };
			return config;
		}
	}

	[EntityLogic(typeof(seburstwaveDef))]
	public sealed class seburstwave : StatusEffect
	{
		protected override void OnAdded(Unit unit)
		{
			ReactOwnerEvent(Battle.Player.TurnStarted, OnTurnStarted);
		}

		private IEnumerable<BattleAction> OnTurnStarted(UnitEventArgs args)
		{
			if (Battle.HandZone.Count() > 0 && Battle.AllAliveEnemies.Count() > 0)
			{
				NotifyActivating();
				SelectCardInteraction interaction = new SelectCardInteraction(0, Level, Battle.HandZone)
				{
					Source = this
				};
				yield return new InteractionAction(interaction);
				IReadOnlyList<Card> cards = interaction.SelectedCards;
				if (cards.Count > 0)
				{
					if (Battle.BattleShouldEnd) { yield break; }
					yield return new ExileManyCardAction(cards);
					if (Battle.BattleShouldEnd) { yield break; }
					yield return new ApplyStatusEffectAction<semburst>(Battle.Player, cards.Count, 0, 0, 0);
					if (Battle.BattleShouldEnd) { yield break; }
					yield return new ApplyStatusEffectAction<Charging>(Battle.Player, cards.Count, 0, 0, 0);
				}
			}
		}
	}
}