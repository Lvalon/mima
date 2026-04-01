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
	public sealed class sePurifierDef : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Positive;
			return config;
		}
	}

	[EntityLogic(typeof(sePurifierDef))]
	public sealed class sePurifier : StatusEffect
	{
		public override bool ForceNotShowDownText => true;
		protected override void OnAdded(Unit unit)
		{
			ReactOwnerEvent(Battle.ManaGained, OnManaGained);
		}

		private IEnumerable<BattleAction> OnManaGained(ManaEventArgs args)
		{
			if (args.Cause != ActionCause.TurnStart)
			{
				NotifyActivating();
				yield return ConvertManaAction.Purify(Battle.BattleMana, 1);
			}
		}
	}
}