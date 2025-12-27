using System.Collections.Generic;
using System.Linq;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Sakuya;
using LBoLEntitySideloader.Attributes;

namespace lvalonmima.StatusEffects
{
	public sealed class seconversionDef : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Positive;
			config.RelativeEffects = new List<string>() { nameof(TimeAuraSe), nameof(seunder) };
			return config;
		}
	}

	[EntityLogic(typeof(seconversionDef))]
	public sealed class seconversion : sehl25
	{
		protected override void OnAdded(Unit unit)
		{
			ReactOwnerEvent(Battle.Player.TurnStarted, OnTurnStarted);
		}

		private IEnumerable<BattleAction> OnTurnStarted(UnitEventArgs args)
		{
			int mult = Battle.AllAliveEnemies.Count();
			if (mult > 0 || mult == 0) { yield break; }
			yield return DamageAction.LoseLife(Battle.Player, mult * Level);
			if (Battle.BattleShouldEnd) { yield break; }
			yield return new ApplyStatusEffectAction<TimeAuraSe>(Battle.Player, Level * mult * (BepinexPlugin.u25 ? 2 : 1), 0, 0, 0);
		}
	}
}