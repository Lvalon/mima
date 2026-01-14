using System;
using System.Collections.Generic;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;
using LBoL.EntityLib.Exhibits;
using LBoLEntitySideloader.Attributes;
using lvalonmima.StatusEffects;
using LBoL.Presentation;
using LBoL.Presentation.Units;
using LBoL.Presentation.UI.Widgets;
using UnityEngine;
using UnityEngine.UI;

namespace lvalonmima.Exhibits
{
	public sealed class exmimabDef : lvalonmimaExhibitTemplate
	{
		public override ExhibitConfig MakeConfig()
		{
			ExhibitConfig exhibitConfig = this.GetDefaultExhibitConfig();
			exhibitConfig.Mana = new ManaGroup() { Colorless = 1 };
			exhibitConfig.BaseManaColor = ManaColor.Colorless;
			exhibitConfig.Rarity = Rarity.Mythic;
			exhibitConfig.HasCounter = true;
			exhibitConfig.RelativeEffects = new List<string>() { nameof(secreative), nameof(seevilspirit), nameof(seabyss) };

			return exhibitConfig;
		}
	}

	[EntityLogic(typeof(exmimabDef))]
	public sealed class exmimab : ShiningExhibit
	{
		bool is0;
		protected override void OnAdded(PlayerUnit player)
		{
			GameRun.RewardAndShopCardColorLimitFlag += 1;
			Counter = 0;
		}
		protected override void OnRemoved(PlayerUnit player)
		{
			GameRun.RewardAndShopCardColorLimitFlag -= 1;
		}
		protected override void OnEnterBattle()
		{
			HandleBattleEvent(Battle.Player.Dying, OnDying, GameEventPriority.Highest + 5);
			HandleBattleEvent(Owner.DamageReceiving, OnDmgReceiving);
			ReactBattleEvent(Owner.DamageReceived, OnDmgReceived, GameEventPriority.Lowest - 1);
			HandleBattleEvent(Battle.BattleEnded, OnBattleEnded);
			HandleBattleEvent(Battle.RoundEnded, OnRoundEnded);
		}

		private void OnRoundEnded(GameEventArgs args)
		{
			Counter = 0;
		}

		private void OnBattleEnded(GameEventArgs args)
		{
			Counter = 0;
		}

		private void OnDmgReceiving(DamageEventArgs args)
		{
			is0 = Owner.Hp == 0;
		}

		private IEnumerable<BattleAction> OnDmgReceived(DamageEventArgs args)
		{
			if (is0)
			{
				Counter++;
				if (Counter >= Owner.MaxHp)
				{
					NotifyActivating();
					GameRun.SetHpAndMaxHp(1, 1, false);
				}
			}
			yield break;
		}

		private void OnDying(DieEventArgs args)
		{
			NotifyActivating();
			React(new ApplyStatusEffectAction<seabyss>(Battle.Player, 1, 0, 0, 0));
		}
	}
}