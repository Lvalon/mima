using System.Collections.Generic;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Others;
using LBoLEntitySideloader.Attributes;

namespace lvalonmima.StatusEffects
{
	public sealed class sestone2Def : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Positive;
			return config;
		}
	}

	[EntityLogic(typeof(sestone2Def))]
	public sealed class sestone2 : StatusEffect
	{
		protected override void OnAdded(Unit unit)
		{
			ReactOwnerEvent(Battle.Player.DamageReceived, OnDmgReceived);
		}

		private IEnumerable<BattleAction> OnDmgReceived(DamageEventArgs args)
		{
			if (args.DamageInfo.IsGrazed)
			{
				NotifyActivating();
				yield return new CastBlockShieldAction(Battle.Player, Level, 0, BlockShieldType.Direct);
			}
		}
	}
}