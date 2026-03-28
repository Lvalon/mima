using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoLEntitySideloader.Attributes;
using lvalonmima.Cards;
using lvalonmima.Exhibits;
using lvalonmima.GunName;
using lvalonmima.Source.Patches;

namespace lvalonmima.StatusEffects
{
	public sealed class seholddamageDef : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Special;
			config.HasCount = true;
			return config;
		}
	}

	[EntityLogic(typeof(seholddamageDef))]
	public sealed class seholddamage : StatusEffect
	{
		public override bool ForceNotShowDownText => true;
		protected override void OnAdded(Unit unit)
		{
			HandleOwnerEvent(unit.DamageTaking, OnDamageTaking, GameEventPriority.Lowest - 10);
			ReactOwnerEvent(unit.TurnStarted, OnTurnStarted);
		}

		private IEnumerable<BattleAction> OnTurnStarted(GameEventArgs args)
		{
			bool willRmv = false;
			int gunid = 15160;
			int[] thresholds = { 0, 10, 25, 50, 100 };
			gunid += thresholds.Count(t => Count > toolbox.hpfrompercent(Owner, t));
			if (Battle.Player.HasExhibit<exquesting>())
			{
				exquesting exhibit = Battle.Player.GetExhibit<exquesting>();
				cardquest24 card = Library.CreateCard<cardquest24>();
				Highlight = false;
				if (exhibit.PendingQuestProgress.TryGetValue(card.Id, out var progress)
				&& Count > toolbox.hpfrompercent(Owner, 50)) // take >50% dmg
				{
					Highlight = true;
					exhibit.PendingQuestProgress[card.Id] = ++progress; // count progress
					if (progress >= card.Config.Value1) //reached goal
					{
						willRmv = true;
						foreach (Unit unit in Battle.AllAliveUnits.Where(u => u != Owner && u.HasStatusEffect<seholddamage>()))
						{
							if (unit.TryGetStatusEffect(out seholddamage holdDmg) && holdDmg.Count > 0)
							{ // resolve them before removing
								int tmpGun = 15160 + thresholds.Count(t => holdDmg.Count > toolbox.hpfrompercent(unit, t));
								yield return DamageAction.Reaction(unit, holdDmg.Count, GunNameID.GetGunFromId(tmpGun));
							}
							yield return new RemoveStatusEffectAction(unit.GetStatusEffect<seholddamage>());
						}
						exhibit.PendingQuestModifiers.TryGetValue(card.Id, out int stack);
						exhibit.PendingQuestModifiers[card.Id] = ++stack; // add modifier
						exhibit.FinalizeQuestByCardId(card.Id); // finish quest
						exhibit.MarkQuestCompleted(card.Id);
						ShopModHandlers.RecordRewardedQuestCompletion(card.Id);
					}
				}
			}
			if (Count > 0)
			{
				NotifyActivating();
				yield return DamageAction.Reaction(Owner, Count, GunNameID.GetGunFromId(gunid));
				Count = 0;
				Highlight = false;
			}
			if (willRmv)
				yield return new RemoveStatusEffectAction(this);
		}

		private void OnDamageTaking(DamageEventArgs args)
		{
			if (args.ActionSource == this) { return; }
			int num = args.DamageInfo.Damage.RoundToInt();
			if (num > 0)
			{
				NotifyActivating();
				Count += num;
				if (Battle.Player.HasExhibit<exquesting>())
				{
					exquesting exhibit = Battle.Player.GetExhibit<exquesting>();
					cardquest24 card = Library.CreateCard<cardquest24>();
					if (exhibit.PendingQuestProgress.ContainsKey(card.Id)
					&& Count > toolbox.hpfrompercent(Owner, 50)) // take >50% dmg
					{
						Highlight = Count > toolbox.hpfrompercent(Owner, 50);
					}
				}
				args.DamageInfo = args.DamageInfo.ReduceActualDamageBy(num);
				args.AddModifier(this);
			}
		}
	}
}