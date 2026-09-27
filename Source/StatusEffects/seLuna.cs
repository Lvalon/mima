using System.Collections.Generic;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoLEntitySideloader.Attributes;

namespace lvalonmima.StatusEffects;

public sealed class seLunaDef : lvalonmimaStatusEffectTemplate
{
	public override StatusEffectConfig MakeConfig() => Cfg(StatusEffectType.Positive, hasCount: true);
}

[EntityLogic(typeof(seLunaDef))]
public sealed class seLuna : StatusEffect
{
	public override bool ForceNotShowDownText => true;
	protected override void OnAdded(Unit unit)
	{
		Count = 0;
		foreach (EnemyUnit mf in Battle.AllAliveEnemies)
			HandleOwnerEvent(mf.Died, OnDied);
		HandleOwnerEvent(Battle.EnemySpawned, OnSpawned);
		ReactOwnerEvent(unit.TurnStarted, OnTurnStarted);
	}

	private IEnumerable<BattleAction> OnTurnStarted(UnitEventArgs args)
	{
		if (Count > 0)
		{
			NotifyActivating();
			yield return new CastBlockShieldAction(Owner, new ShieldInfo(Count));
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
		Count += 3;
	}
}
