using System.Collections.Generic;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoLEntitySideloader.Attributes;

namespace lvalonmima.StatusEffects;

public sealed class sehardenDef : lvalonmimaStatusEffectTemplate
{
	public override StatusEffectConfig MakeConfig() => Cfg(StatusEffectType.Positive, hasCount: true);
}

[EntityLogic(typeof(sehardenDef))]
public sealed class seharden : StatusEffect
{
	public override bool ForceNotShowDownText => true;
	protected override void OnAdded(Unit unit)
	{
		Count = 1;
		HandleOwnerEvent(unit.TurnEnded, OnTurnEnded);
		ReactOwnerEvent(unit.DamageTaking, OnDamageTaking);
	}

	private void OnTurnEnded(UnitEventArgs args)
	{
		Count = 1;
	}

	private IEnumerable<BattleAction> OnDamageTaking(DamageEventArgs args)
	{
		int num = args.DamageInfo.Damage.RoundToInt();
		if (num > 0)
		{
			NotifyActivating();
			yield return new CastBlockShieldAction(Owner, new BlockInfo(Count));
			Count++;
		}
	}
}
