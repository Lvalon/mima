using System.Collections.Generic;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using System.Linq;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Others;
using LBoLEntitySideloader.Attributes;

namespace lvalonmima.StatusEffects
{
	public sealed class sebackgroundDef : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Special;
			config.RelativeEffects = new List<string>() { nameof(Poison) };
			return config;
		}
	}

	[EntityLogic(typeof(sebackgroundDef))]
	public sealed class sebackground : StatusEffect
	{
		protected override void OnAdded(Unit unit)
		{
			ReactOwnerEvent(Owner.DamageDealt, OnDamageDealt);
		}

		private IEnumerable<BattleAction> OnDamageDealt(DamageEventArgs args)
		{
			if (Battle.AllAliveEnemies.Count() > 0 && args.Target.IsAlive && args.ActionSource != this && args.Target.HasStatusEffect<Poison>())
			{
				DamageInfo damageInfo = args.DamageInfo;
				if (damageInfo.DamageType == DamageType.Attack)
				{
					NotifyActivating();
					yield return DamageAction.LoseLife(Battle.Player, Level);
					yield return new DamageAction(Battle.Player, args.Target, DamageInfo.Attack(Level), "Poison");
				}
			}
		}
	}
}