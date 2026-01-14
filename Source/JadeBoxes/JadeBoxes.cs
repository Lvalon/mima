using System.Collections.Generic;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoLEntitySideloader.Attributes;
using lvalonmima.StatusEffects;

namespace lvalonmima.JadeBoxes
{
	public class JadeBoxCreative
	{
		public sealed class JadeBoxCreativeDef : lvalonmimajadeboxtemplate
		{
			public override JadeBoxConfig MakeConfig()
			{
				var config = DefaultConfig();
				return config;
			}
			[EntityLogic(typeof(JadeBoxCreativeDef))]
			public sealed class JadeBoxCreative : JadeBox
			{
				protected override void OnEnterBattle()
				{
					ReactBattleEvent(Battle.BattleStarted, OnBattleStated);
				}

				private IEnumerable<BattleAction> OnBattleStated(GameEventArgs args)
				{
					if (Battle.Player.HasStatusEffect<secreative>()) { yield break; }
					yield return new ApplyStatusEffectAction<secreative>(Battle.Player, 1, 0, 0, 0);
				}
			}
		}
	}
}
