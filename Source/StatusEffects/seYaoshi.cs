using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoLEntitySideloader.Attributes;

namespace lvalonmima.StatusEffects;

public sealed class seYaoshiDef : lvalonmimaStatusEffectTemplate
{
	public override StatusEffectConfig MakeConfig() => Cfg(StatusEffectType.Positive);
}

[EntityLogic(typeof(seYaoshiDef))]
public sealed class seYaoshi : StatusEffect
{
	public override bool ForceNotShowDownText => true;
	protected override void OnAdded(Unit unit)
	{
		foreach (EnemyUnit mf in Battle.AllAliveEnemies)
			HandleOwnerEvent(mf.DamageTaking, OnDamageTaking, GameEventPriority.Lowest - 100);
		HandleOwnerEvent(Battle.EnemySpawned, OnEnemySpawned);
	}

	private void OnEnemySpawned(UnitEventArgs args)
	{
		HandleOwnerEvent(args.Unit.DamageTaking, OnDamageTaking, GameEventPriority.Lowest - 100);
	}

	private void OnDamageTaking(DamageEventArgs args)
	{
		int num = args.DamageInfo.Damage.RoundToInt();
		if (num > 0 && toolbox.Round(args.Target.MaxHp * 0.2) < num)
		{
			NotifyActivating();
			args.DamageInfo = args.DamageInfo.ReduceActualDamageBy(num - toolbox.Round(args.Target.MaxHp * 0.2));
			args.AddModifier(this);
		}
	}
}
