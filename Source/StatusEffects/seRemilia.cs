using System.Collections.Generic;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Basic;
using LBoLEntitySideloader.Attributes;

namespace lvalonmima.StatusEffects;

public sealed class seRemiliaDef : lvalonmimaStatusEffectTemplate
{
	public override StatusEffectConfig MakeConfig() => Cfg(StatusEffectType.Positive);
}

[EntityLogic(typeof(seRemiliaDef))]
public sealed class seRemilia : StatusEffect
{
	float tmp;
	public override bool ForceNotShowDownText => true;
	protected override void OnAdded(Unit unit)
	{
		tmp = 0;
		React(new ApplyStatusEffectAction<Vampire>(Owner));
		ReactOwnerEvent(unit.DamageReceived, OnDmgReceived);
		HandleOwnerEvent(unit.DamageReceiving, OnDamageReceiving, GameEventPriority.Highest + 100);
	}

	private void OnDamageReceiving(DamageEventArgs args)
	{
		tmp = args.DamageInfo.Damage;
	}

	private IEnumerable<BattleAction> OnDmgReceived(DamageEventArgs args)
	{
		if (args.DamageInfo.IsGrazed)
		{
			NotifyActivating();
			yield return new HealAction(Owner, Owner, toolbox.Round(tmp * 1.0 / 2));
		}
		tmp = 0;
	}
}
