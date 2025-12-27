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
using LBoLEntitySideloader.Attributes;
using lvalonmima.GunName;

namespace lvalonmima.StatusEffects
{
	public sealed class seindomitableDef : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Special;
			config.HasCount = true;
			return config;
		}
	}

	[EntityLogic(typeof(seindomitableDef))]
	public sealed class seindomitable : StatusEffect
	{
		public override bool ForceNotShowDownText => true;
		bool go = false;
		protected override void OnAdded(Unit unit)
		{
			go = true;
			Count = 0;
			HandleOwnerEvent(unit.DamageTaking, OnDamageTaking, GameEventPriority.Lowest - 10);
			HandleOwnerEvent(Battle.RoundStarting, OnRoundStarting, GameEventPriority.Highest);
			ReactOwnerEvent(Battle.RoundStarted, OnRoundStarted);
		}

		private void OnRoundStarting(GameEventArgs args)
		{
			go = false;
		}

		private IEnumerable<BattleAction> OnRoundStarted(GameEventArgs args)
		{
			NotifyActivating();
			int gunid = 15160;
			int[] thresholds = { 25, 50, 75, 100, 150 };
			gunid += thresholds.Count(t => Count > toolbox.hpfrompercent(Battle.Player, t));
			yield return DamageAction.LoseLife(Owner, Count, GunNameID.GetGunFromId(gunid));
			yield return new RemoveStatusEffectAction(this);
		}

		public void OnDamageTaking(DamageEventArgs args)
		{
			if (args.ActionSource == this || !go) { return; }
			int num = args.DamageInfo.Damage.RoundToInt();
			if (num > 0)
			{
				NotifyActivating();
				Count += num;
				Highlight = Count > Owner.Hp;
				args.DamageInfo = args.DamageInfo.ReduceActualDamageBy(num);
				args.AddModifier(this);
			}
		}
	}
}