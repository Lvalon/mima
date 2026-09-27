using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;

namespace lvalonmima.StatusEffects;

public sealed class sedefenseplayedDef : lvalonmimaStatusEffectTemplate
{
	public override StatusEffectConfig MakeConfig() => Cfg(StatusEffectType.Special, hasCount: true);
}

[EntityLogic(typeof(sedefenseplayedDef))]
public sealed class sedefenseplayed : CardTypeCounterSe
{
	protected override CardType WatchedType => CardType.Defense;
}
