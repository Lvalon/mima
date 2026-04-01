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
	public sealed class seKoSeriousDef : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Positive;
			config.Order = 10;
			return config;
		}
	}

	[EntityLogic(typeof(seKoSeriousDef))]
	public sealed class seKoSerious : StatusEffect
	{
		protected override void OnAdded(Unit unit)
		{
			Highlight = Owner.HasStatusEffect<KokoroRenzhen>();
			ReactOwnerEvent(unit.StatusEffectAdded, OnSEAdded);
			HandleOwnerEvent(unit.StatusEffectRemoved, OnSERemoved);
			HandleOwnerEvent(Battle.Predraw, OnPredraw);
			HandleOwnerEvent(Battle.ManaGaining, OnManaGaining);
		}

		private void OnManaGaining(ManaEventArgs args)
		{
			if (args.Cause != ActionCause.TurnStart)
			{
				NotifyActivating();
				args.CancelBy(this);
			}
		}

		private void OnPredraw(CardEventArgs args)
		{
			if (args.Cause != ActionCause.TurnStart && !(args.ActionSource is Card card && card.IsReplenish) && Battle.EnumerateAllCardsButExile().Count(c => c.CardType == CardType.Status) < 2)
			{
				NotifyActivating();
				args.CancelBy(this);
			}
		}

		private void OnSERemoved(StatusEffectEventArgs args)
		{
			Highlight = Owner.HasStatusEffect<KokoroRenzhen>();
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
				case KokoroXi _:
					yield return new ApplyStatusEffectAction<seKoXi>(Owner, 1);
					yield return new RemoveStatusEffectAction(this);
					break;
			}
			Highlight = Owner.HasStatusEffect<KokoroRenzhen>();
		}
	}
}