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
	public sealed class serewindDef : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Special;
			config.IsStackable = false;
			config.RelativeEffects = new List<string>() { nameof(ExtraTurn) };
			return config;
		}
	}

	[EntityLogic(typeof(serewindDef))]
	public sealed class serewind : ExtraTurnPartner
	{
		public override bool ForceNotShowDownText => true;
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
			HandleOwnerEvent(Battle.Predraw, OnPredraw);
			ReactOwnerEvent(Battle.Player.TurnEnded, OnPlayerTurnEnded);
		}

		private void OnPredraw(CardEventArgs args)
		{
			if (base.ThisTurnActivating && args.Cause == ActionCause.TurnStart)
			{
				NotifyActivating();
				if (Battle.DiscardZone.Count > 0)
				{
					React(new MoveCardAction(Battle.DiscardZone[Battle.DiscardZone.Count - 1], CardZone.Hand));
				}
				args.CancelBy(this);
			}
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