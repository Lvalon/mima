using System.Collections.Generic;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoLEntitySideloader.Attributes;

namespace lvalonmima.StatusEffects;

public sealed class seTerminatorDef : lvalonmimaStatusEffectTemplate
{
	public override StatusEffectConfig MakeConfig() => Cfg(StatusEffectType.Positive);
}

[EntityLogic(typeof(seTerminatorDef))]
public sealed class seTerminator : StatusEffect
{
	public override bool ForceNotShowDownText => true;
	protected override void OnAdded(Unit unit)
	{
		ReactOwnerEvent(Battle.Player.StatusEffectAdded, OnSEAdded);
	}

	private IEnumerable<BattleAction> OnSEAdded(StatusEffectApplyEventArgs args)
	{
		if (args.Effect is Graze)
		{
			NotifyActivating();
			yield return new ApplyStatusEffectAction<LockedOn>(Battle.Player, 1);
		}
	}
}
