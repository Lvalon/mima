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
using lvalonmima.Cards;
using lvalonmima.Exhibits;

namespace lvalonmima.StatusEffects
{
	public sealed class sehealreceivedDef : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Special;
			config.HasCount = true;
			return config;
		}
	}

	[EntityLogic(typeof(sehealreceivedDef))]
	public sealed class sehealreceived : StatusEffect
	{
		public override bool ForceNotShowDownText => true;
		protected override void OnAdded(Unit unit)
		{
			Count = 0;
			HandleOwnerEvent(unit.HealingReceived, OnHealReceived, GameEventPriority.Lowest - 100);
		}

		private void OnHealReceived(HealEventArgs args)
		{
			if (args.Amount > 0)
			{
				Count += (int)args.Amount;
				if (Battle.Player.HasExhibit<exquesting>() && Count == (int)args.Amount)
				{
					exquesting exhibit = Battle.Player.GetExhibit<exquesting>();
					cardquest25 card = Library.CreateCard<cardquest25>();
					if (exhibit.PendingQuestProgress.TryGetValue(card.Id, out var progress)
					&& Battle.Player.TryGetStatusEffect(out seabilityplayed abilityPlayed) && abilityPlayed.Count > 0
					&& Battle.Player.TryGetStatusEffect(out sedamagereceived damageReceived) && damageReceived.Count > 0
					&& Battle.Player.TryGetStatusEffect(out sehealreceived healReceived) && healReceived.Count > 0
					&& Battle.Player.TryGetStatusEffect(out seblockgained blockGained) && blockGained.Count > 0 && blockGained.Level > 0) // has all SE
					{
						exhibit.PendingQuestProgress[card.Id] = ++progress; // count progress
						if (progress >= card.Config.Value1) //reached goal
						{
							exhibit.PendingQuestModifiers.TryGetValue(card.Id, out int stack);
							exhibit.PendingQuestModifiers[card.Id] = ++stack; // add modifier
							exhibit.FinalizeQuestByCardId(card.Id); // finish quest
							exhibit.MarkQuestCompleted(card.Id);
						}
					}
				}
			}
		}
	}
}