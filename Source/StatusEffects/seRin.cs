using System.Collections.Generic;
using System.Linq;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.EnemyUnits.Normal.Guihuos;
using LBoLEntitySideloader.Attributes;

namespace lvalonmima.StatusEffects;

public sealed class seRinDef : lvalonmimaStatusEffectTemplate
{
	public override StatusEffectConfig MakeConfig() => Cfg(StatusEffectType.Positive);
}

[EntityLogic(typeof(seRinDef))]
public sealed class seRin : StatusEffect
{
	public override bool ForceNotShowDownText => true;
	protected override void OnAdded(Unit unit)
	{
		foreach (EnemyUnit mf in Battle.AllAliveEnemies.Where(e => e is Guihuo))
			ReactOwnerEvent(mf.Died, OnDied);
		HandleOwnerEvent(Battle.EnemySpawned, OnSpawned);
	}

	private void OnSpawned(UnitEventArgs args)
	{
		if (args.Unit is Guihuo)
			ReactOwnerEvent(args.Unit.Died, OnDied);
	}

	private IEnumerable<BattleAction> OnDied(DieEventArgs args)
	{
		NotifyActivating();
		foreach (EnemyUnit unit in Battle.AllAliveEnemies.Where(e => e is Guihuo && e != args.Unit))
			yield return new ApplyStatusEffectAction<Firepower>(unit, 1);
		yield return new ApplyStatusEffectAction<FirepowerNegative>(Owner, 1);
	}
}
