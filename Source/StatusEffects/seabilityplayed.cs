using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Base;
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
	public sealed class seabilityplayedDef : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Special;
			config.HasCount = true;
			return config;
		}
	}

	[EntityLogic(typeof(seabilityplayedDef))]
	public sealed class seabilityplayed : StatusEffect
	{
		protected override void OnAdded(Unit unit)
		{
			Count = 0;
			HandleOwnerEvent(Battle.CardUsed, OnCardUsed);
		}

		private void OnCardUsed(CardUsingEventArgs args)
		{
			if (args.Card.CardType == CardType.Ability)
			{
				Count++;
				if (Count == Level)
				{
					Highlight = true;
				}
				else
				{
					Highlight = false;
				}
			}
		}
	}
}