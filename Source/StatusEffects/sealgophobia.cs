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
	public sealed class sealgophobiaDef : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Special;
			//config.Keywords = Keyword.Exile;
			return config;
		}
	}

	[EntityLogic(typeof(sealgophobiaDef))]
	public sealed class sealgophobia : StatusEffect
	{
		protected override void OnAdded(Unit unit)
		{
			ReactOwnerEvent(Battle.Player.DamageReceived, OnDmgReceived);
		}

		private IEnumerable<BattleAction> OnDmgReceived(DamageEventArgs args)
		{
			if (args.DamageInfo.Amount > 0 && args.ActionSource != this)
			{
				NotifyActivating();
				yield return new DamageAction(Battle.Player, new List<Unit> { Battle.Player }, DamageInfo.HpLose(Level));
			}
		}
	}
}