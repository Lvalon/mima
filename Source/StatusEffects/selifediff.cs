using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoLEntitySideloader.Attributes;

namespace lvalonmima.StatusEffects;

public sealed class selifediffDef : lvalonmimaStatusEffectTemplate
{
	public override StatusEffectConfig MakeConfig() => Cfg(StatusEffectType.Special, hasCount: true);
}

[EntityLogic(typeof(selifediffDef))]
public sealed class selifediff : StatusEffect
{
	protected override void OnAdded(Unit unit)
	{
		Count = unit.Hp;
		HandleOwnerEvent(unit.HealingReceived, OnhealingReceived);
		HandleOwnerEvent(unit.DamageReceived, OndamageReceived);
	}

	private void OndamageReceived(DamageEventArgs args)
	{
		UpdateLifeCount();
	}

	private void OnhealingReceived(HealEventArgs args)
	{
		UpdateLifeCount();
	}
	private void UpdateLifeCount()
	{
		Count = Owner.Hp;
	}
}
