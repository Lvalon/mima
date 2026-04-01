using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Enemy;
using LBoLEntitySideloader.Attributes;

namespace lvalonmima.StatusEffects
{
	public sealed class seJunkoDef : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Positive;
			return config;
		}
	}

	[EntityLogic(typeof(seJunkoDef))]
	public sealed class seJunko : StatusEffect
	{
		bool triggered;
		public override bool ForceNotShowDownText => true;
		protected override void OnAdded(Unit unit)
		{
			triggered = false;
			Highlight = Battle.Player.TryGetStatusEffect<JunkoColor>(out var eff) && eff.Level > 2;
			HandleOwnerEvent(unit.StatusEffectChanged, OnStatusEffectChanged);
			ReactOwnerEvent(Battle.Player.TurnStarted, OnPlayerTurnStarted, (GameEventPriority)99999);
		}

		private IEnumerable<BattleAction> OnPlayerTurnStarted(UnitEventArgs args)
		{
			if (Battle.BattleMana.HasTrivial && Highlight)
			{
				NotifyActivating();
				yield return ConvertManaAction.Purify(Battle.BattleMana, 1);
			}
		}

		private void OnStatusEffectChanged(StatusEffectEventArgs args)
		{
			if (Battle.Player.TryGetStatusEffect<JunkoColor>(out var eff) && eff.Level >= 3)
			{
				Highlight = true;
			}
			if (triggered == false && Battle.Player.TryGetStatusEffect<JunkoColor>(out var eff2) && eff2.Level >= 5)
			{
				triggered = true;
				NotifyActivating();
				PurifyBase();
			}
		}
		private void PurifyBase()
		{
			if (!GameRun.BaseMana.HasTrivial)
				return;
			ManaGroup empty = ManaGroup.Empty;
			empty[GameRun.BaseMana.MaxTrivialColor] += 1;

			GameRun.SetBaseMana(GameRun.BaseMana - empty + ManaGroup.Colorlesses(1), triggerVisual: true);
		}
	}
}