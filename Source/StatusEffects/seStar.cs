using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.Cards.Enemy;
using LBoLEntitySideloader.Attributes;

namespace lvalonmima.StatusEffects
{
	public sealed class seStarDef : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Positive;
			config.HasCount = true;
			return config;
		}
	}

	[EntityLogic(typeof(seStarDef))]
	public sealed class seStar : StatusEffect
	{
		public override bool ForceNotShowDownText => true;
		protected override void OnAdded(Unit unit)
		{
			Count = 0;
			foreach (EnemyUnit mf in Battle.AllAliveEnemies)
			{
				HandleOwnerEvent(mf.Died, OnDied);
			}
			HandleOwnerEvent(Battle.EnemySpawned, OnSpawned);
			ReactOwnerEvent(unit.TurnStarted, OnTurnStarted);
		}

		private IEnumerable<BattleAction> OnTurnStarted(UnitEventArgs args)
		{
			if (Count > 0)
			{
				NotifyActivating();
				yield return new ApplyStatusEffectAction<LockedOn>(Battle.Player, Count);
			}
		}

		private void OnSpawned(UnitEventArgs args)
		{
			HandleOwnerEvent(args.Unit.Died, OnDied);
		}

		private void OnDied(DieEventArgs args)
		{
			NotifyChanged();
			Highlight = true;
			Count++;
		}
	}
}