using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;

namespace lvalonmima.StatusEffects;

public sealed class seattackplayedDef : lvalonmimaStatusEffectTemplate
{
	public override StatusEffectConfig MakeConfig() => Cfg(StatusEffectType.Special, hasCount: true);
}

[EntityLogic(typeof(seattackplayedDef))]
public sealed class seattackplayed : CardTypeCounterSe
{
	protected override CardType WatchedType => CardType.Attack;
}
