using System.Collections.Generic;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Others;
using LBoLEntitySideloader.Attributes;

namespace lvalonmima.StatusEffects
{
	public sealed class sealgophobia2Def : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Special;
			config.RelativeEffects = new List<string>() { nameof(Poison) };
			return config;
		}
	}

	[EntityLogic(typeof(sealgophobia2Def))]
	public sealed class sealgophobia2 : StatusEffect
	{
		protected override void OnAdded(Unit unit)
		{
			ReactOwnerEvent(Battle.Player.DamageReceived, OnDmgReceived);
		}

		private IEnumerable<BattleAction> OnDmgReceived(DamageEventArgs args)
		{
			if (args.DamageInfo.Amount > 0)
			{
				NotifyActivating();
				foreach (Unit unit in Battle.AllAliveEnemies)
				{
					if (!unit.IsAlive || Battle.BattleShouldEnd) { continue; }
					yield return new ApplyStatusEffectAction<Poison>(unit, Level, 0, 0, 0);
				}
			}
		}
	}
}