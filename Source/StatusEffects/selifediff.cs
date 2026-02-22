using System.Collections.Generic;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Core;
using System.Linq;
using LBoL.Core.Battle;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Others;
using LBoLEntitySideloader.Attributes;
using System;

namespace lvalonmima.StatusEffects
{
	public sealed class selifediffDef : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Special;
			config.HasCount = true;
			return config;
		}
	}

	[EntityLogic(typeof(selifediffDef))]
	public sealed class selifediff : StatusEffect
	{
		protected override void OnAdded(Unit unit)
		{
			Count = unit.Hp;
			HandleOwnerEvent(unit.HealingReceived, OnhealingReceived);
			HandleOwnerEvent(unit.DamageReceived, OndamageReceived);
		}

		private void OndamageReceived(DamageEventArgs args)
		{
			UpdateLifeCount();
		}

		private void OnhealingReceived(HealEventArgs args)
		{
			UpdateLifeCount();
		}
		private void UpdateLifeCount()
		{
			Count = Owner.Hp;
		}
	}
}