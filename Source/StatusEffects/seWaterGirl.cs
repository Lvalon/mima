using System.Collections.Generic;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Basic;
using LBoLEntitySideloader.Attributes;

namespace lvalonmima.StatusEffects;

public sealed class seWaterGirlDef : lvalonmimaStatusEffectTemplate
{
	public override StatusEffectConfig MakeConfig() => Cfg(StatusEffectType.Positive);
}

[EntityLogic(typeof(seWaterGirlDef))]
public sealed class seWaterGirl : StatusEffect
{
	public override bool ForceNotShowDownText => true;
	protected override void OnAdded(Unit unit)
	{
		ReactOwnerEvent(unit.DamageDealt, OnDamageDealt);
	}

	private IEnumerable<BattleAction> OnDamageDealt(DamageEventArgs args)
	{
		if (args.DamageInfo.Damage <= 0)
			yield break;
		NotifyActivating();
		if (Battle.Player.TryGetStatusEffect<Drowning>(out var se))
			se.Level++;
	}
}
