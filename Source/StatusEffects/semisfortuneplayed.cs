using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;

namespace lvalonmima.StatusEffects;

public sealed class semisfortuneplayedDef : lvalonmimaStatusEffectTemplate
{
	public override StatusEffectConfig MakeConfig() => Cfg(StatusEffectType.Special, hasCount: true);
}

[EntityLogic(typeof(semisfortuneplayedDef))]
public sealed class semisfortuneplayed : CardTypeCounterSe
{
	protected override CardType WatchedType => CardType.Misfortune;
}
