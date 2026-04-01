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
	public sealed class seKoNuDef : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Positive;
			config.Order = 10;
			return config;
		}
	}

	[EntityLogic(typeof(seKoNuDef))]
	public sealed class seKoNu : StatusEffect
	{
		protected override void OnAdded(Unit unit)
		{
			Highlight = Owner.HasStatusEffect<KokoroNu>();
			ReactOwnerEvent(unit.StatusEffectAdded, OnSEAdded);
			HandleOwnerEvent(unit.StatusEffectRemoved, OnSERemoved);
			ReactOwnerEvent(Battle.CardDrawn, OnCardDrawn);
		}
		private IEnumerable<BattleAction> OnCardDrawn(CardEventArgs args)
		{
			if (args.Cause != ActionCause.TurnStart && !(args.ActionSource is Card card && card.IsReplenish))
			{
				NotifyActivating();
				foreach (EnemyUnit enemy in Battle.AllAliveEnemies)
				{
					yield return new HealAction(Owner, enemy, 1);
				}
				yield return DamageAction.Reaction(Battle.Player, 1);
			}
		}

		private void OnSERemoved(StatusEffectEventArgs args)
		{
			Highlight = Owner.HasStatusEffect<KokoroNu>();
		}

		private IEnumerable<BattleAction> OnSEAdded(StatusEffectApplyEventArgs args)
		{
			switch (args.Effect)
			{
				case KokoroXi _:
					yield return new ApplyStatusEffectAction<seKoXi>(Owner, 1);
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
			Highlight = Owner.HasStatusEffect<KokoroNu>();
		}
	}
}