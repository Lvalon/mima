using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle;
using LBoL.Core.Units;
using LBoL.Core;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using LBoL.EntityLib.StatusEffects.Cirno;
using LBoL.EntityLib.StatusEffects.Others;
using LBoL.EntityLib.StatusEffects.Sakuya;
//using lvalonmima.BattleActions;

namespace lvalonmima.lvalonmimaUlt
{
	public sealed class ultmimaaDef : lvalonmimaUltTemplate
	{
		public override UltimateSkillConfig MakeConfig()
		{
			UltimateSkillConfig config = GetDefaulUltConfig();
			config.Value1 = 2;
			config.Value2 = 9;
			config.PowerCost = 120;
			config.PowerPerLevel = 120;
			config.Keywords = Keyword.None;
			config.RelativeEffects = new List<string>() { nameof(Cold), nameof(Poison), nameof(TimeAuraSe) };
			return config;
		}
	}

	[EntityLogic(typeof(ultmimaaDef))]
	public sealed class ultmimaa : UltimateSkill
	{
		public ultmimaa()
		{
			base.TargetType = TargetType.AllEnemies;
			//base.GunName = GunNameID.GetGunFromId(7021); //盛宴B
		}

		protected override IEnumerable<BattleAction> Actions(UnitSelector selector)
		{
			yield return PerformAction.Spell(Battle.Player, "ultmimaa");
			for (int i = 0; i < Value1; i++)
			{
				foreach (Unit unit in Battle.AllAliveUnits)
				{
					if (!unit.IsAlive || Battle.BattleShouldEnd) { continue; }
					yield return new ApplyStatusEffectAction<Cold>(unit, 0, 0, 0, 0);
				}
			}
			foreach (Unit unit in Battle.AllAliveEnemies)
			{
				if (!unit.IsAlive || Battle.BattleShouldEnd) { continue; }
				yield return new ApplyStatusEffectAction<Poison>(unit, Value2, 0, 0, 0);
			}
			if (Battle.BattleShouldEnd) { yield break; }
			yield return new ApplyStatusEffectAction<TimeAuraSe>(Battle.Player, Value2, 0, 0, 0);
			yield break;
		}
	}
}
