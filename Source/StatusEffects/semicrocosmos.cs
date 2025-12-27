using System.Collections.Generic;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.ExtraTurn;
using LBoLEntitySideloader.Attributes;

namespace lvalonmima.StatusEffects
{
	public sealed class semicrocosmosDef : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Special;
			config.IsStackable = false;
			config.RelativeEffects = new List<string>() { nameof(ExtraTurn), nameof(seabyss), nameof(semburst), nameof(Charging) };
			return config;
		}
	}

	[EntityLogic(typeof(semicrocosmosDef))]
	public sealed class semicrocosmos : exhl25
	{
		protected override void OnAdded(Unit unit)
		{
			if (!(unit is PlayerUnit))
			{
				BepinexPlugin.log.LogWarning(DebugName + " should not apply to non-player unit.");
				React(new RemoveStatusEffectAction(this));
				return;
			}
			base.ThisTurnActivating = false;
			HandleOwnerEvent(base.Battle.Player.TurnStarting, delegate
			{
				if (base.Battle.Player.IsExtraTurn && !base.Battle.Player.IsSuperExtraTurn && base.Battle.Player.GetStatusEffectExtend<ExtraTurnPartner>() == this)
				{
					base.ThisTurnActivating = true;
				}
			});
			ReactOwnerEvent(Battle.CardUsed, OnCardUsed);
			ReactOwnerEvent(Battle.Player.TurnStarted, OnTurnStarted);
			ReactOwnerEvent(Battle.Player.TurnEnded, OnPlayerTurnEnded);
		}

		private IEnumerable<BattleAction> OnTurnStarted(UnitEventArgs args)
		{
			if (Level > 0)
			{
				yield return new ApplyStatusEffectAction<seabyss>(Battle.Player, Level, 0, 0, 0);
			}
		}

		private IEnumerable<BattleAction> OnCardUsed(CardUsingEventArgs args)
		{
			if (base.ThisTurnActivating)
			{
				NotifyActivating();
				if (Battle.BattleShouldEnd) { yield break; }
				yield return new ApplyStatusEffectAction<TempFirepower>(Battle.Player, 1, 0, 0, 0);
				if (Battle.BattleShouldEnd) { yield break; }
				yield return new ApplyStatusEffectAction<semburst>(Battle.Player, 1, 0, 0, 0);
				if (Battle.BattleShouldEnd) { yield break; }
				yield return new ApplyStatusEffectAction<Charging>(Battle.Player, 1, 0, 0, 0);
			}
		}

		public override bool ShouldPreventCardUsage(Card card)
		{
			return base.ThisTurnActivating && card.CardType != CardType.Skill;
		}

		public IEnumerable<BattleAction> OnPlayerTurnEnded(UnitEventArgs args)
		{
			if (base.ThisTurnActivating)
			{
				yield return new RemoveStatusEffectAction(this);
			}
		}
	}
}