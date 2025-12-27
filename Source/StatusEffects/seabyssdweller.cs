using System.Collections.Generic;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoLEntitySideloader.Attributes;

namespace lvalonmima.StatusEffects
{
	public sealed class seabyssdwellerDef : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Special;
			return config;
		}
	}

	[EntityLogic(typeof(seabyssdwellerDef))]
	public sealed class seabyssdweller : StatusEffect
	{
		protected override void OnAdded(Unit unit)
		{
			ReactOwnerEvent(Battle.Player.TurnEnded, OnTurnEnded);
			ReactOwnerEvent(Battle.CardExiled, OnCardExiled);
		}

		private IEnumerable<BattleAction> OnCardExiled(CardEventArgs args)
		{
			yield return new MoveCardAction(args.Card, CardZone.Discard);
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
		}
	}
}