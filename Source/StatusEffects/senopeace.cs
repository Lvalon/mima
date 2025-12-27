using System.Collections.Generic;
using System.Linq;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Koishi;
using LBoLEntitySideloader.Attributes;

namespace lvalonmima.StatusEffects
{
	public sealed class senopeaceDef : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Special;
			config.RelativeEffects = new List<string>() { nameof(MoodPeace) };
			return config;
		}
	}

	[EntityLogic(typeof(senopeaceDef))]
	public sealed class senopeace : StatusEffect
	{
		public override bool ForceNotShowDownText => true;
		public ManaGroup Mana => new ManaGroup() { Colorless = 3 };
		protected override void OnAdded(Unit unit)
		{
			ReactOwnerEvent(Battle.RoundEnded, OnRoundEnded);
		}

		private IEnumerable<BattleAction> OnRoundEnded(GameEventArgs args)
		{
			if (Battle.BattleShouldEnd) { yield break; }
			Mood mood = (Mood)Battle.Player.StatusEffects.FirstOrDefault(se => se is MoodPeace);
			if (mood != null)
			{
				yield return new MoodChangeAction(Battle.Player, mood, null);
			}
		}
	}
}