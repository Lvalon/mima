using System;
using System.Collections.Generic;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.Cards.Enemy;
using LBoL.EntityLib.EnemyUnits.Normal.Ravens;
using LBoLEntitySideloader.Attributes;

namespace lvalonmima.StatusEffects
{
	public sealed class seRavenDef : lvalonmimaStatusEffectTemplate
	{
		public override StatusEffectConfig MakeConfig()
		{
			StatusEffectConfig config = GetDefaultStatusEffectConfig();
			config.Type = StatusEffectType.Positive;
			return config;
		}
	}

	[EntityLogic(typeof(seRavenDef))]
	public sealed class seRaven : StatusEffect
	{
		public override bool ForceNotShowDownText => true;
		protected override void OnAdded(Unit unit)
		{
			Highlight = Owner.HasStatusEffect<Graze>();
			HandleOwnerEvent(unit.StatusEffectAdded, OnSEAdded);
			HandleOwnerEvent(unit.StatusEffectRemoved, OnSERemoved);
			ReactOwnerEvent(unit.DamageReceived, OnDamageReceived);
		}

		private void OnSERemoved(StatusEffectEventArgs args)
		{
			Highlight = Owner.HasStatusEffect<Graze>();
		}

		private void OnSEAdded(StatusEffectApplyEventArgs args)
		{
			Highlight = Owner.HasStatusEffect<Graze>();
		}

		private IEnumerable<BattleAction> OnDamageReceived(DamageEventArgs args)
		{
			if (args.DamageInfo.IsGrazed)
			{
				NotifyActivating();
				if (Owner is RavenWen || Owner is RavenWen3)
					yield return new AddCardsToDiscardAction(Library.CreateCards<AyaNews>(2));
				else
					yield return new AddCardsToDiscardAction(Library.CreateCards<HatateNews>(2));
			}
		}
	}
}