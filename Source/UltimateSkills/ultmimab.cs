using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle;
using LBoL.Core.Units;
using LBoL.Core;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;

namespace lvalonmima.lvalonmimaUlt
{
	public sealed class ultmimabDef : lvalonmimaUltTemplate
	{
		public override UltimateSkillConfig MakeConfig()
		{
			UltimateSkillConfig config = GetDefaulUltConfig();
			config.Keywords = Keyword.None;
			config.PowerCost = 150;
			config.PowerPerLevel = 150;
			config.Value1 = 0;
			return config;
		}
	}

	[EntityLogic(typeof(ultmimabDef))]
	public sealed class ultmimab : UltimateSkill
	{
		public ultmimab()
		{
			base.TargetType = TargetType.AllEnemies;
			//base.GunName = GunNameID.GetGunFromId(7071); //鬼气狂澜B
		}

		protected override IEnumerable<BattleAction> Actions(UnitSelector selector)
		{
			yield return PerformAction.Spell(Battle.Player, "ultmimab");
			bool gohard = Battle.Player.Hp == 0;
			int diff = Battle.Player.Hp;
			yield return new DamageAction(Battle.Player, Battle.Player, DamageInfo.HpLose(diff), "Sacrifice");
			if (Battle.BattleShouldEnd) { yield break; }
			yield return new DamageAction(Battle.Player, Battle.AllAliveEnemies, DamageInfo.HpLose(gohard ? Battle.Player.MaxHp : diff));
		}
	}
}
