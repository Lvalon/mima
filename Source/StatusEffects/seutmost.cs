using System.Collections.Generic;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoLEntitySideloader.Attributes;

namespace lvalonmima.StatusEffects
{
	public sealed class seutmostDef : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Positive;
			config.RelativeEffects = new List<string>() { nameof(semburst) };
			return config;
		}
	}

	[EntityLogic(typeof(seutmostDef))]
	public sealed class seutmost : StatusEffect
	{
		protected override void OnAdded(Unit unit)
		{
			ReactOwnerEvent(Battle.CardUsed, OnCardUsed);
		}
		private IEnumerable<BattleAction> OnCardUsed(CardUsingEventArgs args)
		{
			NotifyActivating();
			yield return new ApplyStatusEffectAction<semburst>(Battle.Player, Level, 0, 0, 0);
		}
	}
}