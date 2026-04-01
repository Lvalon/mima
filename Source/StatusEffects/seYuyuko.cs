using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.EnemyUnits.Character;
using LBoL.EntityLib.StatusEffects.Cirno;
using LBoL.EntityLib.StatusEffects.Enemy;
using LBoLEntitySideloader.Attributes;

namespace lvalonmima.StatusEffects
{
	public sealed class seYuyukoDef : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Positive;
			return config;
		}
	}

	[EntityLogic(typeof(seYuyukoDef))]
	public sealed class seYuyuko : StatusEffect
	{
		public override bool ForceNotShowDownText => true;
		protected override void OnAdded(Unit unit)
		{
			ReactOwnerEvent(Battle.Player.DamageReceived, OnDamageReceived);
		}

		private IEnumerable<BattleAction> OnDamageReceived(DamageEventArgs args)
		{
			if (args.DamageInfo.Damage <= 0) yield break;
			List<string> se = new List<string>()
			{
				nameof(Vulnerable),
				nameof(Weak),
				nameof(Fragil),
			};
			string chosen = se.SampleOrDefault(GameRun.EnemyBattleRng);
			if (chosen != null)
			{
				NotifyActivating();
				switch (chosen)
				{
					case nameof(Vulnerable):
						yield return new ApplyStatusEffectAction<Vulnerable>(Battle.Player, 0, 1);
						break;
					case nameof(Weak):
						yield return new ApplyStatusEffectAction<Weak>(Battle.Player, 0, 1);
						break;
					case nameof(Fragil):
						yield return new ApplyStatusEffectAction<Fragil>(Battle.Player, 0, 1);
						break;
				}
			}
		}
	}
}