using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.Presentation;
using LBoL.Presentation.Bullet;
using LBoL.Presentation.UI.Panels;
using LBoL.Presentation.Units;
using LBoLEntitySideloader.Attributes;
using lvalonmima.Exhibits;
using lvalonmima.GunName;
using lvalonmima.SFX;
using lvalonmima.SFX.Template;
using UnityEngine;
using UnityEngine.UI;
using LBoLEntitySideloader.Resource;
using System.Collections;
using lvalonmima.Patches;

namespace lvalonmima.StatusEffects
{
	public sealed class sehauntedDef : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Special;
			return config;
		}
	}

	[EntityLogic(typeof(sehauntedDef))]
	public sealed class sehaunted : StatusEffect
	{
		public override bool ForceNotShowDownText => true;
		protected override void OnAdded(Unit unit)
		{
			HandleOwnerEvent(unit.Dying, OnDying);
		}
		private void OnDying(DieEventArgs args)
		{
			if (Battle.BattleShouldEnd || (args.DieCause != DieCause.Attack && args.DieCause != DieCause.Reaction && args.DieCause != DieCause.LoseHp))
				return;
			NotifyActivating();
			GameRun.SetEnemyHpAndMaxHp(toolbox.Round(1f * Owner.MaxHp / 2), Owner.MaxHp, (EnemyUnit)Owner, true);
			args.CancelBy(this);
			React(new RemoveStatusEffectAction(this));
		}
	}
}