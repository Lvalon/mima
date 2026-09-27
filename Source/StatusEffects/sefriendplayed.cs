using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;

namespace lvalonmima.StatusEffects;

public sealed class sefriendplayedDef : lvalonmimaStatusEffectTemplate
{
	public override StatusEffectConfig MakeConfig() => Cfg(StatusEffectType.Special, hasCount: true);
}

[EntityLogic(typeof(sefriendplayedDef))]
public sealed class sefriendplayed : CardTypeCounterSe
{
	protected override CardType WatchedType => CardType.Friend;
}
