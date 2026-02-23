using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoLEntitySideloader.Attributes;

namespace lvalonmima.StatusEffects
{
	public sealed class sequest29Def : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Positive;
			return config;
		}
	}

	[EntityLogic(typeof(sequest29Def))]
	public sealed class sequest29 : StatusEffect
	{
		public override bool ForceNotShowDownText => true;
		protected override void OnAdded(Unit unit)
		{
			ReactOwnerEvent(Battle.Player.DamageDealt, OnDamageDealt);
		}
		private IEnumerable<BattleAction> OnDamageDealt(DamageEventArgs args)
		{
			if (args.Source == Owner && args.Target != Owner && args.DamageInfo.Damage > 0)
			{
				NotifyActivating();
				yield return new ApplyStatusEffectAction<TempFirepower>(Owner, Level);
			}
		}
	}
}