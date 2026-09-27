using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;
using LBoL.EntityLib.Exhibits;
using LBoLEntitySideloader.Attributes;
using lvalonmima.StatusEffects;

namespace lvalonmima.Exhibits;

public sealed class exmimaaDef : lvalonmimaExhibitTemplate
{
	public override ExhibitConfig MakeConfig()
	{
		ExhibitConfig exhibitConfig = this.GetDefaultExhibitConfig();
		exhibitConfig.Mana = new ManaGroup() { Philosophy = 1 };
		exhibitConfig.BaseManaColor = ManaColor.Philosophy;
		exhibitConfig.Rarity = Rarity.Mythic;
		exhibitConfig.RelativeEffects = [nameof(secreative), nameof(seevilspirit), nameof(setranscendence)];

		return exhibitConfig;
	}
}

[EntityLogic(typeof(exmimaaDef))]
public sealed class exmimaa : ShiningExhibit
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
		React(new ApplyStatusEffectAction<setranscendence>(Battle.Player, 1, 0, 0, 0));
	}
}
