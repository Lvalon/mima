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
	public sealed class seKoYouDef : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Positive;
			config.Order = 10;
			return config;
		}
	}

	[EntityLogic(typeof(seKoYouDef))]
	public sealed class seKoYou : StatusEffect
	{
		protected override void OnAdded(Unit unit)
		{
			Highlight = Owner.HasStatusEffect<KokoroYou>();
			ReactOwnerEvent(unit.StatusEffectAdded, OnSEAdded);
			HandleOwnerEvent(unit.StatusEffectRemoved, OnSERemoved);
			ReactOwnerEvent(Battle.ManaGained, OnManaGained);
		}

		private IEnumerable<BattleAction> OnManaGained(ManaEventArgs args)
		{
			if (args.Cause != ActionCause.TurnStart && Battle.HandZone.Count > 0)
			{
				NotifyActivating();
				if (Battle.HandZone.Count > 0)
					yield return new DiscardAction(Battle.HandZone.FirstOrDefault());
			}
		}

		private void OnSERemoved(StatusEffectEventArgs args)
		{
			Highlight = Owner.HasStatusEffect<KokoroYou>();
		}

		private IEnumerable<BattleAction> OnSEAdded(StatusEffectApplyEventArgs args)
		{
			switch (args.Effect)
			{
				case KokoroNu _:
					yield return new ApplyStatusEffectAction<seKoNu>(Owner, 1);
					yield return new RemoveStatusEffectAction(this);
					break;
				case KokoroXi _:
					yield return new ApplyStatusEffectAction<seKoXi>(Owner, 1);
					yield return new RemoveStatusEffectAction(this);
					break;
				case KokoroRenzhen _:
					yield return new ApplyStatusEffectAction<seKoSerious>(Owner, 1);
					yield return new RemoveStatusEffectAction(this);
					break;
			}
			Highlight = Owner.HasStatusEffect<KokoroYou>();
		}
	}
}