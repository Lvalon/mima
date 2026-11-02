using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoLEntitySideloader.Attributes;

namespace lvalonmima.StatusEffects;

public sealed class seSeijaDef : lvalonmimaStatusEffectTemplate
{
	public override StatusEffectConfig MakeConfig() => Cfg(StatusEffectType.Positive, hasCount: true);
}

[EntityLogic(typeof(seSeijaDef))]
public sealed class seSeija : StatusEffect
{
	public override bool ForceNotShowDownText => true;
	protected override void OnAdded(Unit unit)
	{
		GameRun.SetEnemyHpAndMaxHp(unit.Hp * 2, unit.MaxHp * 2, (EnemyUnit)unit, true);
	}
}
