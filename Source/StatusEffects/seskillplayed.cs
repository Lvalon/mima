using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;

namespace lvalonmima.StatusEffects;

public sealed class seskillplayedDef : lvalonmimaStatusEffectTemplate
{
	public override StatusEffectConfig MakeConfig() => Cfg(StatusEffectType.Special, hasCount: true);
}

[EntityLogic(typeof(seskillplayedDef))]
public sealed class seskillplayed : CardTypeCounterSe
{
	protected override CardType WatchedType => CardType.Skill;
}
