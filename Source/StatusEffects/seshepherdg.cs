using System.Collections.Generic;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Cirno;
using LBoLEntitySideloader.Attributes;

namespace lvalonmima.StatusEffects
{
	public sealed class seshepherdgDef : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Special;
			config.RelativeEffects = new List<string>() { nameof(Cold) };
			return config;
		}
	}

	[EntityLogic(typeof(seshepherdgDef))]
	public sealed class seshepherdg : StatusEffect
	{
		public ManaGroup Mana
		{
			get
			{
				if (Owner == null)
				{
					return new ManaGroup() { Green = 1 };
				}
				else
				{
					return new ManaGroup() { Green = Level };
				}
			}
		}
		protected override void OnAdded(Unit unit)
		{
			ReactOwnerEvent(Battle.Player.TurnStarted, OnTurnStarted);
		}

		private IEnumerable<BattleAction> OnTurnStarted(UnitEventArgs args)
		{
			NotifyActivating();
			foreach (Unit unit in Battle.AllAliveEnemies)
			{
				for (int i = 0; i < Level; i++)
				{
					if (!unit.IsAlive || Battle.BattleShouldEnd) { yield break; }
					yield return new ApplyStatusEffectAction<Cold>(unit, Level, 0, 0, 0);
				}
			}
			yield return new GainManaAction(Mana);
		}
	}
}