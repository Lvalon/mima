using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoLEntitySideloader.Attributes;

namespace lvalonmima.StatusEffects;

public sealed class sehauntedDef : lvalonmimaStatusEffectTemplate
{
	public override StatusEffectConfig MakeConfig() => Cfg(StatusEffectType.Special);
}

[EntityLogic(typeof(sehauntedDef))]
public sealed class sehaunted : StatusEffect
{
	public override bool ForceNotShowDownText => true;
	protected override void OnAdded(Unit unit)
	{
		HandleOwnerEvent(unit.Dying, OnDying);
	}
	private void OnDying(DieEventArgs args)
	{
		if (Battle.BattleShouldEnd || (args.DieCause != DieCause.Attack && args.DieCause != DieCause.Reaction && args.DieCause != DieCause.LoseHp))
			return;
		NotifyActivating();
		GameRun.SetEnemyHpAndMaxHp(toolbox.Round(1f * Owner.MaxHp / 2), Owner.MaxHp, (EnemyUnit)Owner, true);
		args.CancelBy(this);
		React(new RemoveStatusEffectAction(this));
	}
}
