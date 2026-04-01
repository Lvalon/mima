using System.Collections.Generic;
using System.Linq;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.EnemyUnits.Normal.Maoyus;
using LBoLEntitySideloader.Attributes;

namespace lvalonmima.StatusEffects
{
	public sealed class seMaoyuBlackDef : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Positive;
			return config;
		}
	}

	[EntityLogic(typeof(seMaoyuBlackDef))]
	public sealed class seMaoyuBlack : StatusEffect
	{
		public override bool ForceNotShowDownText => true;
		protected override void OnAdded(Unit unit)
		{
			foreach (EnemyUnit maoyu in Battle.AllAliveEnemies.Where(e => e is MaoyuOrigin && !(e is MaoyuBlack)))
			{
				ReactOwnerEvent(maoyu.BlockShieldGained, OnBlockGained);
			}
			HandleOwnerEvent(Battle.EnemySpawned, args =>
			{
				if (args.Unit is MaoyuOrigin && !(args.Unit is MaoyuBlack))
					ReactOwnerEvent(args.Unit.BlockShieldGained, OnBlockGained);
			});
		}

		private IEnumerable<BattleAction> OnBlockGained(BlockShieldEventArgs args)
		{
			if (args.Block > 0 && args.ActionSource != this)
			{
				NotifyActivating();
				yield return new CastBlockShieldAction(Owner, new BlockInfo((int)args.Block));
			}
		}
	}
}