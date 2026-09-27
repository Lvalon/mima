using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;

namespace lvalonmima.StatusEffects;

public sealed class sestatusplayedDef : lvalonmimaStatusEffectTemplate
{
	public override StatusEffectConfig MakeConfig() => Cfg(StatusEffectType.Special, hasCount: true);
}

[EntityLogic(typeof(sestatusplayedDef))]
public sealed class sestatusplayed : CardTypeCounterSe
{
	protected override CardType WatchedType => CardType.Status;
}
