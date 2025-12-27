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
	public sealed class setranscendenceDef : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Positive;
			return config;
		}
	}

	[EntityLogic(typeof(setranscendenceDef))]
	public sealed class setranscendence : StatusEffect
	{
		public ManaGroup Mana => new ManaGroup() { Philosophy = 1 };
		protected override void OnAdded(Unit unit)
		{
			Highlight = true;
			ReactOwnerEvent(Battle.Player.DamageReceived, OnDamageReceived);
			ReactOwnerEvent(Battle.Player.TurnEnded, OnTurnEnded);
			ReactOwnerEvent(Battle.CardUsed, OnCardUsed);
		}

		private IEnumerable<BattleAction> OnCardUsed(CardUsingEventArgs args)
		{
			if (Battle.Player.TryGetStatusEffect(out seperlereino se))
			{
				NotifyActivating();
				for (int i = 0; i < se.Level; i++)
				{
					yield return new DrawCardAction();
					yield return new GainManaAction(Mana);
				}
			}
		}

		private IEnumerable<BattleAction> OnDamageReceived(DamageEventArgs args)
		{
			NotifyActivating();
			if (Battle.BattleShouldEnd) { yield break; }
			yield return new DrawCardAction();
			if (Battle.BattleShouldEnd) { yield break; }
			yield return new GainManaAction(Mana);
		}
		private IEnumerable<BattleAction> OnTurnEnded(UnitEventArgs args)
		{
			if (Level <= 1)
			{
				yield return new RemoveStatusEffectAction(this);
			}
			else
			{
				Level--;
			}
			yield break;
		}
	}
}