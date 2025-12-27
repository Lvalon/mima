using System.Collections.Generic;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;
using LBoL.EntityLib.Exhibits;
using LBoLEntitySideloader.Attributes;
using lvalonmima.StatusEffects;

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
			exhibitConfig.RelativeEffects = new List<string>() { nameof(secreative), nameof(seevilspirit), nameof(seabyss) };

			return exhibitConfig;
		}
	}

	[EntityLogic(typeof(exmimabDef))]
	public sealed class exmimab : ShiningExhibit
	{
		protected override void OnAdded(PlayerUnit player)
		{
			GameRun.RewardAndShopCardColorLimitFlag += 1;
		}
		protected override void OnRemoved(PlayerUnit player)
		{
			GameRun.RewardAndShopCardColorLimitFlag -= 1;
		}
		protected override void OnEnterBattle()
		{
			HandleBattleEvent(Battle.Player.Dying, OnDying, GameEventPriority.Highest + 5);
		}

		private void OnDying(DieEventArgs args)
		{
			NotifyActivating();
			React(new ApplyStatusEffectAction<seabyss>(Battle.Player, 1, 0, 0, 0));
		}
	}
}