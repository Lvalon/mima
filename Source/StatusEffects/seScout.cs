using System.Collections.Generic;
using System.Linq;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoLEntitySideloader.Attributes;

namespace lvalonmima.StatusEffects;

public sealed class seScoutDef : lvalonmimaStatusEffectTemplate
{
	public override StatusEffectConfig MakeConfig() => Cfg(StatusEffectType.Positive, hasCount: true);
}

[EntityLogic(typeof(seScoutDef))]
public sealed class seScout : StatusEffect
{
	public int limit => lim;
	int lim = 4;
	public override bool ForceNotShowDownText => true;
	protected override void OnAdded(Unit unit)
	{
		lim = 2 + Battle.AllAliveEnemies.Count() * 2; // 4 changed to 2 to remove self
		Count = lim;
		ReactOwnerEvent(Battle.CardUsed, OnCardUsed);
	}

	private IEnumerable<BattleAction> OnCardUsed(CardUsingEventArgs args)
	{
		if (Count >= 1)
		{
			Count--;
			if (Count == 0)
			{
				NotifyActivating();
				yield return new ApplyStatusEffectAction<LockedOn>(Battle.Player, 1);
				Count = lim;
			}
		}
		Highlight = Count == 1;
	}
}
