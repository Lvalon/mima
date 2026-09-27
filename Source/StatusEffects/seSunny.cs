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

public sealed class seSunnyDef : lvalonmimaStatusEffectTemplate
{
	public override StatusEffectConfig MakeConfig() => Cfg(StatusEffectType.Positive);
}

[EntityLogic(typeof(seSunnyDef))]
public sealed class seSunny : StatusEffect
{
	public override bool ForceNotShowDownText => true;
	protected override void OnAdded(Unit unit)
	{
		foreach (EnemyUnit mf in Battle.AllAliveEnemies)
			ReactOwnerEvent(mf.Died, OnDied);
		HandleOwnerEvent(Battle.EnemySpawned, OnSpawned);
	}

	private void OnSpawned(UnitEventArgs args)
	{
		ReactOwnerEvent(args.Unit.Died, OnDied);
	}

	private IEnumerable<BattleAction> OnDied(DieEventArgs args)
	{
		NotifyActivating();
		yield return new ApplyStatusEffectAction<Firepower>(Owner, 1);
	}
}
