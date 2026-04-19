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
using LBoL.EntityLib.Cards.Enemy;
using LBoL.EntityLib.EnemyUnits.Character;
using LBoL.EntityLib.StatusEffects.Basic;
using LBoL.EntityLib.StatusEffects.Cirno;
using LBoL.EntityLib.StatusEffects.Enemy;
using LBoLEntitySideloader.Attributes;

namespace lvalonmima.StatusEffects
{
	public sealed class seSanaeDef : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Positive;
			return config;
		}
	}

	[EntityLogic(typeof(seSanaeDef))]
	public sealed class seSanae : StatusEffect
	{
		public override bool ForceNotShowDownText => true;
		protected override void OnAdded(Unit unit)
		{
			React(new ApplyStatusEffectAction<Weak>(Battle.Player, 0, 3));
			React(new ApplyStatusEffectAction<Vulnerable>(Battle.Player, 0, 3));
			React(new ApplyStatusEffectAction<Fragil>(Battle.Player, 0, 3));
			ReactOwnerEvent(Battle.CardUsed, OnCardUsed);
		}

		private IEnumerable<BattleAction> OnCardUsed(CardUsingEventArgs args)
		{
			if (args.Card.CardType == CardType.Skill)
			{
				NotifyActivating();
				if (Battle.HandZone.Count > 0)
					yield return new DiscardAction(Battle.HandZone.FirstOrDefault());
				yield return new ApplyStatusEffectAction<TempFirepower>(Owner, 1);
				yield return new ApplyStatusEffectAction<TempFirepowerNegative>(Battle.Player, 1);
			}
		}
	}
}