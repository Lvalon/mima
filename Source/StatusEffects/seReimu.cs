using System.Collections.Generic;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.EnemyUnits.Normal.Yinyangyus;
using LBoLEntitySideloader.Attributes;

namespace lvalonmima.StatusEffects
{
	public sealed class seReimuDef : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Positive;
			return config;
		}
	}

	[EntityLogic(typeof(seReimuDef))]
	public sealed class seReimu : StatusEffect
	{
		public override bool ForceNotShowDownText => true;
		protected override void OnAdded(Unit unit)
		{
			foreach (EnemyUnit mf in Battle.AllAliveEnemies)
			{
				ReactOwnerEvent(mf.Died, OnDied);
			}
			HandleOwnerEvent(Battle.EnemySpawned, OnSpawned);
			ReactOwnerEvent(Battle.Player.TurnEnding, OnPlayerTurnEnding);
		}

		private IEnumerable<BattleAction> OnPlayerTurnEnding(UnitEventArgs args)
		{
			if (Battle.BattleMana != ManaGroup.Empty)
			{
				NotifyActivating();
				yield return new CastBlockShieldAction(Owner, new ShieldInfo(3));
			}
		}

		private IEnumerable<BattleAction> OnDied(DieEventArgs args)
		{
			if (args.Unit is YinyangyuRedOrigin || args.Unit is YinyangyuBlueOrigin)
			{
				NotifyActivating();
				yield return new ApplyStatusEffectAction<TempFirepower>(Owner, 3);
			}
		}

		private void OnSpawned(UnitEventArgs args)
		{
			ReactOwnerEvent(args.Unit.Died, OnDied);
		}
	}
}