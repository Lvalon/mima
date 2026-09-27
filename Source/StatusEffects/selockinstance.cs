using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoLEntitySideloader.Attributes;

namespace lvalonmima.StatusEffects;

public sealed class selockinstanceDef : lvalonmimaStatusEffectTemplate
{
	public override StatusEffectConfig MakeConfig() => Cfg(StatusEffectType.Special);
}

[EntityLogic(typeof(selockinstanceDef))]
public sealed class selockinstance : StatusEffect
{
	public override bool ForceNotShowDownText => true;
	protected override void OnAdded(Unit unit)
	{
		Highlight = true;
		HandleOwnerEvent(unit.DamageTaking, OnDamageTaking, GameEventPriority.Lowest - 1);
	}
	public void OnDamageTaking(DamageEventArgs args)
	{
		int num = args.DamageInfo.Damage.RoundToInt();
		if (num > 0)
		{
			NotifyActivating();
			args.DamageInfo = args.DamageInfo.ReduceActualDamageBy(num);
			args.AddModifier(this);
		}
	}
}
