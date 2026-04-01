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
using LBoL.EntityLib.StatusEffects.Enemy;
using LBoL.EntityLib.StatusEffects.Others;
using LBoLEntitySideloader.Attributes;

namespace lvalonmima.StatusEffects
{
	public sealed class seKoXiDef : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Positive;
			config.Order = 10;
			return config;
		}
	}

	[EntityLogic(typeof(seKoXiDef))]
	public sealed class seKoXi : StatusEffect
	{
		bool has = false;
		protected override void OnAdded(Unit unit)
		{
			has = false;
			Highlight = Owner.HasStatusEffect<KokoroXi>();
			ReactOwnerEvent(unit.StatusEffectAdded, OnSEAdded);
			HandleOwnerEvent(unit.StatusEffectRemoved, OnSERemoved);
			HandleOwnerEvent(unit.DamageTaking, OnDamageTaking);
			ReactOwnerEvent(unit.DamageReceived, OnDamageReceived);
		}

		private IEnumerable<BattleAction> OnDamageReceived(DamageEventArgs args)
		{
			if (has && Highlight)
			{
				NotifyActivating();
				yield return new ApplyStatusEffectAction<Graze>(Owner, 1);
			}
			has = false;
		}

		private void OnDamageTaking(DamageEventArgs args)
		{
			if (args.DamageInfo.Damage > 0 && Highlight)
				has = true;
		}

		private void OnSERemoved(StatusEffectEventArgs args)
		{
			Highlight = Owner.HasStatusEffect<KokoroXi>();
		}

		private IEnumerable<BattleAction> OnSEAdded(StatusEffectApplyEventArgs args)
		{
			switch (args.Effect)
			{
				case KokoroNu _:
					yield return new ApplyStatusEffectAction<seKoNu>(Owner, 1);
					yield return new RemoveStatusEffectAction(this);
					break;
				case KokoroYou _:
					yield return new ApplyStatusEffectAction<seKoYou>(Owner, 1);
					yield return new RemoveStatusEffectAction(this);
					break;
				case KokoroRenzhen _:
					yield return new ApplyStatusEffectAction<seKoSerious>(Owner, 1);
					yield return new RemoveStatusEffectAction(this);
					break;
			}
			Highlight = Owner.HasStatusEffect<KokoroXi>();
		}
	}
}